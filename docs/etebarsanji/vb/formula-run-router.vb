<DisplayName("اجراي فرمول")> Public Function Run() As clsOut
    Try    
' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", "T")
' Exit Function
      ' Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NidProc",Info8.GetParvandeh_Apartment().NidProc.ToString) : Exit Function
    
'   If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
'       Info8.AddError(BIZ.SA.EumErrorAction.Stop, "FormName", Parameter.FormName) : Exit Function
'   End If   
     
      
     if Parameter.FormName = "Agreement_ListUGP" Then
          Call Edite_Fani
     End if
    HarimAmniyati=False
    BIZ.SA.ClsCommon.UpdateFormulaLog(Guid.Parse(Parameter.ThisNidFormulaLog), BIZ.SA.ClsEnum.EumFormulaLogType.FirstOfFormul)
    TmpArchiveGroup=0
		   ' توجه	DutyExemption	Stop
		   'توجه	RevisitAgent	Stop
		 '  Info8.AddError(BIZ.SA.EumErrorAction.warning,"Commision",Info8.getrequest.info.nidworkflowdeff.tostring) 
		'     if  ucase(Info8.User.UserGuid.tostring)=ucase("ca742134-44bb-4ba6-b0c4-e5e186600a0a") or ucase(Info8.User.UserGuid.tostring)=ucase("CAC7CB59-B3C8-4E7D-9488-5F94BAE1CF5C")  Then UserTest=True
		'توجه	EditRequest	Stop
		'توجه	CommissionDraft	Stop
		'توجه	RevisitAgent	Stop
		'(10:05:24 AM) امکانی فراهم شود تا در فرمو ل ها , guid  کاربر ایجاد کننده درخواست و گروه آن پاس داده شود
		      
		         'if Info8.User.UserGuid.tostring =  "6f99c240-c92b-4fb3-99db-6760750167eb"  Then 
		         '	if isdbnull(Info8.GetParvandeh_House.GetBase_HouseInfo.DocArea) Then Info8.GetParvandeh_House.GetBase_HouseInfo.DocArea=0
		         '	Msgbox ("AAAA22" , "test")
		         '	Exit Function 
		         'End if 
		     '   a	MokatebatConfirm	Warning
		   '  Info8.AddError(BIZ.SA.EumErrorAction.warning,"aaaaa",Parameter.FormName) 
        Dim UserIdGroup As Integer = 0
		if Parameter.FormName = "EditRequest" Then
			Try
                tmpStr = Info8.GetRequest.Info.NidProc.tostring
            Catch ex as exception
                tmpStr = ""
            End Try
			Call EditRequest()
		ElseIf Parameter.FormName = "MovafeghatOsooli" Then
			Try
                tmpStr = Info8.GetMovafeghatOsooli.NidProc.tostring
            Catch ex As Exception
                tmpStr = ""
            End Try
			Call MovafeghatOsooli()
		ElseIf Parameter.FormName ="MokatebatConfirm" Then
			Call MokatebatConfirm()
        ElseIf Parameter.FormName = "AssignRevisit" Then
        	Try
                tmpStr = Info8.GetRequest.Info.NidProc.ToString
            Catch ex As Exception
                tmpStr = ""
            End Try
            Call AssignRevisit()
        ElseIf Parameter.FormName = "RevisitAgent" Then
            Call RevisitAgent()
        ElseIf Parameter.FormName = "OfficesInquiry" Then
            Call OfficesInquiry()
        ElseIf Parameter.FormName = "AllDiscount" Then
            Call AllDisCount()
		 ElseIf Parameter.FormName = "EditAllDiscount" Then
            Call AllDisCount()
        ElseIf Parameter.FormName = "Patrolman_Request" Then
            '	if Parameter.Warning.Warning.CI_WarningType <> 1 Then
            '		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"توجه","مجاز به ثبت درخواست نمی باشید")
            'End i
        ElseIf Parameter.FormName = "Fine" Then
            Try
                tmpStr = Info8.GetFine.GetCommission_Fine.NidProc.tostring
            Catch ex As Exception
                tmpStr = ""
            End Try
            Call FrmFine()
        ElseIf Parameter.FormName = "DutyExemption" Then
            Call FrmDutyExemption()
        ElseIf Parameter.FormName = "Zabeteh" Then
            Try
                tmpStr = Info8.Zabeteh.NidProc.ToString
            Catch ex As Exception
                tmpStr = ""
            End Try
            Call FrmZabeteh()
        ElseIf Parameter.FormName = "Patrolman_Warning" Then
            'if Parameter.Warning.Revisit.RevisitDate > BIZ.SA.ClsCommon.CurrentShamsiDateString Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"توجه","تاریخ بازدید باید از تاریخ روز کمتر باشد")
            'if parameter.Warning.Warning.CI_WarningType=2 Then 
            '	Dim tt As List(Of DtoSH.SelWarningListResult)
            '	Dim WarningTypeChek As Integer = 0
            '	Dim kk As Integer = 0
            '	tt = parameter.Warning.GetAllWarningList("", "", "","", 0, 0, parameter.Warning.Warning.District,  parameter.Warning.Warning.Region,  parameter.Warning.Warning.Block,  parameter.Warning.Warning.House,  parameter.Warning.Warning.Building,  parameter.Warning.Warning.Apartment,  parameter.Warning.Warning.Shop, 0, 100)
            '	if tt.count>0 Then
            '		For kk = 0 To tt.Count
            '    		If tt(kk).CI_WarningType = 1 Then WarningTypeChek = 1
            '		Next
            '	End if	
            '	If WarningTypeChek = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"توجه","باید یک اخطار اصلی داشته باشد ")
            'End if	

        ElseIf Parameter.FormName = "MapControl" Then
            Call FrmMapControl()
            'msgbox("نام فرم  ",Info8.GetMapControl.NidLib.RRequest.NidWorkflowDeff.tostring)
        ElseIf Parameter.FormName = "Parvandeh_Apartment" Then
            	Call FrmApartment()         ' آپارتمان - پرونده
          
            Dim tmpBase_AddressPostCode As New List(Of DtoSC.Base_AddressPostCode)
            tmpBase_AddressPostCode = Info8.GetParvandeh_Apartment.Base_AddressPostCode()
            
            If tmpBase_AddressPostCode.Count() > 1 Then
                Info8.AddError(BIZ.SA.EumErrorAction.Stop, "کد پستی", "تنها یک کد پستی وارد کنید")
            End If
            
            Dim tmpBaseUsing As List(Of DtoSC.Base_Using) = Info8.GetParvandeh_Apartment.GetBase_Using()
            tmpBaseUsing = tmpBaseUsing.where(Function(f) f.CI_BuildingStep <> 1).tolist()
 
            If tmpBaseUsing.count > 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "کاربر محترم :لطفا مرحله ساختمانی را قابل بهره برداري انتخاب نمایید .")

        ElseIf Parameter.FormName = "Parvandeh_HouseSharing" Then   'پرونده دستگاه 
            Call FrmHouseSharing()

        ElseIf Parameter.FormName = "Parvandeh_Building" Then  'پرونده ساختمان 
            Call FrmBuilding()

        ElseIf Parameter.FormName = "Parvandeh_House" Then 'ملک پرونده 
            Call FrmHouse()

        ElseIf Parameter.FormName = "Parvandeh_Shop" Then  ' پرونده صنف
            Call FrmShop()

        ElseIf Parameter.FormName = "Parvandeh_Job" Then
            Call FrmParvandehJob()   ' پرونده - شغل 

        ElseIf Parameter.FormName = "Revisit_House" Then
            Try
           
                tmpStr = Info8.GetRequest.info.NidProc.tostring
            Catch ex As Exception
                tmpStr = ""
                                
            End Try
            Call FrmRevisitHouse()   ' ملک - بازدید	
           
        ElseIf Parameter.FormName = "Revisit_Building" Then
            Try
                tmpStr = Info8.GetRequest.info.NidProc.tostring 'Info8.Revisit_Building.GetSh_RequestInfo.NidProc.tostring

            Catch ex As Exception
                tmpStr = ""
               
            End Try
            Call FrmRevisitBuilding()   ' ساختمان - بازدید
        ElseIf Parameter.FormName = "Revisit_HouseSharing" Then
            Try
                tmpStr = Info8.GetRequest.info.NidProc.tostring 'Info8.Revisit_Building.GetSh_RequestInfo.NidProc.tostring

            Catch ex As Exception
                tmpStr = ""
            End Try
            Call frmRevisitHouseSharing()   ' دستگاه - بازدید


        ElseIf Parameter.FormName = "Revisit_Apartment" Then
            Try
                tmpStr = Info8.GetRequest.info.NidProc.tostring 'Info8.Revisit_Building.GetSh_RequestInfo.NidProc.tostring 'Info8.GetRevisit_Apartment.GetSh_RequestInfo.NidProc.tostring

            Catch ex As Exception
                tmpStr = ""
                                
            End Try
            Call FrmRevisitApartment()  ' آپارتمان - بازدید

        ElseIf Parameter.FormName = "Revisit_Shop" Then

            Call FrmRevisitShop()  'بازدید صنف

        ElseIf Parameter.FormName = "Barokaf" Then
            Try
                tmpStr = Info8.GetBarokaf.NidLib.RRequest.NidProc.ToString
            Catch ex As Exception
                tmpStr = ""
            End Try
            Call FrmSh_Barokaf()       ' بروکف
        ElseIf Parameter.FormName = "Request" Then
            Call FrmSh_Request()        ' درخواست
        ElseIf Parameter.FormName = "Parvaneh" Then
            Try
                tmpStr = Info8.GetParvaneh.NidLib.RRequest.NidProc.ToString
            Catch ex As Exception
                tmpStr = ""
            End Try
            Call frmsh_parvaneh()        ' پروانه   
        ElseIf Parameter.FormName = "MissFoul" Then
            Try
                tmpStr  = Info8.GetMissFoul.NidLib.RRequest.NidProc.ToString
            Catch ex As Exception
                tmpStr = ""
            End Try
            Call frmsh_adamkhalaf()        ' عدم خلاف  
        ElseIf Parameter.FormName = "Payankar" Then
            Try
                tmpStr = Info8.GetPayankar.NidLib.RRequest.NidProc.ToString
            Catch ex As Exception
                tmpStr = ""
            End Try
            Call frmsh_payankar()        ' پایانکار   
        ElseIf Parameter.FormName = "InquiryResponse" Then
            Try
                tmpStr = Info8.GetInquiryResponse.NidLib.RRequest.NidProc.ToString   'Info8.GetInquiryResponse.GetSh_RequestInfo.NidProc.ToString
            Catch ex As Exception
                tmpStr = ""
            End Try
            Call frmInquiryResponse()        ' پاسخ استعلام     
        ElseIf Parameter.FormName = "AnalysisBuilding_1" Or Parameter.FormName = "AnalysisBuilding_2" Or Parameter.FormName = "AnalysisBuilding_3" Then
            Call FrmAnalysisBuilding()
        ElseIf Parameter.FormName = "ManagerConfirm" Then
			Try
			   	tmpStr =Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc.tostring	
			Catch ex As Exception
				tmpStr = ""
            End Try  
           Call ManagerConfirm()
        ElseIf Parameter.FormName = "Parvandeh_House" Then
            Dim tmpBase_AddressPostCode As New List(Of DtoSC.Base_AddressPostCode)
            tmpBase_AddressPostCode = Info8.GetParvandeh_House.Base_AddressPostCode()
            If tmpBase_AddressPostCode.Count() > 1 Then
                Info8.AddError(BIZ.SA.EumErrorAction.Stop, "کد پستی", "تنها یک کد پستی وارد کنید")
            End If
        ElseIf Parameter.FormName = "Peace_List" Then
            Call FrmPeace()
        ElseIf Parameter.FormName = "Agreement_List" Then  'Agreement
           Call FrmAgreement()
        ElseIf Parameter.FormName = "Commission" Then
            Call Commission()
        ElseIf Parameter.FormName = "Revisit_Job" Then
            Call FnRevisit_Job()
        ElseIf Parameter.FormName = "Income" or Parameter.FormName ="IncomeFromMenu" Then
            Call FnIncome()
        ElseIf Parameter.FormName = "ShenasnameFani" Then
        	Try
			   	tmpStr = Info8.ShenasnameFani._NidProc.ToString()
        	Catch ex As Exception
				tmpStr = ""
            End Try 
            Call FnShenasnameFani()
        ElseIf Parameter.FormName = "Revisit_HouseParvande" Then 
        	Try
                tmpStr = Info8.GetRequest.info.NidProc.tostring
            Catch ex As Exception
                tmpStr = ""
            End Try
			Call FnRevisitHouseParvande()
		 ElseIf Parameter.FormName = "Revisit_Apartment_Parvandeh" Then 
        	Try
                tmpStr = Info8.GetRequest.info.NidProc.tostring
            Catch ex As Exception
                tmpStr = ""
            End Try
			Call FnRevisitApartmentParvande()	
		 ElseIf Parameter.FormName = "ParkingStaff" Then 
			Call FnParkingStaff()	
		ElseIf Parameter.FormName = "Elevator" Then 
			Call Elevator			
		ElseIf Parameter.FormName = "RequestUGP" Then 
			Call RequestUGP	
		ElseIf Parameter.FormName = "MovafeghatOsooliUGP" Then
          Call Edite_Fani
        End If
        
 
        BIZ.SA.ClsCommon.UpdateFormulaLog(Guid.Parse(Parameter.ThisNidFormulaLog), BIZ.SA.ClsEnum.EumFormulaLogType.EndOfFormul)
        
'   If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
  '     Info8.AddError(BIZ.SA.EumErrorAction.warning, "Start", "4444")
      ' Exit Function
'   End If        
     
    Catch ex As Exception

'       Info8.AddError(BIZ.SA.EumErrorAction.warning, "Start", "5555")

    
        Dim TmpSrtrError As String = ex.Message
        If Not Ex.InnerException Is Nothing AndAlso Not Ex.InnerException.Message Is Nothing Then TmpSrtrError &= "____" & ex.InnerException.Message
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Runtime1", TmpSrtrError)
        BIZ.SA.ClsCommon.UpdateFormulaLog(Guid.Parse(Parameter.ThisNidFormulaLog), BIZ.SA.ClsEnum.EumFormulaLogType.FormulaCatch)
        
    End Try

' Logesup1("Etebarsanjiend","End")

End Function

Public Function msgbox(ByVal S1 As String, ByVal s2 As String)

End function

Public Function msg(ByVal S1 As String, ByVal s2 As String)
   If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
       Info8.AddError(BIZ.SA.EumErrorAction.Stop, S1, S2)
       Exit Function
   End If  
End Function


Public Function TaeedMGavahi() As Integer


    Dim Check_com As Integer = 1
    Try
        
        If tmpStr <> "" Then
            Info8.ParameterList.REMove("NidProc")
            Info8.ParameterList.Add("NidProc", tmpStr)

            ' Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 
	
            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 3 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 3 And f.EumManagerConfirmLicence = 2).ToList
              
            If ListMangerConfimTaeed.Any Then
            
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 3 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 3 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                
                    Check_com = 0
                End If

            Else
           
                Check_com = 1
            End If
        End If
    Catch ex As Exception
   ' plogfile("1","1" )
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*هشدار *", "اطلاعات وارد شده در فرم تایید مدیران برای گواهی نادرست است")
        'msgbox("ملک - بازدید	",ex.tostring)
        Throw ex
        
    End Try
    Return Check_com
End Function

Public Function TaeedMLayehe() As Integer

    Dim Check_com As Integer = 1
    Try

        If tmpStr <> "" Then

            'Info8.ParameterList.Add("NidProc", tmpStr)
            'Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 


            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 1 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 1 And f.EumManagerConfirmLicence = 2).ToList
            '   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"ListMangerConfimTaeed.Any",ListMangerConfimTaeed.Any)        
            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 1 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 1 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                    Check_com = 0
                End If

            Else

                Check_com = 1
            End If
        End If
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای جریمه لایحه صحیح نمیباشد")
         Throw ex
    End Try
    Return Check_com
End Function

Public Function Taeed3darsad(ByVal TmpListMangerConfimKh as List(Of DtoSC.Sh_ManagerConfirm)) As Integer

    Dim Check_com As Integer = 1
    Try

        If tmpStr <> "" Then

            'Info8.ParameterList.Add("NidProc", tmpStr)
            'Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 


            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)


	'msgbox("tmpStr:",tmpStr )
	'msgbox("Nidproc:",Info8.FindParameter("Nidproc").tostring )
	'Info8.GetManagerConfirm.LoadObj(Info8.FindParameter("Nidproc"))

            ListMangerConfim = TmpListMangerConfimKh'Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
 
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 10 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 10 And f.EumManagerConfirmLicence = 2).ToList
            '   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"ListMangerConfimTaeed.Any",ListMangerConfimTaeed.Any)        
            
            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 10 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 10 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                    Check_com = 0
                End If

            Else

                Check_com = 1
            End If
        End If
    Catch ex As Exception
    
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای  خطای محاسباتی ثبت و شهرداری صحیح نمیباشد")
        Throw ex

    End Try
    Return Check_com
End Function
Public Function Taeed3darsadcode13(ByVal TmpListMangerConfim as List(Of DtoSC.Sh_ManagerConfirm) ) As Integer

    Dim Check_com As Integer = 1
    Try

        If tmpStr <> "" Then

            'Info8.ParameterList.Add("NidProc", tmpStr)
            'Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 
            
             
            
            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)
            
            
            ListMangerConfim = TmpListMangerConfim 'Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            'if  ListMangerConfim.any Then msgbox("count:",ListMangerConfim.count  & "NidProc:" & Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc.tostring)                              
            
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 13 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 13 And f.EumManagerConfirmLicence = 2).ToList
            '   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"ListMangerConfimTaeed.Any",ListMangerConfimTaeed.Any)        
        
       
            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 13 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 13 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                    Check_com = 0
                End If

            Else

                Check_com = 1
                
            End If
        End If
        
    Catch ex As Exception
    
   
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای  تاييد 3% خطاي محاسباتي  صحیح نمیباشد")
        Throw ex
        
    End Try
      
    Return Check_com
End Function

Public Function TaeedMSolh() As Integer

    Dim Check_com As Integer = 1
    Try

        If tmpStr <> "" Then

            Info8.ParameterList.REMove("NidProc")
            Info8.ParameterList.Add("NidProc", tmpStr)

            Dim ListMangerConfim As New List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed,TmpList As New List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As New List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 6 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 6 And f.EumManagerConfirmLicence = 2).ToList
            '   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"ListMangerConfimTaeed.Any",ListMangerConfimTaeed.Any)        
            If ListMangerConfimTaeed.Any Then
            	Peace_AgreementTaeedDate=ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString
                TmpList=ListMangerConfim.Where(Function(f) f.ConfirmationDate = Peace_AgreementTaeedDate).ToList
				if TmpList.count >0 Then Peace_AgreementTaeedTime=TmpList.Max(Function(f) f.ConfirmationTime).ToString
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0

                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 6 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 6 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If

                    End If
                Else
                    Check_com = 0
                End If

            Else

                Check_com = 1
            End If
        End If
    Catch ex as exception
   
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای صلح و توافق صحیح نمیباشد")
         Throw ex
         
    End Try
    Return Check_com
End Function

Public Function TaeedMMovafeghat() As Integer
    Dim Check_com As Integer = 1
    Try

        If tmpStr <> "" Then

            Info8.ParameterList.REMove("NidProc")
            Info8.ParameterList.Add("NidProc", tmpStr)

            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 2).ToList

            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0

                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If

                    End If
                Else
                    Check_com = 0
                End If

            Else

                Check_com = 1
            End If
        End If
    Catch ex as exception
    
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای موافقت اصولی صحیح نمیباشد")
          Throw ex
          
    End Try
    Return Check_com
End Function

Public Function TaeedMShenasnameFani() As Integer

    Dim Check_com As Integer = 1
    Try
        
        If tmpStr <> "" Then
            Info8.ParameterList.REMove("NidProc")
            Info8.ParameterList.Add("NidProc", tmpStr)


            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 8 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 8 And f.EumManagerConfirmLicence = 2).ToList

            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 8 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 8 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                    Check_com = 0
                End If

            Else
                Check_com = 1
            End If
        End If

    Catch ex As Exception
  ' plogfile("2","2" )
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "@هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای صدور شناسنامه فني ساختمان  نادرست است" )
        'msgbox("ملک - بازدید	",ex.tostring)
        Throw ex
        
    End Try
    'Msgbox ("Check_com" , Check_com)
    Return Check_com
End Function

Public Function TaeedMovafeghatOsooli() As Integer

    Dim Check_com As Integer = 1
    Try
        'Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 
        If tmpStr <> "" Then
            Info8.ParameterList.REMove("NidProc")
            Info8.ParameterList.Add("NidProc", tmpStr)

             'Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 


            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 2).ToList
              ' Info8.AddError(BIZ.SA.EumErrorAction.Stop,ListMangerConfimCancel.Any.tostring,ListMangerConfimTaeed.Any.tostring) 
            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                    Check_com = 0
                End If

            Else
                Check_com = 1
            End If
        End If
    Catch ex As Exception
  ' plogfile("3","3" )
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "$هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای موافقت اصولي نادرست است")
        'msgbox("ملک - بازدید	",ex.tostring)
        Throw ex
        
    End Try
    Return Check_com
End Function

Public Function TaeedMSabeghe() As Integer


    Dim Check_com As Integer = 1
    Try
        'Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 
        If tmpStr <> "" Then
            Info8.ParameterList.REMove("NidProc")
            Info8.ParameterList.Add("NidProc", tmpStr)

            ' Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 


            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 4 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 4 And f.EumManagerConfirmLicence = 2).ToList
            '   Info8.AddError(BIZ.SA.EumErrorAction.Stop,ListMangerConfimCancel.Any.tostring,ListMangerConfimTaeed.Any.tostring) 
            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 4 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 4 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                    Check_com = 0
                End If

            Else
                Check_com = 1
            End If
        End If
    Catch ex As Exception
    
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای تایید سابقه نادرست است")
        '3("ملک - بازدید	",ex.tostring)
        Throw ex
        
    End Try
    Return Check_com
End Function

Public Function taeeddate(ByVal tarikh As String, ByVal lentgh As Integer, ByVal name As String) As Boolean
    Try
        Dim informat As Boolean = True
        if String.IsNullOrEmpty( tarikh) Then   Return True
        
        if tarikh.count=0 Then  Return True
        If tarikh.count <> lentgh Then

            Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا تاریخ " & name & "  را صحیح و به صورت --/--/--13 وارد نمایید.")
            informat = False
        Else
            If tarikh.split("/")(0).count <> 4 Or tarikh.split("/")(1).count <> 2 Or tarikh.split("/")(2).count <> 2 Or tarikh.split("/")(0) < 0 Or tarikh.split("/")(0) < 1300 Or tarikh.split("/")(1) <= 0 Or tarikh.split("/")(1) > 12 Or tarikh.split("/")(2) <= 0 Or tarikh.split("/")(2) > 31 Then
                Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا سال ، ماه و روز را در " & name & " صحیح وارد کنید.")
                informat = False
            End If
        End If

        Return informat
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "لطفا تاریخ " & name & " را صحیح وارد نمایید")
        Throw ex
    End Try
End Function
Function TaeedMGavahi1(ByVal strnidp As String) As Integer


    Dim Check_com As Integer = 1
    Try
        If strnidp <> "" Then
            Info8.ParameterList.REMove("NidProc")
            Info8.ParameterList.Add("NidProc", strnidp)

            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) (f.CI_ResourceManagerConfirm = 3 Or f.CI_ResourceManagerConfirm = 4) And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) (f.CI_ResourceManagerConfirm = 3 Or f.CI_ResourceManagerConfirm = 4) And f.EumManagerConfirmLicence = 2).ToList
            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) (f.CI_ResourceManagerConfirm = 3 Or f.CI_ResourceManagerConfirm = 4) And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) (f.CI_ResourceManagerConfirm = 3 Or f.CI_ResourceManagerConfirm = 4) And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                    Check_com = 0
                End If

            Else
                Check_com = 1
            End If
        End If
    Catch ex As Exception
   
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "&&هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای گواهی نادرست است")
        'msgbox("ملک - بازدید	",ex.tostring)
        Throw ex
    End Try
    Return Check_com
End Function

Function CheckGroupPathOccupation() As Integer



    Try
        Dim k As Integer = 0
        For k = 0 To Info8.User.UserGroups.Count - 1
            '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار",Info8.User.UserGroups(k).GroupGuid.ToString()) 
            If Info8.User.UserGroups(k).GroupGuid.ToString() = "380defc1-bc06-4226-a9e7-6fec5c331b8f" Then Return 1
            If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) =ucase("083a32a0-0fe4-45b6-8366-ed07ed0af346") or _
               ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) =ucase("24657c6f-f48f-4a32-b17a-6956f9214d1c") Then Return 2 ' مدیر درآمد - اداره کل درآمد  sp 474800
            
        Next
        Return 0
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "مشکل در کنترل گروه کاربری")
         Throw ex
    End Try

End Function


Public Function Fiche() As Integer
    Try
        If GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) = 56 Then Return 1 '1481290 فاقد مرحله درآمد  بیمه
        If GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) = 82 Then Return 1 '1438629 فاقد مرحله درآمد  گواهي اتحاديه صنفي
        If GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) = 69 Then Return 1 '1477479 فاقد مرحله درآمد  استعلام الکترونيک
        If GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) = 110 Then Return 1 '1516937
		

        If GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) = 106 Then Return 1
     



      '  Msgbox("bb", Info8.getRequest.Info.NidWorkflowDeff.tostring)




        Dim chek As Integer = 1
        Dim AllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Info8.Income_Fiche=Nothing
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        Dim Str_Income As String
        Dim i As Integer

        Dim Tmp As String
        Try
        	tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
	    Catch e as exception
	    	tmp=Info8.GetRequest.info.NidRevisit.tostring   ' for درخواست های اصلاح نقشه 
	    End Try	
	    
        Dim kk = Info8.GetFunctions.GetNidLib(Guid.Parse(tmp))
        Info8.ParameterList.Remove("Revisit_House_NidBase")
        Info8.ParameterList.Add("Revisit_House_NidBase", kk.RHouse.NidBase.ToString)

        If Info8.GetRevisit_House.GetBase_NosaziCode.District <> 80 Then
            Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116,163"
        Else
            Str_Income = "1,11,7,10,162,163"
        End If

        Dim TmpArr() As String
        Dim intList_Income As New list(Of Integer)

        TmpArr = Str_Income.Split(",")
        For i = 0 To TmpArr.Count - 1
            intList_Income.Add(TmpArr(i))
        Next i

        

        If TmpAllFiche.any Then
            AllFiche = TmpAllFiche.Where(Function(f) f.EumFicheStatus = 3 Or f.EumFicheStatus = 5).ToList() '7  

            If AllFiche.count > 0 Then
                AllFiche = (From A In AllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
                AllFiche = AllFiche.Distinct().ToList()
                If AllFiche.count = 0 Then
                 'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "chek&", chek)
                    chek = 0
                Else
 

                    AllFiche = TmpAllFiche.Where(Function(f) f.EumFicheStatus = 3 And f.Payable > 0 And (f.ExportTemporaryDate > "1393/01/00") And CStr(f.bankpaymentdate) < "1390/00/00").ToList() '7  f.EumFicheStatus=3 or  ''f.Payable > 10000000 And  
					AllFiche= AllFiche.Where(Function(f) f.EumIncomePaymentType<> 4).ToList()
                    If AllFiche.Any Then
                    
                        AllFiche = (From A In AllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
                        AllFiche = AllFiche.Distinct().ToList()
                       
                        If AllFiche.Any Then 
                        '	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "chek%", chek)
                        	chek = 0
                        End if	
                        	
                    End If
                    '157298
                 '   AllFiche = TmpAllFiche.Where(Function(f) f.EumFicheStatus = 3 And f.Payable > 0 And f.EumIncomePaymentType=2).ToList() '7  f.EumFicheStatus=3 or  ''f.Payable > 10000000 And  

                 '   If AllFiche.Any Then
                 '       AllFiche = (From A In AllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
                '        AllFiche = AllFiche.Distinct().ToList()
                '        Dim kkk as Integer =0
                '        If AllFiche.count>0 Then 
                '        	For kkk=0 to  AllFiche.count-1
                '        		 
                 '       		if Fpose(AllFiche(kkk).NidUserUserConfirm.tostring)=0 Then 
                '        		'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "chek!", chek)
                 '       			chek = 0
                 '       		Else
                 '       		
                 '       			chek = 1
                 '       		End if	
                  '      	
                  '      	Next
                   '     End if
                  '  End If
                    '157298
                End If
            Else
               	if  GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) =110 Then 
	            
	            	For kk=0 to TmpAllFiche.count -1 
	            		if TmpAllFiche(kk).Payable>0 Then
	            	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "chek@", chek)
	            			chek = 0 
	            		
	            		End if				
	            	Next
	            	
	            Else
	            'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "chek#", chek)
                	chek = 0
                End if	
	            
            End If
        Else
        'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "chek$", chek)
            chek = 0
        End If


        Return chek

    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "فیش های درآمد" + ex.Message.ToString())
        Throw ex
    End Try
End Function



Public Sub Plogfile(ByVal A As String, ByVal B As String)

   If Info8.User.UserGuid.ToString() = "540d119d-14f1-4f65-94d1-7f39fc2b3b48" Then 
      ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, A, B)
        ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبار سنجی", "تا زمانیکه بازدید تبلت به شهرسازی برگشت داده نشده است امکان ویرایش ندارید")
   End If
End Sub

Private Function MapPriceNew(ByVal Dt As List(Of DtoSC.Base_Edge), ByVal objType As Integer, ByVal EumStatus As Integer, ByVal DocArea As Double) As Double
    ''''''''''''
    Try




        ' خواندن اطلاعات نقشه
        Dim i As Integer

        Dim AreaCurent As Double = 0
        Dim gu As New Guid
        Dim gu2

        Dim lstTmp As New List(Of BIZ.SA.ClsNosaziCodeType)
        Dim asObjectType = objType
        Dim asObjectTypeP = objType
        If objType = 2 Then
            If EumStatus = 0 Then
                lstTmp = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetParvandeh_House.Base_NosaziCode.NidNosaziCode, 0)
                lstTmp = lstTmp.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
                If lstTmp.Count > 0 Then
                    Return -1200
                End If
            Else
                lstTmp = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRevisit_House.Base_NosaziCode.NidNosaziCode, 0)
                lstTmp = lstTmp.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
                If lstTmp.Count > 0 Then
                    Return -1200
                End If
            End If

        End If

        If objType = 2 Then
            If EumStatus = 0 Then
                gu2 = Info8.GetParvandeh_House.GetBase_NosaziCode
            Else
                gu2 = Info8.GetRevisit_House.GetBase_NosaziCode

            End If

        ElseIf objType = 5 Then
            If EumStatus = 0 Then
                gu2 = Info8.GetParvandeh_HouseSharing.GetBase_NosaziCode
            Else
                gu2 = Info8.GetRevisit_HouseSharing.GetBase_NosaziCode
            End If


        End If


        gu = gu2.NidNosaziCode

        Dim strnosazicode As String = ""
        strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
        Dim timeM As New System.Diagnostics.Stopwatch
        Dim milliSec2M As Long = 0
        timeM.start()
        milliSec2M = timeM.ElapsedMilliseconds()

        Dim Tmp As String = "0"
        Dim key As Object
        Dim MapPrice(10) As String
        Dim P_LapList As New List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
        Dim P_LayList As New List(Of Biz.SC.SrvMap.MapService_OverLayResult)
        Dim key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
        Dim code As String()

        code = strnosazicode.Split("-")
        Dim M_Districk As Integer = code(0)
        Dim M_Region As Integer = code(1)
        Dim M_House As Integer = code(3)

        If M_House > 10000 Then Return -1200
        If Dt.count = 0 Then
            Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "لطفا اطلاعات جهات را وارد نمایید")
            Exit Function

        End If

        If M_Districk <> 80 And M_House > 10000 Then
            key_List = Info8.GetMap().GetMapFind(strnosazicode, "1991")

        ElseIf M_Districk = 80 Then
            If M_Region = 80 Then
                key_List = Info8.GetMap().GetMapFind(strnosazicode, "3706")

            Else
                If M_House > 10000 Then
                    key_List = Info8.GetMap().GetMapFind(strnosazicode, "3710")
                Else
                    key_List = Info8.GetMap().GetMapFind(strnosazicode, "5")
                    If key_List.count = 0 Then
                        key_List = Info8.GetMap().GetMapFind(strnosazicode, "839")
                    End If

                End If
            End If
        Else
            key_List = Info8.GetMap().GetMapFind(strnosazicode, "5")
            If key_List.count = 0 Then
                key_List = Info8.GetMap().GetMapFind(strnosazicode, "839")
            End If
        End If


        If Not key_List Is Nothing Then
            If key_List.count > 0 Then
                key = key_List(0).Guid
            Else
                Info8.AddError(BIZ.SA.EumErrorAction.warning, "توجه", "ملک مورد نظر بر روی نقشه جانمایی نشده است")
            End If
        End If

        '''''''''''''''''''''''''''''''''''''''''''

        If M_Districk <> 80 And M_House > 10000 Then

            key_List = Info8.GetMap().GetMapFind(strnosazicode, "1991")

        ElseIf M_Districk = 80 Then
            If M_Region = 80 Then
                key_List = Info8.GetMap().GetMapFind(strnosazicode, "3706")

            Else
                If M_House > 10000 Then
                    key_List = Info8.GetMap().GetMapFind(strnosazicode, "3710")
                Else
                    key_List = Info8.GetMap().GetMapFind(strnosazicode, "5")
                    If key_List.count = 0 Then
                        key_List = Info8.GetMap().GetMapFind(strnosazicode, "839")
                    End If

                End If
            End If
        Else
            key_List = Info8.GetMap().GetMapFind(strnosazicode, "5")
            If key_List.count = 0 Then
                key_List = Info8.GetMap().GetMapFind(strnosazicode, "839")
            End If
        End If



        '''''''''''''''''''''''''منطقه2 ناحیه 4

        Dim strL As String = "678"

        P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(key.tostring, strL)

        Dim CodeLay As Boolean = False
        AreaCurent = key_List(0).Area
        Dim TmpCodeL As Object
        If P_LapList.count > 0 Then
            Dim ia As Integer
            For ia = 0 To P_LapList.count - 1
                TmpCodeL = IIf(CStr(P_LapList(ia).Code) = "", 0, P_LapList(ia).Code)

                If TmpCodeL = 39 Or TmpCodeL = 33 Or TmpCodeL = 32 Then CodeLay = True
            Next
        End If
        '''''''''''''''''''''''''''''''''''  


        Dim jahat As Integer = 0
        Dim EdgMap As New Global.BIZ.SC.SrvMap.ClsEdg
        EdgMap = Info8.GetMap.GetMapEdge(key, 5, 1981, 3, 30, True)
        Dim ii, j, jj As Integer
        Dim p As Double = 0
        For jj = 0 To EdgMap.Segment.count - 1

            If Trim(CStr(EdgMap.Segment(jj).BarPric)) = "" Then EdgMap.Segment(jj).BarPric = 0
            If p < Convert.ToDouble(EdgMap.Segment(jj).BarPric) Then p = Convert.ToDouble(EdgMap.Segment(jj).BarPric)
        Next

        EdgMap.Segment = EdgMap.Segment.where(Function(f) f.BarPric = p).tolist
        If EdgMap.Segment.count > 0 Then jahat = EdgMap.Segment(0).direction
        EdgMap = Info8.GetMap.GetMapEdge(key, 5, 3929, 3, 30, True)
        Dim lstSegment As List(Of Global.BIZ.SC.SrvMap.ClsSegment)
        Dim lstSegmentDirec As List(Of Global.BIZ.SC.SrvMap.ClsSegment)
        Dim ZelMojod, bar, pathw, MabarName As String

        lstSegment = EdgMap.Segment
        Dim Code2 As Boolean = False
        Dim Code4 As Boolean = False
        Dim Code6 As Boolean = False
        Dim Code8 As Boolean = False

        Dim kk As Integer = 0
        For kk = 0 To dt.count - 1
            If Dt(kk).CI_SideCode = 2 Then Code2 = True
            If Dt(kk).CI_SideCode = 4 Then Code4 = True
            If Dt(kk).CI_SideCode = 6 Then Code6 = True
            If Dt(kk).CI_SideCode = 8 Then Code8 = True
        Next
        If Code2 = False Then
            Dim NewR As New DtoSC.Base_Edge
            NewR.CI_SideCode = 2
            NewR.EdgeLenDoc = 0
            Dt.add(NewR)
        End If
        If Code4 = False Then
            Dim NewR As New DtoSC.Base_Edge
            NewR.CI_SideCode = 4
            NewR.EdgeLenDoc = 0
            Dt.add(NewR)
        End If
        If Code6 = False Then
            Dim NewR As New DtoSC.Base_Edge
            NewR.CI_SideCode = 6
            NewR.EdgeLenDoc = 0
            Dt.add(NewR)
        End If
        If Code8 = False Then
            Dim NewR As New DtoSC.Base_Edge
            NewR.CI_SideCode = 8
            NewR.EdgeLenDoc = 0
            Dt.add(NewR)
        End If

        '________________________________________________________________

        Dim MaxArea, MinArea As Double
        If AreaCurent <= 250 Then
            MaxArea = AreaCurent + (AreaCurent * 0.05)
            MinArea = AreaCurent - (AreaCurent * 0.05)
        Else
            MaxArea = AreaCurent + (AreaCurent * 0.05)
            MinArea = AreaCurent - (AreaCurent * 0.05)
        End If

        Dim chekDoc As Integer = 1
        If DocArea <= MaxArea And DocArea >= MinArea Then chekDoc = 0

        'AA  If chekDoc <> 0 And CodeLay = True Then Info8.AddError(BIZ.SA.EumErrorAction.warning, "توجه", "اطلاعات بر و اضلاع و نام معابر از نقشه استخراج شده است")

        '________________ جمع کردن ردیف های مشابه 
        Dim TmpMapList, TmpMapList2 As New List(Of Global.BIZ.SC.SrvMap.ClsSegment)
        Dim reapeated As Boolean
        If lstSegment.count > 0 Then lstSegment = lstSegment.OrderByDescending(Function(f) f.direction).tolist()

        For i = 0 To lstSegment.count - 1
            reapeated = False
            If Trim(lstSegment(i).BarLenght) = "" Then lstSegment(i).BarLenght = "0"
            If Trim(lstSegment(i).BarPric) = "" Then lstSegment(i).BarPric = 0
            If Trim(lstSegment(i).MabarName) = "" Then
                lstSegment(i).MabarName = ""
                If lstSegment(i).BarPric > 0 Then

                    TmpMapList2 = lstSegment.where(Function(f) f.BarPric = lstSegment(i).BarPric).tolist()
                    lstSegment(i).MabarName = TmpMapList2.Max(Function(f) f.MabarName)

                    If lstSegment(i).MabarName = "" Then lstSegment(i).MabarName = "-"
                End If
            End If




            For j = 0 To TmpMapList.count - 1
                If Trim(TmpMapList(j).BarLenght) = "" Then lstSegment(j).BarLenght = "0"

                If lstSegment(i).direction = TmpMapList(j).direction And lstSegment(i).MabarName = TmpMapList(j).MabarName And lstSegment(i).BarPric = TmpMapList(j).BarPric Then
                    TmpMapList(j).BarLenght = Convert.ToDouble(lstSegment(i).BarLenght) + Convert.ToDouble(TmpMapList(j).BarLenght)
                    TmpMapList(j).SegmentLenght = Convert.ToDouble(lstSegment(i).SegmentLenght) + Convert.ToDouble(TmpMapList(j).SegmentLenght)

                    'Msgbox (TmpMapList(j).BarLenght ,"j:" & TmpMapList(j).direction & "a:" &  lstSegment(i).BarLenght  & "b:" & Convert.ToDouble (lstSegment(i).BarLenght) + Convert.ToDouble(TmpMapList(j).BarLenght) )
                    reapeated = True
                    Exit For
                End If
            Next

            If reapeated = False Then
                Dim TmpMapRow As New Global.BIZ.SC.SrvMap.ClsSegment
                TmpMapRow = lstSegment(i)
                TmpMapList.add(TmpMapRow)
            End If

        Next

        If TmpMapList.count > 0 Then lstSegment = TmpMapList
        Dim TmpMaxBar, TmpSideCode As Double
        Dim countPath As Integer = 0
        For i = 0 To lstSegment.count - 1

            TmpSideCode = 0
            TmpMaxBar = 0
            Try
                Dim M_NrequestNo = Info8.GetRequest.Info.NidWorkItem
                If M_NrequestNo = 1415756 And lstSegment(i).direction = 4 Then lstSegment(i).BarPric = 0
                If M_NrequestNo = 1414561 And lstSegment(i).direction = 6 Then lstSegment(i).BarPric = 0
                If M_NrequestNo = 1413707 And lstSegment(i).direction = 2 Then lstSegment(i).BarPric = 0
                If M_NrequestNo = 1427671 And lstSegment(i).direction = 8 Then lstSegment(i).BarPric = 0
                If M_NrequestNo = 1424771 And lstSegment(i).direction = 4 Then lstSegment(i).BarPric = 0
                If M_NrequestNo = 1419179 And lstSegment(i).direction = 6 Then lstSegment(i).BarPric = 0



                If M_NrequestNo = 1417309 And (lstSegment(i).direction = 5 Or lstSegment(i).direction = 7) Then lstSegment(i).direction = 6


            Catch  ex as exception
              Throw ex
            End Try

            If lstSegment(i).MabarName <> "" And lstSegment(i).BarPric > 0 Then
                TmpMapList = lstSegment.where(Function(f) f.BarPric > 0 And f.MabarName = lstSegment(i).MabarName).tolist()

                If TmpMapList.count > 0 Then
                    For j = 0 To TmpMapList.count - 1
                        If convert.todouble(TmpMapList(j).BarLenght) > TmpMaxBar Then TmpMaxBar = convert.todouble(TmpMapList(j).BarLenght)
                    Next
                    TmpMapList = TmpMapList.where(Function(f) f.BarLenght = TmpMaxBar).tolist()
                    If TmpMapList.count > 0 Then
                        TmpSideCode = iif(isdbnull(TmpMapList.max(Function(f) f.direction)), 0, TmpMapList.max(Function(f) f.direction))
                        If TmpSideCode > 0 And TmpSideCode <> lstSegment(i).direction Then
                            If TmpSideCode = lstSegment(i).direction - 1 Or TmpSideCode = lstSegment(i).direction + 1 Or (TmpSideCode = 8 And lstSegment(i).direction = 1) Or (TmpSideCode = 1 And lstSegment(i).direction = 8) Then
                                lstSegment(i).direction = TmpSideCode
                            Else
                                lstSegment(i).BarLenght = 0
                                lstSegment(i).BarPric = 0
                                lstSegment(i).MabarName = " "
                            End If
                        End If
                    End If
                End If

                If lstSegment(i).BarLenght >= 1.5 Then countPath += 1

            End If
        '    Msgbox("dir:" & lstSegment(i).direction, " Bar:" & lstSegment(i).BarLenght & " Len:" & lstSegment(i).SegmentLenght & "Mabar:" & lstSegment(i).MabarName & "Price:" & lstSegment(i).BarPric & "TmpSideCode:" & TmpSideCode)
        Next

        If countPath >= 1 Then '1428138
            For i = 0 To lstSegment.count - 1
                If lstSegment(i).MabarName <> "" And lstSegment(i).BarPric > 0 And lstSegment(i).BarLenght < 1.5 Then
                    lstSegment(i).BarLenght = 0
                    lstSegment(i).BarPric = 0
                    lstSegment(i).MabarName = " "
                End If
            Next
        End If



        '_________________________________

        Dim AZlae As Integer = 0
        For i = 2 To 8 Step 2
            ZelMojod = "0"
            bar = "0"
            pathw = "0"
            MabarName = ""


            Dim k As Integer = 0
            For k = 0 To 1
                Dim lstSegmentDirec1 As List(Of Global.BIZ.SC.SrvMap.ClsSegment)
                If i = 8 And k = 1 Then
                    lstSegmentDirec = lstSegment.where(Function(f) f.direction = 1).tolist
                    lstSegmentDirec1 = lstSegment.where(Function(f) f.direction = 1 Or f.direction = 8).tolist
                Else
                    lstSegmentDirec = lstSegment.where(Function(f) f.direction = i + k).tolist
                    lstSegmentDirec1 = lstSegment.where(Function(f) f.direction = i + k Or f.direction = i).tolist
                End If


                For jj = 0 To lstSegmentDirec.count - 1

                    If CStr(lstSegmentDirec(jj).BarPric) = "" Then lstSegmentDirec(jj).BarPric = 0
                    If CStr(lstSegmentDirec(jj).BarLenght) = "" Then lstSegmentDirec(jj).BarLenght = 0

                    If CStr(lstSegmentDirec(jj).SegmentLenght) = "" Then lstSegmentDirec(jj).SegmentLenght = 0
                Next
                If lstSegmentDirec.count > 0 Then
                    Dim lstSegmentDirecTmp1 As List(Of Global.BIZ.SC.SrvMap.ClsSegment)
                    Dim MaxBar As Double = lstSegmentDirec1.Max(Function(f) f.BarLenght)
                    lstSegmentDirecTmp1 = lstSegmentDirec1.where(Function(f) f.BarLenght = MaxBar).tolist
                    pathw = lstSegmentDirecTmp1.Max(Function(f) f.BarPric)
                    MabarName = lstSegmentDirec(0).MabarName
                  '  Msgbox("i" & i, "pathw:" & pathw & " MabarName:" & MabarName)
                    '	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "طول بر",lstSegmentDirec.sum(Function(f) Double.Parse(f.BarLenght)))
                    If bar = "0" Then
                        bar = Math.round(lstSegmentDirec.sum(Function(f) Double.Parse(f.BarLenght)), 2).tostring
                    Else
                        bar = bar & " + " & Math.round(lstSegmentDirec.sum(Function(f) Double.Parse(f.BarLenght)), 2).tostring
                    End If
                    If ZelMojod = "0" Then
                        ZelMojod = Math.round(lstSegmentDirec.sum(Function(f) Double.Parse(f.SegmentLenght)), 2).tostring
                    Else
                        ZelMojod = ZelMojod & " + " & Math.round(lstSegmentDirec.sum(Function(f) Double.Parse(f.SegmentLenght)), 2).tostring
                    End If
                End If

            Next



            If CodeLay Then

                For j = 0 To dt.count - 1

                    If Dt(j).CI_SideCode = i Then
                        If IsNumeric(ZelMojod) Then
                            If math.abs(bar - ZelMojod) < 1 Then ZelMojod = bar
                        End If

                        If chekDoc = 1 Then

                            'AA  Dt(j).EdgeLenCurrent = ZelMojod

                            'AA  Dt(j).EdgeBarCurrent = bar
                            Dim LenDoc As Double = 0
                            Dim ij As Integer = 0

                            'AA  If Dt(j).EdgeLenDoc.tostring = "" Then Dt(j).EdgeLenDoc = 0
                            Dim TmpStr As String
                            TmpStr = iif(isdbnull(Dt(j).EdgeLenDoc), "", Dt(j).EdgeLenDoc)

                            Dim LstLenDoc() As String
                            If TmpStr <> "" Then
                                LstLenDoc = TmpStr.split("+")

                                For ij = 0 To LstLenDoc.count - 1
                                    If IsNumeric(LstLenDoc(ij)) Then LenDoc += LstLenDoc(ij)
                                Next
                            End If

                            Dim ZelMojodTmp As Double = 0
                            ij = 0
                            Dim LstZelMojod() As String = ZelMojod.split("+")


                            For ij = 0 To LstZelMojod.count - 1
                                If IsNumeric(LstZelMojod(ij)) Then ZelMojodTmp += LstZelMojod(ij)
                            Next



                            If math.abs(ZelMojodTmp - LenDoc) > 0.05 * LenDoc Then
                                'AA Info8.AddError(BIZ.SA.EumErrorAction.Warning, " توجه ", "ضلع موجود : " & ZelMojodTmp.tostring & " ضلع سند : " & LenDoc.tostring & " اختلاف اضلاع موجود و سند از سمت " & Info8.GetFunctions.GetCodeTitle("CI_SideCode", i).tostring & " بیشتر از 5 درصد می باشد قابلیت پاسخگویی ندارد ")
                                AZlae = 1
                            End If


                        ElseIf chekDoc = 0 Then

                            'AA   Dt(j).EdgeLenCurrent = Dt(j).EdgeLenDoc
                            'AA   Dt(j).EdgeBarCurrent = Dt(j).EdgeBarDoc
                        End If

                        If TmpCodeL = 32 Or TmpCodeL = 33 Then Dt(j).AfterEditWidth = pathw

                        'AA Dt(j).PathName = MabarName
                        'AA Dt(j).EdgeAndSideCurrent = Info8.GetFunctions.GetCodeTitle("CI_SideCode", Dt(j).CI_SideCode)

                    End If
                Next


            ElseIf M_Districk = 9 Or M_Districk = 11 Or M_Districk = 1 Then
                AreaCurent = -1200
                For j = 0 To dt.count - 1

                    If Dt(j).CI_SideCode = i Then
                        Dt(j).AfterEditWidth = pathw
                    End If
                Next
            Else
                AreaCurent = -1200

            End If

        Next

        Dim AreaKol = Info8.GetMap.GetIntersectionAreaByID(key, key)

        If AreaCurent <> -1200 Then
            For i = 0 To EdgMap.OmghList.count - 1

                If EdgMap.OmghList(i).Direction = jahat Then

                    If jahat Mod 2 <> 0 Then jahat = iif(jahat = 1, 8, jahat - 1)

                    For j = 0 To dt.count - 1
                        'AA   Dt(j).Depth1 = 0
                        'AA   dt(j).Depth3 = 0

                        'AA If AZlae = 1 Then Dt(j).EdgeLenCurrent = 0

                        If Dt(j).CI_SideCode = jahat Then

                            'AA    Dt(j).Depth1 = iif(Math.round(convert.todouble(AreaKol), 2) < Math.round((Convert.ToDouble(EdgMap.OmghList(i).Area)) * 2, 2), Math.round(convert.todouble(AreaKol), 2), Math.round((Convert.ToDouble(EdgMap.OmghList(i).Area)) * 2, 2))
                            'AA    dt(j).Depth3 = iif(Math.round(convert.todouble(AreaKol), 2) - Math.round((Convert.ToDouble(EdgMap.OmghList(i).Area)) * 2, 2) < 0, 0, Math.round(convert.todouble(AreaKol), 2) - Math.round((Convert.ToDouble(EdgMap.OmghList(i).Area)) * 2, 2))
                        End If
                    Next
                End If
            Next
        End If

        Return AreaCurent
    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, " توجه ", "دیتا جهات اربعه را کامل و صحیح وارد نمایید")
        ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, " توجه ", Ex.Message)
          Throw ex
    End Try
    ''''''''''''''''''
End Function
Private sub logfilefj(ByVal A as String,ByVal B as String)
 if  ucase(Info8.User.UserGuid.ToString()) = ucase("ca742134-44bb-4ba6-b0c4-e5e186600a0a")  Then
 	  Info8.AddError(BIZ.SA.EumErrorAction.warning,A,B) 
   End if
End sub
Public Sub plogAHM(ByVal A as String,ByVal B as String)
 if ucase(Info8.User.UserGuid.ToString()) = "276D9B0C-E2B1-4101-A019-11B1069B4197" or ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 'mousavi & sajedi 
    Info8.AddError(BIZ.SA.EumErrorAction.warning,A,B) 
 End if 
End Sub

Private Function UseBazdidT() As Integer

    Try
        Dim i As Integer = 0
        Dim TlstR As List(Of DtoSC.Sh_Revisit)
        TlstR = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)

        If TlstR.Count > 0 Then TlstR = TlstR.Where(Function(f) f.NidRevisit = Info8.GetRequest.Info.NidRevisit).ToList

        Dim UserList As List(Of Biz.SA.SrvSecurityWCF.UserGroup)

        Dim a = TlstR(0).NidRevisitAgent

        Dim b = Info8.GetRevisitAgent.GetSh_RevisitAgent(a).NidUser
        UserList = Info8.GetFunctions.GetUserGroups(b)

        For i = 0 To UserList.count - 1
        ''272222
        'sp 331338
            If Ucase(UserList(i).GUID.tostring) = "4A58BD9F-DF2A-4A15-B8A7-327D41542697" or Ucase(UserList(i).GUID.tostring) = "5F4030CE-99FB-4731-892B-0D0F525A910E" _
            or Ucase(UserList(i).GUID.tostring) = ucase("1C863B33-96A9-487D-AAC9-A8DC2DAEE625") _
            or Ucase(UserList(i).GUID.tostring) =ucase("4ca317a1-3025-486a-89e9-bdea33844501") or Ucase(UserList(i).GUID.tostring) =ucase("05aed9ee-74d6-4300-a1b3-c7d37cfb15af") Then Return 1
        Next
        '328288
        Dim TabletB() As String = {"CBC70236-F6DC-4D16-AD9D-02527170C473", "50F22591-BAB7-487D-B1C4-06A86ACBAD06",
                "7F260B4B-EA24-4060-B128-06BA986703E9", "CF39113D-7E70-4D6F-A41C-19668244BE69",
        "2AFE139A-374E-4CEC-B779-1FCC51C06B58", "FA8FBD28-A413-4D86-99E9-22B5F6B6BCF4",
        "1DC46692-0F35-4129-A42E-25F072A994FC", "D8BF2867-ECEA-4B88-8D54-307F97C4B734",
        "02917896-8BC8-44AD-91DA-35A311619D84", "25725B9D-48CF-4502-A70B-3CCDE913A767",
        "001F1D2C-1597-4C7B-BF95-4D77061253FF", "7385A97D-FB66-440A-BE54-4F4A3A959B01",
        "2FDE2544-9CD2-4C45-AF17-57025A2307CC", "AFAB977F-B75E-42F0-BB34-5EB3396CFB24",
        "F22B360F-949F-42AC-BEB3-60F985CC5766", "7248B372-00C7-4264-A63C-6620756CBDFC",
        "2FEA51E2-424C-4A24-81D5-69C52A7CD10E", "BE74782A-6C69-4E6A-8DF7-7793A339ACD4",
        "7CE6F8B6-6B37-4F4F-BDD5-831E68E462EE", "98ADB216-BC4F-4F93-876E-840547A0C9DA",
        "371855B6-80BE-44C3-AFBD-843B224784FC", "2EFCCA1D-D0C1-47C7-85E3-886A3C3A53D8",
        "4E6E828F-0FC9-4E23-9F1F-8872930CF8A4", "E0C6069E-1B6F-470E-AC4A-898010EA23D8",
        "59B2F424-AFF2-4FFD-A842-93B6BE1EA9D4", "8095EC07-F028-47E0-A3A9-9E964E5DEF4C",
        "AA4ABA9B-2014-4463-BE98-A26CF158E11C", "EC363FE1-BDD9-4FA7-934A-A8402F56A092",
        "6F728723-4F37-44E6-8F9D-A9C5B3E2F544", "50A7C193-3EE4-4575-87F5-AEF8F4C70899",
        "E7DB882E-7394-4773-BEFB-BC54275CCE13", "13B37055-8CA2-44D7-A770-BE85064BD121",
        "892DA199-3509-49D7-BD59-C7CA791BFFDD", "AFA166E6-595D-4623-9533-C95B8F31766E", "6E3B6547-7F2A-4B30-BEA5-110A4412A915",
        "D6A3779B-1B7B-41A5-90B6-CC6C3ECFFDF0", "D8015082-F770-44BA-8B19-D93DCF680760", "7A496558-E933-483E-B31A-DAE6E3D29DD5",
        "487E1212-300B-414E-A7E9-E337079DC29A", "98195966-5B41-4018-AD49-F1EF84DBCBCD", "124BFE89-53D7-4F80-987C-F69CB20A0646"}

        'For Each b As String In TabletB
        '	    If Ucase(b) = Ucase(Info8.GetRevisitAgent.GetSh_RevisitAgent(TlstR(0).NidRevisitAgent).NidUser.tostring) Then Return 1
        '	 Next  


        Return 0
    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, " NNNNNNگروه تبلت  ", Ex.Message)
        Throw ex
        
    End Try
    ''''''''''''''''''
End Function

Public Function CheckChar(ByVal TmpStr As String) As Boolean
    If TmpStr <> "" Then
        If instr(TmpStr, "/") <> 0 Or instr(TmpStr, "\") <> 0 Then
            Info8.AddError(Biz.SA.EumErrorAction.Stop, "هشدار ", "کاراکتر هاي '/'  و '\' مجاز نمي باشد  ")
            Return False
        End If
    End If
    Return True
End Function

Public Function MassDistanceMain(ByVal LstBaseUsing As List(Of DtoSC.Base_Using) )
  		Dim i as Integer
        Dim MaxFloor As Integer
        Dim TmpLstBaseUsing As List(Of DtoSC.Base_Using)
        TmpLstBaseUsing=LstBaseUsing.where(Function(f) f.FloorNo<99 And (f.CI_UsingType<>832 And f.CI_UsingType<>845)).tolist 
        MaxFloor = TmpLstBaseUsing.Max(Function(f) f.FloorNo)
    
        LstBaseUsing = LstBaseUsing.Where(Function(f) f.FloorNo = MaxFloor And f.FloorNo <>0  And f.CI_UsingType = 801 And f.CI_UsingGroup = 11).ToList() '1814358

       
        If LstBaseUsing.Any Then
            For i = 0 To LstBaseUsing.Count - 1
                If LstBaseUsing(i).MassDistance_Main.ToString = ""  Then 'Or LstBaseUsing(i).MassDistance_Main.ToString = "0"
                    If LstBaseUsing(i).MassDistance_Sub.ToString = ""  Then 'Or LstBaseUsing(i).MassDistance_Sub.ToString = "0"
                        Info8.AddError(Global.BIZ.SA.EumErrorAction.Stop, "توجه", "لطفا فاصله جرم گذاری را برای کاربری انباری پشت بام وارد نمایید")
                    End If
                End If
                'SP475522
                 If LstBaseUsing(i).MassDistance_Main.ToString = "0"  Then 
                    If LstBaseUsing(i).MassDistance_Sub.ToString = "0"  Then
                        Info8.AddError(Global.BIZ.SA.EumErrorAction.warning, "توجه", "کاربر گرامی فاصله جرم گذاری  برای کاربری انباری پشت بام صفر می باشد")
                    End If
                End If
                
            Next
        End If
        
        
        
        
        
        
        
        
End Function


Private Function Fpose(ByVal NidUser as String ) As Integer

    Try
        Dim i As Integer = 0
        Dim UserList As List(Of Biz.SA.SrvSecurityWCF.UserGroup)
        Dim b = Guid.Parse(NidUser)
        UserList = Info8.GetFunctions.GetUserGroups(b)

  
'add B40F1FAF-5885-457B-9217-DDAC6F01898E sp 395107
        For i = 0 To UserList.count - 1
            If lcase(UserList(i).GUID.tostring) = "c65c30a2-b7b8-4af5-95dc-e0be1f2af6e9" or _
               lcase(UserList(i).GUID.tostring) = "e4fd6027-2a09-4b82-b812-6644160636c5" or _
               lcase(UserList(i).GUID.tostring) = "16bc1db1-a38d-4166-9ef4-ab5209dd562d" or _
               lcase(UserList(i).GUID.tostring) = "5aef8344-b54e-412e-8cd9-d03eda410071" or _
               lcase(UserList(i).GUID.tostring) = "B40F1FAF-5885-457B-9217-DDAC6F01898E" or _
               lcase(UserList(i).GUID.tostring) = "81caa6f8-1da9-428b-a0c7-8c17e705b08e" or _
               lcase(UserList(i).GUID.tostring) = "8bba5f92-c46f-4379-b59d-89ac3158164a" Then  Return 0
              ' sp 334302
        Next
       
        Return 1
    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, " XXXX گروه تبلت  ", Ex.Message)
        Throw ex
        
    End Try
    ''''''''''''''''''
End Function
Private Function TaeedMComm20() as Integer


Dim Check_com as Integer =1
Try

 
 
 Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
 Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
 Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)
 
 ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
 ListMangerConfimTaeed = ListMangerConfim.Where(Function(f)  f.CI_ResourceManagerConfirm = 12 And f.EumManagerConfirmLicence = 1).ToList
 ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 12 And f.EumManagerConfirmLicence <> 1).ToList
      '   Info8.AddError(BIZ.SA.EumErrorAction.Stop,ListMangerConfimCancel.Any.tostring,ListMangerConfimTaeed.Any.tostring) 
If ListMangerConfimTaeed.Any Then
	If ListMangerConfimCancel.Any Then
    	If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
        	Check_com=0
    	Else
    		If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
            	Dim tmpC as String=ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString
            		
            	ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 12 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate=tmpC).ToList
            	ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 12 And f.EumManagerConfirmLicence <> 1 And f.ConfirmationDate=tmpC).ToList
            			
            	If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
            		Check_com=0
            	End if
       		End if
    	End if 
	Else
		Check_com=0
   	End if	

Else
	Check_com=1
End if

Catch ex As Exception

	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"QQهشدار","اطلاعات وارد شده در فرم تایید مدیران برای تاييد مدير بند 20 نادرست است") 
	Throw ex
	
 'msgbox("ملک - بازدید	",ex.tostring)
End Try 
Return Check_com
End Function 
 

Public Function TaeedMMokateb(Optional ByVal inputed_CI as Integer = 96) As Integer

' 0 Exist or Confirm
' 1 Not Exist or Canceled

    Dim Check_com As Integer = 1
    Try
        
        If tmpStr <> "" Then
            Info8.ParameterList.REMove("NidProc")
            Info8.ParameterList.Add("NidProc", tmpStr)

            ' Info8.AddError(BIZ.SA.EumErrorAction.Stop,"tmpStr",tmpStr) 
	
            Dim ListMangerConfim As New List(Of DtoSC.Sh_MokatebatConfirm )
            Dim ListMangerConfimTaeed  As New List(Of DtoSC.Sh_MokatebatConfirm )
            Dim ListMangerConfimCancel As New List(Of DtoSC.Sh_MokatebatConfirm )

            ListMangerConfim = Info8.GetMokatebatConfirm.Sh_MokatebatConfirm_List()
           
       
            
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = inputed_CI And  f.EumManagerConfirmLicence = 1).ToList
            
       
            
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = inputed_CI And  f.EumManagerConfirmLicence = 2).ToList
           
              
            If ListMangerConfimTaeed.Any Then
            
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = inputed_CI  And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = inputed_CI And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
                                
                                Check_com = 0
                            End If
                        End If
                    End If
                Else
                                             
                    Check_com = 0
                    
                End If

            Else
           
                Check_com = 1
            End If
        End If
    Catch ex As Exception
     Dim TmpError As String = ex.Message
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "WWهشدار", "اطلاعات وارد شده در فرم تایید مدیران برای مکاتبات نادرست است")
        Throw ex
    End Try
    Return Check_com
End Function
 

'new 135533 sampa

Private Function User137(ByVal NidUser as String,ByVal TmpArchiveGroup as Integer  ) As Integer

    Try
        Dim i As Integer = 0
        Dim UserList As List(Of Biz.SA.SrvSecurityWCF.UserGroup)
        Dim b = Guid.Parse(NidUser)
        UserList = Info8.GetFunctions.GetUserGroups(b)
Dim tmpReturn as Integer
tmpReturn=1
  

        For i = 0 To UserList.count - 1
            If Ucase(UserList(i).GUID.tostring) = "59434E63-FFD2-4DE9-B26D-E795C44A4F6A" _
Or Ucase(UserList(i).GUID.tostring) = "7857A335-E3B9-41AE-AE08-E4AF9361CD09" _
Or Ucase(UserList(i).GUID.tostring) = "56D54004-8F7E-4821-ABF4-CA9236138B05" _
Or Ucase(UserList(i).GUID.tostring) = "05F20D2F-1FB3-488D-9665-F059D8DCF852" _
Or Ucase(UserList(i).GUID.tostring) = "742A8271-C308-4A60-948A-835686C0217B" _
Or Ucase(UserList(i).GUID.tostring) = "2E25A370-A1B7-47A6-BB05-5610331B8BBB" _
Or Ucase(UserList(i).GUID.tostring) = "B3765F13-E747-495E-A9CF-B37EFF270F3D" _
Or Ucase(UserList(i).GUID.tostring) = "0B2A5ECB-FF16-4354-B739-6095F41025E4" _
Or Ucase(UserList(i).GUID.tostring) = "61B89306-A19F-4CFC-88FA-C14CF9EF4EF8" _
Or Ucase(UserList(i).GUID.tostring) = "3CB1C823-B005-49F1-ABCE-D7D00904AB3D" _
Or Ucase(UserList(i).GUID.tostring) = "B8EC6B82-2C26-4696-AB4B-C507F8CA74CA" _
Or Ucase(UserList(i).GUID.tostring) = "EECC76AE-B264-4CB3-8706-6A07FB8DE68F" _
Or Ucase(UserList(i).GUID.tostring) = "45B3DB35-5F26-46A5-B559-996F1B4B230B" _
Or Ucase(UserList(i).GUID.tostring) = "46D6A38A-5FE6-4411-8B46-2759D18B7E26" _
Or Ucase(UserList(i).GUID.tostring) = "EE3CA46E-FD09-4F42-98D5-F128F02F1CF7" _
Or Ucase(UserList(i).GUID.tostring) = "6D831806-ADB8-42D6-8E10-9AA185C14678" _
Or Ucase(UserList(i).GUID.tostring) = "FBD7A90C-BB39-4D1E-BA21-603ABACC284C" _
Or Ucase(UserList(i).GUID.tostring) = "9403456E-05F3-4A4B-A033-2D459E769751" _
Or Ucase(UserList(i).GUID.tostring) = "3AF96D72-B2DB-4837-BF0E-C1EB901E874B" _
Or Ucase(UserList(i).GUID.tostring) = "D41A58F7-DE34-45BB-8C30-8D04E0532E16" _
Or Ucase(UserList(i).GUID.tostring) = "7314ECEE-C820-49C8-B94A-8563A3E21AA8" _
Or Ucase(UserList(i).GUID.tostring) = "79B309EC-9138-4791-8C5F-E2CEDE3BF6BE" _
Or Ucase(UserList(i).GUID.tostring) = "B6ADD20C-2E55-400D-8234-D834F67F5D26" _
Or Ucase(UserList(i).GUID.tostring) = "BB1EC8A5-79C2-4ECA-B083-D125870D5FE0" _
Or Ucase(UserList(i).GUID.tostring) = "7E762E04-0896-46ED-9DC8-8D31327156B2" _
Or Ucase(UserList(i).GUID.tostring) = "1F9FE2E9-E920-481B-978A-8D70F26529D7" _
Or Ucase(UserList(i).GUID.tostring) = "41C37B37-F9C7-4F67-B13B-F60D43D1E8C8" _
Or Ucase(UserList(i).GUID.tostring) = "9C248608-B3C3-4C3B-87D8-6696F15DBE91" _
Or Ucase(UserList(i).GUID.tostring) = "27877A17-E31C-44BE-9794-7F4F4BCFDA39" _
Or Ucase(UserList(i).GUID.tostring) = "1FE67CF7-0ACC-42C4-BF35-4E8CC7BAE1C4" _
Or Ucase(UserList(i).GUID.tostring) = "91999B36-3E90-4DAC-949D-A329AD285940" _
Or Ucase(UserList(i).GUID.tostring) = "01DE8E56-FA2A-48B8-80F1-CABD5B75EE7E" _
Or Ucase(UserList(i).GUID.tostring) = "FEAEB297-F800-48B4-99C1-3A6EC48BD7D2" _
Or Ucase(UserList(i).GUID.tostring) = "BB8D8F82-EB95-4750-8A10-0B5B8DB34664" _
Or Ucase(UserList(i).GUID.tostring) = "D85BBE7F-9A84-4BA7-9F94-F8F534676BE1" _
Or Ucase(UserList(i).GUID.tostring) = "1707CDB1-2218-443A-A728-223108599E8F" _
Or Ucase(UserList(i).GUID.tostring) = "0670556E-841C-4CBD-88DA-CDD81F78EEC9" _
Or Ucase(UserList(i).GUID.tostring) = "4160EF42-D161-419F-AD94-B670A2B0C266" _
Or Ucase(UserList(i).GUID.tostring) = "7650EECF-6A80-411A-8900-DB6E8563408B" _
Or Ucase(UserList(i).GUID.tostring) = "208E0FB0-3A4F-41D0-A429-D04BDCF7EE1B" _
Or Ucase(UserList(i).GUID.tostring) = "7268586B-7ABB-4938-845F-161B2FB94F69" _
Or Ucase(UserList(i).GUID.tostring) = "F8412116-7B66-4EC5-99AB-856BDB0FE2C2" _
Or Ucase(UserList(i).GUID.tostring) = "F3031B03-327F-4D25-B30C-E3AEA12DFCCD" _
Or Ucase(UserList(i).GUID.tostring) = "C5DEE914-6321-465F-AB80-A966AF3D00B7" _
Or Ucase(UserList(i).GUID.tostring) = "4032CEE7-11D7-40E6-9703-D71F64BAB375" _
Or Ucase(UserList(i).GUID.tostring) = "494076C5-47C3-4682-9D2C-6902432046E2" _
Or Ucase(UserList(i).GUID.tostring) = "624CF02F-95F9-4141-BF3C-BCF61E2E822A" _
Or Ucase(UserList(i).GUID.tostring) = "BA6E4345-3FB8-4AE8-9FD8-172374F9A250" _
Or Ucase(UserList(i).GUID.tostring) = "DA7AEDBE-AC57-4F66-9B5F-1E8ACC4E0E4A" _
Or Ucase(UserList(i).GUID.tostring) = "B7C49A07-8ABA-4325-81B3-610BDFFC3358" _
Or Ucase(UserList(i).GUID.tostring) = "FFD43BED-39D6-49F1-AB76-B84A0DC6E799" _
Or Ucase(UserList(i).GUID.tostring) = "39747F4A-9209-4684-9CD3-39265F2EAF2C" _
Or Ucase(UserList(i).GUID.tostring) = "09D30EC3-B9CF-426F-B41B-C0C81A0B8D0A" _
Or Ucase(UserList(i).GUID.tostring) = "C6C4A895-27D6-40D7-AC4B-B5639791A818" _
Or Ucase(UserList(i).GUID.tostring) = "EBC1DDF5-2F65-446B-A71D-3AE5A1D2EAA9" _
Or Ucase(UserList(i).GUID.tostring) = "2821F5C6-35B3-4633-B496-F755F6EB14F1" _
Or Ucase(UserList(i).GUID.tostring) = "73616D68-4941-40AE-A144-097158D760F0" _
Or Ucase(UserList(i).GUID.tostring) = "F015E635-65A2-48D9-9997-0020E55A4AFB" _
Or Ucase(UserList(i).GUID.tostring) = "507A26E0-010C-4136-A690-11836E2E3C8C" _
Or Ucase(UserList(i).GUID.tostring) = "C8A2214D-BEE3-40D3-BC08-DF235306ED7B" _
Or Ucase(UserList(i).GUID.tostring) = "FE7DAC28-D9D2-4D31-8273-311B3E1DFB0E" _
Or Ucase(UserList(i).GUID.tostring) = "B474B169-07A1-41B3-AB95-48793FA8C241" _
Or Ucase(UserList(i).GUID.tostring) = "959192EB-44CB-4194-9C5A-3C6C15C2B29D" _
Or Ucase(UserList(i).GUID.tostring) = "37A86AE6-364A-48F2-A991-3713ACDB7B1A" _
Or Ucase(UserList(i).GUID.tostring) = "55EEF9D9-E5BC-49AB-AD8E-FC4C1F25FBBA" _
Or Ucase(UserList(i).GUID.tostring) = "094C16A1-CC90-4403-83DC-1A4DFB616EF1" _
Or Ucase(UserList(i).GUID.tostring) = "CDC05AC4-ECE6-47E3-ADDF-920AEC44BEFA" _
Or Ucase(UserList(i).GUID.tostring) = "F619A8BD-F192-401E-9DA6-BBDBBC1BFBF7" _
Or Ucase(UserList(i).GUID.tostring) = "8E0E273D-B34C-46A7-97B2-2C6F3A4D6D72" _
Or Ucase(UserList(i).GUID.tostring) = "E4FD6027-2A09-4B82-B812-6644160636C5" _
Or Ucase(UserList(i).GUID.tostring) = ucase("5aef8344-b54e-412e-8cd9-d03eda410071") _
Or Ucase(UserList(i).GUID.tostring) = ucase("16bc1db1-a38d-4166-9ef4-ab5209dd562d") _
Or Ucase(UserList(i).GUID.tostring) = ucase("8bba5f92-c46f-4379-b59d-89ac3158164a") Then  tmpReturn= 0
'sp 334302               
        Next
    if   TmpArchiveGroup <> 1838  And   TmpArchiveGroup <> 1869 And TmpArchiveGroup <> 1870 Then tmpReturn= 1
        Return tmpReturn
    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, " YYYYYY گروه تبلت  ", Ex.Message)
        Throw ex
        
    End Try
    ''''''''''''''''''
End Function

'heydarpour
'(9:03:03 AM) درخواست های کارتایل , با توجه به فانکشن
'
 'GetRequestLog_ForFormula

'جهت مشاهده گردش پرونده درخواست در فرمول ها
'امکانی فراهم شود تا علاوه بر گردش پرونده درخواست , آن را با جزئیات بیشتر که شامل نام فرم و انجام دهنده و تاریخ و ساعت و نوع فعالیت (مشاهده یا ویرایش) می باشد نمایش داده شود
'(9:03:16 AM) WCF.SC 8.09.25.02

'TaskLogList به لیست اضافه شد


Public Function GetShamsiDate(ByVal DateTime As String) As String



        Dim RDate As String

        Dim tmpPersianCalender = New System.Globalization.PersianCalendar()

        Dim tmpYear = tmpPersianCalender.GetYear(DateTime).ToString()
        Dim tmpMonth = tmpPersianCalender.GetMonth(DateTime).ToString()

        Dim tmpDay = tmpPersianCalender.GetDayOfMonth(DateTime).ToString()


        If tmpMonth < 10 Then tmpMonth = "0" & tmpMonth
        If tmpDay < 10 Then tmpDay = "0" & tmpDay
        RDate = tmpYear + "/" + tmpMonth + "/" + tmpDay

        Return RDate
    End Function

Public Function Holidays(ByVal DateTime As String, ByVal day As Integer) As String


        Dim i As Integer = 0
        Dim RDate As String()
        Dim tmpday As String
        Dim tmpMonth As String
        Dim tmpYear As String
        Dim TmpD As String
        TmpD = DateTime

        Do While i < day
       
            RDate = TmpD.Split("/")
            
            tmpYear = RDate(0)
            
            tmpMonth = RDate(1)
              
            tmpday = RDate(2)
              
 
            if Convert.ToInt32(tmpday) + 1 > 31 And Convert.ToInt32(tmpMonth) < 7 Then
                tmpMonth = Convert.ToInt32(tmpMonth) + 1
                tmpday = Convert.ToInt32(tmpday) + 1 - 31
            ElseIf Convert.ToInt32(tmpday) + 1 > 30 And Convert.ToInt32(tmpMonth) > 6 And Convert.ToInt32(tmpMonth) <> 12 Then
                tmpMonth = Convert.ToInt32(tmpMonth) + 1
                tmpday = Convert.ToInt32(tmpday) + 1 - 30
            ElseIf Convert.ToInt32(tmpday) + 1 > 29 And Convert.ToInt32(tmpMonth) = 12 Then
                tmpMonth = 1
                tmpYear = Convert.ToInt32(tmpYear) + 1
                tmpday = Convert.ToInt32(tmpday) + 1 - 29
            Else

                tmpday = Convert.ToInt32(tmpday) + 1

            End If
            
            If tmpMonth.Length < 2 Then tmpMonth = "0" & tmpMonth.ToString

            If tmpday.Length < 2 Then tmpday = "0" & tmpday.ToString
            TmpD = tmpYear + "/" + tmpMonth + "/" + tmpday
            
            Dim LstHoliday As New List(Of Global.Biz.SA.SrvESUPService.HolidaysAndEvents)
          
          ' LstHoliday = Info8.GetFunctions.GetHolidays()


            'LstHoliday = LstHoliday.where(Function(f) f.HDate = TmpD).tolist
           ' If LstHoliday.Count =0 Then
             If Not  Info8.GetFunctions.GetHolidays_ByDate(TmpD) Then  i = i + 1
        Loop
        
        

        Return TmpD
    End Function
    
Public Function TaeedMTaghsid() as Integer'*2544139
     
 Dim Check_com as Integer =1
 Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
 Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
 Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)
 
 ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
 ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence = 1).ToList '20--*2544139  16
 ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence = 2).ToList	

 
		If ListMangerConfimTaeed.Any Then
			If ListMangerConfimCancel.Any Then
		    	If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
		        	Check_com=0
		    	Else
		    		If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
		            	Dim tmpC as String=ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString
		            	ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate=tmpC).ToList
		            	ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate=tmpC).ToList
		            	If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
		            		Check_com=0
		            	End if
		       		End if
		    	End if 
			Else
				Check_com=0
		   	End if
		End if

 
Return Check_com
End Function


Public Function AddDateForHolidays(ByVal DateTime As String, ByVal day As Integer) As String


        Dim i As Integer = 0
        Dim RDate As String()
        Dim tmpday As String
        Dim tmpMonth As String
        Dim tmpYear As String
        Dim TmpD As String
        TmpD = DateTime

        Do While i < day
            RDate = TmpD.Split("/")
            tmpYear = RDate(0)
            tmpMonth = RDate(1)
            tmpday = RDate(2)

            if Convert.ToInt32(tmpday) + 1 > 31 And Convert.ToInt32(tmpMonth) < 7 Then
                tmpMonth = Convert.ToInt32(tmpMonth) + 1
                tmpday = Convert.ToInt32(tmpday) + 1 - 31
            ElseIf Convert.ToInt32(tmpday) + 1 > 30 And Convert.ToInt32(tmpMonth) > 6 And Convert.ToInt32(tmpMonth) <> 12 Then
                tmpMonth = Convert.ToInt32(tmpMonth) + 1
                tmpday = Convert.ToInt32(tmpday) + 1 - 30
            ElseIf Convert.ToInt32(tmpday) + 1 > 29 And Convert.ToInt32(tmpMonth) = 12 Then
                tmpMonth = 1
                tmpYear = Convert.ToInt32(tmpYear) + 1
                tmpday = Convert.ToInt32(tmpday) + 1 - 29
            Else

                tmpday = Convert.ToInt32(tmpday) + 1

            End If
            If tmpMonth.Length < 2 Then tmpMonth = "0" & tmpMonth.ToString

            If tmpday.Length < 2 Then tmpday = "0" & tmpday.ToString
             TmpD = tmpYear + "/" + tmpMonth + "/" + tmpday
            
            Dim LstHoliday As New List(Of Global.Biz.SA.SrvESUPService.HolidaysAndEvents)
          
          ' LstHoliday = Info8.GetFunctions.GetHolidays()


            'LstHoliday = LstHoliday.where(Function(f) f.HDate = TmpD).tolist
           ' If LstHoliday.Count =0 Then
             If Not  Info8.GetFunctions.GetHolidays_ByDate(TmpD) Then  i = i + 1
        Loop
        
'''''تبدیل به میلادی
		Dim RDateMilady As String()
		RDateMilady=TmpD.Split("/")
        tmpYear = RDateMilady(0)
        tmpMonth = RDateMilady(1)
        tmpday = RDateMilady(2)
        Dim tmpPersianCalender = New System.Globalization.PersianCalendar()
        Dim dt As DateTime
        dt = New DateTime(Convert.ToInt32(tmpYear), Convert.ToInt32(tmpMonth),  Convert.ToInt32(tmpday), tmpPersianCalender)
         
        Return dt
    End Function
    
Private Function GetIdWorkflow (ByVal NIdWorkF as String) as Double
Try 


'Msgbox("WorkFlowGuid" , Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)) 
	Select Case Ucase(NIdWorkF)
        
        
        	Case "097C8C41-BA8C-4B60-8B2F-6BED0221F573","2635FE18-FE4C-4A2D-9D05-993756751262"
			Return  500
			
        
		Case "69C923A3-4908-4828-92E4-5F66712974BA" 
		'ابطال پروانه
			Return  1
		Case "9CEBA576-5413-4D13-AE15-8BF6E0675DCA" 
		'اجاره سرقفلی
			Return  2
		Case "C6BF5550-A4CC-4A3E-AA99-053E7980AA94" 
		'اجاره-آستانه
			Return  3
		Case "FB8C442B-0936-46FD-B512-C858337F2C8D" 
		'اجاره-استیجاره
			Return  4
		Case "093746EF-7371-4C24-BB92-4FED6C3C78BC" 
		'اختصاص معوض
			Return  5
		Case "A9107745-BE25-4B98-BEAD-76E9212C0133" 
		'اداره پست
			Return  6
		Case "83406869-9A8B-4B80-9A13-F74F946FF84F" ,"4F56C548-1CF6-4C12-AC40-4F1319BE03C9"
		'استعلام الکترونیک-شهروندسپاری
			Return  7
		Case "4BFCFAE5-ACF9-4159-9BF1-1B2C0781ED0E"
		'استعلام املاک فاقد سند آستانه قدس 
			Return  8
		Case "4351C20C-643A-4A50-A4BD-AEBCEC5EADF8" 
		'استعلام تجدید اجاره آپارتمان/صنف
			Return  9
		Case "12D2CC3F-5CA8-4237-ABAE-CF524DC4FB6C" 
		'اسناد رهنی
			Return  10
		Case "977CD181-D015-40D6-88C9-E8DB8B96C0B9" 
		'اصلاح پروانه 17
			Return  11
		Case "D3D3E0C9-718C-4B11-9F21-8E584268B7AF"
		'اصلاحیه صلح نامه 
			Return  12
		Case "9F91A6D9-1E14-43AA-9968-39A99798D463" 
		'اقامتی موقت
			Return  13
		Case "37A2F79B-C857-420B-B20E-910EE0899336" 
		'امتيازي داراي اولويت
			Return  15
		Case "31747A10-4CF4-477D-8950-BB8A4D114C55" 
		'انتقال سرقفلی
			Return  16
		Case "2BAF4A84-73B2-4270-8B9F-507238635542" 
		'انتقال سند 10 درصد مسکن و شهرسازی
			Return  17
		Case "70C0E402-67CC-4174-991C-41E62DBB6409" 
		'انتقال سند آستان قدس
			Return  18
		Case "97394C4C-F5DC-4BF0-828C-DA9236A2B8D8" 
		'انتقال سند خرید-فروش
			Return  19
		Case "A847C3D8-2C40-401E-AD14-5607676F1274" 
		'اوقاف
			Return  20
		Case "E2964600-0E81-4584-BD5E-82F8EE51F27F" 
		'ایجاد باقیمانده
			Return  21
		Case "F5F8B45F-A8D0-4384-9ED2-42DC747A1A20" 
		'ایجاد کد آپارتمان
			Return  22
		Case "8F844ABD-2642-4854-9A33-8E61B6F67BA7" 
		'بررسی اطلاعات ملک
			Return  23
		Case "BC07498D-CB42-4D1A-A0C6-BDF66F50065A"
		'برگشت از درآمد 
			Return  24
		Case "0082452E-08E3-432C-9FCE-3E9915F253D1" 
		'پایانکار پیشنهادی
			Return  25
		Case "55C9F836-E48B-4BAA-AC92-7B1C208F6831"
		'پروانه 0 تا 100 
			Return  26
		Case "08C2BE75-FAF3-457E-9B33-C97681FA419D" 
		'پروانه پیشنهادی
			Return  27
		Case "A24A966E-0BCB-432F-8474-4ED76CF93A79" 
		'پروانه توسعه بنا
			Return  28
		Case "BB0854BE-4002-4C0C-975C-04B4B8485334" 
		'پروانه ساختمانی
			Return  29
		Case "CB2A1920-7B97-4D20-9BBE-12D50F40BB5A" 
		'پروانه و تغییرات بنا/تجدید
			Return  30
		Case "7B60BD7D-5116-4B6A-8B6C-8EE303345AD9" 
		'پروانه و تغییرات بنا/تمدید
			Return  31
		Case "F75B08E0-6F1A-4762-8D1F-4271F648DEA0" 
		'تایید سابقه تسویه حساب-بدون گواهی
			Return  32
		Case "1A18A93E-8FC1-44D6-8DED-F1381D9CB126"
		'تجدید اجاره سرقفلی 
			Return  33
		Case "4AAC77C4-1793-4388-BCFC-D46AF38E1FC6" 
		'تجمیع
			Return  34
		Case "9756C5AF-40E2-4CE0-83D7-40D8197B809E" 
		'تحویل و تحول
			Return  35
		Case "770FBF44-31CD-4CF0-9CEA-A8072316EE4F" 
		'تخصیص ملک به شهروند
			Return  36
		Case "64FDC480-B8A6-474E-812B-5F4376DDC31E" 
		'تصرف
			Return  37
		Case "356BAAAB-2627-417A-9AE3-A815CAF505C1" 
		'تصویر سوابق ملک - شهروندسپاری
			Return  38
		Case "51209265-3386-4E15-9CF3-91449BD22AE8" 
		'تفکیک
			Return  39
		Case "D086F80D-A1AF-4761-B516-7B6769222E61" 
		'تمدید اجاره آستان قدس
			Return  40
		Case "3855934D-B726-4B14-BD6A-80B6ADFD4697" 
		'تملک قطار شهری-حفاظت
			Return  41
		Case "933FB7B7-2AD8-4019-8175-7620BEAC8212"
		'تهاتر _ ثبت قرارداد
			Return  42
		Case "093B672D-64FB-470F-8873-7C1500E0379D" 
		'خرید
			Return  43
		Case "1EB0EBD6-481F-4C4B-B7F7-9A8340997FD0" 
		'سهمیه 10 درصد مسکن و شهرسازی
			Return  44
		Case "123F060C-550D-47ED-A10F-1FC66BEBB63D" 
		'شناسایی و تطبیق اراضی مسکونی
			Return  45
		Case "EE472107-62E1-4956-844D-2D7EFA824BB4" 
		'فرآيند جرثقيل هاي برجي
			Return  46
		Case "00724A03-F90E-4704-B8E0-17A071AEEBF8" 
		'فروش از طریق مزایده اراضی
			Return  47
		Case "BFC8ACD2-DA65-482D-AB84-107530914639" 
		'فروش از طریق کارگزار
			Return  48
		Case "0901E98B-4858-46D9-9A9C-219B395E047A" 
		'فروش از طریق کمیسیون عالی معاملات
			Return  49
		Case "B0459D8F-FA24-4739-BD76-3B0A2E0D5239"
		'فروش با دستور
			Return  50
		Case "C4F9A87D-2FA8-4E42-9B74-4480C47B1241" 
		'گواهي انتقال ملک شهروند سپاري جديد
			Return  51
		Case "0D1C1D1A-CD60-4592-BAD3-685B2AFF2D98" 
		'گواهی استحکام بنا
			Return  52
		Case "EE64A610-9142-4E2B-953F-6E65B522A9E1" 
		'گواهی انتقال صنفی
			Return  53
		Case "F9DE191A-959F-4C2E-8092-931FEB3589EB" 
		'گواهی انتقال ملک- شهروندسپاری
			Return  54
		Case "6F00FA9D-02A3-43F9-9365-A7F42E9039E9"
		'گواهی پایانکار بهره برداری - جمع سپاری
			Return  55
		Case "2497B40D-9861-445E-96AD-3588A1613682"
		'گواهی پروانه-بیمه
			Return  56
		Case "2CA83747-919F-4FC2-B3DB-D37BA6272320" 
		'گواهی تجمیع
			Return  57
		Case "86E86F40-F36C-42FE-991F-8DDAD743E6ED","61935FDF-D85C-4946-9BA2-DFB7FC1E1322","14B03C7A-097E-4A2A-AAD2-DB23BC03F377"
		'مجوز بهره برداری دکلهای مخابراتی و ارتباطی
			Return  58
		Case "C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB","B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" 
		'مجوز شروع عملیات ساختمانی
			Return  59
		Case "DBDC01C8-0433-490F-931F-1249D96E607B" 
		'مزایای ورود به محدوده
			Return  60
		Case "DE951DFA-1136-4650-82AE-93DC7B63643D" 
		'مشارکتها
			Return  61
		Case "DA0F95E6-1EF9-4BC0-9593-A252EE9F9939" 
		'ملک در طرح معوض
			Return  62
		Case "9EFC2C97-5C01-4DC6-BEC4-6A56ABCCA5E0" 
		'ملک در طرح نقدي
			Return  63
		Case "69AB3B29-9E2A-4948-945E-C4B9A906E446" 
		'ملک در مسیر
			Return  64
		Case "00000000-0000-0000-0000-000000000000" 
		'نامشخص
			Return  65
		Case "E10EDCBA-7945-4DDD-8306-9BAA37529AE2" ,"905557AD-3CE2-4A6F-A14E-A1F36FD89C23"
		'هیئت فنی
			Return  66
		Case "764125F6-0BDC-441B-8AC4-AA667EA065F6" 
		'واگذاری در قبال بدهی
			Return  67
		Case "53379B94-1411-4DC8-AEE1-012D4A9B43A7","C28A7410-B48A-4199-BA3A-B5254A20C1F5" 
		'آماده سازی
			Return  68
		Case "8445278B-A9E0-47A7-8E5E-D1028FB6DB08","0BC14A0D-B94A-449D-B7AB-28E42852E120" 
		'استعلام الکترونیک
			Return  69
		Case "08BB48AD-8AA1-4BE1-8D81-EA966DAA5035","6CC12A36-D0FE-476B-8BD5-EFBF3B29401A" 
		'استعلام تجدید اجاره
			Return  70
		Case "C6230C76-3DEB-41E3-8F68-DEEA920763D1","30F756C2-2AAF-4543-B98D-FBAB4BC916FD" 
		'اصلاح پروانه - شهرداری
			Return  71
		Case "A1C05500-1D9F-4CDF-B3C5-D26B115CC2D5","3CA97718-4AFE-4752-A892-8B5EF88F176C" 
		'پروانه احداث بنا - شهرداری
			Return  72
		Case "86D5016B-AB5D-4577-B318-791A083AFC5F","1661EA82-19A6-4ED6-9EE1-63B16C5FDA8D" 
		'پروانه تجدید بنا - شهرداری
			Return  73
		Case "D62680B7-5038-4066-B651-73B3F94C225F","457475B0-E461-475E-89FC-1FB85FD41FEC" 
		'پروانه توسعه بنا/ تغییر بهره برداری - شهرداری
			Return  74
		Case "1E386CF1-F6BC-4C10-99B5-03F50EEF8104","5DC809DF-16C5-459B-B6F8-C28A049A32AC" 
		'تایید سابقه قبل از 52
			Return  75
		Case "87783CD5-DCF5-40A6-B4B5-5228B62C4D11","A3B24BD2-4CB2-4011-9900-3070AD088A6F" 
		'تایید سابقه مجاز
			Return  76
		Case "83F0FAC4-2268-4005-8D52-34266DD9F595","8B4AF4AD-A7E7-4330-BD2C-F39B3A07ADFD" 
		'تایید سابقه مجاز پایانکار
			Return  77
		Case "008DF040-922B-41A6-A3B2-7F35B1C870BB","79903151-AF7C-4A73-9F18-6EB915F3293F" ,"25F1F6FA-354F-46B4-BE0F-3497E1F82E20"
		'تایید سابقه مجاز پروانه
			Return  78
		Case "CC1FBC44-7EDD-41E7-A64B-3D6DF21E855E","AA2E5691-CD31-452C-A019-ADE4494EC258" 
		'تجدید - اصلاح پروانه - شهرداری
			Return  79
		Case "72A5E955-CC6D-43E9-811B-C6FF410CA009","8EBA8AAB-A256-4EEC-B1E5-DA115686A9B0" 
		'تعیین بروکف
			Return  80
		Case "27CFF1AD-5936-407E-A420-5B8B71F94566","B96261EE-E426-45D1-9118-41D02210CC14" 
		'تمدید اصلاح پروانه
			Return  81
		Case "CBAD978D-3764-4E51-903C-9A8831B18760","B6A28C29-6761-4719-8B62-C8575F88F572" 
		'گواهی اتحادیه صنفی
			Return  82
		Case "F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9","083062F2-6C96-4F21-8955-0FAE1F367704" 
		'گواهی انتقال آپارتمان / تجارتی
			Return  83
		Case "9A15158D-0369-452C-B73D-E8AC3295FE10","D84FA3D1-54FD-4D24-8000-758A300B2441" 
		'گواهی مفاصا بهره برداری صنفی
			Return  84
		Case "CCCFBD5F-B245-408F-9C74-E2A21170AA2E","1A9B0C43-C8C5-4A08-B2F2-E123CD4C370C" 
		'مجوز احداث مساجد
			Return  85
		Case "994AD909-C2B7-476B-9BE2-8821EA1C9841","5CB085FB-C9CB-4D1E-A98F-DFF8E76A618A" 
		'مجوز تعمیرات
			Return  86
		Case "4C1AAC11-CF11-4867-B467-C4B6DF47F972","FFF07F64-7B35-43BA-B363-95332389273B" 
		'مجوز خدماتی
			Return  87
		Case "5437358A-F86B-48D8-A823-168388815F79","F5CAE479-5301-47B9-A7FB-845B1FBCB2C2" ,"E81BBAEF-ED4B-4E44-9F73-DB45F23BC7F9" 
		'هیئت پارکینگ
			Return  88
		Case "8B26053C-5554-48FB-99FB-62431AA06C07","BA5A32EC-C23B-4E0D-8087-7D083D181644","B54E1F02-AB96-46D7-AAC0-41691054CC96" 
		'اراضی غیرمحصور و رها
			Return  89
		Case "D6F25EBA-45A5-4851-ACED-7EED0105E977","B3407F6F-967D-47DE-A8EB-9A7AB98BDA75","6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9","6E8E4E56-A0E7-458B-9842-E25DDC55D9BC"
		'اصلاح نقشه
			Return  90
		Case "8DCD2E0C-8E5E-48A4-BAD6-8C2A1B4901B1","6B26CCDD-4FF5-4B9D-BF15-128312E7A08A","AE758A56-C5AC-4FC7-AEE9-C24C252F758C" 
		'تایید سابقه تسویه حساب (بدون گواهی)
			Return  91
		Case "0B53D213-CB83-4E5C-9319-C551CCB187E9","3DBF0B9B-7A1A-4459-81F2-6FC6FC208FCF","0C2B0730-504F-44AD-A067-6AF8AA1A3132" 
		'تجدید -اصلاح پروانه
			Return  92
		Case "0BD1CA17-17DE-4F91-8544-250F40105DF2","FCA0F71C-6040-46AA-B927-F3ED908BAE80","DEFFC4C0-D37E-4CC3-94FD-ABA24FAAEAB2" 
		'سایر
			Return  93
		Case "BDD563C8-1EB2-40EC-8811-19DB03908488","BDA49CCE-DFE8-4ECC-8EEC-C6D6A1EB6A76","35CFFB97-67A3-488E-8A7A-BEC93C459B99" ,"3708270C-B7D9-495E-B72A-20D072D26D52"
		'گواهی انتقال آپارتمان - شهروندسپاری
			Return  94
		Case "85DF30CD-BE53-466E-AA76-749A40831BA5","62B24930-E3B9-455E-889F-74B32274C320","DD805F08-DC99-4D16-91AE-7BC17A70330F","3B155802-5BD4-4261-A6F3-B65B5177BF5F" 
		'استعلام ادارات
			Return  95
		Case "06E28FB5-E844-45E4-9FBA-631C1C324081","577CB127-0C16-4F4D-B645-A7B4D0EC713B","5482C28A-025D-40FB-A928-B62EAD670ACE","676E0202-F41E-44E9-98C7-E586072F8C17" 
		'گواهی افراز
			Return  96
		Case "20286CC7-9313-4363-8C70-0C7814B79CC9","17FD1A79-AC10-4B37-8D4F-1FAD7550F2C4","E1AC3D8B-4B4E-4837-A7EE-302CB70F5FA3","E33F2C9A-C0A7-46F5-B4A1-6CBDA0FE419D","AB164972-4C84-445E-A8FD-D576578E24B1" 
		'اصلاح پروانه
			Return  97
		Case "7321AE5D-C65E-48DD-A8D5-3BF4773923E7","D9E55CD5-922B-4D56-80C8-7FE5F7178C95","679C82EB-4227-45D5-966D-A62C7FE580EB","307E2C2F-99DC-4AB3-A6A4-AF7A5D731581","938EE557-4E3B-43EE-A9EE-ED12D17351DC" 
		'پروانه توسعه بنا/ تغییر بهره برداری
			Return  98
		Case "F5B285CC-8792-498C-A6C0-0D1571EEB793","50E0B7EF-E10C-4727-BF96-4399B6A37196","27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7","710510E5-D8B3-459D-9CAC-DBE13C7DB8DE","F57C96A0-697F-410C-AA37-DFDE36A29C47" 
		'پروانه دیوارکشی
			Return  99
		Case "B53B60FC-DB43-40E5-B954-0756DE68C498","1BEA53F3-426C-4423-AF29-1FCD2AE1AA62","2518B6D1-54BB-4C94-BEAC-2BA4D19BB100","5AEC0F88-6381-41F3-A5B6-636C8941886E","C4C75308-5055-460B-999E-D2E7FDDE217E","36D59B70-B2BC-4793-932E-7376877025E0"
		'گواهی مفاصا حساب
			Return  100
		Case "2AE54B83-03DB-4DF5-A983-01A761395252","66B37DC0-8081-4745-ADEA-6514AB47DE6F","FE80BD5C-F87B-495A-8F13-77EE05CBFD6D","01E1D2A4-A299-4C88-8270-8419E81D5977","B825F591-1E4E-4FD0-A9B7-D437C56C9BE4","3D4A5DB8-13C1-47A4-BA37-D49E042A1E5F" 
		'بهره برداری موقت
			Return  101
		Case "7D38106F-7362-4283-BDD9-0AC4E4BE2384","8AEAD964-2A11-4072-9F17-0BAB8E4511C5","CBB8C800-70E6-432B-B338-1F229A365712","88E98ADA-C0F2-48B5-A724-4B455E445303","5BD4AD80-D3A5-4F87-81CB-652363740C1D","16C77278-30A1-4375-80DC-7EE8367B39FD" 
		'تجدید پروانه
			Return  102
		Case "BCE823DF-A854-406F-A881-4983C8447DDD","9E7565F4-21C3-4497-887F-A751274EDEEF","DAACD02A-5B8D-4C40-9FFB-DE24B605D084","8012EFB5-5C8C-4BF4-8F3D-E25168B17A24","E34D362D-D8F9-4481-B1C2-E9F880A08582","7FF7C349-A773-4A45-8F52-F8593B467A96" , "B94C2198-CD35-4F87-85ED-7F1B893BD170" 
		'گواهی تفکیک
			Return  103
		Case "E6C9DB23-6CEA-4161-813E-078193082DD9","5F07A635-B569-47F7-A97F-2AA2C36AFE37","FBE3AD14-934C-4F8F-8BA2-5C07BFB4696D","53E7F83A-59A2-4C0D-805A-7BE86E1DC56D","80134AAF-F070-4706-AC6A-E82778B7029C","DE10D8ED-3989-4E6C-AD37-F5D915BC73D6" ,"C4AFBBD2-5B4D-4451-8A5D-A50D4A1260CE"
		'گواهی عدم خلاف
			Return  104
			
				Case "0358fb22-3da1-4642-a138-0d17ad16635a"
		'??????
			Return  105
			
			
		Case "B617A65E-F4BF-4ACC-AEFA-3133D1AA92F1","1472660E-7313-4FC8-8B1D-455A81478F20","C83F6228-987B-4B5E-8906-56F7CD87EA7C","045BEA33-0A29-4F35-8CC1-672417F49DC1","E530B5D8-6283-4E95-B655-6DB83377A6E7","975E3267-1597-4A3F-884D-E72C42E843CA" 
		'مجوز صدور بهره برداری موقت - صنفی
			Return  106		
		Case "18313B50-F76D-428A-BAE4-134A918B2600","8ECF01D4-A93C-44A7-B52B-4685634C54C6","D8A215F8-2E28-45A0-A826-5ED886F16CCF","68C87B74-097E-4EC0-83D1-7723C403B1F8","F32B14FF-E1E8-479B-8A9B-979BF74F7579","5A659F4C-8DC6-4283-A496-BD20C952A810","E5F5C5A4-F51E-4ED1-80E5-E6A11853C746","097C8C41-BA8C-4B60-8B2F-6BED0221F573","2635FE18-FE4C-4A2D-9D05-993756751262"
		'پروانه احداث بنا
			Return  107		
		Case "3411EFED-3AEF-4619-8DED-1C4D845572C1","26ADD6E3-9EEF-4757-87F2-2E9091AB730C","9660D3B0-B41A-4B2A-BA3F-75F0E5B78085","102D761E-C698-4C9B-A5A9-C6D1B57EBBCC","22F8F7D9-71D9-4FBA-90F5-E43AD0EDC746","AA458943-252C-444F-802E-F038156A208C","214CE534-3326-4C0F-9CF5-F6EC3844B847" 
		'پروانه تجدید بنا
			Return  108		
		Case "EF8464E4-38F5-4AAB-8DB5-094D6B5E1398","032DD2F4-DC70-44F4-88FD-119580EC9884","2C1EE596-5D29-4C0A-802A-1C2492B77A9F","3157211F-C484-48E9-B65C-6BF3271A9F15","7318F0DB-13DD-40E4-BB89-6C4C7A23FF62","1426A8E6-68BC-4B2F-8E82-7C478D5CAB14","CDAA2D6E-1184-4983-A516-A8A2FF92795C" ,"9F4041C0-A9E4-4E26-A724-2A242E4F010A"
		'گواهی انتقال
			Return  109	
		Case "E5F7F86A-0D02-4A01-A01C-19F13F6BEDB9","297949D0-9E25-419E-AD7C-62A7034910A1","EBCCF491-D89D-46E6-BEE7-6F4459C01329","3EBB3B53-972C-425F-9939-80E76AAB6BFB","3636B5FF-0BF6-4CBC-BBD3-875DB4A1BB11","6C67F07C-84DA-41CB-A454-BAC84078EA9F","D0127432-AA14-4A16-BA07-D6A6C7DF5C5D" , "203170E1-218F-447C-92CE-DBC484C26F2B"
		'گواهی پایانکار تفکیکی آپارتمان
			Return  110		
		Case "9D2031E0-8ACE-4CB3-A563-05FC1D42C8DF","82FBD9BC-EBA4-495C-99FF-22B035FB97FF","B0602DD4-FEF9-4AEB-A14A-251B3E86A6D4","01AAA0B7-8193-41D6-84F9-437FA39306A7","04DF58F1-731B-4371-8D53-47435F681362","AB69C90F-FBAF-42FC-9414-7DAE032A1182","11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA","677FBEE4-B05C-4FA6-AB19-CE897739195A" ,"D1B85DEF-3F07-4B29-B99B-BF47D0A283F8"			
		'گواهی پایانکار بهره برداری
			Return  111	
		Case "0358fb22-3da1-4642-a138-0d17ad16635a"
		'??????
			Return  112
			
		' sp 484834 مفاصا جساب 	
		Case "f16712d1-9938-4692-ab59-3cd090d70b8f"	
			Return 113	
			
		
	End Select

Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"GetIdWorkflow", "اجرای فرمول GetIdWorkflow")
End Try
End Function
Public Function TaeedMKaifiat() as Integer
     
 Dim Check_com as Integer =1
 Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
 Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
 Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)
 
 ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
 ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 31 And f.EumManagerConfirmLicence = 1).ToList 
 ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 31 And f.EumManagerConfirmLicence = 2).ToList	

 
		If ListMangerConfimTaeed.Any Then
			If ListMangerConfimCancel.Any Then
		    	If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
		        	Check_com=0
		    	Else
		    		If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
		            	Dim tmpC as String=ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString
		            	ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 31 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate=tmpC).ToList
		            	ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 31 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate=tmpC).ToList
		            	If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
		            		Check_com=0
		            	End if
		       		End if
		    	End if 
			Else
				Check_com=0
		   	End if
		End if

 
Return Check_com
End Function


Public Sub  Logesup1(ByVal N1 as String,ByVal N2 as String )
'	Info8.AddError(BIZ.SA.EumErrorAction.warning,N1,N2) 
'	Dim tmpFileStream As New System.IO.FileStream( "C:\logC\" & N1 & ".txt", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite)
'	Dim tmpBinaryWriter As New System.IO.BinaryWriter(tmpFileStream)
'	tmpBinaryWriter.Write ( N2 )
'	tmpFileStream.Close()
End Sub

Public Function TaeedBime(ByVal TmpListMangerConfimKh as List(Of DtoSC.Sh_ManagerConfirm)) As Integer

    Dim Check_com As Integer = 1
    Try

        If tmpStr <> "" Then

            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = TmpListMangerConfimKh
 
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 20 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 20 And f.EumManagerConfirmLicence = 2).ToList
                      
            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
	                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
	                        Check_com = 0
	                    Else
	                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
	                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString
	
	                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 20 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
	                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 20 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList
	
	                            If ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString Then
	                                Check_com = 0
	                            End If
	                        End If
	                    End If
	                Else
	                    Check_com = 0
	                End If
            Else
                    Check_com = 1
            End If
        End If
    Catch ex As Exception
    
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای  کنترل تائیدیه بیمه صحیح نمیباشد")
        Throw ex

    End Try
    Return Check_com
End Function