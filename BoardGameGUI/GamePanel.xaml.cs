using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Level14.BoardGameRules;

namespace Level14.BoardGameGUI
{
    /// <summary>
    /// Interaction logic for GamePanel.xaml
    /// </summary>
    public partial class GamePanel : UserControl
    {
        public GamePanel()
        {
            InitializeComponent();
        }

        public ImageCache ImageCache { private get; set; }

        private Game game;
        public Game Game
        {
            get
            {
                return game;
            }
            set
            {
                game = value;
                if (game == null) return;
                this.Width = game.Size[0] * 30;
                this.Height = game.Size[1] * 30;
            }
        }

        public Coords Highlighted { get; set; }

        public class GamePanelSelectEventArgs : EventArgs
        {
            public Coords Coords { get; private set; }

            public GamePanelSelectEventArgs(Coords coords)
            {
                this.Coords = coords;
            }
        }

        public delegate void GamePanelClick(object sender, GamePanelSelectEventArgs args);
        public event GamePanelClick Selected;

        protected override void OnRender(System.Windows.Media.DrawingContext dc)
        {
            if (game == null) return;
            if (ImageCache == null) return;

            Coords size = game.Size;

            for (int i = 0; i < size[0]; i++)
            {
                for (int j = 0; j < size[1]; j++)
                {
                    int myj = size[1] - j - 1;
                    if (Highlighted != null && Coords.Match(Highlighted, new Coords(i+1, j+1)))
                    {
                        dc.DrawRectangle(Brushes.LightBlue, null, new Rect(i * 30, myj * 30, 30, 30));
                    }
                    else
                    {
                        Brush b = ((i + j) % 2) == 0 ? Brushes.Beige : Brushes.Brown;
                        dc.DrawRectangle(b, null, new Rect(i * 30, myj * 30, 30, 30));
                    }
                }
            }


            foreach (var kvp in game.GetPieces())
            {
                Coords c = kvp.Key;
                Piece p = kvp.Value;
                string player = p.Owner.ID.ToString();
                //string type = p.Type();
                string type = "piece";
                int x = (c[0]-1) * 30;
                int y = (size[1]-c[1]) * 30;

                dc.DrawImage(ImageCache[type + player], new System.Windows.Rect(x, y, 30, 30));
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (Game == null) return;
            var p = e.GetPosition(this);

            int x = (int)p.X / 30 + 1;
            int y = Game.Size[1] - (int)p.Y / 30;

            if (x > Game.Size[0] || x <= 0 || y > Game.Size[1] || y <= 0) return;

            if (Selected != null) Selected(this, new GamePanelSelectEventArgs(new Coords(x, y)));
        }
    }
}
