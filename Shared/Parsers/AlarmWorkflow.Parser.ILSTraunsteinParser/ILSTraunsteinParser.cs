using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using AlarmWorkflow.AlarmSource.Fax;
using AlarmWorkflow.Shared.Core;
using AlarmWorkflow.Shared.Diagnostics;

namespace AlarmWorkflow.Parser.ILSTraunsteinParser
{
    /// <summary>
    /// Provides a parser that parses faxes from the ILSTraunsteinParser.
    /// </summary>
    [Export("ILSTraunsteinParser", typeof(IFaxParser))]
    sealed class ILSTraunsteinParser : IFaxParser
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the ILSTraunsteinParser class.
        /// </summary>
        /// <param name="logger">The logger object.</param>
        /// <param name="replaceList">The RreplaceList object.</param>
        public ILSTraunsteinParser()
        {

        }

        #endregion

        #region IFaxParser Members

        Operation IFaxParser.Parse(string[] lines)
        {
            Operation operation = new Operation();
            OperationResource last = new OperationResource();

            lines = Utilities.Trim(lines);

            CurrentSection section = CurrentSection.AHeader;

            for (int i = 0; i < lines.Length; i++)
            {
                try
                {

                //Definition der bool Variablen
                //bool nextIsOrt = false;
                bool ReplStreet = false;
                bool ReplCity = false;
                bool ReplComment = false;
                bool ReplPicture = false;
                bool Faxtime = false;
                bool nextIsOrt = false;
                //bool getAlarmTime = false;

                foreach (string linex in lines)
                {

                    string msgx;
                    string prefix;
                    int x = linex.IndexOf(':');
                    if (x != -1)
                    {
                        prefix = linex.Substring(0, x);
                        msgx = linex.Substring(x + 1).Trim();

                        prefix = prefix.Trim().ToUpperInvariant();
                        switch (prefix)
                        {

                            //F�llen der Standardinformatione Alarmfax Cases mit  ":"
                            case "EINSATZORT":
                                operation.Einsatzort.Location = msgx;
                                break;
                            case "STRA�E":
                            case "STRABE":
                                operation.Einsatzort.Street = msgx;
                                break;                           
                            case "EINSATZPLAN":
                                operation.OperationPlan = msgx;
                                break;
                        }
                    }
                }

                    string line = lines[i];
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    // Einlesen mehrzeilige Bemerkung
                    switch (line.Trim())
                    {

                        case "BEMERKUNG": { section = CurrentSection.GBemerkung; continue; }
                        case "TEXTBAUSTEINE": { section = CurrentSection.HFooter; continue; }
                        default: break;
                    }

                    string msg = line;

                    // Bemerkung Section
                    switch (section)
                    {

                        case CurrentSection.GBemerkung:
                            {
                                // Append with newline at the end in case that the message spans more than one line
                                operation.Comment = operation.Comment += msg + "\n";
                                operation.Comment = operation.Comment.Substring(0, operation.Comment.Length - 1).Trim();
                            }
                            break;
                        case CurrentSection.HFooter:
                            // The footer can be ignored completely.
                            break;
                        default:
                            break;
                    }

                    //Auslesen der Alarmierungszeit
                    //TODO INFO ILS Ausstehend
                    int x0 = line.IndexOf("DEG FF");
                    if (x0 != -1)
                    {

                        int anfang = line.IndexOf(':');

                        string altime = line.Substring(anfang + 15);       
                        altime = altime.Substring(0, altime.Length - 1); 
                        altime = altime.Trim();                                        
                        operation.CustomData["Alarmtime"] = "Alarmzeit: " + altime;
                        //getAlarmTime = true;

                    }

                    // Auslesen des Zeitpunkts des Faxeingangs
                    if (Faxtime == false)
                    {
                        DateTime uhrzeit = DateTime.Now;
                        operation.CustomData["Faxtime"] = "Faxeingang: " + uhrzeit.ToString("HH:mm:ss ");
                        Faxtime = true;
                    }

                    // Weitere Standardinfos auslesen, ohne ":"
                    if (line.StartsWith("Einsatznummer"))
                    {
                        operation.OperationNumber = line.Substring(14);
                    }

                    if (line.StartsWith("Objekt"))
                    {
                        operation.Einsatzort.Property = line.Substring(7);
                        operation.Einsatzort.Property = operation.Einsatzort.Property.Trim();
                    }                     

                    if (line.StartsWith("Name"))
                    {
                        operation.Messenger = operation.Messenger + line.Substring(5);
                    }

                    operation.Messenger = operation.Messenger + " ";

                    if (operation.Messenger.Contains("Ausger�ckt") == true)
                    {
                        operation.Messenger = operation.Messenger.Replace(": Alarmiert : Ausger�ckt", "");
                        operation.Messenger = operation.Messenger.Trim();
                    }

                    if (line.StartsWith("Schlagw."))
                    {
                        operation.Picture = operation.Picture + line.Substring(11);
                        operation.Picture = operation.Picture.Trim();
                    }
                    //TODO Pr�fen mit TIF Faxen
                    if (line.StartsWith("Stichw. B"))
                    {
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword + line.Substring(10);
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword.Trim();
                    }

                    if (line.StartsWith("Stichw. T"))
                    {
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword + line.Substring(10);
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword.Trim();
                    }

                    if (line.StartsWith("Stichw. S"))
                    {
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword + line.Substring(10);
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword.Trim();
                    }

                    if (line.StartsWith("Stichw. I"))
                    {
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword + line.Substring(10);
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword.Trim();
                    }

                    if (line.StartsWith("Stichw. R"))
                    {
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword + line.Substring(10);
                        operation.Keywords.EmergencyKeyword = operation.Keywords.EmergencyKeyword.Trim();
                    }

                    //Ort Einlesen
                    if ((line.StartsWith("Ort")) && (nextIsOrt == false))
                    {
                        operation.Einsatzort.City = operation.Einsatzort.City + line.Substring(4);
                        operation.Einsatzort.City = operation.Einsatzort.City.Trim();
                        nextIsOrt = true;
                    }

                    // Sonderzeichenersetzung im Meldebild

                    if (ReplPicture == false)
                    {
                        operation.Picture = operation.Picture + " ";
                        ReplPicture = true;
                    }

                    if (operation.Picture.Contains("�") == true)
                    {
                        operation.Picture = operation.Picture.Replace("�", "ss");
                    }

                    if (operation.Picture.Contains("�") == true)
                    {
                        operation.Picture = operation.Picture.Replace("�", "ae");
                    }

                    if (operation.Picture.Contains("�") == true)
                    {
                        operation.Picture = operation.Picture.Replace("�", "oe");
                    }

                    if (operation.Picture.Contains("�") == true)
                    {
                        operation.Picture = operation.Picture.Replace("�", "ue");
                    }

                    // Sonderzeichenersetzung im Ort

                    if (ReplCity == false)
                    {
                        operation.Einsatzort.City = operation.Einsatzort.City + " ";
                        ReplCity = true;
                    }

                    if (operation.Einsatzort.City.Contains("�") == true)
                    {
                        operation.Einsatzort.City = operation.Einsatzort.City.Replace("�", "ss");
                    }

                    if (operation.Einsatzort.City.Contains("�") == true)
                    {
                        operation.Einsatzort.City = operation.Einsatzort.City.Replace("�", "ae");
                    }

                    if (operation.Einsatzort.City.Contains("�") == true)
                    {
                        operation.Einsatzort.City = operation.Einsatzort.City.Replace("�", "oe");
                    }

                    if (operation.Einsatzort.City.Contains("�") == true)
                    {
                        operation.Einsatzort.City = operation.Einsatzort.City.Replace("�", "ue");
                    }

                    // Sonderzeichenersetzung in der Strasse

                    if (ReplStreet == false)
                    {
                        operation.Einsatzort.Street = operation.Einsatzort.Street + " ";
                        ReplStreet = true;
                    }

                    if (operation.Einsatzort.Street.Contains("Haus-Nr.:") == true)
                    {
                        operation.Einsatzort.Street = operation.Einsatzort.Street.Replace("Haus-Nr.:", "");
                    }

                    if (operation.Einsatzort.Street.Contains("�") == true)
                    {
                        operation.Einsatzort.Street = operation.Einsatzort.Street.Replace("�", "ss");
                    }

                    if (operation.Einsatzort.Street.Contains("�") == true)
                    {
                        operation.Einsatzort.Street = operation.Einsatzort.Street.Replace("�", "ae");
                    }

                    if (operation.Einsatzort.Street.Contains("�") == true)
                    {
                        operation.Einsatzort.Street = operation.Einsatzort.Street.Replace("�", "oe");
                    }

                    if (operation.Einsatzort.Street.Contains("�") == true)
                    {
                        operation.Einsatzort.Street = operation.Einsatzort.Street.Replace("�", "ue");
                    }

                    // Sonderzeichenersetzung im Hinweis

                    if (ReplComment == false)
                    {
                        operation.Comment = operation.Comment + " ";
                        ReplComment = true;
                    }

                    if (operation.Comment.Contains("�") == true)
                    {
                        operation.Comment = operation.Comment.Replace("�", "ss");
                    }

                    if (operation.Comment.Contains("�") == true)
                    {
                        operation.Comment = operation.Comment.Replace("�", "ae");
                    }

                    if (operation.Comment.Contains("�") == true)
                    {
                        operation.Comment = operation.Comment.Replace("�", "oe");
                    }

                    if (operation.Comment.Contains("�") == true)
                    {
                        operation.Comment = operation.Comment.Replace("�", "ue");
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
            GBemerkung,
            /// <summary>
            /// Footer text. Can be ignored completely.
            /// </summary>
            HFooter,
        }

        #endregion

    }
}
