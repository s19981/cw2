using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace cw2
{
    public class UczelniaRoot
    {
        public Uczelnia uczelnia { get; set; }
    }

    [XmlRoot("uczelnia")]
    public class Uczelnia
    {
        [XmlAttribute]
        public string createdAt { get; set; }
        [XmlAttribute]
        public string author { get; set; }
        [XmlArrayItem(ElementName = "student", Type = typeof(Student))]
        [XmlArray("studenci")]
        [JsonPropertyName("studenci")]
        public List<Student> Studenci { get; set; }
        [XmlArrayItem(ElementName = "studies", Type = typeof(ActiveStudia))]
        [XmlArray("activeStudies")]
        [JsonPropertyName("activeStudies")]
        public List<ActiveStudia> ActiveStudies { get; set; }

        public Uczelnia()
        {
            createdAt = DateTime.Now.ToString("dd.mm.yyyy");
            author = "Monika Dubel";
        }
    }

    [XmlRoot("student")]
    public class Student
    {
        public string fname { get; set; }
        public string lname { get; set; }
        public string birthdate { get; set; }
        [XmlAttribute]
        public string indexNumber { get; set; }
        public string email { get; set; }
        public string mothersName { get; set; }
        public string fathersName { get; set; }
        public Studia studies { get; set; }

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

        public Student(string[] student)
        {
            fname = student[0];
            lname = student[1];
            indexNumber = student[4];
            birthdate = DateTime.Parse(student[5]).ToString("dd.mm.yyyy");
            email = student[6];
            mothersName = student[7];
            fathersName = student[8];
            studies = new Studia(student[2], student[3]);
        }
    }

    [XmlRoot("studies")]
    public class Studia
    {
        public string name { get; set; }
        public string mode { get; set; }

        public Studia()
        {
            name = null;
            mode = null;
        }

        public Studia(string sName, string sMode)
        {
            name = sName;
            mode = sMode;
        }
    }

    [XmlRoot("studies")]
    public class ActiveStudia
    {
        [XmlAttribute]
        public string name { get; set; }
        [XmlAttribute]
        public int numberOfStudents { get; set; }

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
            var file_path = "data.csv";
            var target_path = "żesult.xml";
            var log_path = "łog.txt";
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

                if (!File.Exists(file_path)) { throw new FileNotFoundException("Plik nazwa nie istnieje"); }

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
                            if (student.Length != 9) { throw new Exception("Wiersz student zawiera błędną liczbę kolumn: " + line); }
                            if (student.Any(x => String.IsNullOrEmpty(x))) { throw new Exception("Wiersz student zawiera pustą kolumnę: " + line); }

                            var st = new Student(student);
                            if (list.Exists(x => x.fname == st.fname && x.lname == st.lname && x.indexNumber == st.indexNumber)) { throw new Exception("Wiersz student jest duplikatem: " + line); }

                            list.Add(st);
                        }
                        catch (Exception e)
                        {
                            sw.WriteLine(e);
                        }
                    }

                    var query = list.GroupBy(
                        student => student.studies.name,
                        student => student.indexNumber,
                        (name, id) => new
                        {
                            Key = name,
                            Count = id.Count()
                        }
                    );

                    foreach (var r in query)
                    {
                        listOfStudies.Add(new ActiveStudia(r.Key, r.Count));
                    }

                    uczelnia.Studenci = list;
                    uczelnia.ActiveStudies = listOfStudies;


                    if (data_format == "xml")
                    {
                        FileStream writer = new FileStream(target_path, FileMode.Create);
                        XmlSerializer serializer = new XmlSerializer(typeof(Uczelnia));
                        XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                        ns.Add("", "");
                        serializer.Serialize(writer, uczelnia, ns);
                    }

                    if (data_format == "json")
                    {
                        var uczelniaRoot = new UczelniaRoot();
                        uczelniaRoot.uczelnia = uczelnia;
                        var jsonString = JsonSerializer.Serialize(uczelniaRoot);
                        File.WriteAllText(target_path, jsonString);
                    }
                }


            }
            catch (Exception e)
            {
                sw.WriteLine(e);
            }
        }
    }
}