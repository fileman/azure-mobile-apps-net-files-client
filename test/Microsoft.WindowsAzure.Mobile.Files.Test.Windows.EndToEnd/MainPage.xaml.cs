using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Microsoft.WindowsAzure.Mobile.Files.Test.Windows.EndToEnd
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static LaunchActivatedEventArgs LaunchArgs;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void btnE2ETests_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["MobileAppUrl"] = txtRuntimeUri.Text;
            VisualStudio.TestPlatform.TestExecutor.UnitTestClient.Run(LaunchArgs.Arguments);
        }
    }
}
