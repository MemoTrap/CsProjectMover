using ClientLib;

namespace ClientGuiApp {
  public partial class Form1 : Form {
    public Form1 () {
      InitializeComponent ();
    }

    private async void button1_Click (object sender, EventArgs e) {
      MyClient client = new ();
      await client.RunAsync ();
    }
  }
}
