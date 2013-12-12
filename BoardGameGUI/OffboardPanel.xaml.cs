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
    /// Interaction logic for OffboardPanel.xaml
    /// </summary>
    public partial class OffboardPanel : UserControl
    {
        public OffboardPanel()
        {
            
        }

        public Game Game { private get; set; }
        public MainWindow.GameStateGetter GetGameState { private get; set; }
        public ImageCache ImageCache { private get; set; }
        List<Piece> pieces;
        private HashSet<Piece> highlight = new HashSet<Piece>();

        public void ClearHighlight()
        {
            highlight.Clear();
        }

        public void AddHighlight(Piece p)
        {
            highlight.Add(p);
        }

        public class OffboardEventArgs : EventArgs
        {
            public Piece Piece { get; private set; }

            public OffboardEventArgs(Piece piece)
            {
                this.Piece = piece;
            }
        }

        public delegate void OffboardClick(object sender, OffboardEventArgs args);
        public event OffboardClick Selected;

        public void Refresh()
        {
            if (Game == null) return;
            pieces = new List<Piece>(GetGameState().CurrentPlayer.GetOffboard(GetGameState()));

            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Game == null || ImageCache == null || pieces == null) return;
            for (int i = 0; i < pieces.Count; i++)
            {
                Piece p = pieces[i];
                string imgName = p.Type + p.Owner.ID.ToString();
                var coords = new Rect(i * 30, 0, 30, 30);

                if (highlight.Contains(p)) dc.DrawRectangle(Brushes.LightBlue, null, coords);
                dc.DrawImage(ImageCache[imgName], coords);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (Game == null || pieces == null) return;
            var p = e.GetPosition(this);

            if (p.Y >= 30) return;
            int id = (int)p.X / 30;
            if (id >= pieces.Count) return;

            if (Selected != null) Selected(this, new OffboardEventArgs(pieces[id]));
        }
    }
}
