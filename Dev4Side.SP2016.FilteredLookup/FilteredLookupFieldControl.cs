using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Web.UI.HtmlControls;
using System.Web;

namespace Dev4Side.SP2016.FilteredLookup
{
  /// <summary>
  /// The rendering control class for filtered lookup field
  /// </summary>
  [CLSCompliant(false)]
  [Guid("36960BE5-5FDD-4368-AA93-EF34A3DC5FD7")]
  public sealed class FilteredLookupFieldControl : LookupField {
    #region Fields
    SPFieldLookupValue _fieldVal;
    List<ListItem> _availableItems = null; 
    #endregion

    #region DefaultTemplateName property
    protected override string DefaultTemplateName { get { return "FilteredLookupFieldControl"; } } 
    #endregion

    #region OnInit and OnLoad methods
    protected override void OnInit(EventArgs e) {
      if (ControlMode == SPControlMode.Edit || ControlMode == SPControlMode.Display) {
        if (base.ListItemFieldValue != null) {
          _fieldVal = base.ListItemFieldValue as SPFieldLookupValue;
        }
        else { _fieldVal = new SPFieldLookupValue(); }
      }
      if (ControlMode == SPControlMode.New) { _fieldVal = new SPFieldLookupValue(); }
      base.OnInit(e);
      Initialize();
    }

    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);
      if (ControlMode != SPControlMode.Display) {
        if (!Page.IsPostBack) {
          SetValue();
        }
      }
    } 
    #endregion

    #region CreateChildControls method
    protected override void CreateChildControls() {
      // 19 items is limit for switching from
      // standard dropdown to SharePoint fancy dropdown
      if (base.Field != null && base.ControlMode != SPControlMode.Display) {
        if (!this.ChildControlsCreated) {
          this.Controls.Clear();
          this.Controls.Add(new LiteralControl("<span dir=\"none\">"));
          FilteredLookupField field = base.Field as FilteredLookupField;

          if (_availableItems != null && _availableItems.Count > 19 && IsExplorerOnWin()) {
            CreateCustomSelect();
          }
          else { CreateStandardSelect(); }
          this.Controls.Add(new LiteralControl("<br /></span>"));
        }
      }
    } 
    #endregion

    #region IsExplorerOnWin method
    /// <summary>
    /// Gets a value that indicates the client is IE on Windows
    /// </summary>
    /// <returns></returns>
    private bool IsExplorerOnWin() {
      HttpBrowserCapabilities hc = this.Page.Request.Browser;
      return (hc.Browser.ToLower() == "ie" &&
        hc.Platform.ToLower() == "winnt" && hc.MajorVersion > 5);
    }    
    #endregion

    #region CreateStandardSelect method
    private void CreateStandardSelect() {
      DropDownList l = new DropDownList();
      l.ID = "Lookup";
      l.ToolTip = string.Format(CultureInfo.InvariantCulture, "{0}", Field.InternalName);
      if (!Util.ListIsNullOrEmpty(_availableItems)) {
        l.Items.Clear();
        l.Items.AddRange(_availableItems.ToArray());
      }
      if (!Field.Required) { l.Items.Insert(0, new ListItem("(None)", "0")); }
      this.Controls.Add(l);
    } 
    #endregion

    #region CreateCustomSelect method
    private void CreateCustomSelect() {
      HtmlInputHidden h = new HtmlInputHidden();
      h.ID = string.Format(CultureInfo.InvariantCulture, "{0}_Hidden", Field.InternalName);
      this.Controls.Add(h);
      this.Controls.Add(new LiteralControl("<span style=\"vertical-align: middle\">"));

      HtmlInputText t = new HtmlInputText();
      t.ID = "Txtbx";
      t.Attributes.Add("class", "ms-lookuptypeintextbox");
      t.Attributes.Add("onfocusout", "HandleLoseFocus()");
      t.Attributes.Add("opt", "_Select");
      t.Attributes.Add("title", string.Format(CultureInfo.InvariantCulture, "{0}", Field.InternalName));
      t.Attributes.Add("optHid", h.ClientID);
      t.Attributes.Add("onkeypress", "HandleChar()");
      t.Attributes.Add("onkeydown", "HandleKey()");
      t.Attributes.Add("match", "");
      t.Attributes.Add("choices", ConcatAvailableItems("|"));
      t.Attributes.Add("onchange", "HandleChange()");
      this.Controls.Add(t);

      this.Controls.Add(new LiteralControl("<img alt=\"Display lookup values\" onclick=\"ShowDropdown('" + t.ClientID + "');\" " +
        "src=\"/_layouts/images/dropdown.gif\" style=\"border-width: 0px; vertical-align: middle;\" />"));

      this.Controls.Add(new LiteralControl("</span>"));
    } 
    #endregion

    #region ConcatAvailableItems method
    private string ConcatAvailableItems(string delimiter) {
      string retval = string.Empty;
      if (!Util.ListIsNullOrEmpty(_availableItems)) {
        if (!this.Field.Required) { retval += string.Format(CultureInfo.InvariantCulture, "{0}{1}{0}0", delimiter, "(None)"); }
        foreach (ListItem i in _availableItems) {
          retval += string.Format("{0}{1}{0}{2}", delimiter, i.Text, i.Value);
        }

        return retval.Trim().Substring(1);
      }

      return retval;
    } 
    #endregion

    #region GetRenderingWebControl method
    /// <summary>
    /// Gets the web control used in rendering the field (new and edit modes only)
    /// </summary>
    /// <returns></returns>
    private Control GetRenderingWebControl() {
      Control ctrl = null;
      foreach (Control c in Controls) {
        if (c.ID == "Lookup" && c.GetType().FullName == "System.Web.UI.WebControls.DropDownList") {
          ctrl = c;
          break;
        }
        else if (c.ID == "Txtbx" && c.GetType().FullName == "System.Web.UI.HtmlControls.HtmlInputText") {
          ctrl = c;
          break;
        }
      }

      return ctrl;
    } 
    #endregion

    #region Value property
    public override object Value {
      get {
        EnsureChildControls();
        Control c = GetRenderingWebControl();
        if (c != null) {
          if (c is System.Web.UI.WebControls.DropDownList) {
            DropDownList ctrl = c as DropDownList;
            if (ctrl.SelectedItem.Value != "0" && ctrl.SelectedItem.Text != "(None)") {
              return (new SPFieldLookupValue(
                int.Parse(ctrl.SelectedItem.Value), ctrl.SelectedItem.Text));
            }
          }
          else if (c is System.Web.UI.HtmlControls.HtmlInputText) {
            return GetCustomSelectValue(((HtmlInputText)c));
          }
        }
        return new SPFieldLookupValue();
      }
      set {
        EnsureChildControls();
        base.Value = value as SPFieldLookupValue;
      }
    } 
    #endregion

    #region Initialize method
    private void Initialize() {
      _availableItems = Util.GetAvailableValues(
        ((FilteredLookupField)base.Field), Context);
      if(!Util.ListIsNullOrEmpty(_availableItems)){
        EnsureValueIsAvailable();
      }
    }
    #endregion

    #region EnsureValueIsAvailable method
    /// <summary>
    /// Ensures that previously selected value is still available
    /// when an item is being edited. This is necessary just in case
    /// the value of the field is not necessarily being changed.
    /// </summary>
    private void EnsureValueIsAvailable() {
      if (_fieldVal != null && !string.IsNullOrEmpty(_fieldVal.LookupValue)) {
        ListItem s = _availableItems.Find(x => (x.Value.ToLower() == _fieldVal.LookupId.ToString().ToLower()));
        if (s == null) {
          _availableItems.Add(new ListItem(_fieldVal.LookupValue, _fieldVal.LookupId.ToString()));
        }
      }
    }  
    #endregion

    #region GetCustomSelectValue and SetCustomSelectValue methods
    private SPFieldLookupValue GetCustomSelectValue(HtmlInputText txtBox) {
      Control h = FindControl(string.Format(CultureInfo.InvariantCulture, "{0}_Hidden", Field.InternalName));
      if (h != null && !string.IsNullOrEmpty(((HtmlInputHidden)h).Value)) {
        ListItem s = _availableItems.Find(x => (x.Value.ToLower() == ((HtmlInputHidden)h).Value.ToLower()));
        if (s != null && (s.Value != "0") && (s.Text.ToLower() == txtBox.Value.ToLower())) {
          return new SPFieldLookupValue(int.Parse(s.Value), s.Text);
        }
      }

      return new SPFieldLookupValue();
    }

    private void SetCustomSelectValue(HtmlInputText txtBox) {
      if (_fieldVal != null && (!string.IsNullOrEmpty(_fieldVal.LookupValue))) {
        txtBox.Value = _fieldVal.LookupValue;
        Control h = FindControl(string.Format(CultureInfo.InvariantCulture, "{0}_Hidden", Field.InternalName));
        if (h != null) { ((HtmlInputHidden)h).Value = _fieldVal.LookupId.ToString(); }
      }
    } 
    #endregion

    #region SetValue method
    private void SetValue() {
      Control c = GetRenderingWebControl();

      if (!Util.ListIsNullOrEmpty(_availableItems) && (c != null)) {
        if (c.GetType().FullName == "System.Web.UI.WebControls.DropDownList") {
          DropDownList ctrl = c as DropDownList;
          if (_fieldVal != null && (!string.IsNullOrEmpty(_fieldVal.LookupValue))) {
            ListItem bitem = ctrl.Items.FindByValue(_fieldVal.LookupId.ToString());
            if (bitem != null) {
              ctrl.SelectedIndex = ctrl.Items.IndexOf(bitem);
              base.ItemIds.Add(_fieldVal.LookupId);
            }
            else { ctrl.SelectedIndex = 0; }

          }
          else { ctrl.SelectedIndex = 0; }
        }
        else { SetCustomSelectValue(((HtmlInputText)c)); }
      }
    } 
    #endregion
  }
}
