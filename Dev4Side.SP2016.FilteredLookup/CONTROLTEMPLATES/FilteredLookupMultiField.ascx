<%@ Control Language="C#" Debug="true" %>
<%@ Assembly Name="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="SharePoint" Assembly="Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" Namespace="Microsoft.SharePoint.WebControls" %>
<SharePoint:RenderingTemplate ID="FilteredLookupMultiFieldControl" runat="server">
  <Template>
    <span dir="none">
      <SharePoint:GroupedItemPicker ID="MultiLookupPicker" runat="server" CandidateControlId="SelectCandidate"
        ResultControlId="SelectResult" AddButtonId="AddButton" RemoveButtonId="RemoveButton" />
      <table class="ms-long" cellpadding="0" cellspacing="0" border="0">
        <tr>
          <td class="ms-input">
            <SharePoint:SPHtmlSelect Height="125" Width="143" ID="SelectCandidate" runat="server" multiple="true" title="<%$Resources:wss,fldpick_possible_flds%>" />
          </td>
          <td style="padding-left: 10px" />
          <td align="center" valign="middle" class="ms-input">
            <button class="ms-buttonheightwidth" id="AddButton" runat="server">
              <SharePoint:EncodedLiteral ID="EncodedLiteral1" runat="server" Text="<%$Resources:wss,multipages_gip_add%>"
                EncodeMethod='HtmlEncode' />
            </button>
            <br />
            <br />
            <button class="ms-buttonheightwidth" id="RemoveButton" runat="server">
              <SharePoint:EncodedLiteral ID="EncodedLiteral2" runat="server" Text="<%$Resources:wss,multipages_gip_remove%>"
                EncodeMethod='HtmlEncode' />
            </button>
          </td>
          <td style="padding-left: 10px" />
          <td class="ms-input">
            <SharePoint:SPHtmlSelect Width="143" Height="125" ID="SelectResult" runat="server" multiple="true" title="<%$Resources:wss,fldpick_selected_flds%>" />
          </td>
        </tr>
      </table>
    </span>
  </Template>
</SharePoint:RenderingTemplate>