using HelixToolkit.Wpf;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;
using Viewer3D;

namespace ModelExplorer
{
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
                catch (Exception ex)
                {
                    if (MainWindow.DebugMode)
                        MessageBox.Show(ex.Message);

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
    public static class MaterialEx
    {


        public static Dictionary<System.Windows.Media.Color, Material> dict = new Dictionary<System.Windows.Media.Color, Material>();


        static public Material Get(System.Windows.Media.Color c)
        {
            if (dict.ContainsKey(c))
                return dict[c];
            dict.Add(c, MaterialHelper.CreateMaterial(c));
            return dict[c];
        }



    }


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
                return Materials.Red;
            }
            else if (descriptor.ShortName == "h")
            {
                return Materials.White;
            }
            else if (descriptor.ShortName == "n")
            {
                return Materials.Blue;
            }
            else if (descriptor.ShortName == "p")
            {
                return Materials.Orange;
            }
            else if (descriptor.ShortName == "br")
            {
                return Materials.Brown;
            }
            else if (descriptor.ShortName == "cl")
            {
                return MaterialEx.Get(System.Windows.Media.Colors.LightGreen);
            }
            else if (descriptor.ShortName == "na")
            {
                return MaterialEx.Get(System.Windows.Media.Colors.DarkBlue);
            }
            else if (descriptor.ShortName == "s")
            {
                return Materials.Gold;
            }
            else return Materials.Green;


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
        public double Complexity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Charge { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double HBondDonorCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double HBondAcceptorCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double RotatableBondCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double HeavyAtomCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double IsotopeAtomCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double AtomStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double DefinedAtomStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UndefinedAtomStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double BondStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double DefinedBondStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int UndefinedBondStereoCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double CovalentUnitCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double Volume3D { get; set; }
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
        public double FeatureCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double FeatureAcceptorCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double FeatureDonorCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double FeatureAnionCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double FeatureCationCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double FeatureRingCount3D { get; set; }
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
        public double EffectiveRotorCount3D { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double ConformerCount3D { get; set; }
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

    //https://www.kegg.jp/kegg/rest/keggapi.html
    //https://ftp.ncbi.nlm.nih.gov/blast/
    //https://chembl.gitbook.io/chembl-interface-documentation/web-services/chembl-data-web-services
    //https://www.lipidmaps.org/resources/REST/index.php
    //https://www.ebi.ac.uk/pdbe/api/doc/search.html
    public class DataEx
    {

        static public Arxiv Arxiv(string name)
        {

            var client = new RestClient("http://export.arxiv.org/api");
            var request = new RestRequest("http://export.arxiv.org/api/query?search_query=all:" + name + "&profile=fuzzy", Method.GET);
            var response = client.Execute(request);
            var content = response.Content;

            Arxiv pages = JsonConvert.DeserializeObject<Arxiv>(content);


            return pages;


            //   http://export.arxiv.org/api/query?search_query=all:electron&start=0&max_results=1
        }

        static public AtomsTable LoadAtoms()
        {

            string content = File.ReadAllText("atoms3.json");

            RootEx r = JsonConvert.DeserializeObject<RootEx>(content);

            return r.AtomsTable;
        }

        static public List<string> Words(string name)
        {

            List<string> words = new List<string>();
            string[] separators = new string[] { ",", ".", "!", "\'", " ", "\'s" };


            foreach (string word in name.Split(separators, StringSplitOptions.RemoveEmptyEntries))
                words.Add(word);

            return words;
        }
        static public BrowserPages GetBrowserPagesForName(string name)
        {
            List<string> words = Words(name);


            name = words.Aggregate("", (max, cur) => max.Length > cur.Length ? max : cur);

            var client = new RestClient("https://en.wikipedia.org/w/rest.php");
            var request = new RestRequest("https://en.wikipedia.org/w/rest.php/v1/search/page?q=" + name + "&profile=fuzzy", Method.GET);
            var response = client.Execute(request);
            var content = response.Content;

            BrowserPages pages = JsonConvert.DeserializeObject<BrowserPages>(content);

            if (pages == null)
                pages = new BrowserPages();


            return pages;

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
            var content = response.Content;

            AutoComplete autos = JsonConvert.DeserializeObject<AutoComplete>(content);

            if (autos == null)
                autos = new AutoComplete();


            return autos;

        }

        static public PropertyTable Description(string name)
        {

            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");

            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/property/MolecularFormula,MolecularWeight,CanonicalSMILES,IsomericSMILES,InChI,InChIKey,IUPACName,XLogP,ExactMass,MonoisotopicMass,TPSA,Complexity,Charge,HBondDonorCount,HBondAcceptorCount,RotatableBondCount,HeavyAtomCount,IsotopeAtomCount,AtomStereoCount,DefinedAtomStereoCount,UndefinedAtomStereoCount,BondStereoCount,DefinedBondStereoCount,UndefinedBondStereoCount,CovalentUnitCount,Volume3D,XStericQuadrupole3D,YStericQuadrupole3D,ZStericQuadrupole3D,FeatureCount3D,FeatureAcceptorCount3D,FeatureDonorCount3D,FeatureAnionCount3D,FeatureCationCount3D,FeatureRingCount3D,FeatureHydrophobeCount3D,ConformerModelRMSD3D,EffectiveRotorCount3D,ConformerCount3D,Fingerprint2D/JSON", Method.POST);
            var response = client.Execute(request);
            var content = response.Content;

            Root props = JsonConvert.DeserializeObject<Root>(content);

            if (props == null)
                props = new Root();

            //https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/cid/1,2,3,4,5/property/MolecularFormula,MolecularWeight,CanonicalSMILES,IsomericSMILES,InChI,InChIKey,IUPACName,XLogP,ExactMass,MonoisotopicMass,TPSA,Complexity,Charge,HBondDonorCount,HBondAcceptorCount,RotatableBondCount,HeavyAtomCount,IsotopeAtomCount,AtomStereoCount,DefinedAtomStereoCount,UndefinedAtomStereoCount,BondStereoCount,DefinedBondStereoCount,UndefinedBondStereoCount,CovalentUnitCount,Volume3D,XStericQuadrupole3D,YStericQuadrupole3D,ZStericQuadrupole3D,FeatureCount3D,FeatureAcceptorCount3D,FeatureDonorCount3D,FeatureAnionCount3D,FeatureCationCount3D,FeatureRingCount3D,FeatureHydrophobeCount3D,ConformerModelRMSD3D,EffectiveRotorCount3D,ConformerCount3D,Fingerprint2D/json";


            return props.PropertyTable;

        }


        static public BitmapImage GetBitmapImage(byte[] imageBytes)
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = new MemoryStream(imageBytes);
            bitmapImage.EndInit();
            return bitmapImage;
        }

        public static byte[] ImageToBytes(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        static public BitmapImage LoadFormula(string name)
        {

            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");

            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/PNG?image_size=500x500", Method.POST);

            var response = client.Execute(request);

            byte[] bytes = response.RawBytes;

            //var stream = new MemoryStream(bytes);
            //Bitmap bb = new Bitmap(stream);
            //bb.MakeTransparent(bb.GetPixel(1, 1));
            //byte[] bbytes = ImageToBytes(bb);

            BitmapImage b1 = null;

            try
            {

                b1 = GetBitmapImage(bytes);


            }
            catch (Exception) { };

            return b1;

        }

        public void Connect(string name)
        {

            object obs = new object();

            //string path = "";

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
                //   var b = Image.FromStream(ms);
                //   pictureBox1.Image = b;
            }
            //?record_type = 3d
            request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/XML?record_type = 3d", Method.POST);
            response = client.Execute(request);
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

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "Files\\" + "histidine.xml", content);


        }

        static public (string, PCCompounds) ConnectEx(string name)
        {

            var client = new RestClient("https://pubchem.ncbi.nlm.nih.gov");

            var request = new RestRequest("https://pubchem.ncbi.nlm.nih.gov/rest/pug/compound/name/" + name + "/XML?record_type=3d", Method.POST);
            var response = client.Execute(request);
            var content = response.Content;

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

                    for (int i = 0; i < bs.PCBonds_aid1.PCBonds_aid1_E.Length; i++)
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


            var (content, c) = ConnectEx(name);


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
                break;
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
            //richTextBox1.Text = content;

            // abort the request on demand
            //asyncHandle.Abort();
        }


        static Font GetFont(int size)
        {
            Font f = new Font("Tahoma", 17);
            return new Font(f, System.Drawing.FontStyle.Bold);

        }

        /// <summary>
        ///  modified from internet
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static public BitmapImage TextToBitmap(string text)
        {
            // Load the original image
            Bitmap bmp = new Bitmap(300, 60);

            // Create a rectangle for the entire bitmap
            RectangleF rectf = new RectangleF(0, 0, bmp.Width, bmp.Height);

            // Create graphic object that will draw onto the bitmap
            Graphics g = Graphics.FromImage(bmp);

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // The interpolation mode determines how intermediate values between two endpoints are calculated.
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Use this property to specify either higher quality, slower rendering, or lower quality, faster rendering of the contents of this Graphics object.
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // This one is important
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            // Create string formatting options (used for alignment)
            StringFormat format = new StringFormat()
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            Font f = GetFont(18);

            // Draw the text onto the image
            g.DrawString(text, /*new Font("Tahoma", 16)*/ f, System.Drawing.Brushes.Maroon, rectf, format);

            // Flush all graphics changes to the bitmap
            g.Flush();

            f.Dispose();

            Image image = (Image)bmp;

            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                byte[] bytes = ms.ToArray();

                return GetBitmapImage(bytes);

            }

        }

    }

    static public class Exporters
    {

        /// <summary>
        /// Exports to a STL file.
        /// </summary>
        /// <param name="view">The viewport.</param>
        /// <param name="fileName">Name of the file.</param>
        public static void ExportStl(System.Windows.Controls.Viewport3D view, string fileName)
        {
            var e = new StlExporter();
            using (var stream = File.Create(fileName))
            {
                e.Export(view, stream);
            }
        }

        /// <summary>
        /// Exports to an X3D file.
        /// </summary>
        /// <param name="view">
        /// The viewport.
        /// </param>
        /// <param name="fileName">
        /// Name of the file.
        /// </param>
        public static void ExportX3D(System.Windows.Controls.Viewport3D view, string fileName)
        {
            var e = new X3DExporter();
            using (var stream = File.Create(fileName))
            {
                e.Export(view, stream);
            }
        }
    }

    public class PropEx
    {
        public string Name { get; set; }

        public string Properties { get; set; }

        public string Ex { get; set; }

    }

    public class Thumbnail
    {
        public string mimetype { get; set; }
        public int? size { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public List<string> duration { get; set; }
        public string url { get; set; }

        public Thumbnail()
        {
            duration = new List<string>();
        }

    }
    public class Pages
    {
        public int id { get; set; }
        public string key { get; set; }
        public string title { get; set; }
        public string excerpt { get; set; }
        public string description { get; set; }

        public Thumbnail thumb = null;
        public Thumbnail thumbnail
        {
            get
            {

                if (thumb == null)
                {
                    thumb = new Thumbnail();
                }

                return thumb;
            }

            set
            {


                thumb = value;

            }
        }

    }
    public class BrowserPages
    {
        public List<Pages> pages { get; set; }


        public BrowserPages()
        {
            pages = new List<Pages>();
        }

    }

    public partial class Arxiv
    {
        [JsonProperty("link")]
        public Link Link { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("id")]
        public Uri Id { get; set; }

        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; set; }

        [JsonProperty("totalResults")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long TotalResults { get; set; }

        [JsonProperty("startIndex")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long StartIndex { get; set; }

        [JsonProperty("itemsPerPage")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long ItemsPerPage { get; set; }

        [JsonProperty("entry")]
        public List<Entry> Entry { get; set; }
    }

    public partial class Entry
    {
        [JsonProperty("id")]
        public Uri Id { get; set; }

        [JsonProperty("published")]
        public DateTimeOffset Published { get; set; }

        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("journal_ref")]
        public string JournalRef { get; set; }

        [JsonProperty("link")]
        public Link[] Link { get; set; }

        [JsonProperty("primary_category")]
        public Category PrimaryCategory { get; set; }

        [JsonProperty("category")]
        public Category Category { get; set; }
    }

    public partial class Author
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Category
    {
        [JsonProperty("@term")]
        public string Term { get; set; }

        [JsonProperty("@scheme")]
        public Uri Scheme { get; set; }
    }

    public partial class Link
    {
        [JsonProperty("@href")]
        public Uri Href { get; set; }

        [JsonProperty("@rel")]
        public string Rel { get; set; }

        [JsonProperty("@type")]
        public string Type { get; set; }

        [JsonProperty("@title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
    }

}
