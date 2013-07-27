using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extensions.System.Windows.Forms {
	/// <summary>
	/// Extension of TextBox that has watermark functionality and allows for retrieving and setting the caret position.
	/// </summary>
	public class TextBoxExtended : TextBox {
		private EventHandler mGotFocus;
		private EventHandler mLostFocus;
		private bool mWatermarked = true;
		private string mWatermark = string.Empty;
		private Color mWatermarkColor = Color.Empty;
		private Color mOriginalForeColor = Color.Empty;

		public TextBoxExtended() : base() {
			base.GotFocus += new EventHandler(CaptureGotFocus);
			base.LostFocus += new EventHandler(CaptureLostFocus);
			GotFocus += new EventHandler(OnFocus);
			LostFocus += new EventHandler(OnBlur);	
		}

		public TextBoxExtended(string pWatermark) : this(pWatermark, Color.DarkGray) { }

		public TextBoxExtended(string pWatermark, Color pWatermarkColor) : base() {
			Watermark = pWatermark;
			mOriginalForeColor = base.ForeColor;
			mWatermarkColor = pWatermarkColor;
			UseWatermark = true;
			UpdateBox(pWatermarkColor, pWatermark);

			base.GotFocus += new EventHandler(CaptureGotFocus);
			base.LostFocus += new EventHandler(CaptureLostFocus);
			GotFocus += new EventHandler(OnFocus);
			LostFocus += new EventHandler(OnBlur);
		}

		#region Caret setting
		public void MoveCaret(int Line, int Column) {
			if (Line < 0 || Column < 0 || Lines.Length < Line || Text.Length < Column) {
				return;
			}
			SelectionStart = GetFirstCharIndexFromLine(Line) + Column;
			SelectionLength = 0;
		}

		public int CaretColumn {
			get { return SelectionStart - GetFirstCharIndexOfCurrentLine(); }
			set {
				SelectionStart = CaretLine + value;
				SelectionLength = 0;
			}
		}

		public int CaretLine {
			get { return GetLineFromCharIndex(SelectionStart); }
			set {
				SelectionStart = GetFirstCharIndexFromLine(value);
				SelectionLength = 0;
			}
		}
		#endregion

		#region Watermark
		public new event EventHandler GotFocus {
			add { mGotFocus += value; }
			remove { mGotFocus -= value; }
		}

		public new event EventHandler LostFocus {
			add { mLostFocus += value; }
			remove { mLostFocus -= value; }
		}

		public string Value {
			get {
				if (mWatermarked) return string.Empty;
				return base.Text;
			}
			set {
				if (string.IsNullOrEmpty(value)) {
					mWatermarked = true;
					UpdateBox(mWatermarkColor, Watermark);
				}
				else {
					mWatermarked = false;
					UpdateBox(mOriginalForeColor, value);
				}
			}
		}

		public string Watermark {
			get { return mWatermark; }
			set {
				mWatermark = value;
				if (mWatermarked && !Focused) base.Text = mWatermark;
			}
		}

		public Color WatermarkColor {
			get { return mWatermarkColor; }
			set {
				mWatermarkColor = value;
				if (mWatermarked && !Focused) base.ForeColor = mWatermarkColor;
			}
		}

		public bool UseWatermark { get; set; }

		private void CaptureGotFocus(object pSender, EventArgs e) {
			if (mGotFocus != null) mGotFocus(pSender, e);
		}

		private void CaptureLostFocus(object pSender, EventArgs e) {
			if (mLostFocus != null) mLostFocus(pSender, e);
		}
	
		private void OnFocus(object pSender, EventArgs e) {
			if (UseWatermark && mWatermarked) UpdateBox(mOriginalForeColor, string.Empty);
		}

		private void OnBlur(object pSender, EventArgs e) {
			if (UseWatermark) {
				if (string.IsNullOrEmpty(base.Text)) {
					mWatermarked = true;
					UpdateBox(mWatermarkColor, Watermark);
				}
				else mWatermarked = false;
			}
		}

		private void UpdateBox(Color pColor, string pText) {
			base.ForeColor = pColor;
			base.Text = pText;
		}
		#endregion
	}
}
