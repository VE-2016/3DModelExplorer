namespace Viewer3D
{
    using HelixToolkit.Wpf;
    using ModelExplorer;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using Exporters = ModelExplorer.Exporters;

    public partial class MainWindow
    {


        string fileFolder = AppDomain.CurrentDomain.BaseDirectory + "Files\\" + "Content";

        public string CurrentModelXml { get; set; }

        static public AtomsTable atomsTable { get; set; }

        static public bool DebugMode = true;

        DataContext dataContext = new DataContext();


        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {


            this.InitializeComponent();

            this.DataContext = dataContext;


            tbModels.IsSelected = true;

            var f = AppDomain.CurrentDomain.BaseDirectory;

            List<XML_Item> items = new List<XML_Item>();
            lb.ItemsSource = items;

            var files = new DirectoryInfo(fileFolder).GetFiles();
            foreach (var df in files)
            {
                XML_Item dg = new XML_Item();
                dg.Name = df.Name;
                items.Add(dg);
            }
            lb.SelectionChanged += Lb_SelectionChanged;


            this.PreviewKeyDown += MainWindow_PreviewKeyDown;


            lb2.SelectionChanged += Lb2_SelectionChanged;

            LoadModelsFolder();

            atomsTable = DataEx.LoadAtoms();

            LoadTreeView();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            RequestHandler requestHandler = new RequestHandler(dataContext);
            Browser.RequestHandler = requestHandler;



        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (dataContext.queue.Count > 0)
            {
                object obs = dataContext.queue.Dequeue();

                if (obs.GetType() == typeof(string))

                {

                    string s = (string)obs;

                    Task.Factory.StartNew(() =>
                    {

                        LoadProperties(s);

                    });

                }
                else if (obs.GetType() == typeof(PropertyTable))
                {

                    Task.Factory.StartNew(() =>
                    {
                        lvModels2.Dispatcher.BeginInvoke(new Action(() => Update(obs)));

                    });


                }
            }
            if (dataContext.browserQueue.Count > 0)
            {

                string name = (string)dataContext.browserQueue.Dequeue();


                Task.Factory.StartNew(() =>
                {
                    lbBrowse.Dispatcher.BeginInvoke(new Action(() => SearchBrowser(name)));

                });

            }

            if (dataContext.loaderQueue.Count > 0)
            {

                string name = (string)dataContext.loaderQueue.Dequeue();

                if (string.IsNullOrEmpty(name))
                    return;

                Task.Factory.StartNew(() =>
                {
                    view.Dispatcher.BeginInvoke(new Action(() => LoadFromXML(name)));

                });

            }

            if (dataContext.importQueue.Count > 0)
            {

                string name = (string)dataContext.importQueue.Dequeue();

                if (string.IsNullOrEmpty(name))
                    return;

                Task.Factory.StartNew(() =>
                {
                    view.Dispatcher.BeginInvoke(new Action(() => LoadImporters(name)));

                });

            }



        }

        private void Update(object obs)
        {
            var entries = dataContext.PropEntries;

            entries.Clear();

            PropertyTable c = (PropertyTable)obs;

            foreach (PropertiesItem pp in c.Properties)
            {
                object obj = pp;
                PropertyInfo[] properties = obj.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var v = p.GetValue(obj);

                    PropEx s = new PropEx();

                    s.Name = p.Name;

                    s.Properties = Convert.ToString(v);

                    entries.Add(s);
                }
            }
        }

        void LoadTreeView()
        {



            string[] s = new string[]{"Alanine", "Arginine","Asparagine","Aspartic acid", "Cysteine", "Glutamic acid","Glutamine","Glycine","Histidine","Hydroxyproline","Isoleucine",
"leucine",
"lysine",
"Methionine",
"Phenylalanine",
"Proline",
"Pyroglutamatic",
"Serine",
"Threonine",
"Tryptophan",
"Tyrosine"
};

            DomainItem root = new DomainItem() { Name = "Models" };
            DomainItem childItem1 = new DomainItem() { Name = "Aminoacids" };
            childItem1.Items.Add(new DomainItem() { Name = "Alanine" });
            childItem1.Items.Add(new DomainItem() { Name = "Fenylonine" });
            foreach (string b in s)
                childItem1.Items.Add(new DomainItem() { Name = b });
            root.Items.Add(childItem1);
            root.Items.Add(new DomainItem() { Name = "Sugars" });
            trvMenu.Items.Add(root);


        }




        public class FolderEx
        {
            public string Name { get; set; }

            public string Path { get; set; }

            public FolderEx(string name, string path) { Name = name; Path = path; }

        }

        string ModelsPath()
        {
            var f = AppDomain.CurrentDomain.BaseDirectory;

            return f + "Models";

        }

        public void LoadModelsFolder()
        {

            var f = AppDomain.CurrentDomain.BaseDirectory;

            var files = System.IO.Directory.GetFiles(f + "Models");

            lvModels2.Items.Clear();

            foreach (var file in files)
            {
                string fileName = System.IO.Path.GetFileName(file);

                FolderEx ff = new FolderEx(fileName, file);

                lvModels2.Items.Add(ff);

            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPlus)
            {

                if (selected == null)
                    return;
                selected.ScaleSphere();
                //selected.TransformRefresh();
                view.InvalidateVisual();

            }
        }

        public static Dictionary<UISphere, List<UIPipe>> dict { get; set; }
        public static Dictionary<UISphere, List<UIPipe>> dict2 { get; set; }

        Dictionary<Atom, UISphere> atoms { get; set; }

        private void Lb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            int i = lb.SelectedIndex;
            if (i < 0)
                return;
            XML_Item d = lb.Items[i] as XML_Item;
            if (d == null)
                return;
            string file = fileFolder + "\\" + d.Name;

            dataContext.loaderQueue.Clear();

            dataContext.loaderQueue.Enqueue(file);


            //tbModels.IsSelected = true;




            //dict = new Dictionary<UISphere, List<UIPipe>>();
            //dict2 = new Dictionary<UISphere, List<UIPipe>>();

            //atoms = new Dictionary<Atom, UISphere>();

            //if (ObservableElements != null)
            //    foreach (var ew in ObservableElements)
            //        view.Children.Remove(ew);

            //ObservableElements = new ObservableCollection<UIElement3D>();

            //var c = DataEx.GetData(file);

            //var filename = Path.GetFileNameWithoutExtension(file);

            //if (c == null)
            //    return;

            //compound = c;

            //string s = Serializer.Serialize<Compound>(c);

            //var content = s;

            //File.WriteAllText("Models\\" + filename + ".xml", content);

            //CurrentModelXml = "Models\\" + filename + ".xml";



            //if (c == null)
            //    return;

            //foreach (var es in c.atoms)
            //{

            //    var v = new UISphere(factor, es, es.center, Materials.Red, view);

            //    view.Children.Add(v);

            //    atoms.Add(es, v);

            //    ObservableElements.Add(v);

            //}

            //foreach (var es in c.bonds)
            //{

            //    var v = new UIPipe(factor, es, es.First, es.Second, es.FirstAtom, es.SecondAtom, 0.2, Materials.Green, view);

            //    List<UIPipe> af = null;

            //    UISphere sf = atoms[es.FirstAtom];
            //    if (dict.ContainsKey(sf))
            //        af = dict[sf];
            //    else
            //    {
            //        af = new List<UIPipe>();
            //        dict.Add(sf, af);
            //    }
            //    af.Add(v);

            //    sf = atoms[es.SecondAtom];
            //    if (dict2.ContainsKey(sf))
            //        af = dict2[sf];
            //    else
            //    {
            //        af = new List<UIPipe>();
            //        dict2.Add(sf, af);
            //    }
            //    af.Add(v);

            //    view.Children.Add(v);

            //    ObservableElements.Add(v);

            //}

            //LoadModelsFolder();

            //view.InvalidateVisual();

            //var namenoext = filename;

            //var image = DataEx.LoadFormula(namenoext);

            //imgFormula.Source = image;

            //BitmapSource b = DataEx.TextToBitmap(namenoext);

            //imgHeader.Source = b;
        }

        private void LoadFromXML(string file)
        {

            dict = new Dictionary<UISphere, List<UIPipe>>();
            dict2 = new Dictionary<UISphere, List<UIPipe>>();

            atoms = new Dictionary<Atom, UISphere>();

            if (ObservableElements != null)
                foreach (var ew in ObservableElements)
                    view.Children.Remove(ew);

            ObservableElements = new ObservableCollection<UIElement3D>();

            var c = DataEx.GetData(file);

            var filename = Path.GetFileNameWithoutExtension(file);

            if (c == null)
                return;

            compound = c;

            string s = Serializer.Serialize<Compound>(c);

            var content = s;

            File.WriteAllText("Models\\" + filename + ".xml", content);

            CurrentModelXml = "Models\\" + filename + ".xml";



            if (c == null)
                return;

            foreach (var es in c.atoms)
            {

                var v = new UISphere(factor, es, es.center, Materials.Red, view);

                view.Children.Add(v);

                atoms.Add(es, v);

                ObservableElements.Add(v);

            }

            foreach (var es in c.bonds)
            {

                var v = new UIPipe(factor, es, es.First, es.Second, es.FirstAtom, es.SecondAtom, 0.2, Materials.Green, view);

                List<UIPipe> af = null;

                UISphere sf = atoms[es.FirstAtom];
                if (dict.ContainsKey(sf))
                    af = dict[sf];
                else
                {
                    af = new List<UIPipe>();
                    dict.Add(sf, af);
                }
                af.Add(v);

                sf = atoms[es.SecondAtom];
                if (dict2.ContainsKey(sf))
                    af = dict2[sf];
                else
                {
                    af = new List<UIPipe>();
                    dict2.Add(sf, af);
                }
                af.Add(v);

                view.Children.Add(v);

                ObservableElements.Add(v);

            }

            lvModels2.Dispatcher.Invoke(new Action(() =>
            {
                LoadModelsFolder();

                view.InvalidateVisual();

                var namenoext = filename;

                var image = DataEx.LoadFormula(namenoext);

                imgFormula.Source = image;

                BitmapSource b = DataEx.TextToBitmap(namenoext);

                imgHeader.Source = b;

            }));


        }


        string ModelName()
        {
            if (lvModels2.SelectedItem == null)
                return "";
            FolderEx item = (FolderEx)lvModels2.SelectedItem;

            if (item == null)
                return "";

            return Path.GetFileNameWithoutExtension(item.Name);
        }

        void LoadProperties(string name = "")
        {

            //var entries = dataContext.PropEntries;

            //entries.Clear();

            if (string.IsNullOrEmpty(name))

                name = ModelName();

            if (string.IsNullOrEmpty(name))
                return;

            var c = DataEx.Description(name);

            if (c == null)
                return;

            dataContext.queue.Enqueue(c);

            //foreach (PropertiesItem pp in c.Properties)
            //{
            //    object obj = pp;
            //    PropertyInfo[] properties = obj.GetType().GetProperties();
            //    foreach (var p in properties)
            //    {
            //        var v = p.GetValue(obj);

            //        PropEx s = new PropEx();

            //        s.Name = p.Name;

            //        s.Properties = Convert.ToString(v);

            //        entries.Add(s);
            //    }
            //}

        }

        private void LoadFromXml(string file)
        {

            dict = new Dictionary<UISphere, List<UIPipe>>();
            dict2 = new Dictionary<UISphere, List<UIPipe>>();

            atoms = new Dictionary<Atom, UISphere>();

            if (ObservableElements != null)
                foreach (var ew in ObservableElements)
                    view.Children.Remove(ew);

            ObservableElements = new ObservableCollection<UIElement3D>();

            var c = DataEx.GetDataEx(file);

            if (c == null)
                return;

            compound = c;

            foreach (var es in c.atoms)
            {

                var v = new UISphere(factor, es, es.center, Materials.Red, view);

                view.Children.Add(v);

                atoms.Add(es, v);

                ObservableElements.Add(v);

            }
            af = new List<UIPipe>();

            foreach (var es in c.bonds)
            {

                var v = new UIPipe(factor, es, es.First, es.Second, es.FirstAtom, es.SecondAtom, 0.2, Materials.Green, view);

                UISphere sf = atoms[es.FirstAtom];



                if (dict.ContainsKey(sf))
                {
                    af = dict[sf];
                }
                else
                {
                    af = new List<UIPipe>();
                }

                {
                    view.Children.Add(v);
                    af.Add(v);
                    ObservableElements.Add(v);

                }

                if (!dict.ContainsKey(sf))
                    dict.Add(sf, af);


                sf = atoms[es.SecondAtom];
                if (dict2.ContainsKey(sf))
                    af = dict2[sf];
                else
                {
                    af = new List<UIPipe>();
                    dict2.Add(sf, af);
                }

                af.Add(v);

            }


            view.InvalidateVisual();
        }

        public Compound compound { get; set; }

        List<UIPipe> af = null;

        private void Lb2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            int i = lb2.SelectedIndex;
            if (i < 0)
                return;
            XML_Item d = lb2.Items[i] as XML_Item;
            if (d == null)
                return;
            string name = d.Name;


            tbModels.IsSelected = true;


            dataContext.importQueue.Clear();

            dataContext.importQueue.Enqueue(name);



            //dict = new Dictionary<UISphere, List<UIPipe>>();
            //dict2 = new Dictionary<UISphere, List<UIPipe>>();

            //atoms = new Dictionary<Atom, UISphere>();

            //if (ObservableElements != null)
            //    foreach (var ew in ObservableElements)
            //        view.Children.Remove(ew);

            //ObservableElements = new ObservableCollection<UIElement3D>();

            //int i = lb2.SelectedIndex;
            //if (i < 0)
            //    return;
            //XML_Item d = lb2.Items[i] as XML_Item;
            //if (d == null)
            //    return;
            //string name = d.Name;

            //var (content, c) = DataEx.GetRawData(name);

            //if (c == null)
            //    return;

            //compound = c;

            //string s = Serializer.Serialize<Compound>(c);

            //content = s;

            //File.WriteAllText("Files\\Content\\" + name + ".xml", content);

            //File.WriteAllText("Models\\" + name + ".xml", content);

            //CurrentModelXml = "Models\\" + name + ".xml";

            //foreach (var es in c.atoms)
            //{

            //    var v = new UISphere(factor, es, es.center, Materials.Red, view);

            //    view.Children.Add(v);

            //    atoms.Add(es, v);

            //    ObservableElements.Add(v);

            //}

            //af = new List<UIPipe>();

            //foreach (var es in c.bonds)
            //{

            //    var v = new UIPipe(factor, es, es.First, es.Second, es.FirstAtom, es.SecondAtom, 0.2, Materials.Green, view);



            //    UISphere sf = atoms[es.FirstAtom];
            //    if (dict.ContainsKey(sf))
            //        af = dict[sf];
            //    else
            //    {
            //        af = new List<UIPipe>();
            //        dict.Add(sf, af);
            //    }
            //    af.Add(v);

            //    sf = atoms[es.SecondAtom];
            //    if (dict2.ContainsKey(sf))
            //        af = dict2[sf];
            //    else
            //    {
            //        af = new List<UIPipe>();
            //        dict2.Add(sf, af);
            //    }
            //    af.Add(v);

            //    view.Children.Add(v);

            //    ObservableElements.Add(v);

            //}


            //view.InvalidateVisual();

            //LoadModelsFolder();
        }

        private void LoadImporters(string name)
        {

            // tbModels.IsSelected = true;

            dict = new Dictionary<UISphere, List<UIPipe>>();
            dict2 = new Dictionary<UISphere, List<UIPipe>>();

            atoms = new Dictionary<Atom, UISphere>();

            if (ObservableElements != null)
                foreach (var ew in ObservableElements)
                    view.Children.Remove(ew);

            ObservableElements = new ObservableCollection<UIElement3D>();

            var (content, c) = DataEx.GetRawData(name);

            if (c == null)
                return;

            compound = c;

            string s = Serializer.Serialize<Compound>(c);

            content = s;

            File.WriteAllText("Files\\Content\\" + name + ".xml", content);

            File.WriteAllText("Models\\" + name + ".xml", content);

            CurrentModelXml = "Models\\" + name + ".xml";

            foreach (var es in c.atoms)
            {

                var v = new UISphere(factor, es, es.center, Materials.Red, view);

                view.Children.Add(v);

                atoms.Add(es, v);

                ObservableElements.Add(v);

            }

            af = new List<UIPipe>();

            foreach (var es in c.bonds)
            {

                var v = new UIPipe(factor, es, es.First, es.Second, es.FirstAtom, es.SecondAtom, 0.2, Materials.Green, view);



                UISphere sf = atoms[es.FirstAtom];
                if (dict.ContainsKey(sf))
                    af = dict[sf];
                else
                {
                    af = new List<UIPipe>();
                    dict.Add(sf, af);
                }
                af.Add(v);

                sf = atoms[es.SecondAtom];
                if (dict2.ContainsKey(sf))
                    af = dict2[sf];
                else
                {
                    af = new List<UIPipe>();
                    dict2.Add(sf, af);
                }
                af.Add(v);

                view.Children.Add(v);

                ObservableElements.Add(v);

            }


            view.InvalidateVisual();


            lvModels2.Dispatcher.Invoke(new Action(() =>
            {
                LoadModelsFolder();

                view.InvalidateVisual();

                var namenoext = Path.GetFileNameWithoutExtension(name);

                var image = DataEx.LoadFormula(namenoext);

                imgFormula.Source = image;

                BitmapSource b = DataEx.TextToBitmap(namenoext);

                imgHeader.Source = b;

            }));

            // LoadModelsFolder();
        }

        object obs = new object();

        async private Task LoadDomainItem(string name)
        {

            //lock (obs)
            {

                tbModels.IsSelected = true;

                dict = new Dictionary<UISphere, List<UIPipe>>();
                dict2 = new Dictionary<UISphere, List<UIPipe>>();

                atoms = new Dictionary<Atom, UISphere>();

                if (ObservableElements != null)
                    foreach (var ew in ObservableElements)
                        view.Children.Remove(ew);

                ObservableElements = new ObservableCollection<UIElement3D>();


                var (content, c) = DataEx.GetRawData(name);

                if (c == null)
                    return;

                compound = c;

                string s = Serializer.Serialize<Compound>(c);

                content = s;

                File.WriteAllText("Models\\" + name + ".xml", content);

                CurrentModelXml = "Models\\" + name + ".xml";

                foreach (var es in c.atoms)
                {

                    var v = new UISphere(factor, es, es.center, Materials.Red, view);

                    view.Children.Add(v);

                    atoms.Add(es, v);

                    ObservableElements.Add(v);

                }

                af = new List<UIPipe>();

                foreach (var es in c.bonds)
                {

                    var v = new UIPipe(factor, es, es.First, es.Second, es.FirstAtom, es.SecondAtom, 0.2, Materials.Green, view);



                    UISphere sf = atoms[es.FirstAtom];
                    if (dict.ContainsKey(sf))
                        af = dict[sf];
                    else
                    {
                        af = new List<UIPipe>();
                        dict.Add(sf, af);
                    }
                    af.Add(v);

                    sf = atoms[es.SecondAtom];
                    if (dict2.ContainsKey(sf))
                        af = dict2[sf];
                    else
                    {
                        af = new List<UIPipe>();
                        dict2.Add(sf, af);
                    }
                    af.Add(v);

                    view.Children.Add(v);

                    ObservableElements.Add(v);

                }


                view.InvalidateVisual();

                LoadModelsFolder();

                var image = DataEx.LoadFormula(Path.GetFileNameWithoutExtension(name));


                imgFormula.Source = image;
            }
        }


        private void Load2Models(string filename)
        {

            dict = new Dictionary<UISphere, List<UIPipe>>();
            dict2 = new Dictionary<UISphere, List<UIPipe>>();

            atoms = new Dictionary<Atom, UISphere>();

            if (ObservableElements != null)
                foreach (var ew in ObservableElements)
                    view.Children.Remove(ew);

            ObservableElements = new ObservableCollection<UIElement3D>();

            int i = lb2.SelectedIndex;
            if (i < 0)
                return;
            XML_Item d = lb2.Items[i] as XML_Item;
            if (d == null)
                return;
            string name = d.Name;

            var (content, c) = DataEx.GetRawData(name);

            if (c == null)
                return;

            compound = c;

            string s = Serializer.Serialize<Compound>(c);

            content = s;

            File.WriteAllText("Models\\" + name + ".xml", content);

            CurrentModelXml = "Models\\" + name + ".xml";

            foreach (var es in c.atoms)
            {

                var v = new UISphere(factor, es, es.center, Materials.Red, view);

                view.Children.Add(v);

                atoms.Add(es, v);

                ObservableElements.Add(v);

            }

            af = new List<UIPipe>();

            foreach (var es in c.bonds)
            {

                var v = new UIPipe(factor, es, es.First, es.Second, es.FirstAtom, es.SecondAtom, 0.2, Materials.Green, view);



                UISphere sf = atoms[es.FirstAtom];
                if (dict.ContainsKey(sf))
                    af = dict[sf];
                else
                {
                    af = new List<UIPipe>();
                    dict.Add(sf, af);
                }
                af.Add(v);

                sf = atoms[es.SecondAtom];
                if (dict2.ContainsKey(sf))
                    af = dict2[sf];
                else
                {
                    af = new List<UIPipe>();
                    dict2.Add(sf, af);
                }
                af.Add(v);

                view.Children.Add(v);

                ObservableElements.Add(v);

            }


            view.InvalidateVisual();

            LoadModelsFolder();
        }


        /// <summary>
        /// Gets or sets the observable elements.
        /// </summary>
        public ObservableCollection<UIElement3D> ObservableElements { get; set; }

        private void Button_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            factor *= 1.1;

            foreach (var c in ObservableElements)
            {
                if (c is UISphere)
                {
                    UISphere s = c as UISphere;
                    s.TransformScale(1.1);
                    s.TransformRefresh();
                }
                else if (c is UIPipe)
                {
                    UIPipe s = c as UIPipe;
                    s.TransformScale(1.1);
                    s.TransformRefresh();
                }
            }
            view.InvalidateVisual();
            e.Handled = true;
        }

        private void Button_PreviewMouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            if (xgrid.Visible)
                xgrid.Visible = false;
            else xgrid.Visible = true;
        }

        /// <summary>
        /// Gets or sets the fixed elements.
        /// </summary>
        /// <value>The fixed elements.</value>

        double factor = 1.0;

        public static UISphere selected { get; set; }

        private void Button_PreviewMouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            view.Camera.SetValue(HelixViewport3D.OrthographicProperty, true);
            view.InvalidateVisual();

            GC.Collect();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                Search();
            }
        }


        //ObservableCollection<XML_Item> entries = new ObservableCollection<XML_Item>();

        private void Button_Search(object sender, RoutedEventArgs e)
        {

            if (dataContext.ViewerMode == false)
            {

                if (string.IsNullOrEmpty(tbSearch.Text))
                    return;
                SearchBrowser(tbSearch.Text);
                return;
            }


            dataContext.Entries.Clear();

            AutoComplete autos = DataEx.Autocomplete(tbSearch.Text);

            List<XML_Item> items = new List<XML_Item>();

            if (autos != null)
            {
                if (autos.dictionary_terms != null)
                {
                    var files = new DirectoryInfo(fileFolder).GetFiles();
                    foreach (var df in autos.dictionary_terms.compound)
                    {
                        XML_Item dg = new XML_Item();
                        dg.Name = df;
                        items.Add(dg);

                        dataContext.Entries.Add(dg);

                    }
                }
            }

            tpSearch.IsSelected = true;
        }

        void Search()
        {
            if (dataContext.ViewerMode == false)
            {

                if (string.IsNullOrEmpty(tbSearch.Text))
                    return;
                SearchBrowser(tbSearch.Text);
                return;
            }


            dataContext.Entries.Clear();

            AutoComplete autos = DataEx.Autocomplete(tbSearch.Text);

            List<XML_Item> items = new List<XML_Item>();

            if (autos != null)
            {

                var files = new DirectoryInfo(fileFolder).GetFiles();
                foreach (var df in autos.dictionary_terms.compound)
                {
                    XML_Item dg = new XML_Item();
                    dg.Name = df;
                    items.Add(dg);

                    dataContext.Entries.Add(dg);

                }

            }

            tpSearch.IsSelected = true;
        }

        private void btnSaveModel_Click(object sender, RoutedEventArgs e)
        {

            var name = tbSearch.Text;

            if (string.IsNullOrEmpty(name))
                return;

            FileStream fs = new FileStream("Models//" + name + ".stl", FileMode.Create);
            StlExporter s = new StlExporter();
            s.Export(view.Viewport, fs);
            fs.Close();

            CurrentModelXml = "";

            LoadModelsFolder();

        }

        public static MemoryStream SerializeToStream(object objectType)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectType);
            return stream;
        }


        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object objectType = formatter.Deserialize(stream);
            return objectType;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderEx f = (FolderEx)lvModels2.SelectedItem;

            StLReader r = new StLReader();

            ModelVisual3D device = new ModelVisual3D();

            var mv = new ModelImporter();

            device.Content = mv.Load(f.Path);




            //view.Children.Clear();

            // Model3D m = mv.ToModel3D();

            //device.Content = mv;

            //view.Viewport.Children.Clear();

            List<ModelVisual3D> m = new List<ModelVisual3D>();

            foreach (var c in view.Viewport.Children)
            {
                if (c.GetType() == typeof(ModelVisual3D))
                {
                    m.Add((ModelVisual3D)c);

                }
            }

            foreach (ModelVisual3D c in m)
            {
                view.Viewport.Children.Remove(c);

            }

            GC.Collect();

            view.Viewport.Children.Add(device);

            GC.Collect();
        }


        public void NewViewPort()
        {
            ArrayList m = new ArrayList();

            foreach (var c in view.Viewport.Children)
            {
                if (c.GetType() == typeof(ModelVisual3D))
                    m.Add((ModelVisual3D)c);
                if (c.GetType() == typeof(UISphere))
                    m.Add(c);
                if (c.GetType() == typeof(UIPipe))
                    m.Add(c);
            }

            foreach (Visual3D c in m)
            {
                view.Viewport.Children.Remove(c);

            }
        }

        private void lvModel2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            tbProperties.IsSelected = true;

            ArrayList m = new ArrayList();

            foreach (var c in view.Viewport.Children)
            {
                if (c.GetType() == typeof(ModelVisual3D))
                    m.Add((ModelVisual3D)c);
                if (c.GetType() == typeof(UISphere))
                    m.Add(c);
                if (c.GetType() == typeof(UIPipe))
                    m.Add(c);
            }

            foreach (Visual3D c in m)
            {
                view.Viewport.Children.Remove(c);

            }

            this.Title = "3D Model Expolorer ";


            FolderEx f = (FolderEx)lvModels2.SelectedItem;

            if (f == null)
                return;

            string file = f.Path;

            if (file.EndsWith(".xml"))
            {


                LoadFromXml(file);

                CurrentModelXml = file;

            }
            else
            {


                StLReader r = new StLReader();

                ModelVisual3D device = new ModelVisual3D();

                var mv = new ModelImporter();

                device.Content = mv.Load(f.Path);

                GC.Collect();

                view.Viewport.Children.Add(device);

                GC.Collect();

            }


            string name = Path.GetFileName(file);

            string namenoext = Path.GetFileNameWithoutExtension(file);

            btnSaveAsXml.Content = "Save as .XML" + " - " + name;

            tbSave.Text = "_" + name;

            this.Title = "3D Model Expolorer  -  " + name;

            var image = DataEx.LoadFormula(namenoext);

            imgHeaderEx.Source = image;

            imgFormula.Source = image;

            //if (dataContext.queue.Count > 0)
            {
                dataContext.queue.Clear();

                dataContext.queue.Enqueue(namenoext);
            }



            BitmapSource b = DataEx.TextToBitmap(namenoext);

            imgHeader.Source = b;

            tbStatusBar.Text = namenoext;

            tbSearch.Text = namenoext;

        }

        private void btnSaveAsXml_Click(object sender, RoutedEventArgs e)
        {


            foreach (UISphere a in atoms.Values)
            {
                a.Update();
            }

            string s = Serializer.Serialize<Compound>(compound);

            File.WriteAllText(CurrentModelXml, s);
        }

        private void XGrid_Click(object sender, RoutedEventArgs e)
        {
            xgrid.Visible = true;
            ygrid.Visible = false;
            zgrid.Visible = false;

            mXGrid.IsChecked = true;
            mYGrid.IsChecked = false;
            mZGrid.IsChecked = false;


        }
        private void YGrid_Click(object sender, RoutedEventArgs e)
        {
            xgrid.Visible = false;
            ygrid.Visible = true;
            zgrid.Visible = false;

            mXGrid.IsChecked = false;
            mYGrid.IsChecked = true;
            mZGrid.IsChecked = false;

        }

        private void ZGrid_Click(object sender, RoutedEventArgs e)
        {
            xgrid.Visible = false;
            ygrid.Visible = false;
            zgrid.Visible = true;

            mXGrid.IsChecked = false;
            mYGrid.IsChecked = false;
            mZGrid.IsChecked = true;
        }

        private void GridVisible_Click(object sender, RoutedEventArgs e)
        {
            xgrid.Visible = true;
            ygrid.Visible = true;
            zgrid.Visible = false;

            mXGrid.IsChecked = true;
            mYGrid.IsChecked = true;
            mZGrid.IsChecked = false;
        }

        private void GridInvisible_Click(object sender, RoutedEventArgs e)
        {
            xgrid.Visible = false;
            ygrid.Visible = false;
            zgrid.Visible = false;

            mXGrid.IsChecked = false;
            mYGrid.IsChecked = false;
            mZGrid.IsChecked = false;

        }


        void ToggleImporters()
        {
            if (mToggleImporters.IsChecked)
            {
                plImporters.ColumnDefinitions[0].Width = new GridLength(0);
                mToggleImporters.IsChecked = false;
            }
            else
            {
                plImporters.ColumnDefinitions[0].Width = new GridLength(300);
                mToggleImporters.IsChecked = true;
            }
        }

        private void ToggleImporters_Click(object sender, RoutedEventArgs e)
        {
            ToggleImporters();
        }

        void ToggleModels()
        {
            if (mToggleModels.IsChecked)
            {
                plImporters.ColumnDefinitions[4].Width = new GridLength(0);
                mToggleModels.IsChecked = false;
            }
            else
            {
                plImporters.ColumnDefinitions[4].Width = new GridLength(300);
                mToggleModels.IsChecked = true;
            }
        }

        private void ToggleModels_Click(object sender, RoutedEventArgs e)
        {

            ToggleModels();
        }


        private void ToolBar_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBar toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness();
            }
        }

        private void Button_PreviewMouseLeftButtonDown_3(object sender)
        {

        }

        async private void trvMenu_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = trvMenu.SelectedItem;
            if (node == null)
                return;
            DomainItem d = (DomainItem)node;

            tbSearch.Text = d.Name;

            e.Handled = true;

            ////this.Dispatcher.BeginInvoke(new Action(() => {


            ////    LoadDomainItem(d.Name);

            ////    var image = DataEx.LoadFormula(Path.GetFileNameWithoutExtension(d.Name));


            ////    imgFormula.Source = image;



            ////}));


            var image = DataEx.LoadFormula(Path.GetFileNameWithoutExtension(d.Name));


            imgFormula.Source = image;

            var delayTask = DelayAsync(d.Name);
            // Wait for the delay to complete.
            await delayTask;


        }

        private async Task DelayAsync(string name)
        {

            await Task.Run(() =>
            {

                this.Dispatcher.BeginInvoke(new Action(() =>
                {


                    LoadDomainItem(name);





                }));

            });

        }

        private void mExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void tbNew_Click(object sender, RoutedEventArgs e)
        {
            NewViewPort();

            DrawName("");
        }

        void DrawName(string name)
        {

            BitmapSource b = DataEx.TextToBitmap(name);

            imgHeader.Source = b;

            tbStatusBar.Text = name;
        }

        private void mNewViewport_Click(object sender, RoutedEventArgs e)
        {
            NewViewPort();
        }

        private void mDebugMode_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.DebugMode = MainWindow.DebugMode == true ? false : true;
        }

        private void mShowFormula_Click(object sender, RoutedEventArgs e)
        {
            if (imgHeaderEx.Visibility != Visibility.Visible)
            {
                imgHeaderEx.Visibility = Visibility.Visible;
            }
            else
            {
                imgHeaderEx.Visibility = Visibility.Collapsed;
            }
        }

        private void mAboutMessage_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("3DModelExplorer (JK)");
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {

            if (lvModels2.SelectedItem == null)
                return;

            int index = lvModels2.SelectedIndex;

            FolderEx ff = (FolderEx)lvModels2.SelectedItem;

            if (ff == null)
                return;

            string s = ff.Name;

            if (string.IsNullOrEmpty(s))
                return;

            string file = ModelsPath() + "\\" + s;

            if (File.Exists(file))
                File.Delete(file);

            btnSaveAsXml.Content = "Save as .XML";

            tbSave.Text = "";

            LoadModelsFolder();

            if (lvModels2.Items.Count <= 0)
                return;

            lvModels2.SelectedIndex = index < lvModels2.Items.Count ? index : index - 1 >= 0 ? index - 1 : 0;

        }

        private void ToggleImporters(object sender, MouseButtonEventArgs e)
        {
            ToggleImporters();
        }

        private void ToggleModels(object sender, MouseButtonEventArgs e)
        {
            ToggleModels();
        }

        private void tbModelsEx_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grMiddle.ColumnDefinitions[1].Width = new GridLength(tbModelsEx.ActualWidth);
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            string s = tbSave.Text;
            if (string.IsNullOrEmpty(s))
                return;
            string filextension = Path.GetExtension(s);


            string filename = ModelsPath() + "\\" + s;

            if (filextension.ToLower() == ".xml")
            {
                foreach (UISphere a in atoms.Values)
                {
                    a.Update();
                }

                string c = Serializer.Serialize<Compound>(compound);

                File.WriteAllText(CurrentModelXml, c);

                LoadModelsFolder();

                return;
            }

            switch (filextension.ToLower())
            {

                case ".stl":
                    Exporters.ExportStl(view.Viewport, filename);
                    LoadModelsFolder();
                    break;
                case ".3ds":
                    Exporters.ExportX3D(view.Viewport, filename);
                    LoadModelsFolder();
                    break;

            }


        }

        private void tbPropertie_Click(object sender, RoutedEventArgs e)
        {
            if (grViewer3D.Visibility != Visibility.Collapsed)
            {
                dataContext.ViewerMode = false;
                //tbModels.IsSelected = true;
                tbProperties.IsSelected = true;
                grViewer3D.Visibility = Visibility.Collapsed;
                tbBrowser.Visibility = Visibility.Visible;
                tbBrowse.Visibility = Visibility.Visible;
                tbModels.Visibility = Visibility.Collapsed;
                tbBrowse.IsSelected = true;
                grBrowser.Visibility = Visibility.Visible;
                tbPropertie.Content = "Browser Mode";
                btnSaveModel.Content = "Search Browse";
                //lvProps.Visibility = Visibility.Collapsed;

                //Browser.Address = "https://github.com/VE-2016/3DModelExplorer";

                //wbSample.Navigate("https://github.com/VE-2016/3DModelExplorer");

                //LoadProperties();

            }
            else
            {
                dataContext.ViewerMode = true;
                tbBrowse.Visibility = Visibility.Collapsed;
                tbModels.Visibility = Visibility.Visible;
                tbModels.IsSelected = true;
                tbBrowse.IsSelected = false;

                grBrowser.Visibility = Visibility.Collapsed;
                grViewer3D.Visibility = Visibility.Visible;
                tbBrowser.Visibility = Visibility.Visible;
                tbPropertie.Content = "3D Viewer Mode";
                btnSaveModel.Content = "Search Model";

            }
        }

        private void txtUrl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Browser.Address = txtUrl.Text;
        }

        private void lvProps_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


        }

        void SearchBrowser(string name)
        {

            dataContext.PagesEntries.Clear();

            var c = DataEx.GetBrowserPagesForName(name);

            foreach (Pages b in c.pages)
            {
                dataContext.PagesEntries.Add(b);
            }
        }

        private void lbBrowse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pages pages = (Pages)lbBrowse.SelectedItem;
            if (pages == null)
                return;

            txtUrl.Text = "https://en.wikipedia.org/wiki/" + pages.title;

            Browser.Address = "https://en.wikipedia.org/wiki/" + pages.title;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Browser.BackCommand.Execute(new object());
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            Browser.ForwardCommand.Execute(new object());
        }



        private void cmRemoveLoaded_KeyDown(object sender, KeyEventArgs e)
        {
            XML_Item v = (XML_Item)lb.SelectedItem;

            if (v == null)
                return;

            string f = AppDomain.CurrentDomain.BaseDirectory + "Files\\Content" + v.Name;

            if (File.Exists(f))
            {
                File.Delete(f);

                lb.Items.Remove(v);
            }
        }

        private void grProperties_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = lvProps.ActualWidth;

            if (w - lvPropsName.ActualWidth <= 0)
                return;

            lvPropsProperties.Width = w - lvPropsName.ActualWidth;

        }

        private void mViewportBlack_Click(object sender, RoutedEventArgs e)
        {
            view.Background = new SolidColorBrush(Colors.Black);
            mViewportBlack.IsChecked = true;
            mViewportWhite.IsChecked = false;
        }

        private void mViewportWhite_Click(object sender, RoutedEventArgs e)
        {
            view.Background = new SolidColorBrush(Colors.White);
            mViewportBlack.IsChecked = false;
            mViewportWhite.IsChecked = true;
        }

        private void tbCenter_Click(object sender, RoutedEventArgs e)
        {
            view.ZoomExtents();
        }
    }
    public class XML_Item
    {
        public string Name { get; set; }
        public int Completion { get; set; }
        public Compound compound { get; set; }
    }

    public class MV
    {

    }

    [Serializable]
    public class UISphere : UIElement3D
    {
        Point3D center { get; set; }

        public Atom atom { get; set; }

        public TranslateTransform3D firstTranslate { get; set; }

        public double sphereSize { get; set; }

        public double factor = 1.0;

        static Dictionary<Material, GeometryModel3D> gf = new Dictionary<Material, GeometryModel3D>();


        static public GeometryModel3D mff = null;

        static public GeometryModel3D mf(Atom atom)
        {

            if (!gf.ContainsKey(atom.Material()))
            {
                MeshBuilder builder = new MeshBuilder();

                builder.AddSphere(new Point3D(), 0.5, 64, 64);

                mff = new GeometryModel3D(builder.ToMesh(), atom.Material());

                gf.Add(atom.Material(), mff);


            }

            return gf[atom.Material()];


        }

        public UISphere(double _factor, Atom atom, Point3D center, Material material, HelixViewport3D view, double sphereSize = 0.5, MeshGeometry3D mesh = null)
        {
            this.view = view;
            this.atom = atom;
            this.sphereSize = sphereSize;
            GeometryModel3D model = null;

            model = mf(atom);

            var c = MainWindow.atomsTable.atoms.Where(s => s.Element.ToLower() == atom.descriptor.ShortName).ToList();

            if (c != null && c.Count() > 0)
            {

                factor = c[0].Atomic__1 ?? 1;

                factor *= 2.0;

            }

            t = new TranslateTransform3D(center.X/**factor*/, center.Y/*factor*/, center.Z/*factor*/);
            s = new ScaleTransform3D(factor, factor, factor);
            g = new Transform3DGroup();
            g.Children.Add(s);
            g.Children.Add(t);
            Transform = g;
            firstTranslate = t;
            center = new Point3D();
            Visual3DModel = model;

            //atom.center = Transform.Transform(atom.center);




        }
        Point3D? pt { get; set; }
        HelixViewport3D view { get; set; }
        protected override void OnMouseDown(MouseButtonEventArgs events)
        {
            base.OnMouseDown(events);

            if (events.RightButton == MouseButtonState.Pressed)
            {
                MainWindow.selected = this;
                GeometryModel3D c = Visual3DModel as GeometryModel3D;
                c.Material = Materials.Brown;

                events.Handled = true;
                return;
            }

            MainWindow.selected = null;

            GeometryModel3D point = Visual3DModel as GeometryModel3D;

            Point3D p = view.FindNearestPoint(events.GetPosition(view)).Value;


            pt = (Point3D)(p - /*center*/t.Transform(center));


            point.Material = Materials.DarkGray;
            events.Handled = true;

            if (pt == null)
                MessageBox.Show("");
        }
        Point3D b;

        public ScaleTransform3D s = new ScaleTransform3D();
        public TranslateTransform3D t = new TranslateTransform3D();
        Transform3DGroup g = new Transform3DGroup();

        public void TransformScale(double factor)
        {
            s = new ScaleTransform3D(s.ScaleX * factor, s.ScaleY * factor, s.ScaleZ * factor);

            t = new TranslateTransform3D(t.OffsetX * factor, t.OffsetY * factor, t.OffsetZ * factor);


        }

        public void ScaleSphere()
        {
            sphereSize *= 1.1;
            GeometryModel3D model = null;
            MeshBuilder builder = new MeshBuilder();
            builder.AddSphere(new Point3D(), sphereSize, 64, 64);

            model = new GeometryModel3D(builder.ToMesh(), atom.Material());



            Visual3DModel = model;
        }

        public void TransformRefresh()
        {
            g = new Transform3DGroup();

            g.Children.Add(s);
            g.Children.Add(t);

            Transform = g;
        }

        protected override void OnMouseMove(MouseEventArgs events)
        {
            base.OnMouseMove(events);

            if (pt == null)
                return;


            GeometryModel3D point = Visual3DModel as GeometryModel3D;
            Point3D p = view.FindNearestPoint(events.GetPosition(view)).Value;
            b = (Point3D)(p - pt);
            g = new Transform3DGroup();
            t = new TranslateTransform3D(b.X, b.Y, b.Z);
            var s = new ScaleTransform3D(factor, factor, factor);
            g.Children.Add(s);
            g.Children.Add(t);

            Transform = g;

            point.Material = Materials.Brown;//.DarkGray;

            if (MainWindow.dict.ContainsKey(this))
            {
                List<UIPipe> fs = MainWindow.dict[this];

                foreach (var bs in fs)
                {
                    bs.Rebuild(g);
                }
            }
            if (MainWindow.dict2.ContainsKey(this))
            {
                List<UIPipe> fs = MainWindow.dict2[this];


                //var bs = atom.Bonds();
                foreach (var bs in fs)
                {
                    bs.Rebuild(g, false);
                }
            }

            atom.center = Transform.Transform(new Point3D());

            events.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs events)
        {
            base.OnMouseUp(events);

            pt = null;

            GeometryModel3D point = Visual3DModel as GeometryModel3D;


            if (this != MainWindow.selected)
                point.Material = atom.Material(); //change mat to red on mouse enter
            events.Handled = true;
        }

        protected override void OnMouseEnter(MouseEventArgs events)
        {
            base.OnMouseEnter(events);

            GeometryModel3D point = Visual3DModel as GeometryModel3D;

            point.Material = Materials.Red; //change mat to red on mouse enter
            events.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs events)
        {
            base.OnMouseEnter(events);

            GeometryModel3D point = Visual3DModel as GeometryModel3D;

            point.Material = atom.Material();
            events.Handled = true;
        }

        public void Update()
        {
            //atom.center = Transform.Transform(atom.center);
        }

    }
    public class UIPipe : UIElement3D
    {
        Point3D center { get; set; }

        public Bond bond { get; set; }

        public Atom firstAtom { get; set; }
        public Atom secondAtom { get; set; }

        double diameter { get; set; }

        Point3D second { get; set; }
        Point3D first { get; set; }
        double factor { get; set; }

        public bool enableEditing = false;

        public UIPipe(double factor, Bond b, Point3D f, Point3D p, Atom fs, Atom sc, double diameter, Material material, HelixViewport3D view)
        {
            bond = b;
            firstAtom = fs;
            secondAtom = sc;
            first = f;
            second = p;
            this.diameter = diameter;
            this.factor = factor;
            this.view = view;
            GeometryModel3D model = null;
            MeshBuilder builder = new MeshBuilder();

            builder.AddCylinder(f, p, diameter, 60);
            model = new GeometryModel3D(builder.ToMesh(), material);


            t = new TranslateTransform3D(center.X * factor, center.Y * factor, center.Z * factor);
            s = new ScaleTransform3D(factor, factor, factor);
            g = new Transform3DGroup();
            g.Children.Add(s);
            g.Children.Add(t);
            Transform = g;

            this.center = new Point3D();
            Visual3DModel = model;
        }

        public void Rebuild(Transform3DGroup d, bool firstAtom = true)
        {
            GeometryModel3D model = null;
            MeshBuilder builder = new MeshBuilder();

            if (firstAtom)
            {
                var a = d.Transform(new Point3D()/*first*/);
                var b = g.Transform(first);

                var c = new Point3D((a.X - b.X) / s.ScaleX, (a.Y - b.Y) / s.ScaleY, (a.Z - b.Z) / s.ScaleZ);

                c = a;

                builder.AddCylinder(c, second, diameter, 60);
                model = new GeometryModel3D(builder.ToMesh(), Materials.Green);

                first = c;

            }
            else
            {

                var a = d.Transform(new Point3D()/*second*/);
                var b = g.Transform(second);

                var c = new Point3D((a.X - b.X) / s.ScaleX, (a.Y - b.Y) / s.ScaleY, (a.Z - b.Z) / s.ScaleZ);

                c = a;

                builder.AddCylinder(first, c, diameter, 60);
                model = new GeometryModel3D(builder.ToMesh(), Materials.Green);

                second = c;
            }

            Visual3DModel = model;
        }

        Point3D? pt { get; set; }
        HelixViewport3D view { get; set; }

        protected override void OnMouseDown(MouseButtonEventArgs events)
        {
            base.OnMouseDown(events);

            GeometryModel3D point = Visual3DModel as GeometryModel3D;

            Point3D p = view.FindNearestPoint(events.GetPosition(view)).Value;


            pt = (Point3D)(p - t.Transform(center));


            point.Material = Materials.DarkGray;
            events.Handled = true;


        }
        Point3D b;

        public ScaleTransform3D s = new ScaleTransform3D();
        public TranslateTransform3D t = new TranslateTransform3D();
        Transform3DGroup g = new Transform3DGroup();

        public void TransformScale(double factor)
        {
            s = new ScaleTransform3D(s.ScaleX * factor, s.ScaleY * factor, s.ScaleZ * factor);

            t = new TranslateTransform3D(t.OffsetX * factor, t.OffsetY * factor, t.OffsetZ * factor);


        }

        public void TransformScaleOnly(double factor)
        {
            s = new ScaleTransform3D(s.ScaleX * factor, s.ScaleY * factor, s.ScaleZ * factor);




        }

        public void TransformRefresh()
        {
            g = new Transform3DGroup();

            g.Children.Add(s);
            g.Children.Add(t);

            Transform = g;
        }


        protected override void OnMouseMove(MouseEventArgs events)
        {
            base.OnMouseMove(events);

            if (!enableEditing)
                return;

            if (pt == null)
                return;

            GeometryModel3D point = Visual3DModel as GeometryModel3D;
            Point3D p = view.FindNearestPoint(events.GetPosition(view)).Value;
            b = (Point3D)(p - pt);
            g = new Transform3DGroup();
            t = new TranslateTransform3D(b.X, b.Y, b.Z);
            g.Children.Add(s);
            g.Children.Add(t);

            Transform = g;




            point.Material = Materials.DarkGray;
            events.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs events)
        {
            base.OnMouseUp(events);

            pt = null;

            GeometryModel3D point = Visual3DModel as GeometryModel3D;

            point.Material = Materials.DarkGray; //change mat to red on mouse enter
            events.Handled = true;
        }

        protected override void OnMouseEnter(MouseEventArgs events)
        {
            base.OnMouseEnter(events);

            GeometryModel3D point = Visual3DModel as GeometryModel3D;

            point.Material = Materials.Red; //change mat to red on mouse enter
            events.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs events)
        {
            base.OnMouseEnter(events);

            GeometryModel3D point = Visual3DModel as GeometryModel3D;

            point.Material = Materials.Blue; //change mat to blue on mouse leave
            events.Handled = true;
        }

        public string ToStringEx()
        {
            return bond.ToStringEx();
        }
    }

    public class DomainItem
    {
        public DomainItem()
        {
            this.Items = new ObservableCollection<DomainItem>();
        }

        public string Name { get; set; }

        public ObservableCollection<DomainItem> Items { get; set; }
    }

    public class DataContext : INotifyPropertyChanged
    {

        public bool viewerMode = true;

        public bool ViewerMode
        {
            get { return viewerMode; }
            set
            {
                viewerMode = value;
                UpdateProperty("ViewerMode");
            }
        }




        public Queue queue = new Queue();

        public Queue browserQueue = new Queue();

        public Queue loaderQueue = new Queue();

        public Queue importQueue = new Queue();

        public ObservableCollection<XML_Item> Entries { get; private set; }

        public ObservableCollection<PropEx> PropEntries { get; private set; }

        public ObservableCollection<Pages> PagesEntries { get; private set; }

        public DataContext()
        {
            Entries = new ObservableCollection<XML_Item>();

            PropEntries = new ObservableCollection<PropEx>();

            PagesEntries = new ObservableCollection<Pages>();

            ViewerMode = true;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void UpdateProperty(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }


        public void BrowseSearchRequired(string name)
        {
            string s = name.Split("//".ToCharArray()).ToList().Last().Replace("//", "").Trim();

            if (string.IsNullOrEmpty(s))
                return;

            browserQueue.Clear();

            browserQueue.Enqueue(s);
        }


    }


}
