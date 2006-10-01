using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Drawing;
using fooManagedWrapper;
using fooTitle.Geometries;
using System.Windows.Forms;


namespace fooTitle.Layers
{
	/// <summary>
	/// Loads itself from an xml file and handles all drawing.
	/// </summary>
	public class Skin : Layer,  IPlayCallbackSender
	{
		XmlDocument document = new XmlDocument();
        XmlNode skin;
        string name;
        

        /// <summary>
        /// Returns the directory of the skin. Can be used for loading images and other data files.
        /// </summary>
        public string SkinDirectory {
            get {
                return Path.Combine(Main.DataDir, name);
            }
        }

        private OnPlaybackNewTrackDelegate onNewTrackRegistered;
        private OnPlaybackTimeDelegate onTimeRegistered;
        private OnPlaybackPauseDelegate onPauseRegistered;
        private OnPlaybackStopDelegate onStopRegistered;

		/// <summary>
		/// Loads the skin from the specified xml file
		/// </summary>
		/// <param name="fileName">Name of the skin. This must be the same as the name of it's directory</param>
		public Skin(string _name) : base()
		{
            name = _name;

			// load the skin xml file
			document.Load(GetSkinFilePath("skin.xml"));

			// read the xml document for the basic properties
			skin = document.GetElementsByTagName("skin").Item(0);

            int width = Int32.Parse(skin.Attributes.GetNamedItem("width").Value);
            int height = Int32.Parse(skin.Attributes.GetNamedItem("height").Value);
            geometry = new AbsoluteGeometry(new Rectangle(0, 0, width, height), width, height, new Point(0,0), AlignType.Left);
			
            // register to main for playback events
            onNewTrackRegistered = new OnPlaybackNewTrackDelegate(OnPlaybackNewTrack);
            onTimeRegistered = new OnPlaybackTimeDelegate(OnPlaybackTime);
            onStopRegistered = new OnPlaybackStopDelegate(OnPlaybackStop);
            onPauseRegistered = new OnPlaybackPauseDelegate(OnPlaybackPause);
            Main.GetInstance().OnPlaybackNewTrackEvent += onNewTrackRegistered;
            Main.GetInstance().OnPlaybackTimeEvent += onTimeRegistered;
            Main.GetInstance().OnPlaybackPauseEvent += onPauseRegistered;
            Main.GetInstance().OnPlaybackStopEvent += onStopRegistered;
		}

        /// <summary>
        /// Call to free this skin (unregistering events, unregistering layer events,...)
        /// </summary>
        public void Free() {
            Main.GetInstance().OnPlaybackNewTrackEvent -= onNewTrackRegistered;
            Main.GetInstance().OnPlaybackTimeEvent -= onTimeRegistered;
            Main.GetInstance().OnPlaybackStopEvent -= onStopRegistered;
            Main.GetInstance().OnPlaybackPauseEvent -= onPauseRegistered;

            display.MouseUp -= mouseUpReg;
            display.MouseDown -= mouseDownReg;
            display.MouseMove -= mouseMoveReg;
        }

		/// <summary>
		/// Resizes the skin and all layers, but not the form.
		/// </summary>
		/// <param name="newWidth">The new width, it's not yet possible to change height</param>
		public void Resize(Size newSize) {
			((AbsoluteGeometry)geometry).Width = newSize.Width;
            ((AbsoluteGeometry)geometry).Height = newSize.Height;
			foreach (Layer l in layers ) {
				l.UpdateGeometry(ClientRect);
			}
		}

        public override Size GetMinimalSize() {
            // don't ask geometry..
            return defaultGetMinimalSize();
        }


        /// <summary>
        /// Asks layers for optimal size and resizes itself and the display in case it's needed.
        /// </summary>
        public void CheckSize() {
            Size size = GetMinimalSize();
            Resize(size);
            Main.GetInstance().Display.SetSize(ClientRect.Width, ClientRect.Height);
        }
        
        /// <summary>
        /// Used for easily finding out what is the path to a skin's file (image, xml, extension,...)
        /// </summary>
        /// <param name="fileName">Name of the file which is searched for.</param>
        /// <returns>Path composed of app's data directory and skin's directory.</returns>
        public string GetSkinFilePath(string fileName) {
            return Path.Combine(SkinDirectory, fileName);
        }

        public void OnPlaybackTime(double time) {
            // pass it on
            sendEvent(OnPlaybackTimeEvent, time);
         }

        public void OnPlaybackNewTrack(MetaDBHandle song) {
            // pass it on
            sendEvent(OnPlaybackNewTrackEvent, song);
            CheckSize();
        }

        public void OnPlaybackStop(IPlayControl.StopReason reason) {
            // pass it on
            sendEvent(OnPlaybackStopEvent, reason);
            if (reason != IPlayControl.StopReason.stop_reason_starting_another)
                CheckSize();
        }

        public void OnPlaybackPause(bool state) {
            // pass it on
            sendEvent(OnPlaybackPauseEvent, state);
        }

        /// <summary>
        /// Does not check for exceptions
        /// </summary>
        /// <param name="_event">This must be a delegate</param>
        /// <param name="p">Parameters for the delegate</param>
        protected static void sendEvent(Object _event, params Object[] p) {
            if (_event != null) {
                System.Delegate d = (System.Delegate)_event;
                d.DynamicInvoke(p);
            }
        }

        #region IPlayCallbackSender Members

        public event OnPlaybackTimeDelegate OnPlaybackTimeEvent;
        public event OnPlaybackNewTrackDelegate OnPlaybackNewTrackEvent;
        public event OnQuitDelegate OnQuitEvent;
        public event OnInitDelegate OnInitEvent;
        public event OnPlaybackStopDelegate OnPlaybackStopEvent;
        public event OnPlaybackPauseDelegate OnPlaybackPauseEvent;


        #endregion

        public event MouseEventHandler OnMouseMove;
        public event MouseEventHandler OnMouseDown;
        public event MouseEventHandler OnMouseUp;
        public event EventHandler OnMouseLeave;

        protected MouseEventHandler mouseMoveReg, mouseDownReg, mouseUpReg;
        protected EventHandler mouseLeaveReg;

        public void Init(Display _display) {
            display = _display;

            // register to mouse events
            mouseMoveReg = new MouseEventHandler(display_MouseMove);
            display.MouseMove += mouseMoveReg;
            mouseDownReg = new MouseEventHandler(display_MouseDown);
            display.MouseDown += mouseDownReg;
            mouseUpReg = new MouseEventHandler(display_MouseUp);
            display.MouseUp += mouseUpReg;
            mouseLeaveReg = new EventHandler(display_MouseLeave);
            display.MouseLeave += mouseLeaveReg;

            loadLayers(skin);
            geometry.Update(new Rectangle(0, 0, ((AbsoluteGeometry)geometry).Width, ((AbsoluteGeometry)geometry).Height));
        }

        void display_MouseUp(object sender, MouseEventArgs e) {
            MouseEventHandler temp = OnMouseUp;
            if (temp != null) {
                temp(sender, e);
            }
        }

        void display_MouseDown(object sender, MouseEventArgs e) {
            MouseEventHandler temp = OnMouseDown;
            if (temp != null) {
                temp(sender, e);
            }
        }

        void display_MouseMove(object sender, MouseEventArgs e) {
            MouseEventHandler temp = OnMouseMove;
            if (temp != null) {
                temp(sender, e);
            }
        }

        void display_MouseLeave(object sender, EventArgs e) {
            EventHandler temp = OnMouseLeave;
            if (temp != null)
                temp(sender, e);
        }
    }
}