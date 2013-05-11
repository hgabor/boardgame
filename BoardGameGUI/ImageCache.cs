using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Level14.BoardGameRules;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Level14.BoardGameGUI
{
    public class ImageCache
    {
        Dictionary<string, ImageSource> pieces = new Dictionary<string, ImageSource>();

        public ImageCache(Game g)
        {
            // Load default images
            pieces.Clear();
            for (int i = 0; i < g.PlayerCount; i++)
            {
                int player = i + 1;
                string pieceName = "piece" + player.ToString();
                BitmapImage img = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + @"\Images\" + pieceName + ".png"));
                pieces.Add(pieceName, img);
            }
        }

        public ImageCache(Game g, string gamename)
            : this(g)
        {
            try
            {
                BitmapImage img = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + @"\Images\bg\" + gamename + ".png"));
                pieces.Add("bg", img);
            }
            catch (System.IO.FileNotFoundException e)
            {
                // Don't worry about it, we'll draw a checkerboard instead
            }
        }

        public ImageSource this[string name]
        {
            get
            {
                return pieces[name];
            }
        }

        public bool Contains(string name) { return pieces.ContainsKey(name); }
    }
}
