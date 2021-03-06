﻿using AlarmWorkflow.Shared.Core;

namespace AlarmWorkflow.Parser.GenericParser.Parsing
{
    /// <summary>
    /// Provides a section parser that parses a "comment" section.
    /// </summary>
    [Export("CommentSectionParser", typeof(ISectionParser))]
    [Information(DisplayName = "CommentSectionParser_DisplayName", Description = "CommentSectionParser_Description")]
    public class CommentSectionParser : ISectionParser
    {
        #region ISectionParser Members

        System.Collections.Generic.IEnumerable<string> ISectionParser.GetTokens()
        {
            yield break;
        }

        void ISectionParser.OnLoad(System.Collections.Generic.IDictionary<string, string> parameters)
        {

        }

        void ISectionParser.OnSave(System.Collections.Generic.IDictionary<string, string> parameters)
        {

        }

        void ISectionParser.OnEnterSection(Operation operation)
        {

        }

        void ISectionParser.OnLeaveSection(Operation operation)
        {
            // Remove trailing newline if any
            if (!string.IsNullOrWhiteSpace(operation.Comment) && operation.Comment.EndsWith("\n"))
            {
                operation.Comment = operation.Comment.Substring(0, operation.Comment.Length - 1).Trim();
            }
        }

        void ISectionParser.Populate(AreaToken token, Operation operation)
        {
            // Just append...
            operation.Comment += token.OriginalValue + "\n";
        }

        #endregion
    }
}
