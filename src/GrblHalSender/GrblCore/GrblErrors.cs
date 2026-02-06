using System.IO;
using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore;

public class GrblErrors
{
    private static Dictionary<int, string> messages = new Dictionary<int, string>();

    public static Dictionary<int, string> List { get { return messages; } }

    public static bool Get()
    {
        return Grbl.GrblViewModel != null && Get(Grbl.GrblViewModel);
    }

    public static bool Get(GrblViewModel model)
    {
        bool? res = null;
        bool noload;

        if (GrblInfo.HasEnums && messages.Count == 0)
        {
            PollGrbl.Suspend();
            CancellationToken cancellationToken = new CancellationToken();

            new Thread(() =>
            {
                res = WaitFor.AckResponse<string>(
                    cancellationToken,
                    response => Process(response),
                    a => model.OnResponseReceived += a,
                    a => model.OnResponseReceived -= a,
                    1000, () => Comms.com.WriteCommand(GrblConstants.CMD_GETERRORCODES));
            }).Start();

            while (res == null)
                EventUtils.DoEvents();

            PollGrbl.Resume();
        }

        if ((noload = messages.Count == 0) || Resources.Locale != "en-US")
        {
            try
            {
                StreamReader file = FileUtils.OpenFile(string.Format("{0}error_codes_{1}.csv", Resources.Path, Resources.Locale == "en-US" ? "en_US" : Resources.Locale));

                if (file == null && Resources.Locale != "en-US")
                    file = FileUtils.OpenFile(string.Format("{0}error_codes_en_US.csv", Resources.Path));

                if (file != null)
                {
                    string msg, line = file.ReadLine(); // Skip header  

                    line = file.ReadLine();

                    while (line != null)
                    {
                        int key;
                        string[] columns = line.Split(',');

                        columns[0].Replace("\"", "");

                        if (columns.Length == 3 && int.TryParse(columns[0], out key))
                        {
                            if (!noload && messages.TryGetValue(key, out msg))
                                messages.Remove(key);

                            messages.Add(key, columns[1] + ": " + columns[2]);
                        }

                        line = file.ReadLine();
                    }
                }

                file.Close();
            }
            catch
            {
            }
        }

        return messages.Count > 0;
    }

    private static void Process(string data)
    {
        if (data.StartsWith("[ERRORCODE:"))
        {
            var details = data.Substring(11).TrimEnd(']').Split('|');
            try
            {
                if (details.Length == 3)
                {
                    if (messages.ContainsKey(int.Parse(details[0])))
                    {
                        messages[int.Parse(details[0])] = details[2] == string.Empty ? details[1] : details[2];
                    }
                    else
                    {
                        messages.Add(int.Parse(details[0]), details[2] == string.Empty ? details[1] : details[2]);
                    }
                       
                }

                else
                    messages.Add(int.Parse(details[0]), details[1]);
            }
            catch
            {
            }
        }
    }

    static GrblErrors()
    {

    }

    public static string GetMessage(string code)
    {
        string message = null;

        try
        {
            if (messages != null)
                messages.TryGetValue(int.Parse(code), out message);
        }
        catch
        {
        }

        return message == null ? string.Format("error:{0}", code) : message;
    }
}