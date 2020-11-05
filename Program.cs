using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace cw2
{
    [XmlRoot("uczelnia")]
    public class Uczelnia
    {
        [XmlAttribute]
        public string createdAt = DateTime.Now.ToString("dd.mm.yyyy");
        [XmlAttribute]
        public string author = "Monika Dubel";
        [XmlArrayItem(ElementName = "student", Type = typeof(Student))]
        [XmlArray("studenci")]
        public List<Student> Studenci;
        [XmlArrayItem(ElementName = "studies", Type = typeof(ActiveStudia))]
        [XmlArray("activeStudies")]
        public List<ActiveStudia> ActiveStudies;
    }
    [XmlRoot("student")]
    public class Student
    {
        public string fname;
        public string lname;
        public string birthdate;
        [XmlAttribute]
        public string indexNumber;
        public string email;
        public string mothersName;
        public string fathersName;
        public Studia studies;

        public Student()
        {
            fname = null;
            lname = null;
            birthdate = null;
            indexNumber = null;
            email = null;
            mothersName = null;
            fathersName = null;
            studies = new Studia();
        }
    }

    [XmlRoot("studies")]
    public class Studia
    {
        public string name;
        public string mode;

        public Studia()
        {
            name = null;
            mode = null;
        }
    }

    [XmlRoot("studies")]
    public class ActiveStudia
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public int numberOfStudents;

        public ActiveStudia()
        {
            name = null;
            numberOfStudents = 0;
        }

        public ActiveStudia(string aName, int aNumber)
        {
            name = aName;
            numberOfStudents = aNumber;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var file_path = "/Users/monika/Desktop/APBD/cw2/data.csv";
            var target_path = "/Users/monika/Desktop/APBD/cw2/żesult.xml";
            var log_path = "/Users/monika/Desktop/APBD/cw2/łog.txt";
            var data_format = "xml";

            StreamWriter sw = File.CreateText(log_path);

            try
            {
                if (args.Length == 3)
                {
                    file_path = args[0];
                    target_path = args[1];
                    data_format = args[2];
                }
            }
            catch (System.IndexOutOfRangeException e)
            {
                sw.WriteLine(e);
            }

            FileStream writer = new FileStream(target_path, FileMode.Create);
            XmlSerializer serializer = new XmlSerializer(typeof(Uczelnia));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var uczelnia = new Uczelnia();
            var list = new List<Student>();
            var listOfStudies = new List<ActiveStudia>();
            string line = null;

            
            using (var stream = new StreamReader(File.OpenRead(file_path)))
            {
                while ((line = stream.ReadLine()) != null)
                {
                    try
                    {
                        string[] student = line.Split(',');
                        if (student.Length != 9)
                        {
                            throw new Exception("Wiersz student zawiera błędną liczbę kolumn: " + line);
                        }
                        if (student.Any(x => String.IsNullOrEmpty(x)))
                        {
                            throw new Exception("Wiersz student zawiera pustą kolumnę: " + line);
                        }
                        var st = new Student();
                        st.fname = student[0];
                        st.lname = student[1];
                        st.indexNumber = student[4];
                        st.birthdate = DateTime.Parse(student[5]).ToString("dd.mm.yyyy");
                        st.email = student[6];
                        st.mothersName = student[7];
                        st.fathersName = student[8];
                        st.studies.name = student[2];
                        st.studies.mode = student[3];

                        if (list.Exists(x => x.fname == st.fname && x.lname == st.lname && x.indexNumber == st.indexNumber))
                        {
                            throw new Exception("Wiersz student jest duplikatem: " + line);
                        }
                        list.Add(st);
                    }
                    catch (Exception e)
                    {
                        sw.WriteLine(e);
                    }
                }
            }

            var query = list.GroupBy(
                student => student.studies.name,
                student => student.indexNumber,
                (name, id) => new
                {
                    Key = name,
                    Count = id.Count()
                });

            foreach (var r in query)
            {
                listOfStudies.Add(new ActiveStudia(r.Key, r.Count));
            }

            uczelnia.Studenci = list;
            uczelnia.ActiveStudies = listOfStudies;
            serializer.Serialize(writer, uczelnia, ns);

        }
    }
}
