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
using System.Globalization;

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

        private ICoordTransformer ict = new IdentityTransformer();
        internal ICoordTransformer ICT { set { this.ict = value; } }

        public bool PrintGameCoords { get; set; }

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
                Coords size = ict.GameToBoard(game.Size);
                this.Width = size[0] * 30;
                this.Height = size[1] * 30;
            }
        }

        private HashSet<Coords> highlightCoord = new HashSet<Coords>();
        private HashSet<Piece> highlightPiece = new HashSet<Piece>();

        public void ClearHighlight()
        {
            highlightCoord.Clear();
            highlightPiece.Clear();
        }

        public void AddHighlight(Coords c)
        {
            highlightCoord.Add(ict.GameToBoard(c));
        }
        public void AddHighlight(Piece p)
        {
            highlightPiece.Add(p);
        }

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
            bool needsCheckerBoard = true;
            if (ImageCache.Contains("bg"))
            {
                dc.DrawImage(ImageCache["bg"], new Rect(0, 0, Width, Height));
                needsCheckerBoard = false;
            }

            Coords size = ict.GameToBoard(game.Size);

            for (int i = 0; i < size[0]; i++)
            {
                for (int j = 0; j < size[1]; j++)
                {
                    int myj = size[1] - j - 1;
                    Coords c = new Coords(i+1, j+1);
                    if (highlightCoord.Contains(c))
                    {
                        dc.DrawRectangle(Brushes.LightBlue, null, new Rect(i * 30, myj * 30, 30, 30));
                    }
                    else if (needsCheckerBoard)
                    {
                        Brush b = ((i + j) % 2) == 0 ? Brushes.DarkSeaGreen : Brushes.Brown;
                        dc.DrawRectangle(b, null, new Rect(i * 30, myj * 30, 30, 30));
                    }
                }
            }


            foreach (var kvp in game.GetPieces())
            {
                Coords c = ict.GameToBoard(kvp.Key);
                Piece p = kvp.Value;
                string player = p.Owner.ID.ToString();
                //string type = p.Type();
                string type = "piece";
                int x = (c[0]-1) * 30;
                int y = (size[1]-c[1]) * 30;

                if (highlightPiece.Contains(p))
                {
                    dc.DrawRectangle(Brushes.LightBlue, null, new Rect(x, y, 30, 30));
                }

                dc.DrawImage(ImageCache[type + player], new System.Windows.Rect(x, y, 30, 30));
            }

            for (int i = 0; i < size[0]; i++)
            {
                for (int j = 0; j < size[1]; j++)
                {
                    int myj = size[1] - j - 1;
                    Coords c = new Coords(i + 1, j + 1);
                    if (PrintGameCoords && ict.IsValidBoardCoord(c))
                    {
                        string str = c.ToString() + "\n" + ict.BoardToGame(c);
                        dc.DrawText(new FormattedText(
                            str,
                            CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface("Consolas"),
                            8,
                            Brushes.Black),
                            new Point(i * 30, myj * 30));

                    }
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (Game == null) return;
            var p = e.GetPosition(this);

            Coords size = ict.GameToBoard(game.Size);

            int x = (int)p.X / 30 + 1;
            int y = size[1] - (int)p.Y / 30;

            if (x > size[0] || x <= 0 || y > size[1] || y <= 0) return;

            if (Selected != null) Selected(this, new GamePanelSelectEventArgs(ict.BoardToGame(new Coords(x, y))));
        }
    }
}
