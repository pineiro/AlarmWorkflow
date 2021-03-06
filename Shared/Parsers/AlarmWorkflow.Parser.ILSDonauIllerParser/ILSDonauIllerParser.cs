﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using AlarmWorkflow.AlarmSource.Fax;
using AlarmWorkflow.Shared.Core;
using AlarmWorkflow.Shared.Diagnostics;
using AlarmWorkflow.Shared.Settings;

namespace AlarmWorkflow.Parser.ILSDonauIllerParser
{
    /// <summary>
    /// Provides a parser that parses faxes from the ILS ILSDonauIllerParser.
    /// </summary>
    [Export("ILSDonauIllerParser", typeof(IFaxParser))]
    sealed class ILSDonauIllerParser : IFaxParser
    {
        #region Constants

        private static readonly string[] Keywords = new[] { 
            "ABSENDER", "FAX", "TERMIN", "EINSATZNUMMER", "NAME", "STRAßE", "ORT", "OBJEKT", "PLANNUMMER", 
            "STATION", "STRAßE", "ORT", "OBJEKT", "STATION", "SCHLAGW", "STICHWORT", "PRIO", 
            "EINSATZMITTEL", "ALARMIERT", "AUSSTATTUNG" };

        #endregion

        #region Constructor

        public ILSDonauIllerParser()
        {
            _fdUnits = new Dictionary<string, string>();
            string[] units = SettingsManager.Instance.GetSetting("Shared", "FD.Units").GetStringArray();
            foreach (string unit in units)
            {
                string[] result = unit.Split(new[] { "=;=" }, StringSplitOptions.None);
                if (result.Length == 2)
                {
                    _fdUnits.Add(result[0], result[1]);
                }
                else
                {
                    _fdUnits.Add(unit, unit);
                }
            }
        }

        #endregion

        #region Fields
        private readonly Dictionary<String, String> _fdUnits;
        #endregion

        #region Methods

        private DateTime ReadFaxTimestamp(string line, DateTime fallback)
        {
            DateTime date = fallback;
            TimeSpan timestamp = date.TimeOfDay;

            Match dt = Regex.Match(line, @"(0[1-9]|[12][0-9]|3[01])[- /.](0[1-9]|1[012])[- /.](19|20)\d\d");
            Match ts = Regex.Match(line, @"([01]?[0-9]|2[0-3]):[0-5][0-9]");
            if (dt.Success)
            {
                DateTime.TryParse(dt.Value, out date);
            }
            if (ts.Success)
            {
                TimeSpan.TryParse(ts.Value, out timestamp);
            }

            return new DateTime(date.Year, date.Month, date.Day, timestamp.Hours, timestamp.Minutes, timestamp.Seconds, timestamp.Milliseconds, DateTimeKind.Local);
        }

        private bool StartsWithKeyword(string line, out string keyword)
        {
            line = line.ToUpperInvariant();
            foreach (string kwd in Keywords)
            {
                if (line.StartsWith(kwd))
                {
                    keyword = kwd;
                    return true;
                }
            }
            keyword = null;
            return false;
        }

        /// <summary>
        /// Returns the message text, which is the line text but excluding the keyword/prefix and a possible colon.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="prefix">The prefix that is to be removed (optional).</param>
        /// <returns></returns>
        private string GetMessageText(string line, string prefix)
        {
            if (prefix == null)
            {
                prefix = "";
            }

            if (prefix.Length > 0)
            {
                line = line.Remove(0, prefix.Length).Trim();
            }
            else
            {
                int colonIndex = line.IndexOf(':');
                if (colonIndex != -1)
                {
                    line = line.Remove(0, colonIndex + 1);
                }
            }

            if (line.StartsWith(":"))
            {
                line = line.Remove(0, 1).Trim();
            }

            return line;
        }

        /// <summary>
        /// Attempts to read the zip code from the city, if available.
        /// </summary>
        /// <param name="cityText"></param>
        /// <returns>The zip code of the city. -or- null, if there was no.</returns>
        private string ReadZipCodeFromCity(string cityText)
        {
            string zipCode = "";
            foreach (char c in cityText)
            {
                if (char.IsNumber(c))
                {
                    zipCode += c;
                    continue;
                }
                break;
            }
            return zipCode;
        }
        private bool GetSection(String line, ref CurrentSection section, out bool keywordsOnly)
        {
            if (line.Contains("MITTEILER"))
            {
                section = CurrentSection.BMitteiler;
                keywordsOnly = true;
                return true;
            }
            if (line.Contains("EINSATZORT"))
            {
                section = CurrentSection.CEinsatzort;
                keywordsOnly = true;
                return true;
            }
            if (line.Contains("ZIELORT"))
            {
                section = CurrentSection.DZielort;
                keywordsOnly = true;
                return true;
            }
            if (line.Contains("EINSATZGRUND"))
            {
                section = CurrentSection.EEinsatzgrund;
                keywordsOnly = true;
                return true;
            }
            if (line.Contains("EINSATZMITTEL"))
            {
                section = CurrentSection.FEinsatzmittel;
                keywordsOnly = true;
                return true;
            }
            if (line.Contains("BEMERKUNG"))
            {
                section = CurrentSection.GBemerkung;
                keywordsOnly = false;
                return true;
            }
            if (line.Contains("ENDE FAX"))
            {
                section = CurrentSection.HFooter;
                keywordsOnly = false;
                return true;
            }
            keywordsOnly = true;
            return false;
        }
        #endregion

        #region IFaxParser Members

        Operation IFaxParser.Parse(string[] lines)
        {
            Operation operation = new Operation();
            OperationResource last = new OperationResource();

            lines = Utilities.Trim(lines);

            CurrentSection section = CurrentSection.AHeader;
            bool keywordsOnly = true;
            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    string line = lines[i];
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    // Try to parse the header and extract date and time if possible
                    operation.Timestamp = ReadFaxTimestamp(line, operation.Timestamp);


                    if (GetSection(line.Trim(), ref section, out keywordsOnly))
                    {
                        continue;
                    }

                    string msg = line;
                    string prefix = "";

                    // Make the keyword check - or not (depends on the section we are in; see above)
                    if (keywordsOnly)
                    {
                        string keyword;
                        if (!StartsWithKeyword(line, out keyword))
                        {
                            continue;
                        }

                        int x = line.IndexOf(':');
                        if (x == -1)
                        {
                            // If there is no colon found (may happen occasionally) then simply remove the length of the keyword from the beginning
                            prefix = keyword;
                            msg = line.Remove(0, prefix.Length).Trim();
                        }
                        else
                        {
                            prefix = line.Substring(0, x);
                            msg = line.Substring(x + 1).Trim();
                        }

                        prefix = prefix.Trim().ToUpperInvariant();
                    }

                    // Parse each section
                    switch (section)
                    {
                        case CurrentSection.AHeader:
                            {
                                switch (prefix)
                                {
                                    case "ABSENDER":
                                        operation.CustomData["Absender"] = msg;
                                        break;
                                    case "TERMIN":
                                        operation.CustomData["Termin"] = msg;
                                        break;
                                    case "EINSATZNUMMER":
                                        operation.OperationNumber = msg;
                                        break;
                                }
                            }
                            break;
                        case CurrentSection.BMitteiler:
                            {
                                // This switch would not be necessary in this section (there is only "Name")...
                                switch (prefix)
                                {
                                    case "NAME":
                                        operation.Messenger = msg;
                                        break;
                                }
                            }
                            break;
                        case CurrentSection.CEinsatzort:
                            {
                                switch (prefix)
                                {
                                    case "STRAßE":
                                        {
                                            // The street here is mangled together with the street number. Dissect them...
                                            int streetNumberColonIndex = msg.LastIndexOf(':');
                                            if (streetNumberColonIndex != -1)
                                            {
                                                // We need to check for occurrence of the colon, because it may have been omitted by the OCR-software
                                                string streetNumber = msg.Remove(0, streetNumberColonIndex + 1).Trim();
                                                operation.Einsatzort.StreetNumber = streetNumber;
                                            }

                                            operation.Einsatzort.Street = msg.Substring(0, msg.IndexOf("Haus-", StringComparison.Ordinal)).Trim();
                                        }
                                        break;
                                    case "ORT":
                                        {
                                            operation.Einsatzort.ZipCode = ReadZipCodeFromCity(msg);
                                            if (string.IsNullOrWhiteSpace(operation.Einsatzort.ZipCode))
                                            {
                                                Logger.Instance.LogFormat(LogType.Warning, this, "Could not find a zip code for city '{0}'. Route planning may fail or yield wrong results!", operation.Einsatzort.City);
                                            }

                                            operation.Einsatzort.City = msg.Remove(0, operation.Einsatzort.ZipCode.Length).Trim();

                                            // The City-text often contains a dash after which the administrative city appears multiple times (like "City A - City A City A").
                                            // However we can (at least with google maps) omit this information without problems!
                                            int dashIndex = operation.Einsatzort.City.IndexOf('-');
                                            if (dashIndex != -1)
                                            {
                                                // Ignore everything after the dash
                                                operation.Einsatzort.City = operation.Einsatzort.City.Substring(0, dashIndex).Trim();
                                            }
                                        }
                                        break;
                                    case "OBJEKT":
                                        operation.Einsatzort.Property = msg.StartsWith("6") ? msg.Substring(10, msg.Length - 10) : msg;
                                        break;
                                    case "PLANNUMMER":
                                        operation.CustomData["Einsatzort Plannummer"] = msg;
                                        break;
                                    case "STATION":
                                        operation.CustomData["Einsatzort Station"] = msg;
                                        break;
                                }
                            }
                            break;
                        case CurrentSection.DZielort:
                            {
                                switch (prefix)
                                {
                                    case "STRAßE":
                                        {
                                            // The street here is mangled together with the street number. Dissect them...
                                            int streetNumberColonIndex = msg.LastIndexOf(':');
                                            if (streetNumberColonIndex != -1)
                                            {
                                                // We need to check for occurrence of the colon, because it may have been omitted by the OCR-software
                                                operation.Zielort.StreetNumber = msg.Remove(0, streetNumberColonIndex + 1).Trim();
                                            }

                                            operation.Zielort.Street = msg.Substring(0, msg.IndexOf("Haus-", StringComparison.Ordinal)).Trim();
                                        }
                                        break;
                                    case "ORT":
                                        {
                                            string plz = ReadZipCodeFromCity(msg);
                                            operation.Zielort.ZipCode = plz;
                                            operation.Zielort.City = msg.Remove(0, plz.Length).Trim();
                                        }
                                        break;
                                    case "OBJEKT":
                                        operation.CustomData["Zielort Objekt"] = msg;
                                        break;
                                    case "STATION":
                                        operation.CustomData["Zielort Station"] = msg;
                                        break;
                                }
                            }
                            break;
                        case CurrentSection.EEinsatzgrund:
                            {
                                switch (prefix)
                                {
                                    case "SCHLAGW.":
                                        operation.Keywords.Keyword = msg;
                                        break;
                                    case "STICHWORT B":
                                        operation.Keywords.B = msg;
                                        break;
                                    case "STICHWORT T":
                                        operation.Keywords.T = msg;
                                        break;
                                    case "STICHWORT S":
                                        operation.Keywords.S = msg;
                                        break;
                                    case "STICHWORT I":
                                        operation.CustomData["Stichwort I"] = msg;
                                        break;
                                    case "STICHWORT R":
                                        operation.Keywords.R = msg;
                                        break;
                                    case "PRIO.":
                                        operation.Priority = msg;
                                        break;
                                }
                            }
                            break;
                        case CurrentSection.FEinsatzmittel:
                            {
                                if (line.StartsWith("NAME", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    msg = GetMessageText(line, "NAME");
                                    last.FullName = msg.Trim();
                                }
                                else if (line.StartsWith("ALARMIERT", StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(msg))
                                {
                                    msg = GetMessageText(line, "Alarmiert");

                                    // In case that parsing the time failed, we just assume that the resource got requested right away.
                                    DateTime dt;
                                    // Most of the time the OCR-software reads the colon as a "1", so we check this case right here.
                                    if (!DateTime.TryParseExact(msg, "dd.MM.yyyy HH1mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                    {
                                        // If this is NOT the case and it was parsed correctly, try it here
                                        DateTime.TryParseExact(msg, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);
                                    }

                                    last.Timestamp = dt.ToString(CultureInfo.InvariantCulture);
                                }
                                else if (line.StartsWith("AUSSTATTUNG", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    msg = GetMessageText(line, "Ausstattung");

                                    // Only add to requested equipment if there is some text,
                                    // otherwise the whole vehicle is the requested equipment
                                    if (!string.IsNullOrWhiteSpace(msg))
                                    {
                                        last.RequestedEquipment.Add(msg);
                                        Logger.Instance.LogFormat(LogType.Info, this, "Aus '" + msg + "'");
                                    }
                                    foreach (KeyValuePair<string, string> fdUnit in _fdUnits)
                                    {
                                        if (last.FullName.ToLower().Contains(fdUnit.Key.ToLower()))
                                        {
                                            operation.OperationPlan += " - " + fdUnit.Value;
                                            break;
                                        }
                                    }
                                    // This line will end the construction of this resource. Add it to the list and go to the next.
                                    operation.Resources.Add(last);

                                    last = new OperationResource();
                                }
                            }
                            break;
                        case CurrentSection.GBemerkung:
                            {
                                // Append with newline at the end in case that the message spans more than one line
                                operation.Comment = operation.Comment += msg + "\n";
                            }
                            break;
                        case CurrentSection.HFooter:
                            // The footer can be ignored completely.
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogFormat(LogType.Warning, this, "Error while parsing line '{0}'. The error message was: {1}", i, ex.Message);
                }
            }

            // Post-processing the operation if needed
            if (!string.IsNullOrWhiteSpace(operation.Comment) && operation.Comment.EndsWith("\n"))
            {
                operation.Comment = operation.Comment.Substring(0, operation.Comment.Length - 1).Trim();
            }
            return operation;
        }

        #endregion

        #region Nested types

        private enum CurrentSection
        {
            AHeader,
            BMitteiler,
            CEinsatzort,
            DZielort,
            EEinsatzgrund,
            FEinsatzmittel,
            GBemerkung,
            /// <summary>
            /// Footer text. Introduced by "ENDE FAX". Can be ignored completely.
            /// </summary>
            HFooter,
        }

        #endregion

    }
}
