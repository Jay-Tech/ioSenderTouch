using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using ioSenderTouch.GrblCore;


namespace ioSenderTouch.ViewModels.Probing
{
    public class ProbingMacros
    {
        public ObservableCollection<ProbingMacro> Macros = new ObservableCollection<ProbingMacro>();
        public void Save()
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<ProbingMacro>));
            try
            {
                FileStream fsout = new FileStream(Resources.Path + "ProbingMacroCommand.xml", FileMode.Create, FileAccess.Write, FileShare.None);
                using (fsout)
                {
                    xs.Serialize(fsout, Macros);
                }
            }
            catch
            {
            }
        }

        public void Load()
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<ProbingMacro>));

            try
            {
                StreamReader reader = new StreamReader(Resources.Path + "ProbingMacroCommand.xml");
                Macros = (ObservableCollection<ProbingMacro>)xs.Deserialize(reader);
                reader.Close();
            }
            catch
            {
            }
        }

    }
}

