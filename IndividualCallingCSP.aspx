<%@ Page Title="" Language="C#" MasterPageFile="~/SanjivaniBriefing/caller.Master" AutoEventWireup="true" CodeBehind="IndividualCallingCSP.aspx.cs" Inherits="RiskManagement.SanjivaniBriefing.IndividualCallingCSP" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
   <script type="text/javascript">
       function applyStickyLogic(gridId) {
           var grid = document.getElementById(gridId);
           var rows = grid.getElementsByTagName('tr'); // Use getElementsByTagName for more reliability

           for (var i = 0; i < rows.length; i++) {
               var cells = rows[i].getElementsByTagName('td');
               if (cells.length > 0) {
                   // Add sticky class to the first cell in each row
                   cells[0].classList.add(i % 2 === 0 ? 'sticky-col1' : 'sticky-col');
               }
           }
       }

       window.onload = function () {
           applyStickyLogic('<%= grdDaily.ClientID %>');
           applyStickyLogic('<%= grdMontly.ClientID %>');
       };
   </script>

   <style>
       /* General Styles */
       body {
           font-family: 'Poppins';
           margin: 0;
           padding: 0;
           background-color: #f4f7f9;
           color: #333;
       }

       /* Sticky Column Styles */
       .sticky-col,
       .sticky-col1 {
           position: -webkit-sticky; /* For Safari */
           position: sticky;
           left: 0;
           font-weight: bold;
           z-index: 1; /* Adjust as needed to ensure columns stay above other content */
           background-color: #ffffff; /* Background color to avoid transparency issues */
           box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1); /* Optional: Adds shadow for better visibility */
       }

       /* Alternating row colors for sticky columns */
       .sticky-col {
           background-color: #eaf1f9; /* Slightly lighter color */
       }

       .sticky-col1 {
           background-color: #f9f9f9; /* Another lighter color for alternating rows */
       }

       /* Scroll container */
       #scroll {
          
           -webkit-overflow-scrolling: touch; /* Smooth scrolling on iOS */
           padding: 10px; /* Padding around the grid */
          /* border: 1px solid #C0C0C0;*/
           background-color: #F0F0F0;
           position: relative;
       }

       /* Table styling */
       #FutureSchemes,
       #CallingData,
       #CallingDataHistory,
       #customers,
       #table2,
       #onlyheaders,
       #onlyheaderDaily,
       #onlyheaderMonthly,
       #onlyheaderCallingData,
       #onlyheaderCallingDataHistory,
       #onlyheaderFuture {
           font-family: 'Poppins';
           border-collapse: collapse;
           width: 100%;
       }

       /* Common table cell styling */
       #FutureSchemes td,
       #CallingData td,
       #CallingDataHistory td,
       #customers td,
       #table2 td,
       #onlyheaders th,
       #onlyheaderDaily th,
       #onlyheaderMonthly th,
       #onlyheaderCallingData th,
       #onlyheaderCallingDataHistory th,
       #onlyheaderFuture th {
           border: 1px solid #ddd;
           padding: 8px;
       }

       /* Table row styles */
       #FutureSchemes tr:nth-child(even),
       #CallingData tr:nth-child(even),
       #CallingDataHistory tr:nth-child(even),
       #customers tr:nth-child(even),
       #table2 tr:nth-child(even) {
           background-color: #f2f2f2;
       }

       #FutureSchemes tr:hover,
       #CallingData tr:hover,
       #CallingDataHistory tr:hover,
       #customers tr:hover,
       #table2 tr:hover {
           background-color: #ddd;
       }

       /* Header styling */
       #table2 th,
       #onlyheaders th,
       #onlyheaderDaily th,
       #onlyheaderMonthly th,
       #onlyheaderCallingData th,
       #onlyheaderCallingDataHistory th,
       #onlyheaderFuture th {
           padding-top: 12px;
           padding-bottom: 12px;
           text-align: left;
           background-color: #005145; /* Dark Green */
           color: white;
       }

       /* Scrollable Container */
       div#scroll {
          /* border: 1px solid #ddd;*/
           background-color: #ffffff;
           width: 100%;
           overflow: auto;
           position: relative;
         /*  border-radius: 8px;
           box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);*/
       }

       /* Image Background for Button */
       .RightArrow {
           background-image: url('~/img/phone.png');
           background-size: cover;
           background-position: center;
           width: 24px;
           height: 24px;
           display: inline-block;
           transition: background-image 0.3s ease;
       }

       .RightArrow:hover {
           background-image: url('~/img/phoneinprogress.png');
       }

       /* Responsive Adjustments */
       @media (max-width: 768px) {
           #sidebar {
               width: 100%;
               height: auto;
               position: relative;
               border-right: none;
           }

           #main {
               margin-left: 0;
           }

           table {
               font-size: 14px;
           }
       }
       .phone-icon i {
    font-size: 20px;
    color: #013220; /* Change color as needed */
    cursor: pointer;
}

   </style>
        <style>
        .btn-custom {
    background-color: #ff5733; /* Eye-catching orange */
    color: white;
    font-size: 16px;
    font-weight: bold;
    padding: 10px 20px;
    border-radius: 5px;
    border: none;
    box-shadow: 0px 4px 8px rgba(0, 0, 0, 0.2);
    transition: background-color 0.3s ease;
}

.btn-custom:hover {
    background-color: #e14b2e; /* Darken on hover */
    cursor: pointer;
}
   </style>
</asp:Content>

    <asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
                <asp:ScriptManager runat="server"></asp:ScriptManager>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div id="scroll">
                        <table style="width: 100%;" id="onlyheaders">
                            <tr>
                                <th style="width: 110px; text-align: center;">CSP Photo
                                </th>
                                <th>CSP Details</th>
                                <%--<th>
                                    <asp:Label ID="Label1" runat="server" Text="Label"></asp:Label></th>--%>
                            </tr>

                            <tr>
                                <td style="width: 110px; text-align: center;">
                                    <asp:Image ID="imgCSP" runat="server" Height="100px" Width="100px" ImageUrl="#" ImageAlign="Middle" /></td>
                                <td>
                                    <table style="width: 100%;" id="customers">
                                        <tr>
                                            <td>CSP Code</td>
                                            <td colspan="5">
                                                <asp:Label ID="lblCode" runat="server"></asp:Label>
                                            </td>
                                            <td>Supervisor Name </td>
                                            <td colspan="2">
                                                <asp:Label ID="lblSupervisorName" runat="server"></asp:Label>
                                            </td>
                                            <td>
                                                <asp:Label ID="lblSupervisorMobile" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:LinkButton ID="lnkSupervisor" runat="server" OnClick="lnkSupervisor_Click" CssClass="phone-icon">
                                                            <i class="fa-solid fa-phone"></i>
                                                 </asp:LinkButton>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>CSP Name</td>
                                            <td colspan="2">
                                                <asp:Label ID="lblName" runat="server" Font-Bold="True"></asp:Label>
                                                <asp:TextBox ID="txtCSPName" runat="server" Visible="false"></asp:TextBox>
                                            </td>
                                            <td>
                                                 <asp:LinkButton ID="btnEditName" runat="server" OnClick="btnEditName_Click" >
                                                         <i class="fa-solid fa-pen-to-square" style="color: #201f23; font-size:30px"></i>
                                                  </asp:LinkButton>
                                            </td>
                                            <td>
                                                <asp:LinkButton ID="btnSaveName" runat="server" OnClick="btnSaveName_Click" Visible="false" >
                                                            <i class="fa-solid fa-floppy-disk"  style="color: #201f23; font-size:30px"></i>
                                                </asp:LinkButton>
                                            </td>
                                            <td>&nbsp;</td>
                                            <td>DC Name</td>
                                            <td colspan="2">
                                                <asp:Label ID="lblDCName" runat="server"></asp:Label>
                                            </td>
                                            <td>
                                                <asp:Label ID="lblDCMobileNo" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:LinkButton ID="imgDc" runat="server" OnClick="imgDc_Click" CssClass="phone-icon">
                                                      <i class="fa-solid fa-phone"></i>
                                                </asp:LinkButton>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Mobile No</td>
                                            <td>
                                                <asp:TextBox ID="hypMobileNo" runat="server" Enabled="False" ReadOnly="True"></asp:TextBox><asp:HyperLink ID="hypcall" runat="server" NavigateUrl="tel:09397006421" ImageUrl="~/img/phone.png" Visible="false"></asp:HyperLink>
                                            </td>
                                            <td>
                                                <%--   <asp:ImageButton  ID="imgCall"   OnMouseOver="src='phoneinprogress.png';"
                                                      OnMouseOut="src='phone.png';" runat="server" ImageUrl="~/img/phone.png" OnClick="imgCall_Click" />--%>

                                                   <asp:LinkButton ID="btnsipcall" runat="server" OnClick="btnsipcall_Click" OnClientClick="sanjivaniOnCallClick();" CssClass="phone-icon">
                                                        <i class="fa-solid fa-phone"></i>
                                                  </asp:LinkButton>
                                            </td>
                                            <td>
                                                <asp:LinkButton ID="editPhoneNum" runat="server" OnClick="editPhoneNum_Click" >
                                                        <i class="fa-solid fa-pen-to-square" style="color: #201f23; font-size:30px"></i>
                                                </asp:LinkButton>
                                            </td>
                                            <td>
                                                 <asp:LinkButton ID="btnPhoneSave" runat="server" OnClick="btnPhoneSave_Click" Visible="false" >
                                                       <i class="fa-solid fa-floppy-disk"  style="color: #201f23; font-size:30px"></i>
                                                  </asp:LinkButton>
                                            </td>
                                            <td>
                                                <%-- <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="tel:09397006421@192.168.2.150">HyperLink</asp:HyperLink>--%>
                                                <asp:ImageButton ID="imgCall" runat="server" ImageUrl="~/img/phone.png" OnClick="imgCall_Click" Style="display:none" />
                                            </td>
                                            <td class="auto-style1">Branch</td>
                                            <td colspan="2">
                                                <asp:Label ID="lblBranch" runat="server" Font-Bold="True"></asp:Label>
                                            </td>
                                            <td>Reset CSP Password</td>
                                            <td><asp:Button ID="btnReset"  CssClass="btn-custom" runat="server" Text="Reset Password" OnClick="btnReset_Click" /></td>
                                        </tr>
                                        <tr>
                                            <td>Alternate Number</td>
                                            <td>
                                                <asp:Label ID="lblAlternate" runat="server"></asp:Label>
                                            </td>
                                            <td>
                                              
                                            </td>
                                            <td colspan="2">
                                                <asp:Label ID="lblAlternate1" runat="server"></asp:Label>
                                            </td>
                                            <td>
                                                <asp:ImageButton ID="imgalternate1" runat="server" ImageUrl="~/img/phone.png" OnClick="imgalternate1_Click" Style="display:none" />
                                            </td>
                                            <td class="auto-style1">District &amp; State</td>
                                            <td>
                                                <asp:Label ID="lblDistrict" runat="server"></asp:Label>
                                            </td>
                                            <td>
                                                <asp:Label ID="lblState" runat="server"></asp:Label>
                                            </td>
                                            <td></td>
                                            <td></td>
                                        </tr>
                                    </table>
                                </td>
                                <td>
                                    <table style="width: 100%;">
                                        <tr runat="server" visible="false" id="tr1">
                                            <td>
                                                <asp:Label ID="lblstaff1" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:Label ID="lblMobileStaff1" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:ImageButton ID="imgstaff1" runat="server" ImageUrl="~/img/phone.png" OnClick="imgstaff1_Click" />
                                            </td>
                                        </tr>
                                        <tr runat="server" visible="false" id="tr2">
                                            <td>
                                                <asp:Label ID="lblstaff2" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:Label ID="lblMobileStaff2" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:ImageButton ID="imgstaff2" runat="server" ImageUrl="~/img/phone.png" OnClick="imgstaff1_Click" />
                                            </td>
                                        </tr>
                                        <tr runat="server" visible="false" id="tr3">
                                            <td>
                                                <asp:Label ID="lblstaff3" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:Label ID="lblMobileStaff3" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:ImageButton ID="imgstaff3" runat="server" ImageUrl="~/img/phone.png" OnClick="imgstaff1_Click" />
                                            </td>
                                        </tr>
                                        <tr runat="server" visible="false" id="tr4">
                                            <td>
                                                <asp:Label ID="lblstaff4" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:Label ID="lblMobileStaff4" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:ImageButton ID="imgstaff4" runat="server" ImageUrl="~/img/phone.png" OnClick="imgstaff1_Click" />
                                            </td>
                                        </tr>
                                        <tr runat="server" visible="false" id="tr5">
                                            <td>
                                                <asp:Label ID="lblstaff5" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:Label ID="lblMobileStaff5" runat="server"></asp:Label></td>
                                            <td>
                                                <asp:ImageButton ID="imgstaff5" runat="server" ImageUrl="~/img/phone.png" OnClick="imgstaff1_Click" />
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>

                        </table>

                    </div>

                    <div id="scroll">
                        <table style="width: 100%;" id="table2">
                            <tr>
                                <th>Total Account(as on 31.03.2022)</th>
                                <th>Zero Balance</th>
                                <th>Avg Balance</th>

                                <th>Micro ATM</th>
                                <th>Passbook Printer</th>
                                <th>No of Computer Terminal</th>
                                <th>IIBF</th>
                                <th>DRA</th>
                                <th>LAM</th>
                                <th>Sub KO-IIBF</th>
                                <th>Sub KO-DRA</th>
                                <th>Sanjivani Group App Status</th>
                            </tr>
                            <tr>
                                <td>
                                    <asp:Label ID="lblTotalAccount" runat="server"></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblZeroBalance" runat="server"></asp:Label>
                                </td>
                                <td>
                                    <asp:Label ID="lblAvgBalance" runat="server"></asp:Label>
                                </td>

                                <td>
                                    <asp:CheckBox ID="chkMicroATM" runat="server" />
                                </td>
                                <td>
                                    <asp:CheckBox ID="chkPassbookPrinter" runat="server" />
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlMultipleTerminal" runat="server">
                                        <asp:ListItem Value="1">T - 1 (Main)</asp:ListItem>
                                        <asp:ListItem Value="2">T - 2</asp:ListItem>
                                        <asp:ListItem Value="3">T - 3</asp:ListItem>
                                        <asp:ListItem Value="4">T - 4</asp:ListItem>
                                        <asp:ListItem Value="5">T - 5</asp:ListItem>
                                        <asp:ListItem Value="6">T - 6</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                                <td>
                                    <asp:CheckBox ID="chkIIBF" runat="server" />
                                </td>
                                <td>
                                    <asp:CheckBox ID="chkDRA" runat="server" />
                                </td>
                                <td>
                                    <asp:CheckBox ID="chlLam" runat="server" />
                                </td>
                                <td>
                                    <asp:CheckBox ID="chkiibfsq" runat="server" />
                                </td>
                                <td>
                                    <asp:CheckBox ID="chkdrasq" runat="server" />
                                </td>

                                    <td>
        <asp:CheckBox ID="appStatus" runat="server" />
        <br />
        <asp:Label ID="lblLoginDateTime" 
                   runat="server" 
                   ForeColor="Green" 
                   Font-Italic="true" 
                   Font-Size="Smaller">
        </asp:Label>
    </td>


                            </tr>
                        </table>
                    </div>

                    <div style="overflow-x: auto; width: 100%;">
                        <table style="width: 100%;" id="onlyheaderDaily">
                            <tr>
                                <th>Daily Transaction for last 30 Days</th>
                            </tr>
                            <tr>
                                <td>
                                    <asp:GridView ID="grdDaily" runat="server" BackColor="White" BorderColor="#999999" BorderStyle="None" BorderWidth="1px" CellPadding="3" GridLines="Vertical" Width="100%" OnRowDataBound="grid_RowDataBound">
                                        <AlternatingRowStyle BackColor="#DCDCDC" />
                                        <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
                                        <HeaderStyle BackColor="#000084" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#EEEEEE" ForeColor="Black" />
                                        <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
                                        <SortedAscendingCellStyle BackColor="#F1F1F1" />
                                        <SortedAscendingHeaderStyle BackColor="#0000A9" />
                                        <SortedDescendingCellStyle BackColor="#CAC9C9" />
                                        <SortedDescendingHeaderStyle BackColor="#000065" />
                                    </asp:GridView>
                                </td>
                            </tr>
                        </table>

                    </div>
                    <div style="overflow-x: auto; width: 100%;">
                        <table style="width: 100%;" id="onlyheaderMonthly">
                            <tr>
                                <th>Monthly Transaction</th>
                            </tr>
                            <tr>
                                <td>
                                    <asp:GridView ID="grdMontly" runat="server" BackColor="White" BorderColor="#999999" BorderStyle="None" BorderWidth="1px" CellPadding="3" GridLines="Vertical" Width="100%" OnRowDataBound="grid_RowDataBound">
                                        <AlternatingRowStyle BackColor="#DCDCDC" />
                                        <FooterStyle BackColor="#CCCCCC" ForeColor="Black" />
                                        <HeaderStyle BackColor="#000084" Font-Bold="True" ForeColor="White" />
                                        <PagerStyle BackColor="#999999" ForeColor="Black" HorizontalAlign="Center" />
                                        <RowStyle BackColor="#EEEEEE" ForeColor="Black" />
                                        <SelectedRowStyle BackColor="#008A8C" Font-Bold="True" ForeColor="White" />
                                        <SortedAscendingCellStyle BackColor="#F1F1F1" />
                                        <SortedAscendingHeaderStyle BackColor="#0000A9" />
                                        <SortedDescendingCellStyle BackColor="#CAC9C9" />
                                        <SortedDescendingHeaderStyle BackColor="#000065" />
                                    </asp:GridView>
                                </td>
                            </tr>
                        </table>


                    </div>

                    <div id="scroll">
                        <table style="width: 100%;">
                            <tr>
                                <td style="vertical-align: top; width: 50%;">
                                    <table style="width: 100%;" id="onlyheaderCallingData">
                                        <tr>
                                            <th>Calling Data</th>
                                        </tr>
                                        <tr>
                                            <td>
                                                <table style="width: 100%;" id="CallingData">
                                                    <tr>
                                                        <td>
                                                            <asp:RadioButton ID="rbConnected" runat="server" Checked="True" GroupName="CallConnect" Text="Call Connected" AutoPostBack="True" OnCheckedChanged="rbConnected_CheckedChanged" />
                                                        </td>
                                                        <td>
                                                            <asp:RadioButton ID="rbFailed" runat="server" GroupName="CallConnect" Text="Call Failed" AutoPostBack="True" OnCheckedChanged="rbConnected_CheckedChanged" />
                                                        </td>
                                                        <td>
                                                            <asp:DropDownList ID="ddlFailed" runat="server" Visible="False">
                                                                <asp:ListItem>Not Picked - Full Ring</asp:ListItem>
                                                                <asp:ListItem>Not Picked - Cut Before Full Ring</asp:ListItem>
                                                                <asp:ListItem>Switched Off</asp:ListItem>
                                                                <asp:ListItem>Out of Coverage Area</asp:ListItem>
                                                                <asp:ListItem>Phone is Busy</asp:ListItem>
                                                                <asp:ListItem>Call Back Later</asp:ListItem>
                                                                <asp:ListItem>Incoming is not allowed</asp:ListItem>
                                                                <asp:ListItem>Number does not exist</asp:ListItem>
                                                                <asp:ListItem>Phone not available</asp:ListItem>
                                                            </asp:DropDownList>
                                                            <asp:DropDownList ID="ddlConnected" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlConnected_SelectedIndexChanged">
                                                                <asp:ListItem>OK</asp:ListItem>
                                                                <asp:ListItem>Code Closed</asp:ListItem>
                                                                <asp:ListItem>Replacement</asp:ListItem>
                                                                <asp:ListItem>Problem Reported</asp:ListItem>
                                                                <asp:ListItem>Target Given</asp:ListItem>
                                                            </asp:DropDownList>
                                                        </td>

                                                    </tr>
                                                    <tr>
                                                        <td>Remarks</td>
                                                        <td colspan="2">
                                                            <asp:TextBox ID="txtRemarks" runat="server" Width="100%" Height="50px" MaxLength="240" TextMode="MultiLine"></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td style="vertical-align: top; width: 50%;">
                                    <table style="width: 100%;" id="onlyheaderCallingDataHistory">
                                        <tr>
                                            <th>Calling History</th>
                                        </tr>
                                        <tr>
                                            <td>
                                                <table style="width: 100%;" id="nocss">
                                                    <tr>
                                                        <td>
                                                            <asp:GridView ID="grdcallingHistory" runat="server" Width="100%" CellPadding="4" ForeColor="#333333"  ShowHeader="false" AutoGenerateColumns="false">

                                                                <AlternatingRowStyle BackColor="White" />
                                                                <Columns>
                                                                    <asp:TemplateField HeaderText="Date" ItemStyle-Width="100px">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="lblDate" runat="server"
                                                                                Text='<%# Eval("Date") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>

                                                                    <asp:TemplateField HeaderText="Conn" ItemStyle-Width="18px">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="lblConn" runat="server"
                                                                                Text='<%# Eval("Connected") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="Remarks">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="lblRemarks" runat="server"
                                                                                Text='<%# Eval("Remarks") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="Reason" ItemStyle-Width="100px">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="lblReason" runat="server"
                                                                                Text='<%# Eval("Reason") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="Caller" ItemStyle-Width="80px">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="lblCaller" runat="server"
                                                                                Text='<%# Eval("Caller") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                </Columns>
                                                                <EditRowStyle BackColor="#7C6F57" />
                                                                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                                                <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
                                                                <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
                                                                <RowStyle BackColor="#E3EAEB" />
                                                                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
                                                                <SortedAscendingCellStyle BackColor="#F8FAFA" />
                                                                <SortedAscendingHeaderStyle BackColor="#246B61" />
                                                                <SortedDescendingCellStyle BackColor="#D4DFE1" />
                                                                <SortedDescendingHeaderStyle BackColor="#15524A" />
                                                            </asp:GridView>
                                                        </td>
                                                    </tr>
                                                </table>

                                            </td>
                                        </tr>
                                    </table>
                                </td>

                            </tr>

                        </table>


                    </div>

                   <div id="scroll">
                      <table style="width: 100%;" id="onlyheaderFuture" runat="server" visible="false">
                        <tr>
                          <th>Future Commitments of Schemes</th>
                       </tr>
                       <tr>
                        <td>
                           <table style="width: 100%;" id="FutureSchemes">
                           <tr>
                             <td>APY</td>
                             <td>
                                 <asp:TextBox ID="txtAPY" runat="server"></asp:TextBox>
                             </td>

                         </tr>
                         <tr>
                             <td>PMJJBY</td>
                             <td>
                                 <asp:TextBox ID="txtPMJJBY" runat="server"></asp:TextBox>
                             </td>

                         </tr>
                         <tr>
                             <td class="auto-style1">PMSBY</td>
                             <td class="auto-style1">
                                 <asp:TextBox ID="txtPMSBY" runat="server"></asp:TextBox>
                             </td>

                         </tr>


                         <tr>
                             <td>No. of Transaction</td>
                             <td>
                                 <asp:TextBox ID="txtnooftransaction" runat="server"></asp:TextBox>
                             </td>
                         </tr>

                         <tr>
                             <td>Will Start Login (input date)</td>
                             <td>
                                 <asp:TextBox ID="txtlogindate" runat="server"></asp:TextBox>
                             </td>

                         </tr>

                     </table>
                 </td>
             </tr>
         </table>
     </div>


                    <div id="scroll">
                        <table style="width: 100%;">
                            <tr>

                                <td style="padding: 10px; text-align: center; background-color: #CCFFFF;">
                                    <asp:Button ID="btnSave" runat="server" Text="Continue Next Call" Width="50%" OnClick="btnSave_Click" Font-Bold="True" Font-Size="Larger" UseSubmitBehavior="false" OnClientClick="this.disabled='true';" />&nbsp;</td>
                                <td style="text-align: center; background-color: #CCFFFF; padding: 10px;">
                                    <asp:Button ID="btnBreak" runat="server" OnClick="btnBreak_Click" Text="Take a Break" Width="50%" Font-Size="Larger" BorderStyle="None" />
                                </td>
                            </tr>

                        </table>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
            <asp:UpdateProgress ID="UpdWaitImage" runat="server" DynamicLayout="true" AssociatedUpdatePanelID="UpdatePanel1">
                <ProgressTemplate>
                    <div class="modal">
                        <div class="center">
                            <strong>Initiating Call. Please wait.....<br />
                            </strong>
                            <img alt="" src="loader.gif" />
                        </div>
                    </div>
                </ProgressTemplate>
            </asp:UpdateProgress>

            <%-- Sanjivani auto-call message panel: appears when the agent clicks the SIP call
                 button. The agent presses PLAY when the customer answers; the recorded
                 message (time-aware IST greeting + intro + AnyDesk request) plays in the
                 browser. Route the browser's audio into MicroSIP's microphone via a virtual
                 audio cable so the CUSTOMER hears it first. The audio/ folder sits at the
                 web app root in this repo. --%>
            <div id="sanjivaniMsgPanel" style="display:none; position:fixed; bottom:18px; right:18px; z-index:9999; background:#ffffff; border:2px solid #005145; border-radius:12px; padding:16px; width:320px; box-shadow:0 4px 16px rgba(0,0,0,0.25); font-family:'Poppins';">
                <div style="font-weight:bold; font-size:16px; margin-bottom:6px;">&#128266; Sanjivani message</div>
                <div id="sanjivaniMsgStatus" style="font-size:13px; color:#555; margin-bottom:10px;">MicroSIP me call lag rahi hai...</div>
                <button type="button" id="btnPlaySanjivani" onclick="playSanjivaniMessage()" class="btn-custom" style="width:100%;">&#9654; Play message to customer</button>
                <div style="font-size:11px; color:#888; margin-top:8px;">Pehle customer yehi sunega, phir tum baat karna.</div>
                <audio id="sanjivaniAudio" preload="auto" style="display:none;"></audio>
            </div>
            <script type="text/javascript">
                var sanjivaniAudioBase = '<%= ResolveUrl("~/audio/") %>';
                function sanjivaniOnCallClick() {
                    var p = document.getElementById('sanjivaniMsgPanel');
                    if (p) p.style.display = 'block';
                    var s = document.getElementById('sanjivaniMsgStatus');
                    if (s) s.innerText = 'MicroSIP me call lag rahi hai... customer ke uthate hi PLAY dabao.';
                }
                function sanjivaniIstHour() {
                    var now = new Date();
                    return new Date(now.getTime() + (now.getTimezoneOffset() + 330) * 60000).getHours();
                }
                function playSanjivaniMessage() {
                    var h = sanjivaniIstHour();
                    var greet = (h >= 5 && h < 12) ? '01_greet_morning.mp3'
                              : (h < 17 ? '02_greet_afternoon.mp3' : '03_greet_evening.mp3');
                    var playlist = [sanjivaniAudioBase + greet,
                                    sanjivaniAudioBase + '04_intro.mp3',
                                    sanjivaniAudioBase + '05_anydesk_prompt.mp3'];
                    var audio = document.getElementById('sanjivaniAudio');
                    var status = document.getElementById('sanjivaniMsgStatus');
                    var btn = document.getElementById('btnPlaySanjivani');
                    var idx = 0;
                    if (btn) btn.disabled = true;
                    status.innerText = 'Baj raha hai... customer sun raha hai.';
                    audio.onended = function () {
                        idx++;
                        if (idx < playlist.length) { audio.src = playlist[idx]; audio.play(); }
                        else {
                            status.innerText = 'Message poora baj gaya. Ab tum baat kar sakte ho.';
                            if (btn) btn.disabled = false;
                        }
                    };
                    audio.onerror = function () {
                        status.innerText = 'Audio file nahi mili: ' + playlist[idx];
                        if (btn) btn.disabled = false;
                    };
                    audio.src = playlist[0];
                    var pr = audio.play();
                    if (pr && pr.catch) pr.catch(function () {
                        status.innerText = 'Play block hua — dobara PLAY dabao.';
                        if (btn) btn.disabled = false;
                    });
                }
            </script>
     
    </asp:Content>
