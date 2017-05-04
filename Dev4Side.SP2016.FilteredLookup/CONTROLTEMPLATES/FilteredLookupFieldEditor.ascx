<%@ Control Language="C#" AutoEventWireup="false" CompilationMode="Always" Inherits="Dev4Side.SP2016.FilteredLookup.FilteredLookupFieldEditor, Dev4Side.SP2016.FilteredLookup, Version=1.0.0.0, Culture=neutral, PublicKeyToken=af31a3eeecf7add1" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormControl" src="~/_controltemplates/InputFormControl.ascx" %>
<%@ Register TagPrefix="wssuc" TagName="InputFormSection" src="~/_controltemplates/InputFormSection.ascx" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<wssuc:InputFormSection runat="server" id="FilterLookupFieldSection" Title="Special Column Settings">
  <template_inputformcontrols>
    <wssuc:InputFormControl runat="server" LabelText="Specify detailed options for the filtered lookup column">
      <Template_Control>
          <div style="width: 100%; text-align: left; border-width: 0px;">
          	<script language="javascript" type="text/javascript">
          	  var bConfirmed = false;
          	  var bClicked = false;
          	  function ConfirmConvert(event) {
          	    var cbx = event.srcElement;
          	    if (cbx == null) { cbx = event.target; }
          	    if (!bClicked && cbx.checked) { bConfirmed = true; }
          	    if (!cbx.checked && !bConfirmed) {
          	      var msg = "<SharePoint:EncodedLiteral runat='server' text='<%$Resources:wss,fldedit_warn_turnoffmultilookup%>' EncodeMethod='HtmlEncode'/>";
          	      bConfirmed = confirm(msg);
          	      cbx.checked = !bConfirmed;
          	    }
          	    bClicked = true;
          	    UpdateDocLibWarning();
          	    UpdateLengthWarning();
          	  }
          	  function UpdateDocLibWarning() {
          	    var cbx = (document.getElementById("<%= cbxAllowMultiValue.ClientID %>"));
          	    var spanDocLibWarning = (document.getElementById("<%= SpanDocLibWarning.ClientID %>"));
          	    if (spanDocLibWarning != null) {
          	      if (cbx.checked) { spanDocLibWarning.style.display = ""; }
          	      else { spanDocLibWarning.style.display = "none"; }
          	    }
          	  }
          	  function UpdateLengthWarning() {
          	    var cbx = (document.getElementById("<%= cbxUnlimitedLengthInDocLib.ClientID %>"));
          	    var spanDocLibWarning = (document.getElementById("<%= SpanLengthWarning.ClientID %>"));
          	    if (spanDocLibWarning != null) {
          	      if (cbx.checked) { spanDocLibWarning.style.display = ""; }
          	      else { spanDocLibWarning.style.display = "none"; }
          	    }
          	  }
          	</script>
            <table style="width: 100%; border-width: 0px; border-collapse: collapse;" cellpadding="0" cellspacing="0">
              <tr>
                <td class="ms-authoringcontrols" style="width: 100%; text-align: left; white-space: nowrap; padding-top: 10px;">
                  <span>Get Information from this site:</span>
                </td>
              </tr>
              <tr>
                <td class="ms-authoringcontrols" style="width: 100%; text-align: left; white-space: nowrap;">
                  <asp:DropDownList runat="server" ID="listTargetWeb" AutoPostBack="true" OnSelectedIndexChanged="SelectedTargetWebChanged" />
                  <asp:Label runat="server" ID="lblTargetWeb" />
                </td>
              </tr>
              <tr>
                <td class="ms-authoringcontrols" style="width: 100%; text-align: left; white-space: nowrap; padding-top: 10px;">
                  <span>Get Information from:</span>
                </td>
              </tr>
              <tr>
                <td class="ms-authoringcontrols" style="width: 100%; text-align: left; white-space: nowrap;">
                  <asp:DropDownList runat="server" ID="listTargetList" AutoPostBack="true" OnSelectedIndexChanged="SelectedTargetListChanged" />
                  <asp:Label runat="server" ID="lblTargetList" />
                </td>
              </tr>
              <tr>
                <td class="ms-authoringcontrols" style="width: 100%; text-align: left; white-space: nowrap; padding-top: 10px;">
                  <span>In this column</span>
                </td>
              </tr>
              <tr>
                <td class="ms-authoringcontrols" style="width: 100%; text-align: left; white-space: nowrap;">
                  <asp:DropDownList runat="server" ID="listTargetColumn" />
                </td>
              </tr>
              <tr>
                <td class="ms-authoringcontrols" style="width: 100%; text-align: left; white-space: nowrap; padding-top: 10px;">
                <span style="padding-left: 0px;">
                  <asp:RadioButtonList CssClass="ms-authoringcontrols" RepeatLayout="Flow" RepeatDirection="Horizontal" runat="server" ID="rdFilterOption" OnSelectedIndexChanged="SelectedFilterOptionChanged" AutoPostBack="true">
                    <asp:ListItem Selected="True" Text="Apply Query Filter" Value="Query" />
                    <asp:ListItem Text="Apply List View Filter" Value="ListView" />
                  </asp:RadioButtonList>
                 </span>
                </td>
              </tr>
              <tr>
                <td runat="server" id="tdQuery" class="ms-authoringcontrols ms-descriptiontext ms-inputformdescription" style="width: 100%; text-align: left; white-space: nowrap;">
                <asp:TextBox runat="server" ID="txtQueryFilter" EnableViewState="true" TextMode="MultiLine" CssClass="ms-input"  Rows="3" Columns="40" />
                <br />
                <asp:CheckBox runat="server" ID="cbxRecursiveFilter" Text="<%$Resources:wss,viewedit_DontShowFolders%>" ToolTip="<%$Resources:wss,viewedit_DontShowFolders%>" />
                </td>
                <td runat="server" id="tdListView" class="ms-authoringcontrols ms-descriptiontext ms-inputformdescription" style="width: 100%; text-align: left; white-space: nowrap;">
                  <asp:DropDownList runat="server" ID="listTargetListView" />
                </td>
              </tr>
              <tr>
                <td class="ms-authoringcontrols" style="width: 100%; text-align: left; white-space: nowrap; padding-top: 10px;">
                <asp:CheckBox id="cbxAllowMultiValue" Text="<%$Resources:wss,fldedit_allowmultivalue%>" ToolTip="<%$Resources:wss,fldedit_allowmultivalue%>" 
                  onClick="ConfirmConvert(event)" runat="server" />
                </td>
              </tr>
            </table>
            <span class="ms-formvalidation" id="SpanDocLibWarning" runat="server" Visible="false">
              <br/>
              <SharePoint:EncodedLiteral ID="EncodedLiteral1" runat="server" text="<%$Resources:wss,fldedit_MultiLookupWarningForDocLibSupport%>" EncodeMethod='HtmlEncode'/>
            </span>
            <br/>
            <asp:CheckBox id="cbxUnlimitedLengthInDocLib" Text="<%$Resources:wss,fldedit_UnlimitedLengthInDocumentLibrary2%>" 
              ToolTip="<%$Resources:wss,fldedit_UnlimitedLengthInDocumentLibrary2%>" onClick="UpdateLengthWarning()" runat="server" />
            <span class="ms-formvalidation" id="SpanLengthWarning" runat="server" Visible="false">
              <br/>
              <SharePoint:EncodedLiteral ID="EncodedLiteral2" runat="server" text="<%$Resources:wss,fldedit_WarningForUnlimitedLengthInDocumentLibrar%>" EncodeMethod='HtmlEncode'/>
            </span>
          </div>
        </Template_Control>
    </wssuc:InputFormControl>
    <script language="javascript" type="text/javascript">
      UpdateDocLibWarning();
      UpdateLengthWarning();
    </script>
  </template_inputformcontrols>
</wssuc:InputFormSection>
