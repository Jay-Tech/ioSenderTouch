using System.IO;
using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore;

public static class GrblSettingGroups
{
    public static List<GrblSettingGroup> Groups { get; private set; } = new List<GrblSettingGroup>();

    public static bool Get()
    {
        return Grbl.GrblViewModel != null && Get(Grbl.GrblViewModel);
    }

    public static bool Get(GrblViewModel model)
    {
        bool? res = null;

        if (GrblInfo.HasEnums && Groups.Count == 0)
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
                    400, () => Comms.com.WriteCommand(GrblConstants.CMD_GETSETTINGSGROUPS));
            }).Start();

            while (res == null)
                EventUtils.DoEvents();

            PollGrbl.Resume();
        }

        if (Resources.Locale != "en-US")
        {
            try
            {
                StreamReader file = new StreamReader(string.Format("{0}setting_groups_{1}.csv", Resources.Path, Resources.Locale));

                if (file != null)
                {
                    string line = file.ReadLine(); // Skip header

                    line = file.ReadLine();

                    while (line != null)
                    {
                        string[] columns = line.Split('\t');

                        if (columns.Length == 3)
                        {
                            var group = Groups.Where(x => x.Id == int.Parse(columns[0])).FirstOrDefault();
                            if (group != null)
                                group.Name = columns[1];
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

        return Groups.Count > 0;
    }

    public static void RemoveUnused()
    {
        List<GrblSettingGroup> remove = new List<GrblSettingGroup>();

        foreach (var group in Groups)
        {
            if (GrblSettings.Settings.Where(x => x.GroupId == group.Id).FirstOrDefault() == null)
                remove.Add(group);
        }

        foreach (var group in remove)
        {
            Groups.Remove(group);
        }
    }

    private static void Process(string data)
    {
        if (data != "ok")
        {
            string[] valuepair = data.TrimEnd(']').Split(':');
            if (valuepair.Length == 2 && valuepair[0] == "[SETTINGGROUP")
            {
                Groups.Add(new GrblSettingGroup(valuepair[1]));
            }
        }
    }
}