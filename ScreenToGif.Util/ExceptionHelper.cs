using System.Text;

namespace ScreenToGif.Util;

public static class ExceptionHelper
{
    public static string AsText(this Exception exception)
    {
        var writer = new StringBuilder();
        var current = exception;
        var level = 1;

        while (current != null)
        {
            writer.AppendLine(new string('▬', level) + $" Message - {Environment.NewLine}\t{current.Message}");
            writer.AppendLine(new string('○', level) + $" Type - {Environment.NewLine}\t{current.GetType()}");
            writer.AppendLine(new string('▲', level) + $" Source - {Environment.NewLine}\t{current.Source}");
            writer.AppendLine(new string('▼', level) + $" TargetSite - {Environment.NewLine}\t{current.TargetSite}");

            if (current is BadImageFormatException bad)
            {
                writer.AppendLine(new string('☼', level) + $" Filename - {Environment.NewLine}\t{bad.FileName}");
                writer.AppendLine(new string('►', level) + $" Fuslog - {Environment.NewLine}\t{bad.FusionLog}");
            }
            else if (current is ArgumentException arg)
            {
                writer.AppendLine(new string('☼', level) + $" ParamName - {Environment.NewLine}\t{arg.ParamName}");
            }

            if (current.HelpLink != null)
                writer.AppendLine(new string('◘', level) + $" Other - {Environment.NewLine}\t{current.HelpLink}");

            writer.AppendLine(new string('♠', level) + $" StackTrace - {Environment.NewLine}{current.StackTrace}");

            if (current.InnerException == null || level >= 6)
                break;

            writer.AppendLine();

            level++;
            current = current.InnerException;
        }

        return writer.ToString();
    }   
}