//using HelixToolkit.Wpf;
using HelixToolkit.Wpf;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace DataProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static public AtomsTable LoadAtoms()
        {

            string content = File.ReadAllText("atoms3.json");

            RootEx r = JsonConvert.DeserializeObject<RootEx>(content);

            return r.AtomsTable;
        }


        static public string GetContentForName(string name)
        {
            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");
            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/XML?record_type=3d", Method.POST);
            var response = client.Execute(request);
            var content = response.Content;
            return content;

        }


        static public AutoComplete Autocomplete(string name)
        {

            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");

            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/autocomplete/compound/" + name + "/json?limit=60000", Method.POST);
            var response = client.Execute(request);
            var content = response.Content; // raw content as 
           // richTextBox1.Text = content;


            AutoComplete autos = JsonConvert.DeserializeObject<AutoComplete>(content);

            // https://pubchem.ncbi.nlm.nih.gov/rest/autocomplete/compound/aspirin/jsonp?limit=6


            return autos;

        }

        public PropertyTable Description(string name)
        {

            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");

            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" +  name + "/property/MolecularFormula,MolecularWeight,CanonicalSMILES,IsomericSMILES,InChI,InChIKey,IUPACName,XLogP,ExactMass,MonoisotopicMass,TPSA,Complexity,Charge,HBondDonorCount,HBondAcceptorCount,RotatableBondCount,HeavyAtomCount,IsotopeAtomCount,AtomStereoCount,DefinedAtomStereoCount,UndefinedAtomStereoCount,BondStereoCount,DefinedBondStereoCount,UndefinedBondStereoCount,CovalentUnitCount,Volume3D,XStericQuadrupole3D,YStericQuadrupole3D,ZStericQuadrupole3D,FeatureCount3D,FeatureAcceptorCount3D,FeatureDonorCount3D,FeatureAnionCount3D,FeatureCationCount3D,FeatureRingCount3D,FeatureHydrophobeCount3D,ConformerModelRMSD3D,EffectiveRotorCount3D,ConformerCount3D,Fingerprint2D/JSON", Method.POST);
            var response = client.Execute(request);
            var content = response.Content; // raw content as 
            richTextBox1.Text = content;


            Root props = JsonConvert.DeserializeObject<Root>(content);

            //https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/cid/1,2,3,4,5/property/MolecularFormula,MolecularWeight,CanonicalSMILES,IsomericSMILES,InChI,InChIKey,IUPACName,XLogP,ExactMass,MonoisotopicMass,TPSA,Complexity,Charge,HBondDonorCount,HBondAcceptorCount,RotatableBondCount,HeavyAtomCount,IsotopeAtomCount,AtomStereoCount,DefinedAtomStereoCount,UndefinedAtomStereoCount,BondStereoCount,DefinedBondStereoCount,UndefinedBondStereoCount,CovalentUnitCount,Volume3D,XStericQuadrupole3D,YStericQuadrupole3D,ZStericQuadrupole3D,FeatureCount3D,FeatureAcceptorCount3D,FeatureDonorCount3D,FeatureAnionCount3D,FeatureCationCount3D,FeatureRingCount3D,FeatureHydrophobeCount3D,ConformerModelRMSD3D,EffectiveRotorCount3D,ConformerCount3D,Fingerprint2D/json";


            return props.PropertyTable;

        }


       
        public void Connect(string name)
        {

            object obs = new object();

            string path = "";

            //https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/glucose/cids/TXT
            //https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/glucose/XML
            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);
            
            //var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/glucose/cids/TXT", Method.POST);
            //var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/assay/aid/504526/description/XML", Method.POST);
            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/PNG?image_size=500x500", Method.POST);
            //request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
            //request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

            // add parameters for all properties on an object
            //request.AddObject(obs);

            // or just whitelisted properties
            // request.AddObject(obs, "PersonId", "Name", ...);

            // easily add HTTP Headers
            //request.AddHeader("header", "value");

            // add files to upload (works with compatible verbs)
            //request.AddFile("file", path);

            // execute the request
            IRestResponse response = client.Execute(request);



            byte[] bytes = response.RawBytes;

            using (var ms = new MemoryStream(bytes))
            {
                var b = Image.FromStream(ms);
                pictureBox1.Image = b;
            }
            //?record_type = 3d
            request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/XML?record_type = 3d", Method.POST);
            response = client.Execute(request);
            var content = response.Content; // raw content as 
            richTextBox1.Text = content;

            // or automatically deserialize result
            // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
            //IRestResponse<Person> response2 = client.Execute<Person>(request);
            //var name = response2.Data.Name;

            // or download and save file to disk
            //client.DownloadData(request).SaveAs(path);

            // easy async support
            //await client.ExecuteAsync(request);

            // async with deserialization
            //var asyncHandle = client.ExecuteAsync<Person>(request, response => {
            //    Console.WriteLine(response.Data.Name);
            //});

            // abort the request on demand
            //asyncHandle.Abort();

            List<List<double>> ls = new List<List<double>>();

            var c = (PCCompounds)Serializer.Deserialize<PCCompounds>(content);
            List<double> ds = new List<double>();
            foreach (var cc in c.PCCompound)
            {
                foreach(var d in cc.PCCompound_coords.PCCoordinates)
                {
                    
                    foreach(var b in d.PCCoordinates_conformers.PCConformer)
                    {

                        var x = b.PCConformer_x.PCConformer_x_E;
                        var y = b.PCConformer_y.PCConformer_y_E;

                        for(int i = 0; i < x.Length; i++)
                        {
                            List<double> dd = new List<double>();
                            dd.Add(x[i]);
                            dd.Add(y[i]);

                            ls.Add(dd);
                        }

                    }
                    
                }
            }

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Files\\" + "histidine.xml", content);

            
        }

        static public (string, PCCompounds) ConnectEx(string name)
        {

            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");

            //?record_type = 3d
            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/XML?record_type=3d", Method.POST);
            var response = client.Execute(request);
            var content = response.Content; // raw content as 
            //richTextBox1.Text = content;

            // or automatically deserialize result
            // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
            //IRestResponse<Person> response2 = client.Execute<Person>(request);
            //var name = response2.Data.Name;

            // or download and save file to disk
            //client.DownloadData(request).SaveAs(path);

            // easy async support
            //await client.ExecuteAsync(request);

            // async with deserialization
            //var asyncHandle = client.ExecuteAsync<Person>(request, response => {
            //    Console.WriteLine(response.Data.Name);
            //});

            // abort the request on demand
            //asyncHandle.Abort();

            List<List<double>> ls = new List<List<double>>();

            var c = (PCCompounds)Serializer.Deserialize<PCCompounds>(content);

         
            if (c == null)
                return (content, c);

            List<double> ds = new List<double>();
            foreach (var cc in c.PCCompound)
            {
                foreach (var d in cc.PCCompound_coords.PCCoordinates)
                {

                    foreach (var b in d.PCCoordinates_conformers.PCConformer)
                    {

                        var x = b.PCConformer_x.PCConformer_x_E;
                        var y = b.PCConformer_y.PCConformer_y_E;

                        for (int i = 0; i < x.Length; i++)
                        {
                            List<double> dd = new List<double>();
                            dd.Add(x[i]);
                            dd.Add(y[i]);

                            ls.Add(dd);
                        }

                    }

                }
            }

            return (content, c);


        }


        public static List<List<double>> GetData()
        {
            Compound cs = new Compound();

            string content = AppDomain.CurrentDomain.BaseDirectory + "Files\\" + "histidine.xml";

            content = File.ReadAllText(content);

            List<List<double>> ls = new List<List<double>>();

            var c = (PCCompounds)Serializer.Deserialize<PCCompounds>(content);
            List<double> ds = new List<double>();
            foreach (var cc in c.PCCompound)
            {
                foreach (var d in cc.PCCompound_coords.PCCoordinates)
                {

                    foreach (var b in d.PCCoordinates_conformers.PCConformer)
                    {
                        
                        var x = b.PCConformer_x.PCConformer_x_E;
                        var y = b.PCConformer_y.PCConformer_y_E;
                        //var z = b.PCConformer_z.PCConformer_z_E;
                        for (int i = 0; i < x.Length; i++)
                        {
                            List<double> dd = new List<double>();
                            dd.Add(x[i]);
                            dd.Add(y[i]);
                          //  dd.Add(z[i]);

                            ls.Add(dd);
                            //Atom a = new Atom(x[i], y[i], z[i]);
                            Atom a = new Atom(i, x[i], y[i], 0, cs);
                            cs.atoms.Add(a);
                        }
                        
                    }
                   
                }
                var bs = cc.PCCompound_bonds.PCBonds;
                {

                    for(int i = 0; i < bs.PCBonds_aid1.PCBonds_aid1_E.Length; i++)
                    {

                        var x = bs.PCBonds_aid1.PCBonds_aid1_E[i];
                        var y = bs.PCBonds_aid2.PCBonds_aid2_E[i];
                        Bond bond = new Bond(cs);
                        bond.first = Convert.ToInt32(x);
                        bond.second = Convert.ToInt32(y);
                        
                        cs.bonds.Add(bond);

                    }

                }
            }
            return ls;
        }

        public static Compound GetData(string name)
        {
            Compound cs = new Compound();

            string content = name;

            content = File.ReadAllText(content);

            List<List<double>> ls = new List<List<double>>();

            var c = (PCCompounds)Serializer.Deserialize<PCCompounds>(content);
            if (c == null)
                return new Compound();
            List<double> ds = new List<double>();
            foreach (var cc in c.PCCompound)
            {
                foreach (var d in cc.PCCompound_coords.PCCoordinates)
                {
                    int k = 0;
                    var aes = cc.PCCompound_atoms.PCAtoms.PCAtoms_element.PCElement;
                    

                    foreach (var b in d.PCCoordinates_conformers.PCConformer)
                    {
                      
                        var x = b.PCConformer_x.PCConformer_x_E;
                        var y = b.PCConformer_y.PCConformer_y_E;
                        var z = b.PCConformer_z.PCConformer_z_E;
                        for (int i = 0; i < x.Length; i++)
                        {
                            string av = aes[k].value.ToString();
                            List<double> dd = new List<double>();
                            dd.Add(x[i]);
                            dd.Add(y[i]);
                            dd.Add(z[i]);

                            ls.Add(dd);
                            Atom a = new Atom(i, x[i], y[i], z[i]);
                            a.descriptor = new AD(av);
                            cs.atoms.Add(a);
                            k++;
                        }
                        
                    }

                }
                var bs = cc.PCCompound_bonds.PCBonds;
                {

                    for (int i = 0; i < bs.PCBonds_aid1.PCBonds_aid1_E.Length; i++)
                    {

                        var x = bs.PCBonds_aid1.PCBonds_aid1_E[i];
                        var y = bs.PCBonds_aid2.PCBonds_aid2_E[i];
                        Bond bond = new Bond(cs);
                        bond.first = Convert.ToInt32(x);
                        bond.second = Convert.ToInt32(y);
                
                        
                        cs.bonds.Add(bond);

                    }
                    for (int i = 0; i < bs.PCBonds_aid2.PCBonds_aid2_E.Length; i++)
                    {

                        var x = bs.PCBonds_aid2.PCBonds_aid2_E[i];
                        var y = bs.PCBonds_aid1.PCBonds_aid1_E[i];
                        Bond bond = new Bond(cs);
                        bond.first = Convert.ToInt32(x);
                        bond.second = Convert.ToInt32(y);
                     

                        cs.bonds.Add(bond);

                    }
                }
            }
            return cs;
        }

        public static Compound GetDataEx(string name)
        {
            Compound cs = new Compound();

            string content = name;

            content = File.ReadAllText(content);

            List<List<double>> ls = new List<List<double>>();

            cs = (Compound)Serializer.Deserialize<Compound>(content);
            if (cs == null)
                return new Compound();
            List<double> ds = new List<double>();

            foreach (Atom a in cs.atoms)
                a.c = cs;
            foreach (Bond b in cs.bonds)
                b.c = cs;



            return cs;
        }


        public static (string, Compound) GetRawData(string name)
        {


            var(content, c) = ConnectEx(name);

         
            Compound cs = new Compound();
         

            List<List<double>> ls = new List<List<double>>();

          
            if (c == null)
                return (content, new Compound());
            List<double> ds = new List<double>();
            foreach (var cc in c.PCCompound)
            {
                foreach (var d in cc.PCCompound_coords.PCCoordinates)
                {
                    int k = 0;
                    var aes = cc.PCCompound_atoms.PCAtoms.PCAtoms_element.PCElement;


                    foreach (var b in d.PCCoordinates_conformers.PCConformer)
                    {

                        var x = b.PCConformer_x.PCConformer_x_E;
                        var y = b.PCConformer_y.PCConformer_y_E;
                        var z = b?.PCConformer_z?.PCConformer_z_E;
                        for (int i = 0; i < x.Length; i++)
                        {
                            string av = aes[k].value.ToString();
                            List<double> dd = new List<double>();
                            dd.Add(x[i]);
                            dd.Add(y[i]);
                            //dd.Add(z[i]);

                            ls.Add(dd);
                            Atom a = null;
                            if (z == null)
                            {
                                a = new Atom(i, x[i], y[i], 0);
                            }
                            else
                            {
                                dd.Add(z[i]);

                                a = new Atom(i, x[i], y[i], z[i]);
                            }
                            a.descriptor = new AD(av);
                            cs.atoms.Add(a);
                            k++;
                        }

                    }

                }
                var bs = cc.PCCompound_bonds.PCBonds;
                {

                    for (int i = 0; i < bs.PCBonds_aid1.PCBonds_aid1_E.Length; i++)
                    {

                        var x = bs.PCBonds_aid1.PCBonds_aid1_E[i];
                        var y = bs.PCBonds_aid2.PCBonds_aid2_E[i];
                        Bond bond = new Bond(cs);
                        bond.first = Convert.ToInt32(x);
                        bond.second = Convert.ToInt32(y);
       

                        cs.bonds.Add(bond);

                    }
                    for (int i = 0; i < bs.PCBonds_aid2.PCBonds_aid2_E.Length; i++)
                    {

                        var x = bs.PCBonds_aid2.PCBonds_aid2_E[i];
                        var y = bs.PCBonds_aid1.PCBonds_aid1_E[i];
                        Bond bond = new Bond(cs);
                        bond.first = Convert.ToInt32(x);
                        bond.second = Convert.ToInt32(y);
                   

                        cs.bonds.Add(bond);

                    }
                }
            }
            return (content, cs);
        }


        public void ConnectRCDB(string name, string server)
        {

            object obs = new object();
            var client = new RestClient(server/*"http://www.rcsb.org"*/);
           //  / pdb / rest / describeHet ? chemicalID = NAG
            //var request = new RestRequest("http://www.rcsb.org/pdb/rest/describeHet?chemicalID=NAG", Method.POST);
            //var request = new RestRequest("/pdb/rest/describeMol?structureId=7ahi", Method.GET);
            var request = new RestRequest("https://files.rcsb.org/download/4hhb.xml", Method.GET);
            var response = client.Execute(request);
            var content = response.Content; // raw content as 
            richTextBox1.Text = content;
            
            // abort the request on demand
            //asyncHandle.Abort();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Connect(comboBox1.Text);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            ConnectRCDB("http://www.rcsb.org/pdb/rest/describeHet?chemicalID=NAG", "http://www.rcsb.org");
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Autocomplete("asp");
        }

        private void tsSynonims_Click(object sender, EventArgs e)
        {
            Synonims("aspirin");
        }

        public void Synonims(string name)
        {
            //https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/aspirin/synonyms/XML
            //https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/smiles/CCCC/synonyms/XML
            //https://pubchem.ncbi.nlm.nih.gov/rest/pug/substance/sid/53789435/synonyms/TXT

            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");

            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/synonyms/json", Method.POST);
            var response = client.Execute(request);
            var content = response.Content; // raw content as 
            richTextBox1.Text = content;

            //https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/cid/1,2,3,4,5/property/MolecularFormula,MolecularWeight,InChIKey/CSV
        }

        private void tsProperties_Click(object sender, EventArgs e)
        {
            Description("aspirin");
        }

        private void tsAtoms_Click(object sender, EventArgs e)
        {
            LoadAtoms();
        }
    }
    public class Serializer
    {
        static public T Deserialize<T>(string input) where T : class
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (StringReader sr = new StringReader(input))
            {
                try
                {
                    return (T)ser.Deserialize(sr);
                }
                catch(Exception e)
                {
                    return null;
                }
            }
        }

        static public string Serialize<T>(T ObjectToSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                return textWriter.ToString();
            }
        }
    }

    public class Compound
    {
        public List<Atom> atoms { get; set; }

        public List<Bond> bonds { get; set; }

        public Compound()
        {
            atoms = new List<Atom>();
            bonds = new List<Bond>();
        }
    }
    //public class Atom
    //{
    //    public Point3D center { get; set; }
    //    public AD descriptor { get; set;}
    //    //public Compound c { get; set; }
    //    public int aid { get; set; }

    //    public Atom()
    //    {

    //    }

    //    public Atom(int aid, double x, double y, double z, Compound c = null)
    //    {
    //        center = new Point3D(x, y, z);
    //        //this.c = c; 
    //    }
    //    public Material Material()
    //    {

    //        if (descriptor.ShortName == "c")
    //        {
    //            return Materials.Gray;
    //        }
    //        else if (descriptor.ShortName == "o")
    //        {
    //            return Materials.Indigo;
    //        }
    //        else if (descriptor.ShortName == "h")
    //        {
    //            return Materials.White;
    //        }
    //        else if (descriptor.ShortName == "n")
    //        {
    //            return Materials.Orange;
    //        }
    //        else return Materials.Red;


    //    }

    //    //public List<Bond> Bonds()
    //    //{
    //    //    List<Bond> b = new List<Bond>();

    //    //    foreach(Bond bond in c.bonds)
    //    //    {
    //    //        if (bond.first == aid)
    //    //            b.Add(bond);
    //    //        if (bond.second == aid)
    //    //            b.Add(bond);
    //    //    }

    //    //    return b;
    //    //}
    //}
    //public class Bond
    //{
    //    public int first { get; set; }
    //    public int second { get; set; }
    //    public int valency { get; set; }
    //    //public Compound c { get; set; }

    //    public List<Atom> atoms { get; set; }

    //    public Bond()
    //    {

    //    }


    //    public Bond(Compound c)
    //    {
    //      //  this.c = c;
    //    }
    //    public Point3D First
    //    {
    //        get
    //        {
    //            return atoms[first -1].center;
    //        }
    //        set
    //        {

    //        }
    //    }
    //    public Point3D Second
    //    {
    //        get
    //        {
    //            return atoms[second -1].center;
    //        }
    //    }
    //    public Atom FirstAtom
    //    {
    //        get
    //        {
    //            return atoms[first - 1];
    //        }
    //    }
    //    public Atom SecondAtom
    //    {
    //        get
    //        {
    //            return atoms[second - 1];
    //        }
    //    }
    //}

    
    public class Atom
    {
        public Point3D center { get; set; }
        public AD descriptor { get; set; }
    
        [XmlIgnore]
        public Compound c { get; set; }
        public int aid { get; set; }

        public Atom()
        {

        }

        public Atom(int aid, double x, double y, double z, Compound c = null)
        {
            center = new Point3D(x, y, z);
            this.c = c;
        }
        public Material Material()
        {

            if (descriptor.ShortName == "c")
            {
                return Materials.Gray;
            }
            else if (descriptor.ShortName == "o")
            {
                return Materials.Indigo;
            }
            else if (descriptor.ShortName == "h")
            {
                return Materials.White;
            }
            else if (descriptor.ShortName == "n")
            {
                return Materials.Orange;
            }
            else return Materials.Red;


        }

        public List<Bond> Bonds()
        {
            List<Bond> b = new List<Bond>();

            foreach (Bond bond in c.bonds)
            {
                if (bond.first == aid)
                    b.Add(bond);
                if (bond.second == aid)
                    b.Add(bond);
            }

            return b;
        }
    }
    public class Bond
    {
        public int first { get; set; }
        public int second { get; set; }
        public int valency { get; set; }
        [XmlIgnore]
        public Compound c { get; set; }

        public Bond()
        {

        }


        public Bond(Compound c)
        {
            this.c = c;
        }
        public Point3D First
        {
            get
            {
                return c.atoms[first - 1].center;
            }
        }
        public Point3D Second
        {
            get
            {
                return c.atoms[second - 1].center;
            }
        }
        public Atom FirstAtom
        {
            get
            {
                return c.atoms[first - 1];
            }
        }
        public Atom SecondAtom
        {
            get
            {
                return c.atoms[second - 1];
            }
        }

        public string ToStringEx()
        {

            int f = first;
            int s = second;

            string d = f > second ? s.ToString() + "-" + first.ToString() : f.ToString() + "-" + s.ToString();

            return d;


        }
    }



    public class AD
    {
        public string Name { get; set; }
        public string ShortName { get; set; }

        public AD()
        {

        }

        public AD(string shortname)
        {
            ShortName = shortname;
        }
       
    }

    public class Status
    {
        public int code { get; set; }
    }

    public class DictionaryTerms
    {
        public IList<string> compound { get; set; }
    }

    public class AutoComplete
    {
        public Status status { get; set; }
        public int total { get; set; }
        public DictionaryTerms dictionary_terms { get; set; }
    }

    public class PropertiesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int CID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MolecularFormula { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double MolecularWeight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CanonicalSMILES { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IsomericSMILES { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InChI { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string InChIKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IUPACName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double XLogP { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double ExactMass { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double MonoisotopicMass { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double TPSA { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Complexity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Charge { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int HBondDonorCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int HBondAcceptorCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int RotatableBondCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int HeavyAtomCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int IsotopeAtomCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int AtomStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DefinedAtomStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UndefinedAtomStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int BondStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int DefinedBondStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UndefinedBondStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int CovalentUnitCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Volume3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double XStericQuadrupole3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double YStericQuadrupole3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double ZStericQuadrupole3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FeatureCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FeatureAcceptorCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FeatureDonorCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FeatureAnionCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FeatureCationCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FeatureRingCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int FeatureHydrophobeCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double ConformerModelRMSD3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int EffectiveRotorCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ConformerCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Fingerprint2D { get; set; }
    }

    public class PropertyTable
    {
        /// <summary>
        /// 
        /// </summary>
        public List<PropertiesItem> Properties { get; set; }
    }

    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public PropertyTable PropertyTable { get; set; }
    }

    public class AtomEx
    {
        public double Atomic { get; set; }
        public string Element { get; set; }
        public double? Atomic__1 { get; set; }
        public double? Ionic { get; set; }
        public double? Covalent { get; set; }
        [JsonProperty("Van-der-Waals")]
        public object VanDerWaals { get; set; }
        public object Crystal { get; set; }
    }

    public class AtomsTable
    {
        public int id { get; set; }
        public List<AtomEx> atoms { get; set; }
    }

    public class RootEx
    {
        public AtomsTable AtomsTable { get; set; }
    }

}
