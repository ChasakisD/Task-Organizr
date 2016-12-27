using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TDLApi.Views
{
    public class TutorialItem
    {
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
    }

    public sealed partial class TutorialFlipView : Page
    {
        public static List<TutorialItem> images = new List<TutorialItem>();
        public TutorialFlipView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            InitImage();
            flipView.ItemsSource = images;
        }

        private void InitImage()
        {
            images.Add(new TutorialItem() { ImageName = "BatMan", ImagePath = "/Images/Tutorial/first.png" });
            images.Add(new TutorialItem() { ImageName = "SpiredMan" });
            images.Add(new TutorialItem() { ImageName = "Yohayo" });
            images.Add(new TutorialItem() { ImageName = "Chat" });
        }
    }
}
