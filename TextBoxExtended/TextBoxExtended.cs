﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Extensions.System.Windows.Forms {
	/// <summary>
	/// Extension of TextBox that has placeholder functionality and allows for retrieving and setting the caret position.
	/// </summary>
	public class TextBoxExtended : TextBox {
		private EventHandler mGotFocus;
		private EventHandler mLostFocus;
		private EventHandler mTextChanged;
		private bool mPlaceholderActive = true;
		private bool mFromOwnTextChange = false;
		private string mPlaceholder = string.Empty;
		private Color mPlaceholderColor = Color.Empty;
		private Color mOriginalForeColor = Color.Empty;

		public TextBoxExtended() : base() {}

		#region Designer
		private bool IsDesignerHosted {
			get {
				if (DesignMode) return DesignMode;
				if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
				return Process.GetCurrentProcess().ProcessName == "devenv";
			}
		}
		#endregion

		#region Caret setting
		public void MoveCaret(int Line, int Column) {
			if (Line < 0 || Column < 0 || Lines.Length < Line || Text.Length < Column) {
				return;
			}
			SelectionStart = GetFirstCharIndexFromLine(Line) + Column;
			SelectionLength = 0;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CaretColumn {
			get { return SelectionStart - GetFirstCharIndexOfCurrentLine(); }
			set {
				SelectionStart = CaretLine + value;
				SelectionLength = 0;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CaretLine {
			get { return GetLineFromCharIndex(SelectionStart); }
			set {
				SelectionStart = GetFirstCharIndexFromLine(value);
				SelectionLength = 0;
			}
		}
		#endregion

		#region Placeholder
		public new event EventHandler GotFocus {
			add { mGotFocus += value; }
			remove { mGotFocus -= value; }
		}

		public new event EventHandler LostFocus {
			add { mLostFocus += value; }
			remove { mLostFocus -= value; }
		}

		public new event EventHandler TextChanged {
			add { mTextChanged += value; }
			remove { mTextChanged -= value; }
		}

		private bool PreventPlaceholderUse {
			get { return IsDesignerHosted || !UsePlaceholder; }
		}

		public new string Text {
			get {
				if (PreventPlaceholderUse) return base.Text;
				if (mPlaceholderActive) return string.Empty;
				return base.Text;
			}
			set {
				if (PreventPlaceholderUse) {
					base.Text = value;
					return;
				}

				if (UsePlaceholder) {
					mPlaceholderActive = string.IsNullOrEmpty(value);
					if (mPlaceholderActive) UpdateBox(mPlaceholderColor, Placeholder);
					else UpdateBox(mOriginalForeColor, value);
				}
				base.Text = value;
			}
		}

		[Description("The text of the control's placeholder.")]
		[Category("Appearance")]
		public string Placeholder {
			get { return mPlaceholder; }
			set {
				mPlaceholder = value;
				if (!PreventPlaceholderUse && mPlaceholderActive && !Focused) base.Text = mPlaceholder;
			}
		}

		[Description("The color of the control's placeholder.")]
		[Category("Appearance")]
		public Color PlaceholderColor {
			get { return mPlaceholderColor; }
			set {
				mPlaceholderColor = value;
				if (!PreventPlaceholderUse && mPlaceholderActive && !Focused) base.ForeColor = mPlaceholderColor;
			}
		}

		[Description("Indicates if the control should use a placeholder.")]
		[Category("Appearance")]
		public bool UsePlaceholder { get; set; }

		private void CaptureGotFocus(object pSender, EventArgs e) {
			if (mGotFocus != null) mGotFocus(pSender, e);
		}

		private void CaptureLostFocus(object pSender, EventArgs e) {
			if (mLostFocus != null) mLostFocus(pSender, e);
		}

		private void CaptureTextChanged(object pSender, EventArgs e) {
			if (mTextChanged != null) mTextChanged(pSender, e);
		}

		private void OnFocus(object pSender, EventArgs e) {
			if (mPlaceholderActive) UpdateBox(mOriginalForeColor, string.Empty);
		}

		private void OnBlur(object pSender, EventArgs e) {
			mPlaceholderActive = string.IsNullOrEmpty(base.Text);
			if (mPlaceholderActive) UpdateBox(mPlaceholderColor, Placeholder);
		}

		private void OnTextChanged(object pSender, EventArgs e) {
			if (mFromOwnTextChange) return;
			mPlaceholderActive = string.IsNullOrEmpty(base.Text);
		}

		private void UpdateBox(Color pColor, string pText) {
			base.ForeColor = pColor;
			mFromOwnTextChange = true;
			base.Text = pText;
			mFromOwnTextChange = false;
		}
		#endregion

		protected override void InitLayout() {
			base.GotFocus += new EventHandler(CaptureGotFocus);
			base.LostFocus += new EventHandler(CaptureLostFocus);
			base.TextChanged += new EventHandler(CaptureTextChanged);

			if (PreventPlaceholderUse) {
				mPlaceholderActive = false;
				return;
			}

			mOriginalForeColor = ForeColor;
			ForeColor = PlaceholderColor;
			base.Text = Placeholder;
			GotFocus += new EventHandler(OnFocus);
			LostFocus += new EventHandler(OnBlur);
			TextChanged += new EventHandler(OnTextChanged);

			base.InitLayout();
		}
	}
}
