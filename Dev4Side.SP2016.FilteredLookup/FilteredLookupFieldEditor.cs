using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Globalization;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Runtime.InteropServices;

namespace Dev4Side.SP2016.FilteredLookup
{
  /// <summary>
  /// The field editor class
  /// </summary>
  [Guid("02E01404-7F2C-4224-820A-A772583ADA8F")]
  public class FilteredLookupFieldEditor : UserControl, IFieldEditor {

    #region fields
    readonly string[] EXCLUDED_FIELDS = new string[]{
      "_Author","_Category", "_CheckinComment", "_Comments", "_Contributor", "_Coverage", "_DCDateCreated",
      "_DCDateModified", "_EditMenuTableEnd", "_EditMenuTableStart", "_EndDate", "_Format",
      "_HasCopyDestinations", "_IsCurrentVersion", "_LastPrinted", "_Level", "_ModerationComments",
      "_ModerationStatus", "_Photo", "_Publisher", "_Relation", "_ResourceType", "_Revision",
      "_RightsManagement", "_SharedFileIndex", "_Source", "_SourceUrl", "_Status", "ActualWork",
      "AdminTaskAction", "AdminTaskDescription", "AdminTaskOrder", "AssignedTo", "Attachments",
      "AttendeeStatus", "Author", "BaseAssociationGuid", "BaseName", "Birthday", "Body",
      "BodyAndMore", "BodyWasExpanded", "Categories", "CheckoutUser", "Comment", "Comments", "Completed",
      "Created", "Created_x0020_By", "Created_x0020_Date", "DateCompleted", "DiscussionLastUpdated",
      "DiscussionTitle", "DocIcon", "DueDate", "Editor", "EmailBody", "EmailCalendarDateStamp",
      "EmailCalendarSequence", "EmailCalendarUid", "EndDate", "EventType", "Expires",
      "ExtendedProperties", "fAllDayEvent", "File_x0020_Size", "File_x0020_Type", "FileDirRef",
      "FileLeafRef", "FileRef", "FileSizeDisplay", "FileType", "FormData", "FormURN", "fRecurrence",
      "FSObjType", "FullBody", "Group", "GUID", "HasCustomEmailBody", "Hobbies", "HTML_x0020_File_x0020_Type",
      "IMAddress", "ImageCreateDate", "ImageHeight", "ImageSize", "ImageWidth", "Indentation", "IndentLevel",
      "InstanceID", "IsActive", "IsSiteAdmin", "ItemChildCount", "Keywords", "Last_x0020_Modified", "LessLink",
      "LimitedBody", "LinkDiscussionTitle", "LinkDiscussionTitleNoMenu", "LinkFilename", "LinkFilenameNoMenu",
      "LinkIssueIDNoMenu", "LinkTitle", "LinkTitleNoMenu", "MasterSeriesItemID", "MessageBody", "MessageId",
      "MetaInfo", "Modified", "Modified_x0020_By", "MoreLink", "Notes", "Occurred", "ol_Department",
      "ol_EventAddress", "owshiddenversion", "ParentFolderId", "ParentLeafName", "ParentVersionString",
      "PendingModTime", "PercentComplete", "PermMask", "PersonViewMinimal", "Picture", "PostCategory",
      "Priority", "ProgId", "PublishedDate", "QuotedTextWasExpanded", "RecurrenceData", "RecurrenceID",
      "RelatedIssues", "RelevantMessages", "RepairDocument", "ReplyNoGif", "RulesUrl", "ScopeId", "SelectedFlag",
      "SelectFilename", "ShortestThreadIndex", "ShortestThreadIndexId", "ShortestThreadIndexIdLookup",
      "ShowCombineView", "ShowRepairView", "StartDate", "StatusBar", "SystemTask", "TaskCompanies",
      "TaskDueDate", "TaskGroup", "TaskStatus", "TaskType", "TemplateUrl", "ThreadIndex", "Threading",
      "ThreadingControls", "ThreadTopic", "Thumbnail", "TimeZone", "ToggleQuotedText", "TotalWork",
      "TrimmedBody", "UniqueId", "VirusStatus", "WebPage", "WorkAddress", "WorkflowAssociation",
      "WorkflowInstance", "WorkflowInstanceID", "WorkflowItemId", "WorkflowListId", "WorkflowVersion",
      "xd_ProgID", "xd_Signature", "XMLTZone", "XomlUrl"
    };

    protected DropDownList listTargetWeb;
    protected DropDownList listTargetList;
    protected DropDownList listTargetColumn;
    protected DropDownList listTargetListView;
    protected Label lblTargetWeb;
    protected Label lblTargetList;
    protected CheckBox cbxAllowMultiValue; // allowMultiple;
    protected CheckBox cbxUnlimitedLengthInDocLib;
    protected HtmlGenericControl SpanDocLibWarning;
    protected HtmlGenericControl SpanLengthWarning;
    protected HtmlTableCell tdQuery;
    protected HtmlTableCell tdListView;
    protected RadioButtonList rdFilterOption;
    protected TextBox txtQueryFilter;
    protected CheckBox cbxRecursiveFilter;
    #endregion

    #region SetTargetWeb method
    private void SetTargetWeb() {
      listTargetWeb.Items.Clear();
      List<ListItem> str = new List<ListItem>();

      SPSite _site = SPControl.GetContextSite(this.Context); 
      
        SPWebCollection _webCollection = _site.AllWebs;
        string contextWebId = SPControl.GetContextWeb(this.Context).ID.ToString();
        foreach (SPWeb web in _webCollection) 
        {
          if (web.DoesUserHavePermissions(
            SPBasePermissions.ViewPages | SPBasePermissions.OpenItems | SPBasePermissions.ViewListItems)) 
          {
            str.Add(new ListItem(web.Title, web.ID.ToString()));
          }
        }
        if (str.Count > 0) 
        {
          str.Sort(delegate(ListItem item1, ListItem item2) {
            return item1.Text.CompareTo(item2.Text);
          });

          listTargetWeb.Items.AddRange(str.ToArray());
          ListItem bitem = null;
          if (!string.IsNullOrEmpty(TargetWebId)) { bitem = listTargetWeb.Items.FindByValue(TargetWebId); }
          else { bitem = listTargetWeb.Items.FindByValue(contextWebId); }
          if (bitem != null) { listTargetWeb.SelectedIndex = listTargetWeb.Items.IndexOf(bitem); }
          else { listTargetWeb.SelectedIndex = 0; }

          SetTargetList(listTargetWeb.SelectedItem.Value, true);
        }
      
    }
    #endregion

    #region SetControlVisibility method
    private void SetControlVisibility() {
      string referrer = this.Request.Url.AbsoluteUri;
      
      if (!string.IsNullOrEmpty(referrer)) {
        if (referrer.IndexOf("_layouts/15/fldNew.aspx") > -1
          || referrer.IndexOf("_layouts/15/FldNewEx.aspx") > -1
          || referrer.IndexOf("_layouts/15/FldEditEx.aspx") > -1) // adding new field
        {
          lblTargetList.Visible = false;
          lblTargetWeb.Visible = false;
          listTargetList.Visible = true;
          listTargetWeb.Visible = true;
          EnsureSelectedFilterOption(true);
        }
        else {
          lblTargetList.Visible = true;
          lblTargetWeb.Visible = true;
          listTargetList.Visible = false;
          listTargetWeb.Visible = false;
          EnsureSelectedFilterOption(false);
        }
      }
      if (SPContext.Current.List != null) {
        SpanDocLibWarning.Visible = (SPContext.Current.List.BaseType == SPBaseType.DocumentLibrary);
        SpanLengthWarning.Visible = (SPContext.Current.List.BaseType == SPBaseType.DocumentLibrary);
        cbxUnlimitedLengthInDocLib.Visible = (SPContext.Current.List.BaseType == SPBaseType.DocumentLibrary);
      }
    }
    #endregion

    #region SelectedTargetWebChanged method
    protected void SelectedTargetWebChanged(Object sender, EventArgs args) {
      if (listTargetWeb.SelectedIndex > -1) {
        SetTargetList(listTargetWeb.SelectedItem.Value, true);
        Page.SetFocus(listTargetList);
      }
    }
    #endregion

    #region SelectedTargetListChanged method
    protected void SelectedTargetListChanged(Object sender, EventArgs args) {
      if (listTargetList.SelectedIndex > -1) {
        string webId = string.Empty;
        if (listTargetWeb.Items.Count > 0) {
          webId = listTargetWeb.SelectedItem.Value;
        }
        else if (!string.IsNullOrEmpty(TargetWebId)) { webId = TargetWebId; }
        SetTargetColumn(webId, listTargetList.SelectedItem.Value);
        SetTargetListView(webId, listTargetList.SelectedItem.Value);
        Page.SetFocus(listTargetColumn);
      }
    }
    #endregion

    #region SelectedFilterOptionChanged method
    protected void SelectedFilterOptionChanged(Object sender, EventArgs args) {
      tdListView.Visible = false;
      tdQuery.Visible = false;
      listTargetListView.Visible = false;
      listTargetListView.Items.Clear();
      txtQueryFilter.Visible = false;
      txtQueryFilter.Text = string.Empty;
      cbxRecursiveFilter.Visible = false;
      cbxRecursiveFilter.Checked = false;
      TargetListViewId = string.Empty;

      if (rdFilterOption.SelectedIndex == 1) {
        SetTargetListView(listTargetWeb.SelectedItem.Value, listTargetList.SelectedItem.Value);
        tdListView.Visible = true;
        listTargetListView.Visible = true;
      }
      else {
        tdQuery.Visible = true;
        txtQueryFilter.Visible = true;
        cbxRecursiveFilter.Visible = true;
      }
    }
    #endregion

    #region EnsureSelectedFilterOption method
    private void EnsureSelectedFilterOption(bool isNew) {
      tdListView.Visible = false;
      tdQuery.Visible = false;
      listTargetListView.Visible = false;
      txtQueryFilter.Visible = false;
      cbxRecursiveFilter.Visible = false;

      if (!isNew && !string.IsNullOrEmpty(TargetListViewId)) {
        // show list view filter
        rdFilterOption.SelectedIndex = 1;
        tdListView.Visible = true;
        listTargetListView.Visible = true;
      }
      else {
        rdFilterOption.SelectedIndex = 0;
        tdQuery.Visible = true;
        txtQueryFilter.Visible = true;
        cbxRecursiveFilter.Visible = true;
      }
    } 
    #endregion

    #region Custom properties
    private string TargetWebId {
      get {
        object o = this.ViewState["TARGET_WEB_ID"];
        return (o != null && !string.IsNullOrEmpty(o.ToString())) ? o.ToString() : string.Empty;
      }
      set { this.ViewState["TARGET_WEB_ID"] = value; }
    }

    private string TargetListId {
      get {
        object o = this.ViewState["TARGET_LIST_ID"];
        return (o != null && !string.IsNullOrEmpty(o.ToString())) ? o.ToString() : string.Empty;
      }
      set { this.ViewState["TARGET_LIST_ID"] = value; }
    }

    private string TargetListViewId {
      get {
        object o = this.ViewState["TARGET_LISTVIEW_ID"];
        return (o != null && !string.IsNullOrEmpty(o.ToString())) ? o.ToString() : string.Empty;
      }
      set { this.ViewState["TARGET_LISTVIEW_ID"] = value; }
    }

    private string TargetColumnId {
      get {
        object o = this.ViewState["TARGET_COLUMN_ID"];
        return (o != null && !string.IsNullOrEmpty(o.ToString())) ? o.ToString() : string.Empty;
      }
      set { this.ViewState["TARGET_COLUMN_ID"] = value; }
    }
    #endregion

    #region OnInit
    protected override void OnInit(EventArgs e) {
      base.OnInit(e);
      Page.MaintainScrollPositionOnPostBack = true; //required to keep section within view
      if (!this.IsViewStateEnabled) { this.EnableViewState = true; }
    }
    #endregion

    #region SetTargetList method
    private void SetTargetList(string selectedWebId, bool setTargetColumn)
    {
        listTargetList.Items.Clear();
        if (!string.IsNullOrEmpty(selectedWebId))
        {
            SPSite _site = SPControl.GetContextSite(this.Context);
            SPWeb _web = _site.OpenWeb(new Guid(selectedWebId));
            List<ListItem> str = new List<ListItem>();
            SPListCollection _listCollection = _web.Lists;
            foreach (SPList list in _listCollection)
            {
                if (!list.Hidden)
                {
                    str.Add(new ListItem(list.Title, list.ID.ToString()));
                }
            }
            if (str.Count > 0)
            {
                str.Sort(delegate(ListItem item1, ListItem item2)
                {
                    return item1.Text.CompareTo(item2.Text);
                });

                listTargetList.Items.AddRange(str.ToArray());

                ListItem bitem = null;
                if (!string.IsNullOrEmpty(TargetListId)) { bitem = listTargetList.Items.FindByValue(TargetListId); }
                if (bitem != null) { listTargetList.SelectedIndex = listTargetList.Items.IndexOf(bitem); }
                else { listTargetList.SelectedIndex = 0; }

                if (setTargetColumn)
                {
                    SetTargetColumn(selectedWebId, listTargetList.SelectedItem.Value);
                }

                SetTargetListView(selectedWebId, listTargetList.SelectedItem.Value);
            }
        }

    }
    #endregion

    #region SetTargetListView method
    private void SetTargetListView(string webId, string selectedListId)
    {
        listTargetListView.Items.Clear(); // clear list, first and foremost
        if (!string.IsNullOrEmpty(webId) && !string.IsNullOrEmpty(selectedListId))
        {
            SPSite _site = SPControl.GetContextSite(this.Context);
            SPWeb _web = _site.OpenWeb(new Guid(webId));
            SPList list = _web.Lists[new Guid(selectedListId)];
            SPViewCollection views = list.Views;
            List<ListItem> str = new List<ListItem>();
            foreach (SPView v in views)
            {
                // use only views that are both visible and shared
                if (!v.Hidden && !v.PersonalView)
                {
                    str.Add(new ListItem(
                      string.Format(CultureInfo.InvariantCulture, "{0}", v.Title), v.ID.ToString()));
                }
            }
            if (str.Count > 0)
            {
                str.Sort(delegate(ListItem item1, ListItem item2)
                {
                    return item1.Text.CompareTo(item2.Text);
                });

                listTargetListView.Items.AddRange(str.ToArray());

                ListItem bitem = null;
                if (!string.IsNullOrEmpty(TargetListViewId)) { bitem = listTargetListView.Items.FindByValue(TargetListViewId); }
                if (bitem != null) { listTargetListView.SelectedIndex = listTargetListView.Items.IndexOf(bitem); }
                else { listTargetListView.SelectedIndex = 0; }
            }
        }
    } 
    #endregion

    #region CanFieldBeDisplayed method
    private bool CanFieldBeDisplayed(SPField f) {
      bool retval = false;
      if (f != null && !f.Hidden && (Array.IndexOf<string>(
        EXCLUDED_FIELDS, f.InternalName) < 0)) {
        switch (f.Type) {
          case SPFieldType.Computed:
            if (((SPFieldComputed)f).EnableLookup) { retval = true; }
            break;
          case SPFieldType.Calculated:
            if (((SPFieldCalculated)f).OutputType == SPFieldType.Text) { retval = true; }
            break;
          default:
            retval = true;
            break;
        }
      }

      return retval;
    } 
    #endregion

    #region SetTargetColumn method
    private void SetTargetColumn(string webId, string selectedListId) {
      listTargetColumn.Items.Clear();
      if (!string.IsNullOrEmpty(webId) && !string.IsNullOrEmpty(selectedListId)) {
        SPSite _site = SPControl.GetContextSite(this.Context); 
          SPWeb _web = _site.OpenWeb(new Guid(webId));
            SPList list = _web.Lists[new Guid(selectedListId)];
            SPFieldCollection fields = list.Fields;
            List<ListItem> str = new List<ListItem>();
            foreach (SPField f in fields) {
              if (CanFieldBeDisplayed(f)){
                str.Add(new ListItem(
                  string.Format(CultureInfo.InvariantCulture, "{0}", f.Title), f.Id.ToString()));
              }
            }
            if (str.Count > 0) {
              str.Sort(delegate(ListItem item1, ListItem item2) {
                return item1.Text.CompareTo(item2.Text);
              });

              listTargetColumn.Items.AddRange(str.ToArray());

              ListItem bitem = null;
              if (!string.IsNullOrEmpty(TargetColumnId)) { bitem = listTargetColumn.Items.FindByValue(TargetColumnId); }
              if (bitem != null) { listTargetColumn.SelectedIndex = listTargetColumn.Items.IndexOf(bitem); }
              else { listTargetColumn.SelectedIndex = 0; }
            }
          
      }
    }
    #endregion

    #region IFieldEditor Members

    public bool DisplayAsNewSection { get { return true; } }

    #region InitializeWithField method
    public void InitializeWithField(SPField field) {
      EnsureChildControls();
      FilteredLookupField _f = null;
      try { _f = field as FilteredLookupField; }
      catch { }

      if (_f != null) {
        // this bit only happens when field is not null
        if (!IsPostBack) {
          cbxAllowMultiValue.Checked = _f.AllowMultipleValues;
          txtQueryFilter.Text = (!string.IsNullOrEmpty(_f.QueryFilterAsString)) ?
            SPHttpUtility.HtmlDecode(_f.QueryFilterAsString) : string.Empty;
          cbxRecursiveFilter.Checked = _f.IsFilterRecursive;
          TargetWebId = _f.LookupWebId.ToString();
          TargetListId = _f.LookupList;
          TargetColumnId = _f.LookupField;
          TargetListViewId = _f.ListViewFilter; 
        }
      }

      // this bit must always happen, even when field is null
      if (!IsPostBack) {
        SetTargetWeb();
        lblTargetWeb.Text = listTargetWeb.SelectedItem.Text;
        lblTargetList.Text = listTargetList.SelectedItem.Text;
        SetControlVisibility();
      }
    }
    #endregion

    public void OnSaveChange(SPField field, bool isNewField) {
      FilteredLookupField _f = null;
      try { _f = field as FilteredLookupField; }
      catch { }

      if (_f != null) {
        string s = txtQueryFilter.Text;
        bool rec = cbxRecursiveFilter.Checked;
        string view = (listTargetListView.SelectedIndex > -1) ?
          listTargetListView.SelectedItem.Value : string.Empty;
        string col = listTargetColumn.SelectedItem.Value;
        string list = listTargetList.SelectedItem.Value;
        bool multi = cbxAllowMultiValue.Checked;

        if (isNewField) { // can only change list and web if new field
          SPSite _site = SPControl.GetContextSite(this.Context);
          SPWeb _web = _site.OpenWeb(new Guid(listTargetWeb.SelectedItem.Value));
              _f.LookupWebId = _web.ID;
              _f.LookupList = list;
           
        }

        if (rdFilterOption.SelectedItem.Value == "Query") {
          _f.QueryFilterAsString = (!string.IsNullOrEmpty(s)) ? SPHttpUtility.HtmlEncode(s) : "";
          _f.ListViewFilter = "";
        }
        else if (rdFilterOption.SelectedItem.Value == "ListView") {
          _f.ListViewFilter = (!string.IsNullOrEmpty(view)) ? view : "";
          _f.QueryFilterAsString = "";
        }

        _f.LookupField = col;
        _f.IsFilterRecursive = rec;
        _f.UnlimitedLengthInDocumentLibrary = cbxUnlimitedLengthInDocLib.Checked;
        _f.CountRelated = IsCountRelated(_f.LookupField, _f.LookupList);
        _f.AllowMultipleValues = (_f.CountRelated) ? false : multi;
      }
    }

    private bool IsCountRelated(string lookupColumnId, string lookupListId) {
      return false; // for now until we work out what really goes into CountRelated
    }

    #endregion
  }
}
