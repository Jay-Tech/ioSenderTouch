using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using GrblHalSender.GrblCore;

namespace GrblHalSender.ViewModels.Probing
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
                if (File.Exists(Resources.Path + "ProbingMacroCommand.xml"))
                {
                    using var reader = new StreamReader(Resources.Path + "ProbingMacroCommand.xml");
                    var contents = xs.Deserialize(reader);
                    if (contents != null)
                    {
                        Macros = (ObservableCollection<ProbingMacro>)contents;
                    }
                    
                    reader.Close();
                }
                //else
                //{
                //    if (Directory.Exists(Resources.Path))
                //    {
                //        File.Create(Resources.Path + "ProbingMacroCommand.xml");
                //    }
                    
                //}
                
            }
            catch
            {
            }
        }

    }
}

