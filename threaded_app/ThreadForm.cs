using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace threaded_app {
    public partial class ThreadForm : Form {
        public ThreadForm() {
            InitializeComponent();

            int interval = rnd.Next(5, 40);
            /*/
            Size sz = new System.Drawing.Size(100, 20);
            for (int i = 0; i < maxTextBoxes; i++)
            {
                TextBox tb = textBoxes[i] = new TextBox();
                tb.Location = new Point(rnd.Next(this.ClientSize.Width - rects[i].Width), rnd.Next(this.ClientSize.Height - rects[i].Height));
                tb.Name = tb.Text = string.Format("textBox{0}", i);
                tb.Size = sz;
                tb.TabIndex = i;
                deltas[i] = new Point(rnd.Next(1, 5), rnd.Next(1, 3));
                waits[i] = rnd.Next(5, 40);
                this.Controls.Add(tb);
            }
            /**/
            for (int i = 0; i < this.Controls.Count; i++) {
                deltas[i] = new Point(this.Controls.Count - i + 2, this.Controls.Count - i + 2);
                waits[i] = rnd.Next(5, 40);
                textBoxes[i] = (TextBox)this.Controls[i];
                textBoxes[i].Text = string.Format("textBox{0}", i);
                rects[i] = textBoxes[i].Bounds;
            }
            /**/
            exec = new execute(OnFocus);
            for (int i = 0; i < maxTextBoxes; i++) {
                ThreadParameter state = new ThreadParameter(this, i);
                threads[i] = new Thread(new ParameterizedThreadStart(ThreadFunc));
                threads[i].Start(state);
            }
        }

        delegate void execute(object obj);
        const int maxTextBoxes = 3;
        static execute exec;
        static Rectangle[] rects = new Rectangle[maxTextBoxes];
        static TextBox[] textBoxes = new TextBox[maxTextBoxes];
        static Thread[] threads = new Thread[maxTextBoxes];
        static Point[] deltas = new Point[maxTextBoxes];
        static Random rnd = new Random(DateTime.Now.Millisecond);
        static int[] waits = new int[maxTextBoxes];

        void OnFocus(object obj) {
            Container container = obj as Container;
            if (container == null)
                return;
            textBoxes[container.Index].Location = container.Location;
        }

        static void ThreadFunc(object state) {
            ThreadParameter tp = state as ThreadParameter;
            if (tp == null)
                return;

            Thread.Sleep(500);
            bool[] invert = new bool[maxTextBoxes];
            Container container = new Container();
            Point loc = new Point();
            ThreadForm frm = tp.Form;
            int i = tp.Index;

            int YTop;
            int YBottom;
            int XLeft;
            int XRight;
            bool YTopLeft;
            bool YTopRight;
            bool YBottomLeft;
            bool YBootomRight;
            bool XLeftTop;
            bool XLeftBottom;
            bool XRightTop;
            bool XRightBottom;
            bool bYTop;
            bool bYBottom;
            bool bXLeft;
            bool bXRight;

            while (true) {
                Monitor.Enter(container);
                //lock (container)
                //{
                container.Index = i;
                loc.X = rects[i].X + deltas[i].X;
                loc.Y = rects[i].Y + deltas[i].Y;
                container.Location = rects[i].Location = loc;
                textBoxes[i].Invoke(exec, container);

                if ((rects[i].X < 0) || (rects[i].X + rects[i].Width > frm.ClientSize.Width))
                    deltas[i].X = -deltas[i].X;

                if ((rects[i].Y < 0) || (rects[i].Y + rects[i].Height > frm.ClientSize.Height))
                    deltas[i].Y = -deltas[i].Y;

                for (int j = i + 1; j < maxTextBoxes; j++) {
                    if (
                        (Math.Abs(rects[i].X - rects[j].X) < rects[i].Width)// + Math.Abs(deltas[i].X)
                        &&
                        (Math.Abs(rects[i].Y - rects[j].Y) < rects[i].Height)// + Math.Abs(deltas[j].Y)
                       ) {
                        YTop = rects[i].Bottom - rects[j].Top;
                        YBottom = rects[j].Bottom - rects[i].Top;
                        XLeft = rects[i].Right - rects[j].Left;
                        XRight = rects[j].Right - rects[i].Left;

                        YTopLeft = YTop < XLeft && XLeft < rects[i].Width && YTop < rects[i].Height;
                        YTopRight = YTop < XRight && XRight < rects[i].Width && YTop < rects[i].Height;
                        YBottomLeft = YBottom < XLeft && XLeft < rects[i].Width && YBottom < rects[i].Height;
                        YBootomRight = YBottom < XRight && XRight < rects[i].Width && YBottom < rects[i].Height;

                        XLeftTop = XLeft < YTop && YTop < rects[i].Height && XLeft < rects[i].Width;
                        XLeftBottom = XLeft < YBottom && YBottom < rects[i].Height && XLeft < rects[i].Width;
                        XRightTop = XRight < YTop && YTop < rects[i].Height && XRight < rects[i].Width;
                        XRightBottom = XRight < YBottom && YBottom < rects[i].Height && XRight < rects[i].Width;

                        bYTop = YTop == 0;
                        bYBottom = YTop == rects[i].Height;
                        bXLeft = XLeft == 0;
                        bXRight = XLeft == rects[i].Width;

                        if (
                            YTopLeft && deltas[i].Y > 0 ||
                            YTopRight && deltas[i].Y > 0 ||
                            YBottomLeft && deltas[i].Y < 0 ||
                            YBootomRight && deltas[i].Y < 0 ||
                            XLeftTop && deltas[i].X < 0 ||
                            XLeftBottom && deltas[i].X < 0 ||
                            XRightTop && deltas[i].X > 0 ||
                            XRightBottom && deltas[i].X > 0 ||
                            bXLeft || bXRight
                            ) {
                            if (invert[j])
                                continue;
                            InvertY(i, j);
                            invert[j] = true;
                        }
                        else if (
                            YTopLeft && deltas[i].Y < 0 ||
                            YTopRight && deltas[i].Y < 0 ||
                            YBottomLeft && deltas[i].Y > 0 ||
                            YBootomRight && deltas[i].Y > 0 ||
                            XLeftTop && deltas[i].X > 0 ||
                            XLeftBottom && deltas[i].X > 0 ||
                            XRightTop && deltas[i].X < 0 ||
                            XRightBottom && deltas[i].X < 0 ||
                            bYTop || bYBottom
                            ) {
                            if (invert[j])
                                continue;
                            InvertX(i, j);
                            invert[j] = true;
                        }
                        else if (XLeft == YTop || YTop == XRight || XRight == YBottom || XLeft == YBottom) {
                            //if (invert[j])
                            //    continue;
                            InvertX(i, j);
                            InvertY(i, j);
                            invert[j] = true;
                        }
                    }
                    else
                        invert[j] = false;
                }
                //}
                Monitor.Exit(container);
                Thread.Sleep(waits[i]);
            }
        }

        static void InvertX(int i, int j) {
            if (deltas[i].X * deltas[j].X < 0)
                deltas[j].X = -deltas[j].X;
            else if (Math.Abs(deltas[j].Y) > Math.Abs(deltas[i].Y)) {
                deltas[j].Y = -deltas[j].Y;
                return;
            }
            deltas[i].X = -deltas[i].X;
        }

        static void InvertY(int i, int j) {
            if (deltas[i].Y * deltas[j].Y < 0)
                deltas[j].Y = -deltas[j].Y;
            else if (Math.Abs(deltas[j].X) > Math.Abs(deltas[i].X)) {
                deltas[j].X = -deltas[j].X;
                return;
            }
            deltas[i].Y = -deltas[i].Y;
        }

        protected override void OnClosed(EventArgs e) {
            for (int i = 0; i < maxTextBoxes; i++) {
                if (threads[i] != null)
                    threads[i].Abort();
            }
        }
    }

    class ThreadParameter {
        public int Index { get; set; }
        public ThreadForm Form { get; set; }
        public ThreadParameter(ThreadForm frm, int index) {
            Index = index;
            Form = frm;
        }
    }

    class Container {
        public Point Location { get; set; }
        public int Index { get; set; }
    }

}
