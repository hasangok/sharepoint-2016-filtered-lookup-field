using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Globalization;

namespace Dev4Side.SP2016.FilteredLookup
{
  /// <summary>
  /// The custom filtered lookup field class
  /// </summary>
  [CLSCompliant(false)]
  [Guid("CADE9B7D-1777-4503-854E-B3EE09A6554B")]
  [SharePointPermission(SecurityAction.Demand, ObjectModel = true)]
  public class FilteredLookupField : SPFieldLookup {

    #region Fields
    /// <summary>
    /// The GUID of the list view to use in data filtering
    /// </summary>
    private string _listViewFilter;
    /// <summary>
    /// The CAML query to use in data filtering
    /// </summary>
    private string _queryFilter;
    /// <summary>
    /// Indicates whether field supports multiple values or not
    /// </summary>
    private string _allowMultiple;
    /// <summary>
    /// Indicates whether the filter should be applied recursively or not
    /// </summary>
    private string _isFilterRecursive;
    #endregion

    #region constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="fieldName"></param>
    public FilteredLookupField(SPFieldCollection fields, string fieldName)
      : base(fields, fieldName) {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="typeName"></param>
    /// <param name="displayName"></param>
    public FilteredLookupField(SPFieldCollection fields, string typeName, string displayName)
      : base(fields, typeName, displayName) {
    } 
    #endregion

    #region OnAdded method
    /// <summary>
    /// Fires when a new filtered lookup field is added
    /// </summary>
    /// <param name="op"></param>
    public override void OnAdded(SPAddFieldOptions op) {
      base.OnAdded(op);
      Update();
    } 
    #endregion

    #region Update method
    /// <summary>
    /// Updates the properties of the filtered lookup field
    /// </summary>
    public override void Update() {

      UpdateFieldProperties();
      base.Update();
      CleanUpThreadData();
    } 
    #endregion

    #region UpdateFieldProperties method
    /// <summary>
    /// Updates custom properties of the filtered lookup field
    /// </summary>
    private void UpdateFieldProperties() {
      string _v = GetFieldThreadDataValue("ListViewFilter", true);
      string _l = GetFieldThreadDataValue("QueryFilterAsString", true);
      string _m = GetFieldThreadDataValue("SupportsMultipleValues", true);
      string _r = GetFieldThreadDataValue("IsFilterRecursive", true);
      base.SetCustomProperty("ListViewFilter", _v);
      base.SetCustomProperty("QueryFilterAsString", _l);
      base.SetCustomProperty("SupportsMultipleValues", _m);

      // this property matters only if and when query filter is used
      // we can always derive the equivalent if and when view filter is used
      base.SetCustomProperty("IsFilterRecursive", ((!string.IsNullOrEmpty(_l)) ? _r : "false"));

      if (this.AllowMultipleValues) {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(base.SchemaXml);
        EnsureAttribute(doc, "Mult", "TRUE");
        base.SchemaXml = doc.OuterXml;
      }
    } 
    #endregion

    #region EnsureAttribute method
    /// <summary>
    /// Ensures the given attribute of an xml node exists in an xml document and has the specified value
    /// </summary>
    /// <param name="doc">Xml document containing the node</param>
    /// <param name="name">Name of the node</param>
    /// <param name="value">Value of the node</param>
    private void EnsureAttribute(XmlDocument doc, string name, string value) {
      XmlAttribute attribute = doc.DocumentElement.Attributes[name];
      if (attribute == null) {
        attribute = doc.CreateAttribute(name);
        doc.DocumentElement.Attributes.Append(attribute);
      }
      doc.DocumentElement.Attributes[name].Value = value;
    } 
    #endregion

    #region GetValidatedString method
    public override string GetValidatedString(object value) {
      if (this.Required) {
        string _s = string.Format(CultureInfo.InvariantCulture,
          "{0} is required.", this.Title);
        if (value == null) {
          throw new SPFieldValidationException(_s);
        }
        else {
          if (this.AllowMultipleValues) {
            SPFieldLookupValueCollection c = value as SPFieldLookupValueCollection;
            if (c.Count < 0) {
              throw new SPFieldValidationException(_s);
            }
          }
          else {
            SPFieldLookupValue v = value as SPFieldLookupValue;
            if (v.LookupId < 1 && (string.IsNullOrEmpty(v.LookupValue) || v.LookupValue == "(None)")) {
              throw new SPFieldValidationException(_s);
            }
          }
        }
      }
      return base.GetValidatedString(value);
    } 
    #endregion

    #region GetFieldThreadDataValue method
    private string GetFieldThreadDataValue(string propertyName, bool ignoreEmptyValue) {
      string _d = (string)Thread.GetData(Thread.GetNamedDataSlot(propertyName));
      if (string.IsNullOrEmpty(_d) && !ignoreEmptyValue) {
        _d = (string)base.GetCustomProperty(propertyName);
      }
      return _d;
    }

    private void SetFieldThreadDataValue(string propertyName, string value) {
      Thread.SetData(Thread.GetNamedDataSlot(propertyName), value);
    } 
    #endregion

    #region CleanUpThreadData method
    private void CleanUpThreadData() {
      Thread.FreeNamedDataSlot("ListViewFilter");
      Thread.FreeNamedDataSlot("QueryFilterAsString");
      Thread.FreeNamedDataSlot("SupportsMultipleValues");
      Thread.FreeNamedDataSlot("IsFilterRecursive");
    } 
    #endregion

    #region Sortable property
    public override bool Sortable {
      get {
        return (this.AllowMultipleValues) ? false : base.Sortable;
      }
    } 
    #endregion

    #region AllowMultipleValues property
    public override bool AllowMultipleValues {
      get {
        if (_allowMultiple == null) {
          _allowMultiple = GetFieldThreadDataValue("SupportsMultipleValues", false);
        }
        return (!string.IsNullOrEmpty(_allowMultiple) && _allowMultiple.ToLower() == "true") ? true : false;
      }
      set {
        SetFieldThreadDataValue("SupportsMultipleValues", value.ToString());
      }
    } 
    #endregion

    #region IsFilterRecursive property
    public bool IsFilterRecursive {
      get {
        if (_isFilterRecursive == null) {
          _isFilterRecursive = GetFieldThreadDataValue("IsFilterRecursive", false);
        }
        return (!string.IsNullOrEmpty(_isFilterRecursive) && _isFilterRecursive.ToLower() == "true") ? true : false;
      }
      set {
        SetFieldThreadDataValue("IsFilterRecursive", value.ToString());
      }
    } 
    #endregion
    
    #region ListViewFilter property
    public string ListViewFilter {
      get {
        if (_listViewFilter == null) {
          _listViewFilter = GetFieldThreadDataValue("ListViewFilter", false);
        }
        return (!string.IsNullOrEmpty(_listViewFilter)) ? _listViewFilter : null;
      }
      set {
        SetFieldThreadDataValue("ListViewFilter",
          (!string.IsNullOrEmpty(value) ? value : ""));
      }
    } 
    #endregion

    #region QueryFilterAsString property
    public string QueryFilterAsString {
      get {
        if (_queryFilter == null) {
          _queryFilter = GetFieldThreadDataValue("QueryFilterAsString", false);
        }
        return (!string.IsNullOrEmpty(_queryFilter)) ? _queryFilter : null;
      }
      set {
        SetFieldThreadDataValue("QueryFilterAsString",
          (!string.IsNullOrEmpty(value) ? value : ""));
      }
    } 
    #endregion

    #region QueryFilter property
    public SPQuery QueryFilter {
      get {
        SPQuery q = null;
        if (!string.IsNullOrEmpty(this.QueryFilterAsString)) {
          q = new SPQuery();
          q.Query = SPHttpUtility.HtmlDecode(this.QueryFilterAsString);
          if (IsFilterRecursive) {
            q.ViewAttributes = "Scope=\"Recursive\"";
          }
        }
        else if (!string.IsNullOrEmpty(this.ListViewFilter)) {
          try {
              SPWeb w = SPContext.Current.Site.OpenWeb(LookupWebId);
              SPView _v = w.Lists[new Guid(LookupList)].Views[new Guid(ListViewFilter)];
              if (_v != null) {
                q = new SPQuery();
                q.Query = _v.Query; // use only view's query to avoid view's excess baggage :)
                if (_v.Scope != SPViewScope.Default) {
                  q.ViewAttributes = string.Format(CultureInfo.InvariantCulture, "Scope=\"{0}\"", _v.Scope);
                }
              }            
          }
          catch { }
        }

        return q;
      }
    }
    #endregion

    #region FieldRenderingControl property
    public override BaseFieldControl FieldRenderingControl {
      [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
      get {
        BaseFieldControl fieldControl = null;
        if (this.AllowMultipleValues) {
          fieldControl = new MultipleFilteredLookupFieldControl();
        }
        else {
          fieldControl = new FilteredLookupFieldControl();
        }
        fieldControl.FieldName = this.InternalName;

        return fieldControl;
      }
    } 
    #endregion
  }
}
