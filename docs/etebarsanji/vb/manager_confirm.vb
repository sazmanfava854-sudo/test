<DisplayName("تایید مدیر")> Public Function ManagerConfirm() As clsOut

 Try
 '	time1.start()
' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","aaaa") 

  'If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48")  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "asss", "AA")
'If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","aaaa") 
'  If ucase(Info8.User.UserGuid.tostring) = ucase("6ADBDE3E-1FA3-42D0-AA28-177C4460BCBB")  Then  Exit Function 'sp 433823 kafili
 
 ISTarhJame3=True
    
   ReportArchive=0
   Info8.ParameterList.REMove("NidProc")
   Info8.ParameterList.Add("NidProc", Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc.tostring)
            
  Sara10=Info8.GetRequest.Info.IsSara10Request  
	
 If ucase(Info8.User.UserGuid.ToString()) = "E7D536CF-3FE6-4F23-9C05-CE6EEC87987F" Then Exit Function  'ms Ghafarian
 


'Exit Function

     'Dim LstUserDaramad() AS String
'sp  509194	 LstUserDaramad={"85873E46-6558-41FA-AB16-529EADA31FCF"} 
			LstUserDaramad={"083a32a0-0fe4-45b6-8366-ed07ed0af346"}  'sp 509194
	 '	msgbox("jj:",Info8.GetRevisit_Building.GetBase_Using.count )  
  
 TmpArchiveGroup=0
	'And TmpFlorBaseUsing20.Sum (Function(f) f.UsingArea )<20
    Dim i As Integer = 0
    TypeObjMain = 0
    Dim CI_ResourceManagerConfirm As Integer
        ' if TmpListManger.any() Then
        
    Dim TmpListManger As DtoSC.Sh_ManagerConfirm = Info8.GetManagerConfirm.Sh_ManagerConfirm
   ' msgbox("++a",TmpListManger.CI_ResourceManagerConfirm.tostring)
   
   StrComments=Info8.GetManagerConfirm.Sh_ManagerConfirm.Comments
 
 '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "asssaaاعتبارسنجی", TmpListManger.Comments)
    '	For i=0 to TmpListManger.count -1
    if String.IsNullOrEmpty(TmpListManger.CI_ResourceManagerConfirm.tostring)=True Then
          CI_ResourceManagerConfirm=0
    Else
 	      CI_ResourceManagerConfirm=TmpListManger.CI_ResourceManagerConfirm.tostring
 	End if
 	  
    If TmpListManger.CI_ResourceManagerConfirm.tostring = "" Then CI_ResourceManagerConfirm = 0
       
    If TmpListManger.CI_ResourceManagerConfirm Is Nothing Then CI_ResourceManagerConfirm = 0
       
    If CI_ResourceManagerConfirm = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر" , "لطفا منبع تایید را در ردیف " & i + 1 & " وارد نمایید") :  Exit Function
       
    If TmpListManger.EumManagerConfirmLicence Is Nothing Or TmpListManger.EumManagerConfirmLicence.tostring = "" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "لطفا نوع مجوز را در ردیف " & i + 1 & " وارد نمایید") :  Exit Function
    '	Next
    ' End if
    
    Dim TmPUserIdGroup7 As Integer = 0
    Dim TmPUserIdGroup20 as Integer=0
    Dim NidWDeff7 As String
    Info8.ParameterList.REMove("NidProc")
    Info8.ParameterList.Add("NidProc", TmpListManger.NidProc.tostring)
    
    
    
    if Info8.GetRequest.Info.NidWorkitem = 10083817 Then 
    '	Call elzam()
    End if
    
 If Info8.Getrequest.info.NidWorkItem = 11180569 Then
 	Info8.GetManagerConfirm.Sh_ManagerConfirm.Comments &= " - (دارای سمپا به شماره 526053) "
 	Exit Function  'sp 526053
 End if
  
    
'sp370210 'sp 333438=========================================================================================   
 'sp370210if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="B617A65E-F4BF-4ACC-AEFA-3133D1AA92F1" _  
'sp370210	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="C83F6228-987B-4B5E-8906-56F7CD87EA7C" _
	'sp370210  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="045BEA33-0A29-4F35-8CC1-672417F49DC1" _
	'sp370210  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="E530B5D8-6283-4E95-B655-6DB83377A6E7" _ 
	'sp370210  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="975E3267-1597-4A3F-884D-E72C42E843CA") And TmpListManger.EumManagerConfirmLicence=1 Then
	  'sp370210  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی","به استناد سمپای 333438 تائید مدیر برای این درخواست امکان پذیر نمی باشد.") 
	  'sp370210  Exit Function
'sp370210 End if 
 '==================================================================================================


 ' sp 482460	sp 475317
	Dim LOwner As New List(Of Global.DtoSC.Base_Owner)
	
	
	Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
	TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))
	
	' sp 359885
	
	If TmpNosazi.EumNosaziCodeObjType = 4 Then
		Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
	    Info8.ParameterList.Add("Revisit_Apartment_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    LOwner = Info8.GetRevisit_Apartment.GetBase_Owner()
	   
	ElseIf TmpNosazi.EumNosaziCodeObjType = 5 Then
		Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
	    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    LOwner = Info8.GetRevisit_HouseSharing.GetBase_Owner()
	ElseIf TmpNosazi.EumNosaziCodeObjType = 2 Then
	    Info8.ParameterList.Remove("Revisit_House_NidBase")
	    Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    LOwner = Info8.GetRevisit_House.GetBase_Owner()
	ElseIf TmpNosazi.EumNosaziCodeObjType = 6 Then
	    Info8.ParameterList.Remove("Revisit_Shop_NidBase")
	    Info8.ParameterList.Add("Revisit_Shop_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    LOwner = Info8.GetRevisit_Shop.GetBase_Owner()	    
	Else
		Dim TmpErrorg As New Global.Biz.SA.clsBizError
		Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
	 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)
	 
	    ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 3).ToList
	    Info8.ParameterList.Remove("Revisit_building_NidBase")
	    Info8.ParameterList.Add("Revisit_Building_NidBase", Info8.GetFunctions.GetNidBase(ListParent(0).NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
		LOwner = Info8.GetRevisit_Building.GetBase_Owner()
	End If

	Dim iii as Integer 
	
	
'	if ucase(Info8.GetRequest.Info.Nidworkflowdeff.ToString) = ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") Or  _
'		ucase(Info8.GetRequest.Info.Nidworkflowdeff.ToString) = ucase("24b35887-28e0-45c5-8952-b8e796067cfc") Then
		
'	Else

'sp 509299


'511538 if( Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=4 And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirmdetails=82) _
'511538 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then 

'511538 	Dim Districtt as String
'511538     Dim Tmp As String
'511538     tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
			      				
'511538     Districtt = Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District.tostring
        
    
'511538     if Districtt = "10" or Districtt = "9" or Districtt = "2" Then   
'511538          sp509299 = True
'511538 	 ' GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)
'511538 	  if WorkFlowArchive=1 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop," سمپا 509299 "," دسترسی تایید مدیر تسویه حساب درخواستهای گروه پروانه مناطق 2-9-10 لغو گردیده است ") : Exit Function
'511538         sp509299 = False  ' اگر شرط برقرار نبود برای جلوگیری از مشکلل احتمالی false بشه	
'511538     End if
			        
			        
'511538 End if
'end sp 509299

'sp 516962

'end sp 516962

'sp 514905
if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 And  Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 Then 
'در زمان صدور گواهی در صورتی که  در تحلیل کد 92  و کد 93  وجود داشته باشد.
' در شرایطی که تایید مکاتبات   "تاییدیه قرارداد پارکینگ " در ردیف یک وجود نداشته باشد امکان تایید گواهی نباشد 

    Try

        Dim Cur_AnaliysDetails As New List(Of DtoSC.AnalysisBuilding_Details)

            	Cur_AnaliysDetails = Info8.GetAnalysisBuilding.GetAnalysisBuilding_Details()
            	
            	if Cur_AnaliysDetails.any Then
            		Cur_AnaliysDetails = Cur_AnaliysDetails.where(Function (f) {92,93}.contains(f.Ci_penalty)).tolist
            		if Cur_AnaliysDetails.any Then
		            	if TaeedMMokateb(116) <> 0 Then 
		            		Dim txt = Info8.GetFunctions().GetCodeTitle("CI_IncomeCalculation", Cur_AnaliysDetails(0).Ci_Penalty)
		            		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبار سنجی - 514905",  "با توجه به استفاده از بند " _ 
		            		& txt & " مصوبه 17240 می بایست قرار داد مربوط به پارکینگ تایید شود ")
		            	End if
	            	End if
	            End if	

    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "exeption - sp 514905 ", ex.message)
    End Try

End if 'end sp 514905


if( Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 or Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=4 or _
	Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=5 ) And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then 
		If LOwner.Count > 0 Then
		   For iii = 0 To LOwner.Count - 1
		       If Info8.GetFunctions.CheckInBlackListWithNationalCode(LOwner(iii).NationalCode) Then
					Info8.AddError(BIZ.SA.EumErrorAction.Stop," کد ملی "," در لیست کنترلی بودن کد ملی - sp 486661 ")
					Exit Function
				End If
		   Next
		 End If
End if	 	'end sp 486661
 ' end sp 482460 ' end sp 475317

'sp sp 490785 
if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=33 Then 
	Dim AccessOK as Boolean = False
    Try
        Dim k As Integer = 0
        For k = 0 To Info8.User.UserGroups.Count - 1
            If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("39aeed54-5135-49cb-9f68-72f175de5bff") or _
               ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("4259e59f-4dda-45eb-8c94-60771fe6d8dd") Then  'جامع درآمد- رییس درامد منطقه  , صفا رایانه
            	AccessOK = True
            	Exit For
            End if	
        Next
        if AccessOK = False Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", " فقط رئیس درآمد مناطق مجاز میباشند - sp 490785") : Exit Function
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "مشکل در فرمول تایید مدیران ( تقسیط بند 3 مصوبه 24480) ")
         Throw ex
    End Try
End if
'end sp 490785 


'sp 523551
if ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString)=ucase("b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a") And 1=2 Then
if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then 	
		Dim Nosazi_Temp as guid
		Dim Include_Ci_DocType as Boolean = True
		
		If TmpNosazi.EumNosaziCodeObjType = 5 Then
			Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
		    Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 0, 0, Guid.Empty ).tostring)
		    
			if {5,7,8,11,12,15,18}.contains(Info8.GetParvandeh_HouseSharing().GetBase_CommonEstate().CI_DocType)	Then  'sp 520110
				'طبق سمپا520110 اسناد مشمول و غیرمشمول ارسال شده است
				Include_Ci_DocType = False
			End if			    
		    
			Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
		 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)	
		 	ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList	  
		 	Nosazi_Temp =  ListParent(0).NidNosaziCode 		    
		    
		ElseIf TmpNosazi.EumNosaziCodeObjType = 3 Then
		    Info8.ParameterList.Remove("Parvandeh_House_NidBase")
		    Info8.ParameterList.Add("Parvandeh_House_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 0, 0, Guid.Empty).tostring)
		    
			if {5,7,8,11,12,15,18}.contains(Info8.GetParvandeh_House().GetBase_CommonEstate().CI_DocType)	Then  'sp 520110
				'طبق سمپا520110 اسناد مشمول و غیرمشمول ارسال شده است
				Include_Ci_DocType = False
			End if	
				    
			Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
		 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)	
		 	ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList	  
		 	Nosazi_Temp =  ListParent(0).NidNosaziCode 		    
		End if  
		

	
	if Include_Ci_DocType = True Then				
		
		Dim ReqMade14 As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
		ReqMade14= Info8.GetRequest.GetAllRequestList(Nosazi_Temp)
		
		ReqMade14 = ReqMade14.where(Function (f) f.eumrequeststatus <> 3 And ucase(f.NidWorkFlowDeff.tostring) = "4C052600-BAB2-48F8-A1C7-5DF81741281C").tolist 'ابطال نباشد		  	
		ReqMade14 = ReqMade14.orderByDescending(Function (f) f.requestDate).tolist
		
		
		
		if ReqMade14.any Then
			if ExecSp_Made14(ReqMade14(0).NidProc) Then 
				
			Else
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ماده 14", "کد رهگیری برای درخواست ماده 14 به شماره " & ReqMade14(0).NidWorkItem & " هنوز دریافت نشده است - sp 523551") 
				Exit Function			
			End if
		Else
			Dim ExportParvaneh As DtoSC.Sh_ExportParvaneh 
			ExportParvaneh = Info8.GetParvaneh.GetSh_ExportParvaneh 
			
			if ExportParvaneh is Nothing Then		
			Else
				Dim Date_LastParvaneh = ExportParvaneh.CertificateDate
				if Date_LastParvaneh>"1404/11/14" Then 								
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ماده 14", "شما درخواست ماده 14 ثبت شده با کد رهگیری دریافت شده ندارید - sp 523551") 
					Exit Function
				End	if
			End if		
		End if
	End if		'Include_Ci_DocType
End if
End if'end sp 523551


if ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString)=ucase("4c052600-bab2-48f8-a1c7-5df81741281c") Then
	if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=34 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then

			ElzamNewWebServices					
		'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", "One") : Exit Function
	End if	
End if

if Info8.Getrequest.info.NidWorkItem = 11231222 Then 	'11176445   '11304349
	Call Tamin()
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تامین اجتماعی", "تامین اجتماعی") 
	Exit Function
End if



if Sara10 AndAlso  Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
	'sp 509207
	Dim TaskDate = GetCreatedTaskDate("Activity_03mcpg3")
	Dim TaskDate_tmp = BIZ.SA.ClsCommon.ConvertDateTimeToPersionString((BIZ.SA.ClsCommon.ConvertStringToDateTime(TaskDate)).AddDays(7))
	Dim C_Check = ControlCheck("379d0f82-1af3-41e4-84cd-5f18f645406f")
	if TaskDate_tmp >= BIZ.SA.ClsCommon.CurrentShamsiDateString And C_Check = 0 Then 
		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی - 509207"," تایید فرم جریمه لایحه منوط به بسته شدن مرحله اتش نشانی باشد " ) 
		Exit Function 			
	End if	'sp 509207
End if



 '==================================================================================================
'sp 510525
if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then

    Dim ListPenalty As New List(Of DtoSC.AnalysisBuilding_Details)
    Dim List_Layehe As New List(Of DtoSC.Commission_FinePenalty)
    
     Dim AnlysBuildinfDetails_Date as String="0000/00/00"
     Dim ListLayehe_Date as String="0000/00/00"
    
    ListPenalty = Info8.GetAnalysisBuilding.GetAnalysisBuilding_Details()
    List_Layehe = Info8.GetFine.GetCommission_FinePenalty



	Dim ListPishNevis As New List(Of DtoSC.SrvGetCommissionList_InNidProcResult)
'	ListPishNevis = Info8.GetCommission_List().CommissionList_InNidProc
	
	if ListPishNevis.count>0 Then ListPishNevis=ListPishNevis.where(Function(f) f.IsCancel.tostring="false" And f.CreateDate>=AnlysBuildinfDetails_Date).toList()

	if ListPenalty.count > 0 Then AnlysBuildinfDetails_Date=ListPenalty.Max(Function(f) f.BuildingDate) 

	if List_Layehe.count > 0 Then  ListLayehe_Date = List_Layehe.Max(Function(f) f.HappenDate).tostring
				
	if AnlysBuildinfDetails_Date > ListLayehe_Date Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","تاریخ ایجاد لایحه قبل از ایجاد تحلیل میباشد - 510525") : Exit Function
'	if ListPishNevis.count<=0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","تاریخ ایجاد پیش نویس کمیسیون قبل از ایجاد تحلیل میباشد - 510525") : Exit Function

End if

 '==================================================================================================
 
'sp 477720 
if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 Then 
	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("d1b85def-3f07-4b29-b99b-bf47d0a283f8") _  
	or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("2635fe18-fe4c-4a2d-9d05-993756751262") _
	or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("829936e9-af11-4c1b-adc0-8d519379f729") _
	or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("36d59b70-b2bc-4793-932e-7376877025e0") _ 
	or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f16712d1-9938-4692-ab59-3cd090d70b8f​") Then
		
		' بررسی جاری بودن درخواست 
	'	Dim  TmpListReq As List(Of DtoSC.Sh_RequestInfo)
	' 	TmpListReq = Info8.GetRequest.GetActiveRequestList(Info8.GetRequest.Info.NidNosaziCode)
	' 	TmpListReq = TmpListReq.where(Function(f) ucase(f.NidWorkflowDeff.tostring)= ucase("0BD1CA17-17DE-4F91-8544-250F40105DF2")).tolist
	 	
        Dim TmpParent_STmp As New List(Of Global.Biz.SA.ClsNosaziCodeType)
        Dim TmpParent_SDasTmp As New List(Of Global.Biz.SA.ClsNosaziCodeType)
        TmpParent_STmp = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
        TmpParent_SDasTmp = TmpParent_STmp.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
        Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
        TmpListReq_N = Info8.GetRequest.GetAllRequestList(TmpParent_SDasTmp(0).NidNosaziCode)
        TmpListReq_N = TmpListReq_N.Where(Function(f) ucase(f.NidWorkflowDeff.ToString) = ucase("0BD1CA17-17DE-4F91-8544-250F40105DF2") And (f.EumRequestStatus = 0 Or f.EumRequestStatus = 1)).ToList

	    
	    	 		
	 	if TmpListReq_N.count >0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"طرح تفضیلی "," متقاضی دارای درخواست مزایای طرح تفضیلی میباشد - 477720") : Exit Function		
	End if
End if 

'end sp 
'================================================================================================== 

'sp 475317
'Dim ii as Integer 
'if ucase(Info8.GetRequest.Info.Nidworkflowdeff.ToString) = ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") Or  _
'	ucase(Info8.GetRequest.Info.Nidworkflowdeff.ToString) = ucase("24b35887-28e0-45c5-8952-b8e796067cfc") Then
	
'Else
'Dim LstOwner As New List(Of Global.DtoSC.Base_Owner)
'	If LstOwner.Count > 0 Then
'	   For ii = 0 To LstOwner.Count - 1
'	       If Info8.GetFunctions.CheckInBlackListWithNationalCode(LstOwner(ii).NationalCode) Then
'		   		Info8.GetOnSendTaskParameter.Result ="false" 		
'				TotalS= " در لیست کنترلی بودن کد ملی "
'			End If
'	   Next
'	 End If
'End if	 	
'end sp 475317
'==================================================================================================  
 
 'RE 2755435

 Dim NoContorol_TaeedTaghsid as Boolean=False
 if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="B617A65E-F4BF-4ACC-AEFA-3133D1AA92F1" _  
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="C83F6228-987B-4B5E-8906-56F7CD87EA7C" _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="045BEA33-0A29-4F35-8CC1-672417F49DC1" _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="E530B5D8-6283-4E95-B655-6DB83377A6E7" _ 
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="975E3267-1597-4A3F-884D-E72C42E843CA") And TmpListManger.EumManagerConfirmLicence=3 And CI_ResourceManagerConfirm=3 Then
	  NoContorol_TaeedTaghsid=True
	  'msgbox("NoContorol_TaeedTaghsid:" ,NoContorol_TaeedTaghsid)
 End if

 'sp 482598
 if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
       if  Taghsid(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc,0)=1 Then
			Dim LstMangerCon As New List(Of Global.DtoSC.Sh_ManagerConfirm)
			LstMangerCon = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
			If LstMangerCon.Count > 0 Then LstMangerCon = LstMangerCon.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence=1).ToList
			If LstMangerCon.Count = 0 And NoContorol_TaeedTaghsid=False  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه &", " به دلیل داشتن تقسیط ، پرونده باید تایید تقسیط داشته باشد ") : Exit Function		
		End if	
		
		if TaghsidTaeed28() = False Then 
			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه ", " به ذلیل وجود چک سررسید شده با نوع انتقال به خزانه و عودت امکان تایید ندارید - sp 482598 ") : Exit Function
		End if
End if 'end sp 482598
 
 
'sp 316208=========================================================================================   
'sp 342326
' if (Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=1 or Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=5) And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=2 Then 
' 	Dim LstUserMOdirNew() AS String 					  
'	LstUserMOdirNew={"3a718066-1c33-445e-9b03-e0277f6cc756"} 
'	if LstUserGroupId(LstUserMOdirNew)=0 Then 
	 
'		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی+++","مجاز به ابطال نمیباشید") : Exit Function
' Else

   
'================================================
'sp426232
 if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=27  And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
   if BIZ.SA.ClsCommon.CurrentShamsiDateString >"1402/10/30"  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","با توجه به اتمام مهلت امکان تائید آیتم مذکور وجود ندارد") : Exit Function 'sp 449002
       Dim USERGRP as Integer=Set_UserGroupId
      if Raisesharsazi=False And  ucase(Info8.User.UserGuid.tostring) <> ucase("CB32C037-2DD0-429A-B801-D9F29E6E7507") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی","کاربر گرامي،مجاز به تائيد اصول سه گانه فنی،بهداشتی و شهرداری نمی باشید .") : Exit Function
      Call FnReadLastVoteDate() 'in function dar tahlil sakhteman ast
      msgbox("LastRayDate#:",LastRayDate)
      if LastRayDate>"1400/12/29" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی V","با توجه به تاریخ رای ، پرونده حائز شرایط تائید نمی باشد") : Exit Function
 End if
 '================================================



'sp 409055

if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And Info8.getRequest.Info.IsSara10Request Then
'sp 411938
'sp 412961
  	
  		if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("097C8C41-BA8C-4B60-8B2F-6BED0221F573") or _
  		   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") or _
  		   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("2635FE18-FE4C-4A2D-9D05-993756751262") Then
  		  ' ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("25f1f6fa-354f-46b4-be0f-3497e1f82e20") or _
  		  ' ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a") Then
				if MapGosal() <>"0" Then 
					'if Info8.getRequest.Info.Nidworkitem="10088764" Then 
						Info8.AddError(BIZ.SA.EumErrorAction.Warning,"*اعتبارسنجی","پلاک در لایه حریم گسل های مشهد قرارداد")
  				'	Else
					'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی","پلاک در لایه حریم گسل های مشهد قرارداد") : Exit Function	
					'End if 	
				End if 
		End if

End if 		

 If ucase(Info8.User.UserGuid.ToString()) <> "E7D536CF-3FE6-4F23-9C05-CE6EEC87987F" Then   'ms Ghafarian
'sp 421701
  if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And Info8.getRequest.Info.IsSara10Request Then
  		 
  		if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a") Then
  		   'sp 421701 ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") Then
		  
		  '===================================================================
		  'sp403078
		 
			   if TaeedBime(Info8.GetManagerConfirm.GetSh_ManagerConfirm_List)=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
				  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"تائید مدیر"," با توجه به عدم وجود  کنترل تاييديه بيمه مجاز به تایید  نمیباشید **") 
				  Exit Function
			   End if
		
		  '===================================================================
		  
		Else  
		
	  		Dim LstUserMOdirSara10() AS String
	  		'sp 415726
		    LstUserMOdirSara10={"08d20b53-d68a-4d22-a0e0-b43eec51461e","3a718066-1c33-445e-9b03-e0277f6cc756","782384b8-752e-4540-af0d-e9db3c71d16e","f9623472-adfc-45a9-a1b8-ddd1bffd36a7","8364897d-8836-4004-866d-514959163786","CDD67927-5EE9-4BEB-8C0E-830AF3F90AB0","4259E59F-4DDA-45EB-8C94-60771FE6D8DD","DD1E6DA0-4BA3-41DC-AC1D-85E7BA15D529"} 
			if LstUserGroupId(LstUserMOdirSara10)=0 And (ucase(Info8.User.UserGuid.tostring) <> ucase("89E8DEFF-C832-45D2-A376-1BC6CBEE003E")) And ( Info8.User.UserGuid.ToString() <> "23ecbac0-5bf5-430f-bd25-d4f6f6a0b78d") Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجیa&+++","مجاز به تایید  نمیباشید *") : Exit Function
	   End if 
   End if 
  	  
'sp 367414---ad  8364897d-8836-4004-866d-514959163786  sp  390234 naghibi
'sp 403999

'sp 462674
	if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And Info8.getRequest.Info.IsSara10Request Then
		if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("097C8C41-BA8C-4B60-8B2F-6BED0221F573") or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("2635FE18-FE4C-4A2D-9D05-993756751262") or _
		   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("2635fe18-fe4c-4a2d-9d05-993756751262") or _
		   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") Then
		   	   if (ObligationBime(Info8.GetManagerConfirm.GetSh_ManagerConfirm_List)=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1) And _
		   	   	  (TaeedBime(Info8.GetManagerConfirm.GetSh_ManagerConfirm_List)=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1) Then
			   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"تائید مدیر"," با توجه به عدم وجود  کنترل تاييديه بيمه یا تعهد ارائه تاییدیه بیمه مجاز به تایید  نمیباشید *") 
			   Exit Function
		   End if		
		End if	   
	End if	 
	


  if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And Info8.getRequest.Info.IsSara10Request Then
  		 
  	'	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("097C8C41-BA8C-4B60-8B2F-6BED0221F573") or _
  	if	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("36d59b70-b2bc-4793-932e-7376877025e0") or _
  		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("29d2c864-7bee-4170-85f3-6f3c5a926c57") or _ 
  		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") or _
  		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a") or _
  		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("d1b85def-3f07-4b29-b99b-bf47d0a283f8") or _
  		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("c4afbbd2-5b4d-4451-8a5d-a50d4a1260ce") Then 'or _
  		 '  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") Then
' end sp 462674			  
		  '===================================================================
		  'sp403078
		 'sp 421701
		 'sp 439963 new
		   if TaeedBime(Info8.GetManagerConfirm.GetSh_ManagerConfirm_List)=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
			   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"تائید مدیر"," با توجه به عدم وجود  کنترل تاييديه بيمه مجاز به تایید  نمیباشید") 
			   Exit Function
		   End if
		'sp 421701	  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"تائید مدیر"," با توجه به عدم وجود  کنترل تاييديه بيمه مجاز به تایید  نمیباشید") 
		'sp 421701	  Exit Function
		 'sp 421701  End if
		  '===================================================================
			   if ({"097C8C41-BA8C-4B60-8B2F-6BED0221F573","8D20BE44-78AB-4001-BB94-F064D22B9CE1","2635FE18-FE4C-4A2D-9D05-993756751262"}.contains(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))) Then 
				  		Dim TmpBaseUsing As New List(Of DtoSC.Base_Using)
					    TmpBaseUsing = Info8.GetMovafeghatOsooli.GetBase_Using 
					    if TmpBaseUsing.count=0 Then	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"توجه","دیتای موافقت اصولی را صحیح وارد نمایید") : Exit Function
					    Dim FloorSamen as Integer = TmpBaseUsing.Max(Function(f) f.FloorNo) + (-1) * TmpBaseUsing.Min(Function(f) f.FloorNo) +1 
					    if TmpBaseUsing.Sum (Function(f) f.UsingArea )<5000 And  FloorSamen<11 Then  
					    	Dim LstUserMOdirSara10() AS String
					    	'sp 415726
				    		LstUserMOdirSara10={"08d20b53-d68a-4d22-a0e0-b43eec51461e","3a718066-1c33-445e-9b03-e0277f6cc756","782384b8-752e-4540-af0d-e9db3c71d16e","f9623472-adfc-45a9-a1b8-ddd1bffd36a7","8364897d-8836-4004-866d-514959163786","CDD67927-5EE9-4BEB-8C0E-830AF3F90AB0"} 
							if LstUserGroupId(LstUserMOdirSara10)=0 And (ucase(Info8.User.UserGuid.tostring) <> ucase("89E8DEFF-C832-45D2-A376-1BC6CBEE003E")) And ( Info8.User.UserGuid.ToString() <> "23ecbac0-5bf5-430f-bd25-d4f6f6a0b78d") Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی+","مجاز به تایید  نمیباشید") : Exit Function
						Else
							Dim LstUserMOdirSara10_N() AS String
							'sp 433343  8364897D-8836-4
							'sp 415726
					    	LstUserMOdirSara10_N={"08d20b53-d68a-4d22-a0e0-b43eec51461e","f9623472-adfc-45a9-a1b8-ddd1bffd36a7","8364897D-8836-4004-866D-514959163786"} 
							if LstUserGroupId(LstUserMOdirSara10_N)=0 And (ucase(Info8.User.UserGuid.tostring) <> ucase("89E8DEFF-C832-45D2-A376-1BC6CBEE003E")) Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی&b+++","مجاز به تایید  نمیباشید") : Exit Function	    
				  		End if 
			  End if		
		  		
		Else  
		
	  		Dim LstUserMOdirSara10() AS String
	  		'sp 415726
		    LstUserMOdirSara10={"08d20b53-d68a-4d22-a0e0-b43eec51461e","3a718066-1c33-445e-9b03-e0277f6cc756","782384b8-752e-4540-af0d-e9db3c71d16e","f9623472-adfc-45a9-a1b8-ddd1bffd36a7","8364897d-8836-4004-866d-514959163786","CDD67927-5EE9-4BEB-8C0E-830AF3F90AB0","DD1E6DA0-4BA3-41DC-AC1D-85E7BA15D529"} 
			if LstUserGroupId(LstUserMOdirSara10)=0 And (ucase(Info8.User.UserGuid.tostring) <> ucase("89E8DEFF-C832-45D2-A376-1BC6CBEE003E"))  Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی&c+++","مجاز به تایید  نمیباشید") : Exit Function
	   End if 
   End if 	
   
 End if	  'ms Ghafarian     
   'sp 405959 or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A") 
   if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 Then

    if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("27f2e6c5-498b-47f4-8e1a-b83efdd2eaa7") _  
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("bb0854be-4002-4c0c-975c-04b4b8485334") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("307e2c2f-99dc-4ab3-a6a4-af7a5d731581") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("cb2a1920-7b97-4d20-9bbe-12d50f40bb5a") _ 
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("7b60bd7d-5116-4b6a-8b6c-8ee303345ad9") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("c5e8f6c0-48da-4b3e-8184-4bd6bdd96ceb") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("de10d8ed-3989-4e6c-ad37-f5d915bc73d6") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("5aec0f88-6381-41f3-a5b6-636c8941886e") _ 
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("677fbee4-b05c-4fa6-ab19-ce897739195a") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("3157211F-C484-48E9-B65C-6BF3271A9F15") _
	   Then
	  	Dim LstUserMOdirNew() AS String
	    LstUserMOdirNew={"782384b8-752e-4540-af0d-e9db3c71d16e","08d20b53-d68a-4d22-a0e0-b43eec51461e","3a718066-1c33-445e-9b03-e0277f6cc756","05AED9EE-74D6-4300-A1B3-C7D37CFB15AF"} 'add 05AED9EE-74D6-4300-A1B3-C7D37CFB15AF  sp  350931
		if LstUserGroupId(LstUserMOdirNew)=0 And  ucase(Info8.User.UserGuid.tostring) <> ucase("CB32C037-2DD0-429A-B801-D9F29E6E7507") Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی+++","مجاز نمیباشید") : Exit Function
	ElseIf ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("3b155802-5bd4-4261-a6f3-b65b5177bf5f") _  
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("83406869-9a8b-4b80-9a13-f74f946ff84f") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("4bfcfae5-acf9-4159-9bf1-1b2c0781ed0e") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("08bb48ad-8aa1-4be1-8d81-ea966daa5035") _ 
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("4351c20c-643a-4a50-a4bd-aebcec5eadf8") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("bdd563c8-1eb2-40ec-8811-19db03908488") _
	  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("d0127432-aa14-4a16-ba07-d6a6c7df5c5d") Then
	  	Dim LstUserMOdirNew() AS String
	  	'sp 415726
	    LstUserMOdirNew={"782384b8-752e-4540-af0d-e9db3c71d16e","08d20b53-d68a-4d22-a0e0-b43eec51461e","","8364897d-8836-4004-866d-514959163786","CDD67927-5EE9-4BEB-8C0E-830AF3F90AB0","3a718066-1c33-445e-9b03-e0277f6cc756"} 
		if LstUserGroupId(LstUserMOdirNew)=0  And  ucase(Info8.User.UserGuid.tostring) <> ucase("CB32C037-2DD0-429A-B801-D9F29E6E7507") Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی/////","مجاز نمیباشید") : Exit Function
   
	End if    
' End if 
  
  if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=1 Then
	  	Dim LstUserMOdirNew() AS String
	    LstUserMOdirNew={"782384b8-752e-4540-af0d-e9db3c71d16e","08d20b53-d68a-4d22-a0e0-b43eec51461e","3a718066-1c33-445e-9b03-e0277f6cc756"} 
		if LstUserGroupId(LstUserMOdirNew)=0 Then
			if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=2 Then 
				Dim LstUserMOavenat() AS String 					  
				LstUserMOavenat={"3a718066-1c33-445e-9b03-e0277f6cc756"} 
				if LstUserGroupId(LstUserMOavenat)=0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی+++","مجاز به ابطال نمیباشید") : Exit Function
			Else
		   		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی---","مجاز نمیباشید") : Exit Function
		   	End if	
		End if
 End if 
 
End if 
Msgbox("a",30)
  




if  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("0fc9224b-94da-4156-89c2-c9e7c7acdfda") or _
	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("25f1f6fa-354f-46b4-be0f-3497e1f82e20")  Then
  	  if (Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm<>4)  Then
		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی"," یه دلیل نوع درخواست مجاز به -- تاييد سابقه  مجاز -- می باشید" ) : Exit Function
	  End if
End if   
'==================================================================================================

'sp 461242	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("097C8C41-BA8C-4B60-8B2F-6BED0221F573")
'sp 461242	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1")
'sp 401698
'sp 416809
if  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("29d2c864-7bee-4170-85f3-6f3c5a926c57") or _
	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("2f67d8af-89e3-4738-9ecb-b51d4501d640") or _
	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("66dd0a5f-0833-45d5-8958-ea802c2db18b") or _
	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("829936e9-af11-4c1b-adc0-8d519379f729") or _
	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f12bb40b-67ab-4c9c-a82e-5aae80a76fa2")  Then
		  if (Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3) And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
		     Dim StrN as String= NodeType()
		     if StrN <>"" Then
		     	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی"," یه دلیل عدم پاسخگویی  -- " & StrN & "-- مجاز به بستن نمی باشید" ) : Exit Function
		     End if 
		  End if
End if   
'==================================================================================================


'==================================================================================================
'sp 391513
  if (Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=13 or Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=10 ) And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
     Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی","در حال حاضر امکان تائید مدیر 3% و یا اختلاف ثبت و شهرداری امکان پذیر نمی باشد") : Exit Function
  End if
'==================================================================================================
'sp 352888
  if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=5 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
     Call Fiche_EshghalMabar_OutOfBreakDate
  End if
'==================================================================================================
Msgbox("a",31) 

Dim Usergp as Integer=Set_UserGroupId
if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And  Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1   Then
    ' sp 387945
	if 1=2 Then 	  

    if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= "677FBEE4-B05C-4FA6-AB19-CE897739195A" or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= "11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA" _
	or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="04DF58F1-731B-4371-8D53-47435F681362" or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="AB69C90F-FBAF-42FC-9414-7DAE032A1182" Then
        Dim ClsRoofGarden As New Global.Biz.Communication.ClsRoofGarden
        Dim RoofGardenData As New Global.Biz.Communication.RoofGardenData
        Dim RoofGardenOutput As New Global.Biz.Communication.RoofGardenOutputResult
        Dim TmpParent_STmp As New List(Of Global.Biz.SA.ClsNosaziCodeType)
        TmpParent_STmp = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
	    TmpParent_STmp=TmpParent_STmp.where(Function (f) f.EumNosaziCodeObjType=2).tolist
	
 
        RoofGardenOutput = ClsRoofGarden.GetRequestInfo(Info8.GetFunctions.GetNosaziNickName(TmpParent_STmp(0).NidNosaziCode), "GreenService", "d158aeeb-eaff-4182-88e8-67b47e3076ba")
        
        Dim lstBase_GreenSpace As New List(Of Global.DtoSC.Base_GreenSpace)
        Dim BaseGreenSpace As New Global.DtoSC.Base_GreenSpace
        
       
        BaseGreenSpace.NidBase = Info8.GetFunctions.GetNidBase(TmpParent_STmp(0).NidNosaziCode, Global.Biz.SA.ClsEnum.EumBaseInfoGroup.Revisit, Global.Biz.SA.ClsEnum.EumRevisitGroup.Revisit, Info8.GetRequest.Info.NidRevisit)
        
        BaseGreenSpace.NidGreenSpace = Guid.NewGuid()
        BaseGreenSpace.CI_GreenSpaceType = 2
        
        
        
        Dim ExecutionApprovalDatenew as String ="0"
        
       
              
       Try
        BaseGreenSpace.GreenSpaceValue = RoofGardenOutput.data.ExecutionGreenArea
        ExecutionApprovalDatenew=RoofGardenOutput.data.ExecutionApprovalDate
        Catch ex As Exception
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", ex.message.tostring)
				Exit Function
		End Try 
       
      
        Dim tmpExecutionApprovalDate as String()
    
        if String.IsNullOrEmpty(ExecutionApprovalDatenew)=False Then
        
		        tmpExecutionApprovalDate = RoofGardenOutput.data.ExecutionApprovalDate.Split("T")
		        Dim tmpPersianCalender = New System.Globalization.PersianCalendar()
		        Dim aa As Date
		        
		        aa = tmpExecutionApprovalDate(0).tostring
		        Dim tmpYear As String = tmpPersianCalender.GetYear(aa).ToString()
		        Dim tmpMonth As String = tmpPersianCalender.GetMonth(aa).ToString()
		        Dim tmpDay As String = tmpPersianCalender.GetDayOfMonth(aa).ToString()
		        
		        
		        ExecutionApprovalDatenew=tmpYear & "/" & tmpMonth & "/" & tmpDay
		        ExecutionApprovalDatenew=ExecutionApprovalDatenew & "T" & tmpExecutionApprovalDate(1)
		   
		        BaseGreenSpace.Comments = ExecutionApprovalDatenew & "#" &  RoofGardenOutput.data.ExecutionApprovalStatus
		     
		      
		       lstBase_GreenSpace.Add(BaseGreenSpace)
		    	Info8.GetFunctions.SaveBase_GreenSpace(lstBase_GreenSpace)
		End if    
	End if 
'sp 387945
	End if 
End if	 
  
   ' Exit Function
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''204192
''253446-435240

Msgbox("a",40)	
    Dim ChekCi as Integer=0 
    

'097C8C41-BA8C-4B60-8B2F-6BED0221F573   احداث
'8d20be44-78ab-4001-bb94-f064d22b9ce1   توسعه
'd1b85def-3f07-4b29-b99b-bf47d0a283f8   پایانکار
'c4afbbd2-5b4d-4451-8a5d-a50d4a1260ce   عدم خلاف
'36d59b70-b2bc-4793-932e-7376877025e0  مفاصا
    'if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"DE10D8ED-3989-4E6C-AD37-F5D915BC73D6" And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=23 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "منبع تایید متناسب با نوع درخواست نمی باشد") :  Exit Function
     if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"DE10D8ED-3989-4E6C-AD37-F5D915BC73D6" And _
       	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<> Ucase("097C8C41-BA8C-4B60-8B2F-6BED0221F573") And _
       	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<> Ucase("2635FE18-FE4C-4A2D-9D05-993756751262") And _
        ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<> Ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") And _
        ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<> Ucase("d1b85def-3f07-4b29-b99b-bf47d0a283f8") And _
        ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<> Ucase("c4afbbd2-5b4d-4451-8a5d-a50d4a1260ce") And _
        ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<> Ucase("36d59b70-b2bc-4793-932e-7376877025e0") And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=23 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "منبع تایید متناسب با نوع درخواست نمی باشد") :  Exit Function
    if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=23 Then Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 : ChekCi=1
   
'----sp226829 dar soorati ke takhalofat beyne 86 ta 93 dashte bashad tarikh jarime layehe bayad bad az  1398/07/01  bashad
if TmpListManger.CI_ResourceManagerConfirm =1 And TmpListManger.EumManagerConfirmLicence=1 Then
	Dim Tarikh_Mohasebe as String=""
	Dim List_Layehe As New List(Of DtoSC.Commission_FinePenalty) 
	Dim Date_List_Layehe As New List(Of DtoSC.Commission_FinePenalty) 

	    List_Layehe = Info8.GetFine.GetCommission_FinePenalty
		if List_Layehe.any Then  List_Layehe = List_Layehe.Where(Function(f) f.CI_CommissionFinePenalty =1000).ToList
		if List_Layehe.any Then  Tarikh_Mohasebe= List_Layehe(0).HappenDate
		'---
	    Date_List_Layehe = Info8.GetFine.GetCommission_FinePenalty    
	    if Date_List_Layehe.any Then Date_List_Layehe = Date_List_Layehe.where(Function(f) f.CI_CommissionFinePenalty =3019 or f.CI_CommissionFinePenalty =3021 or  f.CI_CommissionFinePenalty =3022 or  f.CI_CommissionFinePenalty =3023 or f.CI_CommissionFinePenalty =3024 or f.CI_CommissionFinePenalty =2252 or f.CI_CommissionFinePenalty =3046).tolist()   
	    if Date_List_Layehe.any Then Date_List_Layehe = Date_List_Layehe.where(Function(f) f.HappenDate>"1385/12/29" And f.HappenDate<"1394/01/01"  ).tolist()   
	
        'plogfile("Tarikh_Mohasebe:",Tarikh_Mohasebe & "Date_List_Layehe:" & Date_List_Layehe.count)
	    if Tarikh_Mohasebe<"1398/07/01"  And Tarikh_Mohasebe<>"" And Date_List_Layehe.count>0 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر" , " لطفا جریمه لایحه را به روز رسانی فرمائید") :  Exit Function
 End if
 '------
Msgbox("a",50)
'sp 363206 
'if TmpListManger.CI_ResourceManagerConfirm =3 And  TmpListManger.EumManagerConfirmLicence=1 Then
'	Dim BaseUNew As New List(Of Global.DtoSC.Base_Using)
'	Dim BaseU As New List(Of Global.DtoSC.Base_Using)
'	BaseU=Info8.GetRevisit_Building.GetBase_Using
'	BaseUNew= BaseU.where( Function(f) f.FloorNo<80).tolist
'	
'	
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 530 And f.CI_UsingType <> 853 And f.CI_UsingType <> 746 And f.CI_UsingType <> 751 And f.CI_UsingType <> 855 And f.CI_UsingType <> 1002 And f.CI_UsingType <> 1014 And f.CI_UsingType <> 1026).tolist
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2016 And f.CI_UsingType <> 2038 And f.CI_UsingType <> 810 And f.CI_UsingType <> 850 And f.CI_UsingType <> 851 And f.CI_UsingType <> 812 And f.CI_UsingType <> 703 And f.CI_UsingType <> 840).tolist
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 817 And f.CI_UsingType <> 844 And f.CI_UsingType <> 801 And f.CI_UsingType <> 2033 And f.CI_UsingType <> 811 And f.CI_UsingType <> 822 And f.CI_UsingType <> 845 And f.CI_UsingType <> 720).tolist
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 820 And f.CI_UsingType <> 838 And f.CI_UsingType <> 832 And f.CI_UsingType <> 831 And f.CI_UsingType <> 998 And f.CI_UsingType <> 846 And f.CI_UsingType <> 2019 And f.CI_UsingType <> 803).tolist
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 821 And f.CI_UsingType <> 981 And f.CI_UsingType <> 802 And f.CI_UsingType <> 821 And f.CI_UsingType <> 822 And f.CI_UsingType <> 805 And f.CI_UsingType <> 806 And f.CI_UsingType <> 832 And f.CI_UsingType <> 843).tolist
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 111 And f.CI_UsingType <> 833 And f.CI_UsingType <> 431 And f.CI_UsingType <> 2035 And f.CI_UsingType <> 834 And f.CI_UsingType <> 904 And f.CI_UsingType <> 906 And f.CI_UsingType <> 922 And f.CI_UsingType <> 923).tolist
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 967 And f.CI_UsingType <> 813 And f.CI_UsingType <> 814 And f.CI_UsingType <> 827 And f.CI_UsingType <> 828 And f.CI_UsingType <> 854 And f.CI_UsingType <> 994 And f.CI_UsingType <> 813 And f.CI_UsingType <> 814).tolist
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 827 And f.CI_UsingType <> 828 And f.CI_UsingType <> 854 And f.CI_UsingType <> 994 And f.CI_UsingType <> 2036 And f.CI_UsingType <> 847 And f.CI_UsingType <> 933 And f.CI_UsingType <> 753 And f.CI_UsingType <> 999).tolist 
'	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2024 And f.CI_UsingType <> 830 And f.CI_UsingType <> 1013 And f.CI_UsingType <> 829 And f.CI_UsingType <> 930).tolist
'	Dim UsingArea as Double= BaseUNew.sum( Function (f) f.UsingArea)
'	Dim FloorNoNew as Integer = BaseUNew.Max( Function (f) f.FloorNo)  +1 - BaseUNew.Min( Function (f) f.FloorNo)

' 	if (UsingArea>800 or FloorNoNew>6)   Then 	
 '	End if  
'End if

	if TmpListManger.CI_ResourceManagerConfirm =3 or TmpListManger.CI_ResourceManagerConfirm =5 Then 
		if TmpListManger.EumManagerConfirmLicence=1 Then
		 'sp 333614
	   		if TmpListManger.CI_ResourceManagerConfirm =5 Then Call FicheP_Taeed()
	   		
	        Call FicheBazAfrini_No_Taeed()
	   
	      
			Dim tmStr80 As String
			Dim tmpClsNidLib80 As BIZ.SC.ClsNidLib
		   	tmStr80 = Info8.GetRequest.Info.NidRevisit.tostring
			Info8.ParameterList.REMove("NidRevisit")
			Info8.ParameterList.Add("NidRevisit",tmStr80)
			Dim TypeObjT as Integer=2				
			tmpClsNidLib80 = Info8.GetFunctions.GetNidLib(Guid.Parse(tmStr80))
			If Not (tmpClsNidLib80.RHouse Is Nothing) Then TypeObjT = 2
			If Not (tmpClsNidLib80.RDastgah Is Nothing) Then TypeObjT = 5
		    If Not (tmpClsNidLib80.RBuilding Is Nothing) Then TypeObjT = 3
		    If Not (tmpClsNidLib80.RApartment Is Nothing) Then TypeObjT = 4
		    If Not (tmpClsNidLib80.RSenfi Is Nothing) Then TypeObjT = 6
			if TypeObjT=2 or TypeObjT=3 Then 
				Info8.ParameterList.REMove("Revisit_House_NidBase")
				Info8.ParameterList.Add("Revisit_House_NidBase", tmpClsNidLib80.RHouse.NidBase.ToString)
				Dim ki as Integer=0	
		       	For ki=0 to Info8.GetRevisit_House.GetBase_Owner.Count-1
		          if Info8.GetFunctions.CheckInBlackListWithNationalCode(Info8.GetRevisit_House.GetBase_Owner(ki).NationalCode) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه**","مالک با کد ملی " & Info8.GetRevisit_House.GetBase_Owner(ki).NationalCode.tostring & " در لیست سیاه میباشد" )
		        Next
		        if TypeObjT=3 Then
		        	Info8.ParameterList.REMove("Revisit_Building_NidBase")
					Info8.ParameterList.Add("Revisit_Building_NidBase", tmpClsNidLib80.RBuilding.NidBase.ToString)
				End if	
			ElseIf TypeObjT=5 Then 
				Info8.ParameterList.REMove("Revisit_HouseSharing_NidBase")
				Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", tmpClsNidLib80.RDastgah.NidBase.ToString)
				Dim ki as Integer=0	
		       	For ki=0 to Info8.GetRevisit_HouseSharing .GetBase_Owner.Count-1
		          if Info8.GetFunctions.CheckInBlackListWithNationalCode(Info8.GetRevisit_HouseSharing.GetBase_Owner(ki).NationalCode) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه**","مالک با کد ملی " & Info8.GetRevisit_HouseSharing.GetBase_Owner(ki).NationalCode.tostring & " در لیست سیاه میباشد" )
		        Next	
			ElseIf TypeObjT=4 Then
				Info8.ParameterList.REMove("Revisit_Apartment_NidBase")
				Info8.ParameterList.Add("Revisit_Apartment_NidBase", tmpClsNidLib80.RApartment.NidBase.ToString)
				Dim ki as Integer=0	
		       	For ki=0 to Info8.GetRevisit_Apartment.GetBase_Owner.Count-1
		          if Info8.GetFunctions.CheckInBlackListWithNationalCode(Info8.GetRevisit_Apartment.GetBase_Owner(ki).NationalCode) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه**","مالک با کد ملی " & Info8.GetRevisit_Apartment.GetBase_Owner(ki).NationalCode.tostring & " در لیست سیاه میباشد" )
		        Next	 
			End if
		End if
	End if
'''''''''''''''''''''''204192

'  If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a2", time1.ElapsedMilliseconds()) 
Msgbox("a",60)
'-sp205643
if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" or  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("3157211F-C484-48E9-B65C-6BF3271A9F15") ) And TmpListManger.CI_ResourceManagerConfirm =3 And TmpListManger.EumManagerConfirmLicence=1 Then
   Dim N as Integer
   Dim Val34 as Double=0
   Dim Val54 as Double=0
   Dim Val45 as Double=0
   Dim Val49 as Double=0
   Dim Val79 as Double=0
   Dim Val83 as Double=0
   Dim U as Integer=0
   Dim ListPenalty As New List(Of DtoSC.AnalysisBuilding_Details)
       ListPenalty = Info8.GetAnalysisBuilding.GetAnalysisBuilding_Details()
       
   if  ListPenalty.count>0 Then 
	  For N=0 to ListPenalty.count -1  
		 Dim temp As Integer = ListPenalty(N).CI_penalty
		  Select Case temp
			     Case 17,57,42,19,21,20,22,23,24,46,15,47,52
			          U=1
			     Case 34
			          Val34=ListPenalty(N).PenaltyValue
			     Case 54
			          Val54=ListPenalty(N).PenaltyValue
			     Case 45
			          Val45+=ListPenalty(N).PenaltyValue
			     Case 49
			          Val49+=ListPenalty(N).PenaltyValue 
			     Case 79
			          Val79+=ListPenalty(N).PenaltyValue
			    Case 83
			          Val83+=ListPenalty(N).PenaltyValue
		  End Select
	  Next 
	  if U=1 or (Val34-Val54=0) or (Val45-Val49=0) or (Val79-Val83=0) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه"," با توجه به مغایرت بازدید با سابقه مجازملک لذا امکان پاسخگویی به این درخواست نمیباشد "  )
	  
  End if
 

End if

 
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
 if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9" or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="B3407F6F-967D-47DE-A8EB-9A7AB98BDA75" Then Exit Function
 
    NidWDeff7 = Lcase(Info8.getRequest.Info.NidWorkflowDeff.tostring)

    Dim TmPUserIdGroup7_M As Integer = 0
    Dim GroupIn7(), TempStr7 As String
    Dim CurrentGroup7 As String
    Dim UserList7 As New List(Of BIZ.SA.ClsUserGroup)
    UserList7 = Info8.User.UserGroups

    Dim ii As Integer
	
    If UserList7.Any Then
        For ii = 0 To UserList7.Count - 1
            TempStr7 += UserList7(ii).GroupGuid.ToString()
            If ii < UserList7.Count - 1 Then TempStr7 = TempStr7 & ","
        Next ii
    End If
    GroupIn7 = TempStr7.Split(",")
    For Each bb As String In GroupIn7

        If "B7DDF8C9-8EB6-4A8A-A10C-74BA1D1D0808" = Ucase(bb) Then TmPUserIdGroup7_M = 1
        If "52446541-62D2-45E0-AE2E-610C5592CBBF" = Ucase(bb) Then TmPUserIdGroup20 = 1
    Next
    Dim BazdidOk As Boolean

    Dim WorkflowTaskList, TmpWorkflowTaskList As New List(Of BIZ.SC.SrvTaskService.Task)
    if Sara10=False Then   WorkflowTaskList = Info8.GetFunctions().GetRequestLog(TmpListManger.NidProc)
    Dim IsDafaterMsg as Boolean
    
   ''new 2249811

  if   GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=6 Then
  	NotaeedMojaz=True
 	if GuidWorkFlow=200 or GuidWorkFlow=201 or GuidWorkFlow=202 or GuidWorkFlow=203 Then TaeedSabegheMojaz=True
 	
 End if 

 
 'sp208334
  if   GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=6 Then
 	if GuidWorkFlow=151 Then Exit Function
  End if 
  		
  Msgbox("a",70)
 
  'new 226361  خطای محاسباتی و ثبت و شهرداری جمعا 3 در صد 		
  	 if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=10 or  Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=	13 Then
  	 
  	
  	   if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
  	   
  	   
  	   'sp 333473-----------------------------------------------------
          Dim ListPenalt As New List(Of DtoSC.AnalysisBuilding_Details)
          Dim TmpListPenalt As New List(Of DtoSC.AnalysisBuilding_Details)
              ListPenalt = Khalaf() 'ReadAnalisbuilding
              TmpListPenalt=ListPenalt
           if ListPenalt.any Then
              ListPenalt=ListPenalt.where(Function (f) f.CI_penalty=15).tolist
              if ListPenalt.any Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر", " با توجه به وجود آیتم کسری پارکینگ در تحلیل امکان تائید مدیر وجود ندارد")  
           End if
        'sp 359891--------------------------------------------------- 
           if TmpListPenalt.any Then
              TmpListPenalt=TmpListPenalt.where(Function (f) f.CI_penalty=138).tolist
              if TmpListPenalt.any Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر", " با توجه به شناسایی اعیان در مسیر ،امکان تائید مدیر وجود ندارد")  
           End if
           
        'sp 359891--------------------------------------------------- 
                   	Dim TmpA As String
				    TmpA = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
				    Dim TypeObjT As Integer
				    TypeObjT = 0
				    Dim q = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))
				    If Not (q.RDastgah Is Nothing) Then TypeObjT = 5
				    If Not (q.RBuilding Is Nothing) Then TypeObjT = 3
				    If Not (q.RHouse Is Nothing) Then TypeObjT = 3
				    If Not (q.RHouse Is Nothing) Then TypeObjMain = 2
				    If Not (q.RBuilding Is Nothing) Then TypeObjMain = 3
				    If Not (q.RDastgah Is Nothing) Then TypeObjMain = 5
				    If Not (q.RApartment Is Nothing) Then TypeObjMain = 4
				    If Not (q.RSenfi Is Nothing) Then TypeObjMain = 6
				Try
					    Info8.ParameterList.Remove("Revisit_House_NidBase")
					    Info8.ParameterList.Add("Revisit_House_NidBase", q.RHouse.NidBase.ToString)
					    if  Not (q.RDastgah Is Nothing) Then
						    Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
						    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", q.RDastgah.NidBase.ToString)
					    End if
					    if  Not (q.RBuilding Is Nothing) Then
							Info8.ParameterList.Remove("Revisit_Building_NidBase")
						    Info8.ParameterList.Add("Revisit_Building_NidBase", q.RBuilding.NidBase.ToString)
						End if
						if  Not (q.RApartment Is Nothing) Then
							Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
						    Info8.ParameterList.Add("Revisit_Apartment_NidBase", q.RApartment.NidBase.ToString)
						End if
				Catch
					   ' Info8.AddError(BIZ.SA.EumErrorAction.Warning, "a", "a")
				End Try  
				'
				'
                Dim OtherEquipmentRevisit As New List(Of DtoSC.Base_OtherEquipment)
                OtherEquipmentRevisit = Info8.GetRevisit_House.GetBase_OtherEquipment()
                if OtherEquipmentRevisit.any Then
                 If ucase(Info8.User.UserGuid.tostring) = ucase("CB32C037-2DD0-429A-B801-D9F29E6E7507")  Then
                  Msgbox("AllVot_InTotalcod",AllVot_InTotalcod)
                  End if
                  OtherEquipmentRevisit = OtherEquipmentRevisit.where(Function(x) x.CI_OtherEquipmentType = 139).Tolist
                  if OtherEquipmentRevisit.any And AllVot_InTotalcod=False Then
                   Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر", " با توجه به شناسایی تجاوز به معبر عمومی، امکان تائید مدیر وجود ندارد")  
                  End if
                End if
               
  	     '-----------------------------------------------------------
  	     
  	   
  	   Dim hh as Integer
  	   Dim yyy as Integer=0
  	       yyy= FnUsed2DensityK()
  	       
  	       'sp 307513
  	       '  در تایید مدیر نیز در شرایطی که مجوز گروه عدم خلاف و پایانکار باشد ، و در موجود سازه ای با تاریخ احداث بعد از ماکزیمم احداث مجوز شناسایی گردد پیام ذیل نمایش داده شود 
  	       Dim ListBaseUsing  As New List(Of DtoSC.Base_Using)
		   If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.Request.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).EumNosaziCodeObjType = 3 Then
			   ListBaseUsing=Info8.GetRevisit_Building.GetBase_Using
			   if ListBaseUsing.any Then
			      
			      'RE 3991195
			      For hh=0 to ListBaseUsing.count-1
			         if ((ListBaseUsing(hh).ci_usingtype=850 or ListBaseUsing(hh).ci_usingtype=841) And ListBaseUsing(hh).usingarea<5) or (ListBaseUsing(hh).ci_usingtype=930 And ListBaseUsing(hh).usingarea<=12 ) Then ''RE 3991195---ad 930 in sp 379596
			           ListBaseUsing(hh).GenerateDate="1300/01/01"
			         Else
			         End if
			      Next 
			      '
			      
			      Dim MaxDateEhdas as String=""
			      MaxDateEhdas=ListBaseUsing.max(Function(f) f.GenerateDate)
			       
			      if MaxDateEhdas>Date_payankar_Adamkhalaf And String.IsNullOrEmpty(Date_payankar_Adamkhalaf)=False  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*تایید مدیر", " با توجه به احداث بعد از مجوز امکان تایید وجود ندارد  ")  
			   End if
		  End if
  	      '
  	      
  	      if yyy =1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*تایید مدیر", " با توجه به استفاده از تسهیلات 3% در درخواست های قبلی امکان استفاده از این تائید وجود ندارد ")  '3272381
  	      if yyy =2 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*تایید مدیر", "رای کمیسیون دارد امکان تایید وجود ندارد")  '260555
  	      if yyy =3 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*******++تایید مدیر", "متراژ خلاف بیشتر از 3 درصد می باشد امکان تایید وجود ندارد")  '260555
  	      
  	      
  	      
  	   End if
  	 End if 
  Msgbox("",3)	
  
  
   
  ''=============================================================================================	
  'sp 371446 حذف میان طبقه از پروانه ها (اقدام فوری ) -band 2
  'If ucase(Info8.User.UserGuid.tostring) = ucase("CB32C037-2DD0-429A-B801-D9F29E6E7507")  Then
  if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
     if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=1 Then
       
        Dim TmpBaseUsing As New List(Of DtoSC.Base_Using)
        Dim TmpBase805 As New List(Of DtoSC.Base_Using)
        Dim TmpBaseEghamati As New List(Of DtoSC.Base_Using)
        Dim TmpBaseMaskooni As New List(Of DtoSC.Base_Using)
        Dim TmpBaseUsingTm As New List(Of DtoSC.Base_Using)
        Dim TmpBaseUsing_parvaneh As New List(Of DtoSC.Base_Using)
	        TmpBaseUsing = Info8.GetMovafeghatOsooli.GetBase_Using 
	        
	        if TmpBaseUsing.count>0 Then TmpBase805=TmpBaseUsing.where(Function (f) f.ci_usingtype=805).tolist
	        if TmpBaseUsing.count>0 Then TmpBaseUsing=TmpBaseUsing.where(Function (f) f.floorno> 90).tolist
	        
	        'sp 388630
	        if TmpBaseUsing.count>0 Then TmpBaseUsing=TmpBaseUsing.where(Function (f) (f.ci_usinggroup=11 )).tolist
	        '
	        if TmpBaseUsing.count>0 Then TmpBaseEghamati=TmpBaseUsing.where(Function (f) (f.ci_usinggroup=17)).tolist
	        if TmpBaseUsing.count>0 Then TmpBaseMaskooni=TmpBaseUsing.where(Function (f) (f.ci_usinggroup=11 And (f.ci_usingtype=801 or f.ci_usingtype=802 or f.ci_usingtype=805 or f.ci_usingtype=831 or f.ci_usingtype=829 or f.ci_usingtype=830))).tolist
	           '*************************************************************
				
					 Dim tmStrPMain  as String 
					 Dim TmpA As String
					 Dim M_ObjectType As Integer
					 Dim NidBaseTo As Guid
					 Dim tmpNidR As Guid
					 Dim M_TmpPayankarUsing,M_TmpParvanehUsing, TmpListBU,M_TmpPayankarUsingT As New List(Of DtoSc.Base_Using)
					 Dim M_TmpPayankarFront, M_TmpParvanehFront As New List(Of DtoSc.Base_Front)
					 tmStrPMain = Info8.FindParameter("NidProc") 
					 
					   if tmStrPMain="" Then
					 	  Info8.ParameterList.REMove("NidProc")
					   	  Info8.ParameterList.Add("NidProc", Info8.GetFunctions.GetSh_RequestInfo_FromRevisit (Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit).nidproc.tostring )
					   	  tmStrPMain=Info8.FindParameter("Nidproc")
					   End if
					   TmpA = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
					   M_ObjectType = 0
					   Dim q = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))
					    
					   If Not (q.RDastgah Is Nothing) Then M_ObjectType = 5
					   If Not (q.RBuilding Is Nothing) Then M_ObjectType = 3
					   If Not (q.RHouse Is Nothing) Then M_ObjectType = 3
					
					   If Not (q.RHouse Is Nothing) Then M_ObjectType = 2
					   If Not (q.RBuilding Is Nothing) Then M_ObjectType = 3
					   If Not (q.RDastgah Is Nothing) Then M_ObjectType = 5
					   If Not (q.RApartment Is Nothing) Then M_ObjectType = 4
					   If Not (q.RSenfi Is Nothing) Then M_ObjectType = 6
					 
				        Dim NidPayankar, HouseNidPayankar As Guid
				        Dim ReadParvaneh As Boolean = False
				        Dim ReadPayankar As Boolean = False
				        Dim Tmpdto_Savabegh, dto_Savabegh ,Dto_PayankarSavabegh As New List(Of DtoSC.Base_ShahrsaziArchive)
				        Dim M_lastCertificateDate As String
				        Dim M_LastCertificateGroup As Integer
				        Dim NidNosaziCode As Guid
				        NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
					   
				       If M_ObjectType = 5 Then
				            NidBaseTo = Info8.GetFunctions.GetNidBase(NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, BIZ.SA.ClsEnum.EumRevisitGroup.Revisit, tmpNidR)
				            Info8.ParameterList.REMove("Parvandeh_HouseSharing_NidBase")
				            Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", NidBaseTo.ToString)
				            dto_Savabegh = Info8.GetParvandeh_HouseSharing.GetBase_ShahrsaziArchive
				       ElseIf M_ObjectType = 3 Then
				           
				            Dim ListParent As New List(Of BIZ.SA.ClsNosaziCodeType)
				            ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NidNosaziCode, 0)
				            ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
							Info8.ParameterList.REMove("Parvandeh_House_NidBase")
				            Info8.ParameterList.Add("Parvandeh_House_NidBase", ListParent(0).NidBase.tostring())
				 			dto_Savabegh = Info8.GetParvandeh_House.GetBase_ShahrsaziArchive()
				       End If
					  
				      'msgbox("dto_Savabegh*:", dto_Savabegh.count)	
			          If dto_Savabegh.Any Then
				            M_lastCertificateDate = IIf(IsDBNull(dto_Savabegh.Max(Function(f) f.CertificateDate)), "", dto_Savabegh.Max(Function(f) f.CertificateDate))
				            Tmpdto_Savabegh = dto_Savabegh.where(Function(f) f.CertificateDate = M_lastCertificateDate).tolist
				            If Tmpdto_Savabegh.any Then
				                M_LastCertificateGroup = IIf(IsDBNull(Tmpdto_Savabegh.Max(Function(f) f.Ci_ShahrsaziArchiveGroup)), 0, Tmpdto_Savabegh.Max(Function(f) f.Ci_ShahrsaziArchiveGroup))
				                If M_LastCertificateGroup = 1 Then ReadParvaneh = True
				                If M_LastCertificateGroup = 2 Then ReadPayankar = True
				                'If M_LastCertificateGroup = 2 Or M_LastCertificateGroup = 3 Or M_LastCertificateGroup = 4 Then ReadParvaneh = True
				                 msgbox("*:", "ReadParvaneh:" & ReadParvaneh & "ReadPayankar:" & ReadPayankar & "M_LastCertificateGroup:" & M_LastCertificateGroup )	
				            End If
			          End If
					      Dim tmpClsNidLib = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))
					     
					      If  ReadParvaneh = True Then 
								 	    Dim Nidparvaneh As Guid
								        Nidparvaneh = Info8.GetFunctions.GetNidBase(tmpClsNidLib.RHouse.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazParvaneh, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidR)
								        Info8.ParameterList.REMove("MojazParvaneh_House_NidBase")
								    	Info8.ParameterList.Add("MojazParvaneh_House_NidBase", Nidparvaneh.ToString)
							       	    msgbox("",1)
								    	If M_ObjectType=5 Then 
											Nidparvaneh = Info8.GetFunctions.GetNidBase(tmpClsNidLib.RDastgah.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazParvaneh, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidR)
								        	Info8.ParameterList.REMove("MojazParvaneh_HouseSharing_NidBase")
								    		Info8.ParameterList.Add("MojazParvaneh_HouseSharing_NidBase", Nidparvaneh.ToString)
								        End if	
							    	     msgbox("",2)
								        Info8.ParameterList.REMove("MojazParvaneh_House_LoadFunc")
								    	Info8.ParameterList.Add("MojazParvaneh_House_LoadFunc", "")
							    	
								        if Nidparvaneh.ToString<>"00000000-0000-0000-0000-000000000000" Then 
										    If M_ObjectType=5 Then
										        MY_ParvanehUsing = Info8.GetMojazParvaneh_HouseSharing.GetBase_Using 
										    Else
										        MY_ParvanehUsing = Info8.GetMojazParvaneh_House.GetBase_Using
										    End if
									 	End if
							 
						   ElseIf ReadPayankar=True Then 
						   	 
						   	 	        Dim Nidparvaneh As Guid
								        Nidparvaneh = Info8.GetFunctions.GetNidBase(tmpClsNidLib.RHouse.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazPayankar, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidR)
								         msgbox("Nidparvaneh:",Nidparvaneh.tostring)
								        Info8.ParameterList.REMove("MojazPayankar_House_NidBase")
								    	Info8.ParameterList.Add("MojazPayankar_House_NidBase", Nidparvaneh.ToString)
							       	    msgbox("M_ObjectType",M_ObjectType)
								    	If M_ObjectType=5 Then 
											Nidparvaneh = Info8.GetFunctions.GetNidBase(tmpClsNidLib.RDastgah.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazPayankar, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidR)
								        	Info8.ParameterList.REMove("MojazPayankar_HouseSharing_NidBase")
								    		Info8.ParameterList.Add("MojazPayankar_HouseSharing_NidBase", Nidparvaneh.ToString)
								    	ElseIf 	M_ObjectType=3 Then 
								    	    Nidparvaneh = Info8.GetFunctions.GetNidBase(tmpClsNidLib.RBuilding.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazPayankar, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidR)
								        	Info8.ParameterList.REMove("MojazPayankar_Building_NidBase")
								    		Info8.ParameterList.Add("MojazPayankar_Building_NidBase", Nidparvaneh.ToString)
								        End if	
							    	     
								        Info8.ParameterList.REMove("MojazPayankar_House_LoadFunc")
								    	Info8.ParameterList.Add("MojazPayankar_House_LoadFunc", "")
							    	
								        if Nidparvaneh.ToString<>"00000000-0000-0000-0000-000000000000" Then 
										    If M_ObjectType=5 Then
										        MY_ParvanehUsing = Info8.GetMojazPayankar_HouseSharing.GetBase_Using
										    Else
										        MY_ParvanehUsing = Info8.GetMojazPayankar_Building.GetBase_Using
										    End if
									 	End if
						   	 	
					       End If
					       
					       					       					      
					       msgbox("MY_ParvanehUsing",MY_ParvanehUsing.count)
					       
	           '*************************************************************
	           'READ Fiche sp 372132 تدقیق وضعیت نیم طبقه (میان طبقه ) در چیدمان های پروانه (فوری) 
			        Dim Str_Income As String
			        Dim TmpAllFiche As New List(Of DtoSC.Income_Fiche)
			        Dim AllFiche_Tahator As New List(Of DtoSC.Income_Fiche)
			        Dim AllFiche_GHateyat As New List(Of DtoSC.Income_Fiche)
			        Dim AllFiche_Zero As New List(Of DtoSC.Income_Fiche)
			        Info8.Income_Fiche=Nothing
			        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
			
			        Dim Tmp As String
			        tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
						      				
			        If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District <> 80 Then
			           Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116,143,150,157,163"
			        Else
			            Str_Income = "1,7,10,11,143,150,157,162,163"
			        End If
			
			        Dim TmpArr() As String
			        Dim intList_Income As New list(Of Integer)
					Dim Taeed as Integer =1
			        TmpArr = Str_Income.Split(",")
			        For i = 0 To TmpArr.Count - 1
			            intList_Income.Add(TmpArr(i))
			        Next i
			   		Dim Hasfiche_befor14000829 as Boolean=False  
			   		Dim kindGp as Integer=0     
			        if TmpAllFiche.Any Then TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
			        '
			        '
			        
			        'RE 4159538 لطفا در شرايط عدم شناسايي فيش شهرداري با مبلغ و فيش پرداختي اول قطعي و فيش تهاتر ، تاريخ فيش شهرداري آخر ( غير از پروانه ) ملاک عمل قرار بگيرد 
			        If TmpAllFiche.Any Then AllFiche_Zero = TmpAllFiche.Where(Function(f) (f.EumFicheStatus = 3 And f.Payable=0) ).ToList()
			        '
			        '
			        If TmpAllFiche.Any Then AllFiche_Tahator = TmpAllFiche.Where(Function(f) (f.CI_IncomeAccountGroup=157) ).ToList()
			        If AllFiche_Tahator.count>0 Then
			           AllFiche_Tahator = AllFiche_Tahator.Where(Function(f) (f.UserConfirmDate<="1400/08/28") ).ToList()
			         End if
			        msgbox("",6)
			        '
			        '
			         TmpAllFiche= TmpAllFiche.Where(Function(f)(f.CI_IncomeAccountGroup <> 157)).ToList()
					If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5 or f.EumIncomePaymentType=4) ).ToList()
					If TmpAllFiche.Any Then AllFiche_GHateyat=TmpAllFiche 
					
					If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.BankPaymentDate<="1400/08/29") ).ToList()
					If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.Payable>0) ).ToList()
					if TmpAllFiche.count>0 Then Hasfiche_befor14000829=True 
					'	              
	                if TmpBaseUsing.count>0 Then TmpBaseUsingTm=TmpBaseUsing.where(Function (f) f.ci_usinggroup=23 or f.ci_usinggroup=31 or f.ci_usinggroup=18 or f.ci_usinggroup=12 or f.ci_usinggroup=37).tolist
	                if TmpBaseUsingTm.count>0 Then  kindGp=1 'دارای کاربری میان طبقه حمل و نقل انبار، صنعتی، کارگاهی ، تجاری و نمایشگاهی میباشد   
	                '
	                'sp 372132
	                'در شرایط شراسایی فیش تایید شده با مالکلیت "مبلغ تهاتر" تاریخ تایید دستی کنترل شود اگر قبل از تاریخ 28/8/1400 باشد تایید مدیر و محاسبه درآمد بلا مانع 
	                Dim HasFiche_Tahator as Boolean=False
			        if  AllFiche_Tahator.any Then  HasFiche_Tahator=True
	                '
	                '
	                 
	                'در شرایطی که تاریخ پرداخت بانک بعد از 29/8/1400 باشد اما صدور قطعی فیش قبل از این تاریخ انجام شده باشد و فیش در مهلتت باشد . امکان انجام محاسبه درآمد و تایید مدیر ( تایید گواهی ) فراهم شود 
					'sp 372132
					Dim Hasfichesefr as Boolean=False
					Dim Flag_ExportPermanentDate as Boolean=False
					If AllFiche_GHateyat.Any Then
					 AllFiche_GHateyat = AllFiche_GHateyat.Where(Function(f) (f.BankPaymentDate>"1400/08/29" And f.ExportPermanentDate<"1400/08/29" And f.PaymentBreakDate>=f.BankPaymentDate And f.Payable>0)).ToList() 'sp 372132
					 if AllFiche_GHateyat.Any Then Flag_ExportPermanentDate=True
					End if
					'
					''RE 4159538 لطفا در شرايط عدم شناسايي فيش شهرداري با مبلغ و فيش پرداختي اول قطعي و فيش تهاتر ، تاريخ فيش شهرداري آخر ( غير از پروانه ) ملاک عمل قرار بگيرد 
					  msgbox("",7)
	                if AllFiche_Zero.any  And (Hasfiche_befor14000829=False And HasFiche_Tahator=False And Flag_ExportPermanentDate=False )Then
	                 msgbox("",8)
	                  'msgbox("AllFiche_Zero:",AllFiche_Zero.count)
	                  Dim  tempSort = From f In AllFiche_Zero Order By f.ExportTemporaryDate Descending.ToList
	                  AllFiche_Zero=tempSort
	                  Dim AllCalculation As New List(Of DtoSC.Income_Calculation)
	                  Dim nidxx as guid
	                  Dim Datemovaghat as String=""
	                  For i=0 to AllFiche_Zero.count-1
	                     
	                        AllCalculation.Clear()
			                Info8.ParameterList.REMove("NidIncome")
			                Info8.ParameterList.Add("NidIncome", AllFiche_Zero(i).NidIncome.tostring)
	                        AllCalculation = Info8.GetIncome().GetIncome_Calculation_InNidIncome(AllFiche_Zero(i).NidIncome)
	                        If AllCalculation.Any Then AllCalculation=AllCalculation.Where(Function(f) (f.CI_IncomeCalculation=999999)).ToList() 
	                        msgbox("",9)
	                        msgbox("AllCalculationRRR",AllCalculation.count)
	                        If AllCalculation.Any Then
	                        Dim desc as String=""
	                       if String.IsNullOrEmpty(AllCalculation(0).Comments) =False Then
	                          desc=AllCalculation(0).Comments.tostring
	                       End if
	                       
	                        
			                        'Dim desc as String=AllCalculation(0).Comments.tostring
			                        
			                        if String.IsNullOrEmpty(desc) =False Then 
			                           if InStr(desc, "پروانه")<> 0 Then
			                           Else
			                               nidxx=AllFiche_Zero(i).NidIncome
			                               Exit For
			                           End if
			                        Else
			                          nidxx=AllFiche_Zero(i).NidIncome
			                          Exit For
			                        End if
	                        End if
	                  Next
	                   msgbox("",10)	                   
	                 if AllFiche_Zero.any Then AllFiche_Zero=AllFiche_Zero.Where(Function(f) (f.NidIncome=nidxx)).ToList() 
	                    'msgbox("AllFiche_Zero.count B:",AllFiche_Zero.count)
	                    if AllFiche_Zero.any Then Datemovaghat=AllFiche_Zero(0).ExportTemporaryDate
	                    'msgbox("Datemovaghat:",Datemovaghat)
	                    if String.IsNullOrEmpty(Datemovaghat) =False Then  
	                       if Datemovaghat<"1400/08/29" Then Hasfichesefr=True
	                    End if
						'msgbox("Hasfichesefr:",Hasfichesefr)
	                 End if
	                '
	                '
	                msgbox("",11)
	           '*************************************************************
	        if MY_ParvanehUsing.count>0 Then MY_ParvanehUsing=MY_ParvanehUsing.where(Function (f) f.floorno> 90).tolist
	        'msgbox("Hasfichesefr:","TmpBaseUsing:" & TmpBaseUsing.count & "MY_ParvanehUsing:" & MY_ParvanehUsing.count & "kindGp:" & kindGp & "Hasfiche_befor14000829:" & Hasfiche_befor14000829 )
	        '
	        '
	        'sp 381610 میان طبقه در کاربری اقامتی 	 
	        Dim IsNimFN_EghamatiMaskooni  as Boolean=False
	        if TmpBase805.count>0 And (TmpBaseEghamati.count>0 or TmpBaseMaskooni.count>0 ) Then IsNimFN_EghamatiMaskooni=True
	        
	        '
	        '
	         msgbox("",9)
	        msgbox("Hasfichesefr:","TmpBaseUsing:" & TmpBaseUsing.count & "MY_ParvanehUsing:" & MY_ParvanehUsing.count & "kindGp:" & kindGp & "Hasfiche_befor14000829:" & Hasfiche_befor14000829)
	        if (TmpBaseUsing.count>0  And  MY_ParvanehUsing.count=0 And kindGp=0) And Hasfiche_befor14000829=False And HasFiche_Tahator=False And Flag_ExportPermanentDate=False And Hasfichesefr=False And Info8.GetRequest.Info.NidWorkItem.tostring <> 3908000 Then 'RE 3908000 Dastoor Hozamoavanat
	            if IsNimFN_EghamatiMaskooni=False Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر", "با توجه به اعلام میان طبقه در  موافقت اصولی ، تایید گواهی امکان پذیر نمی باشد") 
	            Exit Function
	        End if
     End if
  End if
  'End if
  ''=============================================================================================	
'SP483879
 
 '  if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then'1970422 =1
  
  	 ' if Check_DoubleRow=True Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"توجه- سمپای 483879","کاربر گرامی به دلیل وجود تخلف تکراری در جریمه لایحه امکان تایید وجود ندارد "  ) 
'	End if      
 ''=======================================================================

   'new 188350	
   if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=9 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then'1970422 =1
            
            'sp 385629  ابطال مصوبه تبدیل آرای قلع به جریمه
            '   
            Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "با عنایت به ابطال مصوبه تعیین تکلیف احکام معوق قلع طبق دادنامه 140009970905813270 امکان تایید وجود ندارد") 
		    Exit Function	
		    '
		    '
      	
         if (ucase(Info8.User.UserGuid.tostring)=ucase("6f99c240-c92b-4fb3-99db-6760750167eb") or LstUserGroupId(LstUserDaramad)=1 )Then
             'sp 246399
            ' Info8.AddError(BIZ.SA.EumErrorAction.warning,  "تایید مدیر", " امکان تایید مدیر احکام معوق و اجرا نشده قلع  وجود ندارد ") 
         Else
   
		   	' Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر", " امکان تایید مدیر احکام معوق و اجرا نشده قلع  وجود ندارد ") 
		   	' Exit Function	
	   	 End if    
   End if 
 	 
   '199503 '202715
  if  ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="356BAAAB-2627-417A-9AE3-A815CAF505C1" And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm =3 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*تایید مدیر", "امکان تایید گواهی برای این درخواست نمی باشد") : Exit Function	    
  if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=3 And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm<>12 _  
  And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm<>21 Then 
    if ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"E10EDCBA-7945-4DDD-8306-9BAA37529AE2" And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
    And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" Then '294625
  		    Dim tmStr80 As String
			Dim tmpClsNidLib80 As BIZ.SC.ClsNidLib
		   	tmStr80 = Info8.GetRequest.Info.NidRevisit.tostring
			
			Info8.ParameterList.REMove("NidRevisit")
			Info8.ParameterList.Add("NidRevisit",tmStr80)
						
									
			tmpClsNidLib80 = Info8.GetFunctions.GetNidLib(Guid.Parse(tmStr80))
			Info8.ParameterList.REMove("Revisit_House_NidBase")
			Info8.ParameterList.Add("Revisit_House_NidBase", tmpClsNidLib80.RHouse.NidBase.ToString)
			
  		Dim Str_Income As String
  		If Info8.GetRevisit_House.GetBase_NosaziCode.District <> 80   Then
    		Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116,163"
   		Else
    		Str_Income = "1,7,10,162,163"
    	End If

    	Dim TmpArr() As String
    	Dim intList_Income As New list(Of Integer)

    	TmpArr = Str_Income.Split(",")
    	For i = 0 To TmpArr.Count - 1
        	intList_Income.Add(TmpArr(i))
    	Next i

    	Dim M_AllFiche As New List(Of DtoSc.Income_Fiche)
    	Dim M_AllFicheExport As New List(Of DtoSc.Income_Fiche)
        Info8.Income_Fiche=Nothing
    	M_AllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
    	M_AllFicheExport=M_AllFiche.where(Function (f) f.EumFicheStatus=0 or f.EumFicheStatus=1).tolist
       if M_AllFicheExport.count>0 Then 	Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*تایید مدیر", "لطفا تمام فیش ها را تعیین تکلیف کنید (فیش صادرشده نداشته باشد )") : Exit Function	    
        
        
        If M_AllFiche.Any Then M_AllFiche = M_AllFiche.Where(Function(f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5)).ToList()

	    If M_AllFiche.Any Then
	        M_AllFiche = (From AA In M_AllFiche Join B In intList_Income On AA.CI_IncomeAccountGroup Equals B Select AA).ToList
	    End If
	    M_AllFiche = M_AllFiche.Distinct().ToList()
	   
	    Dim MDate as String ="" 
	    Dim Dif as Integer=0
		If M_AllFiche.Any Then MDate=M_AllFiche.max(Function(f) f.BankPaymentDate)
 		if MDate<>"" Then Dif= DifDate(BIZ.SA.ClsCommon.CurrentShamsiDateString)- DifDate(MDate)
    	if Dif< 365  Then 
    	         
    	         'sp 334224
			     if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="B617A65E-F4BF-4ACC-AEFA-3133D1AA92F1" _  
				  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="C83F6228-987B-4B5E-8906-56F7CD87EA7C" _
				  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="045BEA33-0A29-4F35-8CC1-672417F49DC1" _
				  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="E530B5D8-6283-4E95-B655-6DB83377A6E7" _ 
				  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="975E3267-1597-4A3F-884D-E72C42E843CA") Then
			    	
    	          Else
    					Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*تایید مدیر", "امکان عدم تایید با توجه به داشتن فیش تایید شده در تاریخ " & MDate & " وجود ندارد") : Exit Function	    
    			 End if
    	Else
                 'sp 334224
    	         if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="B617A65E-F4BF-4ACC-AEFA-3133D1AA92F1" _  
				  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="C83F6228-987B-4B5E-8906-56F7CD87EA7C" _
				  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="045BEA33-0A29-4F35-8CC1-672417F49DC1" _
				  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="E530B5D8-6283-4E95-B655-6DB83377A6E7" _ 
				  or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="975E3267-1597-4A3F-884D-E72C42E843CA") Then
			    	
    	          Else
			    	    '191033
			    		if  MDate >"1396/09/00" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر", "امکان عدم تایید با توجه به داشتن فیش تایید شده در تاریخ " & MDate & " وجود ندارد") : Exit Function	    
			      End if
    	End if	

	End if
  End if  	
   	   	
    	 Msgbox("",66666)
    	 Msgbox("Lst",LstUserGroupId(LstUserDaramad))
    	  	 
  '------203153--------req 2596829 
    
   	if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=16 And LstUserGroupId(LstUserDaramad)<>1  Then
  	
   	Dim TmpA As String
    TmpA = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
    Dim TypeObjT As Integer
    TypeObjT = 0
    Dim q = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))
    
    If Not (q.RDastgah Is Nothing) Then TypeObjT = 5
    If Not (q.RBuilding Is Nothing) Then TypeObjT = 3
    If Not (q.RHouse Is Nothing) Then TypeObjT = 3

    If Not (q.RHouse Is Nothing) Then TypeObjMain = 2
    If Not (q.RBuilding Is Nothing) Then TypeObjMain = 3
    If Not (q.RDastgah Is Nothing) Then TypeObjMain = 5
    If Not (q.RApartment Is Nothing) Then TypeObjMain = 4
    If Not (q.RSenfi Is Nothing) Then TypeObjMain = 6
  	
Try
    Info8.ParameterList.Remove("Revisit_House_NidBase")
   
    Info8.ParameterList.Add("Revisit_House_NidBase", q.RHouse.NidBase.ToString)
   
    if  Not (q.RDastgah Is Nothing) Then
	    Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
	    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", q.RDastgah.NidBase.ToString)
    End if
     
    if  Not (q.RBuilding Is Nothing) Then
    
		Info8.ParameterList.Remove("Revisit_Building_NidBase")
	    Info8.ParameterList.Add("Revisit_Building_NidBase", q.RBuilding.NidBase.ToString)
	End if
	 
	if  Not (q.RApartment Is Nothing) Then

		Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
	    Info8.ParameterList.Add("Revisit_Apartment_NidBase", q.RApartment.NidBase.ToString)
	End if
	
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "a", "zzz")
End Try

    
   
    If (TypeObjMain = 2 Or TypeObjMain = 3 Or TypeObjMain = 5)  Then
        'if Info8.User.UserGuid.tostring = "6f99c240-c92b-4fb3-99db-6760750167eb" Then
        Dim M_OtherEQ, M_OtherEQMap As New List(Of Dtosc.Base_OtherEquipment)
        If Not (q.RHouse Is Nothing) Then
            If Not (q.RDastgah Is Nothing) Then
                Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
                Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", q.RDastgah.NidBase.ToString)
            End If
            If Not (q.RBuilding Is Nothing) Then
                Info8.ParameterList.Remove("Revisit_Building_NidBase")
                Info8.ParameterList.Add("Revisit_Building_NidBase", q.RBuilding.NidBase.ToString)
            End If
            If Not (q.RMapControl Is Nothing) Then
                Info8.ParameterList.Remove("MapControl_NidBase")
                Info8.ParameterList.Add("MapControl_NidBase", q.RMapControl.NidBase.ToString)
            End If
            Dim MaxIncomedate, TahlilDate, TahlilTime, DateMapControl, TimeMapControl, DateBazdid, TimeBazdid As String
            M_OtherEQ = Info8.GetRevisit_House.GetBase_OtherEquipment()
            If Not (q.RMapControl Is Nothing) Then M_OtherEQMap = Info8.GetMapControl.GetBase_OtherEquipment()
            If Not (q.RBuilding Is Nothing) And TypeObjT = 3 Then M_OtherEQ = M_OtherEQ.Concat(Info8.GetRevisit_Building.GetBase_OtherEquipment).Tolist
            If Not (q.RDastgah Is Nothing) And TypeObjT = 5 Then M_OtherEQ = M_OtherEQ.Concat(Info8.GetRevisit_HouseSharing.GetBase_OtherEquipment).Tolist


            TahlilDate = Analys()
            TahlilTime = AnaliysParvaneh_Time

            If M_OtherEQMap.any Then
                Dim TmpList As New List(Of Dtosc.Base_OtherEquipment)
                TmpList = M_OtherEQMap.where(Function(f) f.CI_OtherEquipmentType = 250).tolist()
                If TmpList.count > 0 Then
                    DateMapControl = iif(isdbnull(TmpList.Max(Function(f) f.OtherEquipmentValue)), "1300/00/00", TmpList.Max(Function(f) f.OtherEquipmentValue))
                    TmpList = TmpList.where(Function(f) f.OtherEquipmentValue = DateMapControl).tolist()
                    If TmpList.count > 0 Then TimeMapControl = iif(isdbnull(TmpList.Max(Function(f) f.Comments)), "", TmpList.Max(Function(f) f.Comments))
                End If
            End If


            If (DateMapControl > TahlilDate Or (DateMapControl = TahlilDate And TimeMapControl > TahlilTime)) And TahlilDate > "1300/00/00" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "کاربر گرامی ، ترتیب محاسبات رعایت نشده است") :  Exit Function
            'با توجه به تغییر اطلاعات کنترل نقشه لطفا تحلیل را مجدد محاسبه نمایید


            If M_OtherEQ.any Then
                Dim TmpList As New List(Of Dtosc.Base_OtherEquipment)
                TmpList = M_OtherEQ.where(Function(f) f.CI_OtherEquipmentType = 250).tolist()
                If TmpList.count > 0 Then
                    DateBazdid = iif(isdbnull(TmpList.Max(Function(f) f.OtherEquipmentValue)), "1300/00/00", TmpList.Max(Function(f) f.OtherEquipmentValue))

                    TmpList = TmpList.where(Function(f) f.OtherEquipmentValue = DateBazdid).tolist()
                    If TmpList.count > 0 Then TimeBazdid = iif(isdbnull(TmpList.Max(Function(f) f.Comments)), "", TmpList.Max(Function(f) f.Comments))
                End If
            End If
                                    
            If (DateBazdid > TahlilDate Or (DateBazdid = TahlilDate And TimeBazdid > TahlilTime)) And TahlilDate > "1300/00/00" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "کاربر گرامی ، ترتیب محاسبات رعایت نشده است") :  Exit Function
            'با توجه به تغییر اطلاعات بازدید لطفا تحلیل را مجدد محاسبه نمایید

            '''''''''''''''''''''''''''''''''
            If TahlilDate > "1300/00/00" Then
                Dim ListIncome As New List(Of DtoSC.Income)
                ListIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
                If ListIncome.Count > 0 Then MaxIncomedate = IIf(IsDBNull(ListIncome.Max(Function(f) f.CreateDate)), "1300/00/00", ListIncome.Max(Function(f) f.CreateDate))
                
			'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "aa#*" & TahlilDate, MaxIncomedate)
                If TahlilDate > MaxIncomedate Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه#*", "کاربر گرامی ، ترتیب محاسبات رعایت نشده است") :  Exit Function
                'با توجه به تغییر اطلاعات تحلیل لطفا درآمد را مجدد محاسبه نمایید
            End If

            'Msgbox("tayeed Date", "Income:" & MaxIncomedate & "TahlilDate:" & TahlilDate & "TahlilTime :" & TahlilTime & " DateMapControl:" & DateMapControl & "DateBazdid:" & DateBazdid & " TimeBazdid:" & TimeBazdid)

        End If
       
    End If
   	   	
   	
   	End if
   '--------------	
   	
    Msgbox("",15)       	
    '''''''''''''''''''''''''''''''''''''تایید تقسیط 
   

   if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=16 And (LstUserGroupId(LstUserDaramad)<> 1 or ucase(Info8.User.UserGuid.tostring) = ucase("CB32C037-2DD0-429A-B801-D9F29E6E7507")) Then 'mohamadi-moghadam sp 201899
   	msgbox("a","00")
   		if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="1" Then 	Call ComfirmFicheAmozesh() 'sp217502 'chek fiche amoozeshParvaresh
   			msgbox(1,"")
   		Call FicheBazAfrini_No_Taeed 'chek fiche bazafrini 'sp 333605
   		msgbox(2,"")
   		Call Fiche_Train_No_Taeed 'chek fiche ghatar shahri 'sp 333605
   		msgbox(3,"")
   		Call Fiche_BarghashtAzSeporde_No_Taeed 'chek fiche bargashtAzseporde 'sp 333605
   		msgbox(4,"")
   		Call Fiche_payAvaleGhatee_No_Taeed 'chek fiche pay avale ghatee 'sp 333605
   		msgbox(5,"")
   		Call Fiche_EshghalMabar_No_Taeed 'chek fiche eshghal Mabar 'sp 333605
   		msgbox(6,"")
   		Call TaghsidTaeed() 'chek fiche shhrdari 'sp 333605
   		
   End if 
   ''
   Msgbox("",7)
   
   if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=2 And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=16 And LstUserGroupId(LstUserDaramad)<>1 Then 'mohamadi-moghadam sp 221908
   		Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر", "مجاز به ابطال تایید تقسیط نمی باشید") : Exit Function	    
   End if 

   '------------------------------------------------------
   '162520
   If  TmpListManger.CI_ResourceManagerConfirm = 18 Then
   			Dim tmStr80 As String
			Dim tmpClsNidLib80 As BIZ.SC.ClsNidLib
		   	tmStr80 = Info8.GetRequest.Info.NidRevisit.tostring
			Info8.ParameterList.REMove("NidRevisit")
			Info8.ParameterList.Add("NidRevisit",tmStr80)
						
									
			tmpClsNidLib80 = Info8.GetFunctions.GetNidLib(Guid.Parse(tmStr80))
			Info8.ParameterList.REMove("Revisit_House_NidBase")
			Info8.ParameterList.Add("Revisit_House_NidBase", tmpClsNidLib80.RHouse.NidBase.ToString)
					
   		if Info8.GetRevisit_House.GetBase_NosaziCode.District <> 80 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر @", "مجاز به انتخاب این ایتم نمی باشید")	: Exit Function
   		if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=1 Then 	
				Dim LstMangerCon As New List(Of Global.DtoSC.Sh_ManagerConfirm)
			    LstMangerCon = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
			    If LstMangerCon.Count > 0 Then LstMangerCon = LstMangerCon.Where(Function(f) f.CI_ResourceManagerConfirm = 5 And f.EumManagerConfirmLicence = 2).ToList
			    If TaeedMMovafeghat() = 1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر*", "مجاز به انتخاب این ایتم نمی باشید") : Exit Function
			    Dim TmpBaseUsing As New List(Of DtoSC.Base_Using)
	            TmpBaseUsing = Info8.GetMovafeghatOsooli.GetBase_Using 
	            Dim FloorSamen as Integer = TmpBaseUsing.Max(Function(f) f.FloorNo) + (-1) * TmpBaseUsing.Min(Function(f) f.FloorNo) +1 
	            if TmpBaseUsing.Sum (Function(f) f.UsingArea )<5000 And  FloorSamen<11 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر$", "مجاز به انتخاب این ایتم نمی باشید") : Exit Function
			    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		Else
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "تایید مدیر#", "مجاز به انتخاب این ایتم نمی باشید") : Exit Function	    
		End if
   End if 
  '162520 
  
   If TmpListManger.EumManagerConfirmLicence = 2 And TmpListManger.CI_ResourceManagerConfirm = 3 Then  ' هنگام ابطال(2) - گواهی(3) 
    Dim GavahiS as Integer =0
	Dim tmpClsNidLib As BIZ.SC.ClsNidLib
    Dim tmstr As String = String.Empty
    tmStr = Info8.GetRequest.Info.NidRevisit.tostring
     
    tmpClsNidLib = Info8.GetFunctions.GetNidLib(Guid.Parse(tmstr))
	Dim ListOutPut As New List(Of DtoSC.SrvGetOutputListResult)
		 
    ListOutPut = Info8.GetOutputs.GetOutputList(2, tmpClsNidLib.RMainCodeInfo.District, tmpClsNidLib.RMainCodeInfo.Region, tmpClsNidLib.RMainCodeInfo.Block, tmpClsNidLib.RMainCodeInfo.House, tmpClsNidLib.RMainCodeInfo.Building, tmpClsNidLib.RMainCodeInfo.Apartment, tmpClsNidLib.RMainCodeInfo.Shop,"")
        
    ListOutPut = ListOutPut.Where(Function(f) f.NidProc = TmpListManger.NidProc And f.OutputStatusEng = "Export" And f.OutputEngTitle <>"Output_MovafeghatOsooli").ToList
   
      
    
    If ListOutPut.Count >0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", " /*لطفا ابتدا صدور گواهی را ابطال نمایید") :  Exit Function
End if 


   
  
    
   '------------------------------------------------------
    '''''''''''''''''''''''''''''''''''''''''''بدهی نوسازی
    '--sp200695
       
 if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 And (Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 or Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=5) Then '292031



'------------------275950
'275950if  ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" Then 
'275950'		Dim tmpSMSMelk As New BIZ.SC.SrvSMSService.ClsParameters
'275950		tmpSMSMelk.BizCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
'275950		tmpSMSMelk.Number = Info8.GetRequest.Info.Cellphone
'275950		tmpSMSMelk.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
'275950		tmpSMSMelk.SMSType = "تایید مدیر/انتقال ملک"
'275950		tmpSMSMelk.Text =   "شهروند محترم درخواست شما به شماره  " + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf + "توسط منطقه تائید شد.برای پرداخت عوارض نوسازی و صدور گواهی به درخواست پیشین در منوی خدمات اختصاصی مراجعه نمائید. شهرداری مشهد " 
'275950		tmpSMSMelk.UserID = Info8.User.UserGuid.ToString
'275950		tmpSMSMelk.UserName = Info8.User.UserName
'275950		Dim smsErrorResult As New BIZ.SA.ClsErrorResult
'275950		smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMSMelk)
'275950End if 
'---------------------------------275950
'200931
 
	IF GuidWorkFlow = 118 Then Info8.AddError(BIZ.SA.EumErrorAction.Warning, "تایید مدیر", "در صورت عدم وجود تایید آتش نشانی یا نظام مهندسی یا سایر الزامات حتما در گواهی قید گردد")
'198264



 if ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="032DD2F4-DC70-44F4-88FD-119580EC9884" or  ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" or  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("3157211F-C484-48E9-B65C-6BF3271A9F15")  Then
 	
 	Dim dto_SavabeghTmp, TmpSavabeghTmp As New List(Of DtoSC.Base_ShahrsaziArchive)
	Dim TmpParent_STmp As New List(Of Global.Biz.SA.ClsNosaziCodeType)
	Dim TmpParent_SDasTmp As New List(Of Global.Biz.SA.ClsNosaziCodeType)
	
	TmpParent_STmp = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
	TmpParent_SDasTmp=TmpParent_STmp.where(Function (f) f.EumNosaziCodeObjType=3).tolist
	Dim Anteghal as Integer =0
	Dim TmpBaseUsing_A As New List(Of DtoSC.Base_Using)

	if TmpParent_SDasTmp.count>0 Then 
	
		'''''''''''''''''''
		
		Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	    
	    TmpListReq_N= Info8.GetRequest.GetAllRequestList(TmpParent_SDasTmp(0).NidNosaziCode )
	    ''200726
	    'sp 344885 And f.RequestDate>"1397/10/01" 
	    if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 And f.RequestDate>"1397/10/01" ).tolist ''ابطال نباشد 	 
	    Dim jj as Integer
	    
	    For jj =0 to TmpListReq_N.count-1 
	    
	    	if Taghsid(TmpListReq_N(jj).nidproc,1)=2 Then
	    
	    		if UnTaeedTaghsit()=1 Then
	    			 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*توجه اعتبار سنجی", "این ملک دارای چک سررسید نشده میباشد") : Exit Function
	    		Else
	    			Info8.AddError(BIZ.SA.EumErrorAction.warning, "*توجه اعتبار سنجی", "این ملک دارای چک سررسید نشده میباشد") 
	    		End if
	    		
	    	End if
	    Next
	
		'''''''''''''''''''''''''''''''''''''
		Info8.ParameterList.REMove("Parvandeh_Building_NidBase")
    	Info8.ParameterList.Add("Parvandeh_Building_NidBase", TmpParent_STmp(0).NidBase.ToString)
    	TmpBaseUsing_A = Info8.GetParvandeh_Building.GetBase_Using 
		Anteghal=1
	End if	
	

	TmpParent_SDasTmp=TmpParent_STmp.where(Function (f) f.EumNosaziCodeObjType=5).tolist

	If TmpParent_SDasTmp.count>0 Then
		
		Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	    
	    TmpListReq_N= Info8.GetRequest.GetAllRequestList(TmpParent_SDasTmp(0).NidNosaziCode )
	    ''200726
	    if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 ).tolist ''ابطال نباشد 	 
	    Dim jj as Integer
	    For jj =0 to TmpListReq_N.count-1 
	    	if Taghsid(TmpListReq_N(jj).nidproc,1)=2 Then
	    		if UnTaeedTaghsit()=1 Then 	
	    			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "#توجه اعتبار سنجی", "این ملک دارای چک سررسید نشده میباشد") : Exit Function
	    		Else
	    			Info8.AddError(BIZ.SA.EumErrorAction.warning, "*توجه اعتبار سنجی", "این ملک دارای چک سررسید نشده میباشد") 
	    		End if	
	    	End if
	    Next
	
		TmpParent_STmp=TmpParent_STmp.where(Function (f) f.EumNosaziCodeObjType=5).tolist
	    Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
	    Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", TmpParent_STmp(0).NidBase.ToString)
		dto_SavabeghTmp =Info8.GetParvandeh_HouseSharing.GetBase_ShahrsaziArchive()
		TmpBaseUsing_A = Info8.GetParvandeh_HouseSharing.GetBase_Using 
		Anteghal=1
	Else

		TmpParent_STmp=TmpParent_STmp.where(Function (f) f.EumNosaziCodeObjType=2).tolist
		Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	    
	    TmpListReq_N= Info8.GetRequest.GetAllRequestList(TmpParent_STmp(0).NidNosaziCode )
	    ''200726
	    if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 ).tolist ''ابطال نباشد 	 
	    Dim jj as Integer
	    	
	    For jj =0 to TmpListReq_N.count-1 
	    
	    	if Taghsid(TmpListReq_N(jj).nidproc,1)=2 Then
	    		if UnTaeedTaghsit()=1 Then 	
	    			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "!@توجه اعتبار سنجی" & UnTaeedTaghsit() , "این ملک دارای چک سررسید نشده میباشد") : Exit Function
	    		Else
	    			Info8.AddError(BIZ.SA.EumErrorAction.warning, "*توجه اعتبار سنجی", "این ملک دارای چک سررسید نشده میباشد") 
	    		End if	
	    	End if
	    Next
		
	    Info8.ParameterList.Remove("Parvandeh_House_NidBase")
	    Info8.ParameterList.Add("Parvandeh_House_NidBase", TmpParent_STmp(0).NidBase.ToString)
	    dto_SavabeghTmp = Info8.GetParvandeh_House.GetBase_ShahrsaziArchive()
	                
   End If
   
   dto_SavabeghTmp=dto_SavabeghTmp.where(Function(f) (f.CI_ShahrsaziArchiveGroup=2 or f.CI_ShahrsaziArchiveGroup=3) And f.CI_ShahrsaziArchiveKind<> 140 And f.CI_ShahrsaziArchiveKind<> 200 And f.CI_ShahrsaziArchiveKind<> 201 And f.CI_ShahrsaziArchiveKind<> 202 And f.CI_ShahrsaziArchiveKind<> 203 ).tolist	
  '198264
  	'206072							
  ' if dto_SavabeghTmp.count=0 And Anteghal=1 And TmpBaseUsing_A.count>0 Then 
  '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه اعتبار سنجی", "به دلیل نداشتن سابقه شهرسازی امکان تایید مدیر نمی باشد") : Exit Function
  '   Exit Function
  ' End if    
 '206072	
 End if
 
 
 
  
Msgbox("",100)
'''''''''''''''''''''''''''''''''167710
'sp275950

if ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="4BFCFAE5-ACF9-4159-9BF1-1B2C0781ED0E" or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" _
or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="83406869-9A8B-4B80-9A13-F74F946FF84F" or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="BDD563C8-1EB2-40EC-8811-19DB03908488" Then 

	if Info8.GetRequest.Info.ActiveNidZabeteh=Guid.Empty Then
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*تایید مدیر", "به دلیل نداشتن ضابطه امکان تایید مدیر نمی باشد") :  Exit Function
	ElseIf Info8.GetRequest.Info.ActiveNidZabeteh.tostring="" Then 
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "@تایید مدیر", "به دلیل نداشتن ضابطه امکان تایید مدیر نمی باشد") :  Exit Function
	End if	
	Try
		Info8.ParameterList.REMOVE("NidZabeteh")
		Info8.ParameterList.REMOVE("NidNosaziCode")
		Info8.ParameterList.Add("NidZabeteh", Info8.GetRequest.Info.ActiveNidZabeteh.ToString)
		Info8.ParameterList.REMove("Revisit_House_NidBase")
		Dim tmpClsNidLibZabteh As BIZ.SC.ClsNidLib				
		tmpClsNidLibZabteh = Info8.GetFunctions.GetNidLib(Info8.GetRequest.Info.NidRevisit)
		Info8.ParameterList.Add("Revisit_House_NidBase", tmpClsNidLibZabteh.RHouse.NidBase.ToString)
		Info8.ParameterList.Add("NidNosaziCode", Info8.GetRevisit_House.GetBase_NosaziCode.NidNosaziCode.ToString)
    
		'RE 3897241  if  Info8.GetZabeteh(True).GetZabeteh_Details.Count<1  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", ".. درخواست اعلام ضابطه ندارد ")    :  Exit Function 'new And TypeObjMain<>4 2416833
		'RE 3897241
		 if  Info8.GetZabeteh(True).GetZabeteh.CI_PlanType<1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", ".. درخواست اعلام ضابطه ندارد ")    :  Exit Function 
		
	Catch
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "* درخواست اعلام ضابطه ندارد")
	End Try
End if	
'''''''''''''''''''''''''''''''''''''''''''

'if TaghsidTaeed()=1 Then

if  Taghsid(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc,0)=1 Then
	Dim LstMangerCon As New List(Of Global.DtoSC.Sh_ManagerConfirm)
	LstMangerCon = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
	If LstMangerCon.Count > 0 Then LstMangerCon = LstMangerCon.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence=1).ToList
	If LstMangerCon.Count = 0 And NoContorol_TaeedTaghsid=False Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", " به دلیل داشتن تقسیط ، پرونده باید تایید تقسیط داشته باشد ") : Exit Function

End if
Msgbox("",200)


'''''''''''''''''''''''BlackList
If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 4  Then  'sp 492403
	if ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") And _
		ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>ucase("2635fe18-fe4c-4a2d-9d05-993756751262") Then 	
			Dim CodeN As List(Of Global.Biz.SA.ClsNosaziCodeType)
			Dim iiN as Integer
			CodeN = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
			
			For iiN=0 to CodeN.count-1 
				Dim LstBlack As New List(Of Global.DtoSA.Sh_BlackList)
			    LstBlack = Info8.GetFunctions.CheckInBlackList(CodeN(iiN).NidNosaziCode)
			    For i = 0 To LstBlack.Count - 1
			    	If LstBlack(i).EumBlackListControl = 2 Or LstBlack(i).EumBlackListControl = 1 Then
			        	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "پرونده در لیست سیاه است") :  Exit Function
			            Exit Function
			        End If
			    Next
			Next 
	End if
			
Else

	Dim CodeN As List(Of Global.Biz.SA.ClsNosaziCodeType)
	Dim iiN as Integer
	CodeN = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
	
	For iiN=0 to CodeN.count-1 
		Dim LstBlack As New List(Of Global.DtoSA.Sh_BlackList)
	    LstBlack = Info8.GetFunctions.CheckInBlackList(CodeN(iiN).NidNosaziCode)
	    For i = 0 To LstBlack.Count - 1
	    	If LstBlack(i).EumBlackListControl = 2 Or LstBlack(i).EumBlackListControl = 1 Then
	        	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "پرونده در لیست سیاه است") :  Exit Function
	            Exit Function
	        End If
	    Next
	Next 	 
End if	'end sp 492403

Dim tempJob as String =""
'--------------------------
	
		if  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="1472660E-7313-4FC8-8B1D-455A81478F20" or _
			Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="975E3267-1597-4A3F-884D-E72C42E843CA" or _
			Ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)="E530B5D8-6283-4E95-B655-6DB83377A6E7" Or _
			Ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)="C83F6228-987B-4B5E-8906-56F7CD87EA7C" Then
			' ======================
			
			 Dim tmpBase_JobInfo as  New list(of DtoSC.Base_JobInfo)	  
			 Dim ErsalComm20 as Integer=100
			 Dim tmStraa As String
			 Dim tmpClsNidLibaa As BIZ.SC.ClsNidLib
			 Try
		     	Dim Fi as Integer=0
		        Dim tempyear As New PersianCalendar
		        Dim currentyear As Integer = tempyear.GetYear(Now)
		        
		         
		        Try
					tmStraa = Info8.GetRequest.Info.NidRevisit.tostring
					Info8.ParameterList.REMove("NidRevisit")
					Info8.ParameterList.Add("NidRevisit",tmStraa)
					
					Info8.ParameterList.Remove ("NidProc")
					Info8.ParameterList.Add("NidProc",Info8.GetRequest.Info.NidProc.tostring)
				
					tmpClsNidLibaa = Info8.GetFunctions.GetNidLib(Guid.Parse(tmStraa))
					Info8.ParameterList.REMove("Revisit_Job_NidBase")
					Info8.ParameterList.Add("Revisit_Shop_NidBase", tmpClsNidLibaa.RSenfi.NidBase.ToString)
				
		        	
		            tmpBase_JobInfo = Info8.GetRevisit_Shop.GetBase_JobInfo  
		           
		      	Catch
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Tryارسال پرونده", "job") :  Exit Function
				End Try 
		 
		               For Fi=0 to tmpBase_JobInfo.count -1  
		              
		                   If tmpBase_JobInfo(Fi).JobDeActivateDate = "" Or  tmpBase_JobInfo(Fi).JobDeActivateDate is Nothing  Or tmpBase_JobInfo(Fi).JobDeActivateDate > BIZ.SA.ClsCommon.CurrentShamsiDateString  Then
		                    Dim temp As Integer = tmpBase_JobInfo(Fi).CI_JobName
							tempJob=tmpBase_JobInfo(Fi).NidJob.tostring
		                    Select Case temp
		
		                        Case 2,10,15,16,18,23,27,30,34,41,53,56,58,72,85,91,105,106,115,128,138,139,
		                         140,143,146,151,154,157,183,191,192,194,196,208,219,221,225,234,237,241,
		                         249,253,261,264,266,270,272,283,299,333,356,370,371,380,381,387,388,397,
		                         407,438,439,440,441,445,446,448,449,450,451,456,479,484,488,492,493,494,
		                         495,496,497,499,500,502,519,525,543,547,553,562,576,581,584,592,593,605,
		                         612,613,619,625,645,655,663,671,672,777,778,779,780,793,805,812,813,838,839                   
		                      
		                            ErsalComm20=20 
		                        Case Else
		                            ErsalComm20=0 ' عدم ارسال به ک م 20 
		                    End Select
		              End if      
		      	Next 
			 	if ErsalComm20=100 Then
     				ErsalComm20=0
        	
			    End if
			    
		    Catch ex As Exception
		    End Try
			' =========================
			if ErsalComm20=20 Then
				if TaeedMComm20() = 1 Then
					if TmpListManger.CI_ResourceManagerConfirm <> 5 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید بند 20", "به علت نداشتن تایید مدیر بند 20 مجاز به تایید گواهی نمی باشید") :  Exit Function
				End if 
			End if	
		End if		
	Msgbox("",300)	

'270850 if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<> "35CFFB97-67A3-488E-8A7A-BEC93C459B99"
if  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<> "E10EDCBA-7945-4DDD-8306-9BAA37529AE2" Then 
'msgbox("ty:","t" )
'295365
'msgbox(" tttt(0):", Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).split("-")(0))

		if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)= "C28A7410-B48A-4199-BA3A-B5254A20C1F5" And Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).split("-")(0)=12 Then 
		ElseIf Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" Then 
		Else
	
		Dim BedehiNosazi As New Global.Biz.SB.ClsDutyCalculate
		Dim EumType As Integer = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)).EumNosaziCodeObjType
		
		If  EumType = 4 or  EumType = 5 Then
		
	'rq10301241 این پیغام فقط برای کدهای نوسازی طرح جامع 3 نمایش داده می شود
		  MapTarhJame3()
 
			If BedehiNosazi.HasDept(Info8.GetRequest.Info.NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode) Then
		    	if Info8.GetRequest.Info.NidworkItem= "2089512" Then
		    	Else
		    
		    		if TmpListManger.CI_ResourceManagerConfirm <> 5  And ISTarhJame3=True And  ucase(Info8.User.UserGuid.tostring) <> ucase("CB32C037-2DD0-429A-B801-D9F29E6E7507")  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " بدهی نوسازی .", "پرونده تا سال جاری دارای بدهی نوسازی  میباشد.") :  Exit Function
		    	End if	
		    End If
		  '  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "aaa", BedehiNosazi.HasDept(Info8.GetRequest.Info.NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode))
		    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		   
		    '	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "BedehiNosazi.ErrorResult.BizErrors.Count ",BedehiNosazi.ErrorResult.BizErrors.Count )
		   ' msgbox("ib:","ib" )
			Dim TmpParentSenf As New List(Of Global.Biz.SA.ClsNosaziCodeType)
		    TmpParentSenf=Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
			TmpParentSenf=TmpParentSenf.where(Function (f) f.EumNosaziCodeObjType=6).tolist
			if TmpParentSenf.count>0 Then 
				Dim Si as Integer=0
				For Si=0 to TmpParentSenf.count -1 
					Try   
		    			Info8.ParameterList.REMove("Parvandeh_Job_NidBase")
		    			Info8.ParameterList.REMove("Parvandeh_Shop_NidBase")
		    		Catch
		    		End Try	
					Info8.ParameterList.Add("Parvandeh_Shop_NidBase",TmpParentSenf(Si).NidBase.ToString)
					Info8.ParameterList.Add("Parvandeh_Job_NidBase",TmpParentSenf(Si).NidBase.ToString)
					Dim tmpBase_JobInfoNew as  New list(of DtoSC.Base_JobInfo)
					tmpBase_JobInfoNew = Info8.GetParvandeh_Shop.GetBase_JobInfo 
					Dim Ji as Integer=0
					For Ji=0 to tmpBase_JobInfoNew.count -1  
		              If tmpBase_JobInfoNew(Ji).JobDeActivateDate = "" Or  tmpBase_JobInfoNew(Ji).JobDeActivateDate is Nothing  Or tmpBase_JobInfoNew(Ji).JobDeActivateDate > BIZ.SA.ClsCommon.CurrentShamsiDateString  Then
			              Dim BedehiSenfi As New Global.Biz.SB.ClsDutyCalculate
			              Dim tempJobNew as String =""
			              tempJobNew=tmpBase_JobInfoNew(Ji).NidJob.tostring 
			              if Len(tempJobNew)>0 Then 
		            		  Dim kTmp As New List(Of Global.DtoSC.Duty_PayOffHistory)
			                  kTmp = BedehiSenfi.GetDuty_PayOffHistory(Guid.Parse(tempJobNew), 2).tolist
			                  if kTmp.Max(Function(f) f.CI_DutyYear_Payoff) < BIZ.SA.ClsCommon.CurrentShamsiYear Then 
		            		  	if TmpListManger.CI_ResourceManagerConfirm <> 5 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " بدهی صنفی* "  , "پرونده تا سال جاری دارای بدهی صنفی میباشد.") :  Exit Function
		            			if TmpListManger.CI_ResourceManagerConfirm <> 5 Then Exit Function
		            		  End if
		            	 End if	  
	            	  End if	  	 
					Next
				Next	
			End if			
		  '  msgbox("id:","id" )
		ElseIf EumType = 3 or EumType = 2 Then
			
			Dim TmpParent As New List(Of Global.Biz.SA.ClsNosaziCodeType)
			Dim TmpParentApar As New List(Of Global.Biz.SA.ClsNosaziCodeType)
		    TmpParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
		    TmpParentApar=Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
		    TmpParentApar = TmpParentApar.Where(Function(f) f.EumNosaziCodeObjType = 4 ).ToList
		    if TmpParentApar.any Then
		    	TmpParent=TmpParentApar
		    Else
		    	TmpParent = TmpParent.Where(Function(f) f.EumNosaziCodeObjType = 2 ).ToList
		     
		    End if 
		    Dim jj as Integer
		    
		    If TmpParent.Count > 0 Then
		    
'    If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
'       Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", TmpParent.Count.tostring)
'       Exit Function
'   End If 		    
		    
		    	For jj=0 to TmpParent.Count-1 
		    		Dim BedehiNosazi_M As New Global.Biz.SB.ClsDutyCalculate
		    				    		
		    
			    	If BedehiNosazi_M.HasDept(TmpParent(jj).NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode) Then
					
						if Info8.GetRequest.Info.NidworkItem= "2089512" Then
				    	Else
				   
							if TmpListManger.CI_ResourceManagerConfirm <> 5  Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*aa بدهی نوسازی بر روی کد  "  & Info8.GetFunctions.GetNosaziNickName (TmpParent(jj).NidNosaziCode) , "پرونده تا سال جاری دارای بدهی نوسازی  میباشد.") :  Exit Function
						End if	
			        End If
			        
		    ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "wwww", BedehiNosazi_M.HasDept(TmpParent(jj).NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode))
		    '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		   
		   	'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "w.Count ",BedehiNosazi.ErrorResult.BizErrors.Count )

			    Next    
		    End If
		    
		    '''''''''''''''''''''
		    	  
		    Dim TmpParentSenf As New List(Of Global.Biz.SA.ClsNosaziCodeType)
		    TmpParentSenf=Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
			TmpParentSenf=TmpParentSenf.where(Function (f) f.EumNosaziCodeObjType=6).tolist
			if TmpParentSenf.count>0 Then 
			 
				Dim Si as Integer=0
				For Si=0 to TmpParentSenf.count -1 
					Try   
		    			Info8.ParameterList.REMove("Parvandeh_Job_NidBase")
		    			Info8.ParameterList.REMove("Parvandeh_Shop_NidBase")
		    		Catch
		    		End Try	
					Info8.ParameterList.Add("Parvandeh_Shop_NidBase",TmpParentSenf(Si).NidBase.ToString)
					Info8.ParameterList.Add("Parvandeh_Job_NidBase",TmpParentSenf(Si).NidBase.ToString)
					Dim tmpBase_JobInfoNew as  New list(of DtoSC.Base_JobInfo)
					tmpBase_JobInfoNew = Info8.GetParvandeh_Shop.GetBase_JobInfo 
					Dim Ji as Integer=0
					For Ji=0 to tmpBase_JobInfoNew.count -1  
		              If tmpBase_JobInfoNew(Ji).JobDeActivateDate = "" Or  tmpBase_JobInfoNew(Ji).JobDeActivateDate is Nothing  Or tmpBase_JobInfoNew(Ji).JobDeActivateDate > BIZ.SA.ClsCommon.CurrentShamsiDateString  Then
			              Dim BedehiSenfi As New Global.Biz.SB.ClsDutyCalculate
			              Dim tempJobNew as String =""
			              tempJobNew=tmpBase_JobInfoNew(Ji).NidJob.tostring 
			              if Len(tempJobNew)>0 Then
			              Dim kTmp As New List(Of Global.DtoSC.Duty_PayOffHistory)
			              kTmp = BedehiSenfi.GetDuty_PayOffHistory(Guid.Parse(tempJobNew), 2).tolist
			                 if kTmp.Max(Function(f) f.CI_DutyYear_Payoff) < BIZ.SA.ClsCommon.CurrentShamsiYear Then 
			                 
'		   If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
'		   		Dim ResualtFire22 As New  Biz.Communication.SrvFireFighting.resualt2
'		   		Dim Fire As New Global.Biz.Communication.ClsFireFighting
'		   			ResualtFire22 = Fire.GetReauestState(Info8.GetRequest.Info.NidWorkItem.tostring ,Info8.User.UserGuid,Info8.User.UserName.ToString)
'      			 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "CI_ResourceManagerConfirm", TmpListManger.CI_ResourceManagerConfirm.tostring)
'      			 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ReturnCode", ResualtFire22.returnCode )
'      			 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "failMessage ", ResualtFire22.failMessage )
'      			 Exit Function
'  			 End If				                 
			                 
		            		  	if TmpListManger.CI_ResourceManagerConfirm <> 5 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop, " بدهی صنفی #"  , "پرونده تا سال جاری دارای بدهی صنفی میباشد.") 
		            			if TmpListManger.CI_ResourceManagerConfirm <> 5 Then  Exit Function
		            		  End if
		            	  End if	  
	            	  End if	  	 
					Next
				Next	
			End if			
		    
		Else
			'127642
	
				Dim AllFicheNew As New List(Of DtoSC.Income_Fiche)
        		Dim TmpAllFiche As New List(Of DtoSC.Income_Fiche)
				tarikhFich="1300/01/01"
				Info8.Income_Fiche=Nothing
				TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
			    Dim strCodeNew as String
			    Dim Str_Income as String
				Dim tmpDistrict as Integer=0
				strCodeNew = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
				tmpDistrict =  strCodeNew.Split("-")(0)
			
				If tmpDistrict <> 80 Then
					Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116,163"
				Else
					Str_Income = "1,7,10,11,162,163"
				End If
					
				Dim TmpArr() As String
					
				Dim intList_Income As New list(Of Integer)
				TmpArr = Str_Income.Split(",")
				For i = 0 To TmpArr.Count - 1
					intList_Income.Add(TmpArr(i))
				Next i
					
				If TmpAllFiche.Any Then
				
				
					TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
				'	msgbox("ij:",TmpAllFiche.count)
					If TmpAllFiche.any Then AllFicheNew = TmpAllFiche.Where(Function(f) f.EumFicheStatus = 3 Or f.EumFicheStatus = 5).ToList()'*1129298 
				'	msgbox("AllFicheNew:",AllFicheNew.count)		
					If AllFicheNew.Any Then tarikhFich =AllFicheNew.Max(Function(f) f.BankPaymentDate)
			'	msgbox("tarikhFich xxxx:",tarikhFich)
				End if		
	
			
				
			
		'	msgbox("ij:","ij" )
				 if  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="1472660E-7313-4FC8-8B1D-455A81478F20" or _
					 Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)= "975E3267-1597-4A3F-884D-E72C42E843CA" or _
					 Ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)="E530B5D8-6283-4E95-B655-6DB83377A6E7" Then
					 '	msgbox("d:",tempJob )
					 if tempJob<>""  And tarikhFich>"1397/12/20" Then 
				'	 msgbox("d:","F" )
					 	Dim BedehiSenfi As New Global.Biz.SB.ClsDutyCalculate
					 		            
			            Dim kTmp As New List(Of Global.DtoSC.Duty_PayOffHistory)
			            kTmp = BedehiSenfi.GetDuty_PayOffHistory(Guid.Parse(tempJob), 2).tolist
			            if kTmp.Max(Function(f) f.CI_DutyYear_Payoff) < BIZ.SA.ClsCommon.CurrentShamsiYear Then 
					 	'	if BedehiSenfi.GetPayOffYear(Guid.Parse(tempJob), 2, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode).CI_DutyYear_Payoff < BIZ.SA.ClsCommon.CurrentShamsiYear Then 
            				if TmpListManger.CI_ResourceManagerConfirm <> 5 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop, " بدهی صنفی "  , "پرونده تا سال جاری دارای بدهی صنفی  میباشد.") :  Exit Function
            				if TmpListManger.CI_ResourceManagerConfirm <> 5 Then Exit Function
            			End if	 
					 End if
				End if
'msgbox("f:","f" )
		    '127642
	'	Else
	'	
	'			Dim TmpParentApar As New List(Of Global.Biz.SA.ClsNosaziCodeType)
	'			Dim TmpParent As New List(Of Global.Biz.SA.ClsNosaziCodeType)
	'			Dim TmpCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).Split("-")
	'		
	'	    	TmpParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
	'	    
	'	    	if TmpCode(5)<>0 Then  TmpParentApar = TmpParent.Where(Function(f) f.EumNosaziCodeObjType = 4).ToList
	'	    	if TmpCode(5)=0 And TmpCode(4)<>0 Then  TmpParentApar = TmpParent.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
	'	    	
	'	    	If TmpParentApar.Count = 0 Then 
	'	    		TmpParent = TmpParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
	'	    	Else
	'	    		TmpParent=TmpParentApar
	'	    	End if
	''		    If TmpParent.Count > 0 Then
	'		    	If BedehiNosazi.HasDept(TmpParent(0).NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode) Then
	'					Info8.AddError(BIZ.SA.EumErrorAction.Stop, " بدهی نوسازی ", "پرونده تا سال جاری دارای بدهی نوسازی  میباشد.")
	'		        End If
	'	    	End If
	'127642
		 
		End If
	End if 
	End if

	' msgbox("g:","g" )
	
'sp  361513  add   
'در تایید گواهی پروانه مرحله اول در سرا 10 لزوم تایید شناسنامه فنی حذف شود 
'8D20BE44-78AB-4001-BB94-F064D22B9CE1    097C8C41-BA8C-4B60-8B2F-6BED0221F573

    
Msgbox("",400)	
	
			if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=1 And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"BB0854BE-4002-4C0C-975C-04B4B8485334" And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"8D20BE44-78AB-4001-BB94-F064D22B9CE1" And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"097C8C41-BA8C-4B60-8B2F-6BED0221F573" or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"2635FE18-FE4C-4A2D-9D05-993756751262" Then 	
			'190200
				if (ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="CB2A1920-7B97-4D20-9BBE-12D50F40BB5A" or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") And Info8.GetRequest.Info.RequestDate>"1397/05/07" Then
				Else
				'sp 446250 
				'sp 446250 	if TmpListManger.CI_ResourceManagerConfirm <> 5 Then
				'sp 446250 		Dim LstMangerCon As New List(Of Global.DtoSC.Sh_ManagerConfirm)
				'sp 446250 	    LstMangerCon = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
				'sp 446250 	    If LstMangerCon.Count > 0 Then LstMangerCon = LstMangerCon.Where(Function(f) f.CI_ResourceManagerConfirm = 8 Or f.CI_ResourceManagerConfirm = 17).ToList
			'sp 446250 		    If LstMangerCon.Count = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه&&", "لطفا ابتدا شناسنامه فنی تعیین کنید ") : Exit Function
				'sp 446250 	 End if   
				End if    
				    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
			End if
			
		'	--188272
			if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=2 And (	TmpArchiveGroup=1838 or 	TmpArchiveGroup=1870 or 	TmpArchiveGroup=1869) Then 	
				'188662
				Dim dto_Savabegh, TmpSavabegh As New List(Of DtoSC.Base_ShahrsaziArchive)
				Dim TmpParent_S As New List(Of Global.Biz.SA.ClsNosaziCodeType)
				Dim TmpParent_SDas As New List(Of Global.Biz.SA.ClsNosaziCodeType)
			    TmpParent_S = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
			    TmpParent_SDas=TmpParent_S.where(Function (f) f.EumNosaziCodeObjType=5).tolist
	            If TmpParent_SDas.count>0 Then
					TmpParent_S=TmpParent_S.where(Function (f) f.EumNosaziCodeObjType=5).tolist
	                Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
	                Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", TmpParent_S(0).NidBase.ToString)
					dto_Savabegh =Info8.GetParvandeh_HouseSharing.GetBase_ShahrsaziArchive()
	               
	            Else
					TmpParent_S=TmpParent_S.where(Function (f) f.EumNosaziCodeObjType=2).tolist
	                Info8.ParameterList.Remove("Parvandeh_House_NidBase")
	                Info8.ParameterList.Add("Parvandeh_House_NidBase", TmpParent_S(0).NidBase.ToString)
	                dto_Savabegh = Info8.GetParvandeh_House.GetBase_ShahrsaziArchive()
	                
	            End If
				dto_Savabegh=dto_Savabegh.where(Function(f) f.CI_ShahrsaziArchiveGroup=1 And f.CertificateDate>"1397/02/06" And f.CI_ShahrsaziArchiveKind<> 140 And f.CI_ShahrsaziArchiveKind<> 200 And f.CI_ShahrsaziArchiveKind<> 201 And f.CI_ShahrsaziArchiveKind<> 202 And f.CI_ShahrsaziArchiveKind<> 203 ).tolist	
				'188662
				if dto_Savabegh.count>0 Then 
					if TmpListManger.CI_ResourceManagerConfirm <> 5 Then
						Dim LstMangerCon As New List(Of Global.DtoSC.Sh_ManagerConfirm)
					    LstMangerCon = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
					    If LstMangerCon.Count > 0 Then LstMangerCon = LstMangerCon.Where(Function(f) f.CI_ResourceManagerConfirm = 20 And f.EumManagerConfirmLicence=1).ToList
					    If LstMangerCon.Count = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه**", "لطفا ابتدا -کنترل تاییدیه بیمه - تایید کنید ") : Exit Function
					 End if   
				End if    
			    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
			End if
	End if	
		
'msgbox("h:","h" )
    
    Dim Test = GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)	
    'if Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=9 And ArchiveGroup>0 Then 
    	
   ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    IsDafaterMsg=False
    if Sara10=False Then 
    If WorkflowTaskList.Any Then
        Dim MaxNidSort As Long
        MaxNidSort = WorkflowTaskList.Max(Function(f) f.NidSort)
        TmpWorkflowTaskList = WorkflowTaskList.Where(Function(f) f.NidSort = MaxNidSort).ToList()

        If TmpWorkflowTaskList.Any Then
            Dim TaskTitel As String
            TaskTitel = TmpWorkflowTaskList(0).TaskTitel
            If TaskTitel <> "" Then

                BazdidOk = False

                If InStr(TaskTitel, "محاسبات") <> 0 And InStr(TaskTitel, "پروانه") <> 0 And InStr(TaskTitel, "منطقه") <> 0 Then BazdidOk = True
                 
				If InStr(TaskTitel, "تاييد محاسبات منطقه") <> 0 or InStr(TaskTitel, "محاسبات پروانه منطقه") <> 0  Then IsDafaterMsg = True


            End If

        End If
    End If
   End If
    If BazdidOk = True Then
        Dim WorkflowTitle As String
        WorkflowTitle = Info8.GetRequest.Info.WorkflowTitel
        If WorkflowTitle <> "" Then
            If instr(WorkflowTitle, "شهردار") <> 0 Then BazdidOk = False
        End If
    End If
    
   

    Dim TaeedDafater As Integer = 0
    Dim IsDafater as Integer =0
    If UserList7.Any Then
        For ii = 0 To UserList7.Count - 1

            If Ucase(UserList7(ii).GroupGuid.tostring) = "91152337-9688-4618-9D9A-6401925743A9" Then
                TaeedDafater = 1
            End If
            'add  B40F1FAF-5885-457B-9217-DDAC6F01898E sp 395107
            If Ucase(UserList7(ii).GroupGuid.tostring) = "81CAA6F8-1DA9-428B-A0C7-8C17E705B08E"  or Ucase(UserList7(ii).GroupGuid.tostring) = "C65C30A2-B7B8-4AF5-95DC-E0BE1F2AF6E9" or Ucase(UserList7(ii).GroupGuid.tostring) = "B40F1FAF-5885-457B-9217-DDAC6F01898E" Then
            	IsDafater = 1
            End if 
        Next ii
    End If
 
  '  if IsDafaterMsg And TmpListManger.CI_ResourceManagerConfirm<> 7  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "شما در این مرحله فقط مجاز به تایید مدیر دفتر می باشید ")
	if TmPUserIdGroup20=1  And TmpListManger.CI_ResourceManagerConfirm <> 12  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "#تایید مدیر", "شما مجاز به تاييد نمی باشید ") :  Exit Function
   '**207156 If TaeedDafater = 0 And BazdidOk  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "@شما مجاز به تاييد نمی باشید ") :  Exit Function
    If TaeedDafater = 1 And BazdidOk And TmpListManger.CI_ResourceManagerConfirm <> 7 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "+تایید مدیر", "شما مجاز به تاييد مدير دفتر می باشید ") :  Exit Function
	if IsDafater = 1 And  TmpListManger.CI_ResourceManagerConfirm <> 7 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "شما مجاز به تاييد مدير دفتر می باشید ") :  Exit Function '89246

'msgbox("i:","i" )

    If TmPUserIdGroup7_M = 1 Then
       If ucase(NidWDeff7) <> "EF8464E4-38F5-4AAB-8DB5-094D6B5E1398" And ucase(NidWDeff7) <> "3157211F-C484-48E9-B65C-6BF3271A9F15" _
         And ucase(NidWDeff7) <> "7318F0DB-13DD-40E4-BB89-6C4C7A23FF62" And ucase(NidWDeff7) <> "1426A8E6-68BC-4B2F-8E82-7C478D5CAB14" And ucase(NidWDeff7) <> "CDAA2D6E-1184-4983-A516-A8A2FF92795C" _
         And ucase(NidWDeff7) <> "083062F2-6C96-4F21-8955-0FAE1F367704" And ucase(NidWDeff7) <> "297949D0-9E25-419E-AD7C-62A7034910A1" _
         And ucase(NidWDeff7) <> "EBCCF491-D89D-46E6-BEE7-6F4459C01329" And ucase(NidWDeff7) <> "3EBB3B53-972C-425F-9939-80E76AAB6BFB" And ucase(NidWDeff7) <> "3636B5FF-0BF6-4CBC-BBD3-875DB4A1BB11" _
         And ucase(NidWDeff7) <> "85DF30CD-BE53-466E-AA76-749A40831BA5" And ucase(NidWDeff7) <> "62B24930-E3B9-455E-889F-74B32274C320" _
         And ucase(NidWDeff7) <> "DD805F08-DC99-4D16-91AE-7BC17A70330F" And ucase(NidWDeff7) <> "3B155802-5BD4-4261-A6F3-B65B5177BF5F" Then
            Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر /اعتبارسنجی", "شما مجاز به تغییر داده نمی باشید") :  Exit Function
        End If

    End If

  '------------- Sajedi 'sp 459756 and 477985
'	Dim SumGheirMaskooni as Double =0 	'sp 459756
'	Dim TmpFlorBaseUsing_GhM As New List(Of DtoSC.Base_Using)  'sp 459756	
	Dim groupNumbers As New List(Of Integer)  'sp 459756
	

    If TmpListManger.EumManagerConfirmLicence <> 2 Then

        If ucase(NidWDeff7) <> "2497B40D-9861-445E-96AD-3588A1613682" Then
            Dim strNosazi As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
            Dim Nosazi As String()
            Nosazi = strNosazi.Split("-")
 

            If Nosazi(3) < 10000 Then


                If TmpListManger.CI_ResourceManagerConfirm = 5 Or TmpListManger.CI_ResourceManagerConfirm = 3 Then '136911
                	
                	 
''''''''''''''''''''317799
                
                	
                	 
                '''''''''''''''''''''''''''''''
                   if ucase(NidWDeff7)<>"E5F7F86A-0D02-4A01-A01C-19F13F6BEDB9" And ucase(NidWDeff7)<> "297949D0-9E25-419E-AD7C-62A7034910A1" _
 					And ucase(NidWDeff7)<>"EBCCF491-D89D-46E6-BEE7-6F4459C01329" And ucase(NidWDeff7)<>"3EBB3B53-972C-425F-9939-80E76AAB6BFB" _
 					And ucase(NidWDeff7)<>"3636B5FF-0BF6-4CBC-BBD3-875DB4A1BB11" And ucase(NidWDeff7)<>"6C67F07C-84DA-41CB-A454-BAC84078EA9F" _
 					And	ucase(NidWDeff7)<>"D0127432-AA14-4A16-BA07-D6A6C7DF5C5D" And ucase(NidWDeff7)<>"4BFCFAE5-ACF9-4159-9BF1-1B2C0781ED0E" _
 					And	ucase(NidWDeff7)<>"B6A28C29-6761-4719-8B62-C8575F88F572" And ucase(NidWDeff7)<> "35CFFB97-67A3-488E-8A7A-BEC93C459B99" _
 					And ucase(NidWDeff7)<>"C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB" And ucase(NidWDeff7)<> "BDD563C8-1EB2-40EC-8811-19DB03908488"  _
 					And ucase(NidWDeff7)<>"B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" Then 
                    
                    Call ComfirmFiche()
                    '	msgbox("fff:",GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString) )
                    	if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="1" Then 	Call ComfirmFicheAmozesh()
                    	'	Call ComfirmFicheSepordeh()
                   End if  
                   '''''''''''''''''''''''''''''''''
                 ' msgbox("aaaawww:","aaawww" )
                   '190192
                   

 
                   if  Taghsid(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc,0)=1 Then
                  ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ssk. ", "S" ) 
						Dim LstMangerCon As New List(Of Global.DtoSC.Sh_ManagerConfirm)
						LstMangerCon = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
						If LstMangerCon.Count > 0 Then LstMangerCon = LstMangerCon.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence=1).ToList
						If LstMangerCon.Count = 0 And NoContorol_TaeedTaghsid=False  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه &", " به دلیل داشتن تقسیط ، پرونده باید تایید تقسیط داشته باشد ") : Exit Function
					
					End if
					'190192
                   	'''''''''''''''''''''''
                   	'187556
                   	
                   	Dim LstMangerConFire As New List(Of Global.DtoSC.Sh_ManagerConfirm)
				    LstMangerConFire = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
				    
				    If LstMangerConFire.Count > 0 Then LstMangerConFire = LstMangerConFire.Where(Function(f) (f.CI_ResourceManagerConfirm =10) And f.EumManagerConfirmLicence=1).ToList 'add ci 10  2987947 ---- del  220894    f.CI_ResourceManagerConfirm = 13 or
				    
                Dim Maskoni as Boolean 
                Dim TmpFloor as Integer=0   	
               ' msgbox("LstMangerConFire.Count:",LstMangerConFire.Count)
                   	
      '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''201992  
       '313755    	 if TmpListManger.CI_ResourceManagerConfirm <> 5 Then
       
       'sp  325796
            
 			Dim GetFireFighting_Comments = FireFighting_Func(TmpListManger,LstMangerConFire.Count)
 
            TmpListManger.Comments = GetFireFighting_Comments
          
          
	End if
	End if
	End if
	End if
' 313755 End if 'پایانکار ثبت شهرداری
    '---------------------------
    
if Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 Then 
	
		Dim Mant137 As Integer = 0
        Select Case Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District
            Case 1
                Mant137 = 19
            Case 2
                Mant137 = 20
            Case 3
                Mant137 = 21
            Case 4
                Mant137 = 22
            Case 5
                Mant137 = 23
            Case 6
                Mant137 = 24
            Case 7
                Mant137 = 25
            Case 8
                Mant137 = 26
            Case 9
                Mant137 = 27
            Case 10
                Mant137 = 28
            Case 11
                Mant137 = 29
            Case 12
                Mant137 = 30
            Case 80
                Mant137 = 31

        End Select
        
 ' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","aaaa")  
 
      
 if Sara10=False Then    
  
if User137(Info8.User.UserGuid.tostring,TmpArchiveGroup)=1 Then


'	Try
		'  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "JobLocations", Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name.tostring)   
		' Catch ex As Exception
				'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی"  , Info8.GetFunctions.GetUser(Info8.User.UserGuid).  )
	     '   End Try	 
		  
'plogfile("aa","aa" & "ArchiveGroup:" & ArchiveGroup ) 
	if  TmpArchiveGroup>10 And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" Then '  (Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=9  or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=8 or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=3 or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=12 _
	    'or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=5) Andadd 8-3 -9  161246-161249-161250
	   
	  
		  Dim Fava137 As New Biz.Communication.ClsFava137
		  Dim Fava137Message As New Biz.Communication.ClsFava137MessageContract
		  
		  Fava137Message.key = "5tgb6yhn^YHN%TGB"
		  
		  Fava137Message.Name = Info8.GetRequest.Info.RequesterName 
		  
		  Fava137Message.LastName = " "
		  Fava137Message.ReferenceNum = Info8.GetRequest.Info.NidWorkItem
		 
		  
		  Fava137Message.TypeOfReq=TmpArchiveGroup
		 
		  Dim TmpWorkflowTaskList137 As New List(Of Global.Biz.SC.SrvTaskService.Task)
		  Dim NewTmpWorkflowTaskList137 As New List(Of Global.Biz.SC.SrvTaskService.Task)
	      TmpWorkflowTaskList137 = Info8.GetFunctions().GetRequestLog(Guid.Parse(Info8.GetRequest.Info.NidProc.ToString))
	     
	     
	      NewTmpWorkflowTaskList137 = TmpWorkflowTaskList137.Where(Function(f) f.TaskTitel = "بررسي اطلاعات درخواست" ).ToList()
	       
	       if NewTmpWorkflowTaskList137.count=0 Then NewTmpWorkflowTaskList137 = TmpWorkflowTaskList137.Where(Function(f) f.TaskTitel = "ثبت درخواست" ).ToList()
	        
	        
	        TmpWorkflowTaskList137=NewTmpWorkflowTaskList137     
	       'new 612064
	      
	       
	     
	 
	  
	       
	       Dim joblocationNull as Boolean=False
	       if Info8.GetFunctions.GetUser(TmpWorkflowTaskList137(0).AssingTo).User.JobLocations Is Nothing  Then 
	      	 joblocationNull=True
	       End if
	       
	      
	      if joblocationNull=True Then 
	      	Fava137Message.OrgCellPhon="-"
	      Else
	     	 Fava137Message.OrgCellPhon=Info8.GetFunctions.GetUser(TmpWorkflowTaskList137(0).AssingTo).User.JobLocations.Name.tostring
	      End if
	      
	       
	'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ارسال پروندهddddd", Info8.GetFunctions.GetUser(TmpWorkflowTaskListfIRE(0).AssingTo).User.JobLocations.Name)
	
	' + "شماره دفتر :" + Info8.GetFunctions.GetUser(TmpWorkflowTaskListfIRE(0).AssingTo).User.JobLocations.Name + VbCrLf _

	      If TmpArchiveGroup=1875 or TmpArchiveGroup=1876 or TmpArchiveGroup=1880 Then 
	      		Fava137Message.LevelNum = 3
	      		Fava137Message.LevelExplain ="شماره گام: 3" + VbCrLf _
				  + "شماره دفتر :" + Fava137Message.OrgCellPhon + VbCrLf _
				  + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
				  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +"گواهی  ملک شما صادر شد. "  
				'  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ارسال پرونده", "dd")
				  Fava137Message.Regdate = Date.Now.Date
				  
				  Fava137Message.TrackingDate = AddDateForHolidays(GetShamsiDate(Date.Now.Date.tostring), 1) 'Date.Now.Date.AddDays(1)
				   
				  Fava137Message.CellPhone = Info8.GetRequest.Info.Cellphone
		  ElseIf TmpArchiveGroup=1883 Then 
		  		Fava137Message.LevelNum = 7
	      		Fava137Message.LevelExplain =	  "شماره گام: 7" + VbCrLf _
				  + "شماره دفتر :" + Fava137Message.OrgCellPhon + VbCrLf _
				  + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
				  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" پروانه دیوار کشی شما صادر شد. "  
				'  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ارسال پرونده", "dd")
				  Fava137Message.Regdate = Date.Now.Date
				
				  Fava137Message.TrackingDate = AddDateForHolidays(GetShamsiDate(Date.Now.Date), 1) 'Date.Now.Date.AddDays(1)
				 
				  Fava137Message.CellPhone = Info8.GetRequest.Info.Cellphone
	      
	      Else
	       		Fava137Message.LevelNum = 10
				  Fava137Message.LevelExplain =	  "شماره گام: 10" + VbCrLf _
				  + "شماره دفتر :" + Fava137Message.OrgCellPhon + VbCrLf _
				  + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
				  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +"پروانه / پایانکار بهره‌برداری ملک شما صادر شد. "  
				'  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ارسال پرونده", "dd")
				  Fava137Message.Regdate = Date.Now.Date
				 
				  Fava137Message.TrackingDate = AddDateForHolidays(GetShamsiDate(Date.Now.Date), 1) 'Date.Now.Date.AddDays(1)
				 
				  Fava137Message.CellPhone = Info8.GetRequest.Info.Cellphone
		
		
		End if
		   '''''
		   msgbox("wwww:","wwwwww")
		   Fava137Message.CityCode=Info8.GetFunctions.GetUser(TmpWorkflowTaskList137(0).AssingTo).User.JobLocations.Name.tostring
		   
		   Fava137Message.Subject="1134"
		   Fava137Message.SubSubject=TmpArchiveGroup
		 
		   Fava137Message.Zone=Mant137
		  Dim NosaziLst As List(Of Biz.SA.ClsNosaziCodeType)
	 	  NosaziLst = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		  NosaziLst = NosaziLst.Where(Function(f) f.EumNosaziCodeObjType = 2).tolist
			  
		  if NosaziLst.any Then 
		  	
	 		  Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
	 		  Info8.ParameterList.Add( "Parvandeh_House_NidBase" , Info8.GetFunctions.GetNidBase(NosaziLst(0).NidNosaziCode, Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000")).ToString)
	 	 Else
	 	 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "137اعتبارسنجی", "137 مشکل در ارسال به 137 ") :  Exit Function
	 	 End if 	
	
	Try
		  Fava137Message.Adress=Info8.GetParvandeh_House.GetBase_AddressInfo.MainAddress.ToString 
		 Catch ex As Exception
					Info8.AddError(BIZ.SA.EumErrorAction.warning, "اعتبارسنجی"  , "ادرس ملک برای ارسال به 137 معتبر نمی باشد")
	        End Try	  
		  
		  if Info8.GetParvandeh_House.GetBase_PreCodeInfo.Count>0 Then 
	 	  	Fava137Message.RenovationCode=Info8.GetParvandeh_House.GetBase_PreCodeInfo(0).PreCode	
	 	  Else
		  	Fava137Message.RenovationCode="0"	
	 	  End if 	
	 	 
	 	  Fava137Message.RenovationCodeNew=Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)	

	
		  If Fava137.SaveRequestnew(Fava137Message) Then
		  '	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "137اعتبارسنجی", "137 موفقیت")
		  	Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
				 tmpSMS.BizCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
				 tmpSMS.Number = Info8.GetRequest.Info.Cellphone
				 tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
				 tmpSMS.SMSType = "تایید مدیر"
				 tmpSMS.Text =   " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
		 		 + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +"پروانه / پایانکار بهره‌برداری ملک شما صادر شد. "  
	
				 tmpSMS.UserID = Info8.User.UserGuid.ToString
				 tmpSMS.UserName = Info8.User.UserName
				
				
				  Dim smsErrorResult As New BIZ.SA.ClsErrorResult
				  smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
		  Else
		 '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "137اعتبارسنجی", "137 مشکل در ارسال به 137 ")
		  End If
		  
	   End if 
  
End if
End if
End if '* 179379




if ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString) = ucase("6E8E4E56-A0E7-458B-9842-E25DDC55D9BC") Then 

	Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	
if lstR_T.count<=0  Then

	if  Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=22 _
	And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1  Then
	
		Try
		
		     '   Dim RRevist As New Biz.SC.ClsAssignRevisit()
		 
		        Dim UserTMP As Biz.SA.ClsUser
		        UserTMP = Info8.User
		        
		        Dim RevisiAgentT As New DtoSC.Sh_Revisit
	        
		        
		        RevisiAgentT.NidRevisitAgent =  Info8.User.UserGuid  ' New Guid("BC6E1D24-3187-408F-8B95-3D0D39276F19")  Info8.User.UserGuid 
		        RevisiAgentT.NidProc = Info8.GetRequest.Info.NidProc
		        RevisiAgentT.NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
		        RevisiAgentT.NidUser = Info8.User.UserGuid
		        RevisiAgentT.UserName = Info8.User.UserName
		        RevisiAgentT.RevisitDate = Biz.SA.ClsCommon.CurrentShamsiDateString
		        RevisiAgentT.RevisitTimeFrom = "8:00"
				RevisiAgentT.RevisitTimeTo = "9:00"
				
			'	Info8.GetFunctions.AddRevisitAgentForRequest(RevisiAgentT, UserTMP)
		        Info8.GetFunctions.AddRevisitAgentForRequest(RevisiAgentT, Info8.User)
		        
		       ' Info8.GetRequest.Info.NidRevisit = RevisiAgentT.NidRevisit  		       	 
   		       
		         
		Catch e as exception
			   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی- اصلاح نقشه", e.message) 
			   Exit Function
		End Try
	End if	
End if	
End if   
    	 

    Dim Tmpaa As String
  ' Dim  baseInfotmp as New DtoSC.Base_Info
    
    Try
    	tmpaa = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
    Catch e as exception
    	tmpaa=Info8.GetRequest.info.NidRevisit.tostring
    '	tmpaa=baseInfotmp.Nidbase.tostring
    '	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", e.message)
    End Try	
 
    Dim TypeObj As Integer
    TypeObj = 0
 	   	
    Dim aj = Info8.GetFunctions.GetNidLib(Guid.Parse(tmpaa))
 	If Not (aj.RDastgah Is Nothing) Then TypeObj = 5
    If Not (aj.RBuilding Is Nothing) Then TypeObj = 3
    If Not (aj.RHouse Is Nothing) Then TypeObj = 3

    If Not (aj.RHouse Is Nothing) Then TypeObjMain = 2
    If Not (aj.RBuilding Is Nothing) Then TypeObjMain = 3
    If Not (aj.RDastgah Is Nothing) Then TypeObjMain = 5
    If Not (aj.RApartment Is Nothing) Then TypeObjMain = 4
    If Not (aj.RSenfi Is Nothing) Then TypeObjMain = 6
' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","aaaa")  




'if ucase(Info8.Getrequest.info.nidworkflowdeff.tostring) <> ucase("6E8E4E56-A0E7-458B-9842-E25DDC55D9BC") Then 
Try
	
    Info8.ParameterList.Remove("Revisit_House_NidBase")
   
    Info8.ParameterList.Add("Revisit_House_NidBase", aj.RHouse.NidBase.ToString)
   
    if  Not (aj.RDastgah Is Nothing) Then
	    Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
	    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", aj.RDastgah.NidBase.ToString)
    End if
     
    if  Not (aj.RBuilding Is Nothing) Then
    
		Info8.ParameterList.Remove("Revisit_Building_NidBase")
	    Info8.ParameterList.Add("Revisit_Building_NidBase", aj.RBuilding.NidBase.ToString)
	End if
	 
	if  Not (aj.RApartment Is Nothing) Then

		Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
	    Info8.ParameterList.Add("Revisit_Apartment_NidBase", aj.RApartment.NidBase.ToString)
	End if
	
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "trytt", "GetActiveRevisitBaseInfo")
End Try 

	
		
	if  Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 _
	And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1  Then
		Dim ij as Integer
		'122719
		'273502
	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" And ucase(Info8.Getrequest.info.Nidnosazicode.tostring)<> ucase("9c642710-a9ca-44d0-986e-16da244de66b") Then
		Try
		
		Info8.ParameterList.REMOVE("NidZabeteh")
		Info8.ParameterList.REMOVE("NidNosaziCode")
		Info8.ParameterList.Add("NidZabeteh", Info8.GetRequest.Info.ActiveNidZabeteh.ToString)
		Info8.ParameterList.Add("NidNosaziCode", Info8.GetRevisit_House.GetBase_NosaziCode.NidNosaziCode.ToString)
	
		For  ij =0  to Info8.GetZabeteh(True).GetZabeteh_Details.Count-1
			if  Info8.GetZabeteh(True).GetZabeteh_Details(ij).CI_Zabeteh=251  And TypeObjMain<>4 Then Info8.AddError(BIZ.SA.EumErrorAction.Warning, "اعتبارسنجی", " با توجه به وجود تغییر طرح لطفا در صورت وجود بدهی در این خصوص ، در توضیحات گواهی موضوع مربوطه درج گردد ")   'new And TypeObjMain<>4 2416833
		Next 
		'122719
		
	Catch

		Info8.AddError(BIZ.SA.EumErrorAction.Warning, "اعتبارسنجی", "!درخواست اعلام ضابطه ندارد")
	End Try
		''118411
		'273502
End if 
'If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","aaaa")  




Msgbox("A8","A8")
	if 	TypeObjMain <6 Then
	 
	'	if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString) = 2 or Info8.GetRequest.Info.NidworkItem= "2089512" Then 
		if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA" or _
Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="677FBEE4-B05C-4FA6-AB19-CE897739195A" or _
Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="82FBD9BC-EBA4-495C-99FF-22B035FB97FF" or _
Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B0602DD4-FEF9-4AEB-A14A-251B3E86A6D4" or _
Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="9D2031E0-8ACE-4CB3-A563-05FC1D42C8DF" or _
Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="AB69C90F-FBAF-42FC-9414-7DAE032A1182" or _
Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="01AAA0B7-8193-41D6-84F9-437FA39306A7" or _
Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="04DF58F1-731B-4371-8D53-47435F681362" or _
Info8.GetRequest.Info.NidworkItem= "2089512" Then	
			 
			if TypeObjMain =4 Then 
		
				Info8.ParameterList.Add("NidBase", Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 0, 0,Guid.Parse("00000000-0000-0000-0000-000000000000")).ToString)
				Dim TmpUnitCount As Integer = 0
				
	            Dim TmpBaseUsing As New List(Of Global.DtoSC.Base_Using)
	            TmpBaseUsing = Info8.GetRevisit_Apartment.GetBase_Using
	
	            TmpBaseUsing = TmpBaseUsing.Where(Function(f) f.CI_UsingType = 831).ToList
	            
	         if  TmpBaseUsing .count>0 Then
	            
	            TmpUnitCount = TmpBaseUsing.max(Function(f) f.UnitCount) 'sum
	            
	         End if   
	             
	            Dim TmpBaseElevator As New List(Of Global.DtoSC.Base_Elevator)
	            TmpBaseElevator = Info8.GetElevator.Base_Elevator
	            
	           ' TmpBaseElevator.Where(Function(f) f.IsRevoke <> 1 ) '0
	        '  TmpBaseElevator=TmpBaseElevator.Where(Function(f) f.Daterevoke < "1300/00/00").tolist '  new 2130468  
	            
	            Dim TmpUnitCountElevator As Integer = 0
	            Dim iji as Integer
	            For iji =0 to TmpBaseElevator.count -1 
	            	
	            	if Not isdbnull(TmpBaseElevator(iji).IsRevoke) Then 
	               		if  TmpBaseElevator(iji).IsRevoke Then 
	               		Else
			            	if TaeedMMokateb()=0 Then 
			            		TmpUnitCountElevator=TmpUnitCountElevator+1  
			            	Else	
			            		if GetDiffDate(TmpBaseElevator(iji).LetterDate, BIZ.SA.ClsCommon.CurrentShamsiDateString, 3)<=366 Then TmpUnitCountElevator=TmpUnitCountElevator+1 : Msgbox("TmpUnitCountElevator a",TmpUnitCountElevator) 
			            	End if 
			            End if 	
		            Else
		            
			           if TaeedMMokateb()=0 Then 
			            	TmpUnitCountElevator=TmpUnitCountElevator+1  
			            Else	
			            	if GetDiffDate(TmpBaseElevator(iji).LetterDate, BIZ.SA.ClsCommon.CurrentShamsiDateString, 3)<=366 Then TmpUnitCountElevator=TmpUnitCountElevator+1  : Msgbox("TmpUnitCountElevator b",TmpUnitCountElevator) 
			            End if 
			           
		            End if 
	            	
		            		
	            Next 
	            
	     
	            if TmpUnitCount=0 And TmpBaseUsing.count>0 Then TmpUnitCount=1 
	            
	            If TmpUnitCount <> TmpUnitCountElevator Then
	            	            	
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی *", "لطفا اطلاعات اسانسور را تکمیل نمایید دیتا بازدید شامل " & TmpUnitCount & "می باشد اطلاعات مورد تایید اسانسور شامل " & TmpUnitCountElevator & "می باشد" ) :  Exit Function
	            End If
			Else
				
			'	if TypeObjMain =2 Then 
					Info8.ParameterList.Add("NidBase", Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 0, 0,Guid.Parse("00000000-0000-0000-0000-000000000000")).ToString)
			'	ElseIf TypeObjMain =3 or TypeObjMain =5 Then 
			'		Info8.ParameterList.Add("NidBase", Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 0, 0,Guid.Parse("00000000-0000-0000-0000-000000000000")).ToString) 
			'	End if	
			
				Dim TmpUnitCount As Integer = 0
				Dim TmpUnitCountNew As Integer = 0
	            Dim TmpBaseUsing As New List(Of Global.DtoSC.Base_Using)
	            if TypeObjMain =3  Then 
	            	TmpBaseUsing = Info8.GetRevisit_Building.GetBase_Using
	            ElseIf TypeObjMain =5 Then 
	            	TmpBaseUsing = Info8.GetRevisit_HouseSharing.GetBase_Using
	            End if		
				 Dim tmpnew As New List(Of DtoSC.Base_Using)
	            TmpBaseUsing = TmpBaseUsing.Where(Function(f) f.CI_UsingType = 831).ToList
	            tmpnew=TmpBaseUsing 'new 1076866
	          
	           
	            TmpUnitCountNew=0
	            Dim Min_Floor,Max_Floor,jj as Integer
	            
	            if TmpBaseUsing.count>0 Then
	            	Min_Floor = TmpBaseUsing.Min(Function(f) f.FloorNo)
	            	Max_Floor = TmpBaseUsing.max(Function(f) f.FloorNo) ' -min
	            	'Msgbox("","Min_Floor :" & Min_Floor )
	            	'Msgbox("","Max_Floor :" & Max_Floor )
	            
		            For jj=Min_Floor to Max_Floor
		          
		            		tmpnew = TmpBaseUsing.Where(Function(f) f.FloorNo = jj).ToList
		            		TmpUnitCount = tmpnew.sum(Function(f) f.UnitCount) 'Sum
		            	'	plogfile("TmpUnitCount!",TmpUnitCount & "tmpnew.count" & tmpnew.count & "jj:" & jj) 
		            	'Msgbox("a" & jj,"TmpUnitCountNew :" & TmpUnitCountNew & "TmpUnitCount:" & TmpUnitCount )
		            		if TmpUnitCountNew<TmpUnitCount Then TmpUnitCountNew=TmpUnitCount
		            'Msgbox("b","TmpUnitCountNew :" & TmpUnitCountNew )
		             Next
	              
	            	TmpUnitCount = TmpUnitCountNew '130566
	          
	            End if
	             
	           
	            Dim TmpBaseElevator As New List(Of Global.DtoSC.Base_Elevator)
	            TmpBaseElevator = Info8.GetElevator.Base_Elevator
	               
	         ' TmpBaseElevator=TmpBaseElevator.Where(Function(f) f.IsRevoke <> True).tolist '0
	         
	         
	         '	TmpBaseElevator=TmpBaseElevator.Where(Function(f) f.IsRevoke.tostring <> "True").tolist '  new 2130468
	        	
	           	
	            Dim TmpUnitCountElevator As Integer = 0
	          
	            Dim iji as Integer
	             Msgbox("TmpBaseElevator:",TmpBaseElevator.count) 
	             Try
	            For iji =0 to TmpBaseElevator.count -1 
	              
	               
	               if Not isdbnull(TmpBaseElevator(iji).IsRevoke) Then 
	             
	               		if  TmpBaseElevator(iji).IsRevoke Then 
	               		Else
	               		 
			            	if TaeedMMokateb()=0 Then 
			            	
			            		TmpUnitCountElevator=TmpUnitCountElevator+1  
			            	Else	
			           
			            	    if   String.IsNullOrEmpty(TmpBaseElevator(iji).CI_ElevatorType.tostring)=False Then
				                      
				                     if TmpBaseElevator(iji).CI_ElevatorType=2 Then 'RE 4329708
				                       TmpUnitCountElevator=TmpUnitCountElevator+1 'RE 4329708
				                     Else 'RE 4329708
				            		   if GetDiffDate(TmpBaseElevator(iji).LetterDate, BIZ.SA.ClsCommon.CurrentShamsiDateString, 3)<=366 Then TmpUnitCountElevator=TmpUnitCountElevator+1 : Msgbox("TmpUnitCountElevator c",TmpUnitCountElevator)  
				            		 End if
				                Else
				               	
				                   	 Msgbox("zza:","LetterDate:" & TmpBaseElevator(iji).LetterDate.tostring & "CurrentShamsiDateString:" & BIZ.SA.ClsCommon.CurrentShamsiDateString & "GetDiffDate:" & GetDiffDate(TmpBaseElevator(iji).LetterDate, BIZ.SA.ClsCommon.CurrentShamsiDateString, 3) )
				            		 if GetDiffDate(TmpBaseElevator(iji).LetterDate, BIZ.SA.ClsCommon.CurrentShamsiDateString, 3)<=366 Then TmpUnitCountElevator=TmpUnitCountElevator+1 : Msgbox("TmpUnitCountElevator c",TmpUnitCountElevator)  
			            	    End if
			            		 
			            		
			            		
			            	End if 
			            
			         	
			            End if 	
		            Else
		           
		            	
			            	if TaeedMMokateb()=0 Then 
			            		TmpUnitCountElevator=TmpUnitCountElevator+1  
			            	Else	
			            		if GetDiffDate(TmpBaseElevator(iji).LetterDate, BIZ.SA.ClsCommon.CurrentShamsiDateString, 3)<=366 Then TmpUnitCountElevator=TmpUnitCountElevator+1 : Msgbox("TmpUnitCountElevator d",TmpUnitCountElevator) 
			            	End if 
			           
		            End if 
	               
	            	
	            Next 
	          	 Catch
	          	 		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی #", "لطفا اطلاعات اسانسو و یا اطلاعات مکاتبات مربوطه صحیح وارد نشده است " ) :  Exit Function
	          	 End Try
	            if TmpUnitCount=0 And TmpBaseUsing.count>0 Then TmpUnitCount=1
	           ' plogfile("TmpUnitCount",TmpUnitCount) 
	            Msgbox("TmpUnitCountElevator",TmpUnitCountElevator) 
	            Msgbox("TmpUnitCount b",TmpUnitCount)
	            if TmpUnitCountElevator>TmpUnitCount Then 'RE 4329708 or Info8.GetRequest.Info.NidWorkItem=4049277 
	            ElseIf TmpUnitCount <> TmpUnitCountElevator Then
	           		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی #", "لطفا اطلاعات اسانسور را تکمیل نمایید دیتا بازدید شامل " & TmpUnitCount & "می باشد اطلاعات مورد تایید اسانسور شامل " & TmpUnitCountElevator & "می باشد" ) :  Exit Function
	            End If
			End if
		End if 
	End if	
''118411

	End if
	

	'Info8.AddError(BIZ.SA.EumErrorAction.Stop, Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm, "sss")
 'If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","aaaa")  


if  Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And (Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirmDetails=39 or Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirmDetails=35) _
And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1  Then

'کد ملی 
Msgbox("A9","A9")
'----------------------------------------------------------------
 if TypeObj=2 Then 
	 Dim tmpOwner As LIST(Of DtoSC.Base_Owner) = Info8.GetRevisit_House.GetBase_Owner
	 If Not tmpOwner.ANY Then
	    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا اطلاعات مالک را وارد نمایید") :  Exit Function
	 Else
	    For I = 0 To tmpOwner.COUNT - 1
	        If tmpOwner(I).OwnerName = "" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "8989نام مالک را وارد نمایید") :  Exit Function
	        If tmpOwner(I).EumOwnerCharacter = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "نوع شخصیت مالک را وارد نمایید") :  Exit Function
	        if tmpOwner(I).EumOwnerCharacter <>2 Then
			    If isdbnull(tmpOwner(I).NationalCode) Then 
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا کد ملی را وارد کنید") :  Exit Function 
				ElseIf tmpOwner(I).NationalCode ="" Then 
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا کد ملی را وارد کنید") :  Exit Function
				Else
					if Not CheckMeli( tmpOwner(I).NationalCode) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد ملی معتبر نمی باشد") :  Exit Function
				End if
			End if	
	     Next
	 End If
ElseIf TypeObj=4 Then 
	 Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
     Info8.ParameterList.Add("Revisit_Apartment_NidBase", aj.RApartment.NidBase.ToString)
     Dim tmpOwner As LIST(Of DtoSC.Base_Owner) 
     tmpOwner = Info8.GetRevisit_Apartment.GetBase_Owner
	 If Not tmpOwner.ANY Then
	    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا اطلاعات مالک را وارد نمایید") :  Exit Function
	 Else
	    For I = 0 To tmpOwner.COUNT - 1
	        If tmpOwner(I).OwnerName = "" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "7850نام مالک را وارد نمایید") :  Exit Function
	        If tmpOwner(I).EumOwnerCharacter = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "نوع شخصیت مالک را وارد نمایید") :  Exit Function
	        if tmpOwner(I).EumOwnerCharacter <>2 Then
			    If isdbnull(tmpOwner(I).NationalCode) Then 
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا کد ملی را وارد کنید") :  Exit Function
				ElseIf tmpOwner(I).NationalCode ="" Then 
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا کد ملی را وارد کنید") :  Exit Function
				Else
					if Not CheckMeli( tmpOwner(I).NationalCode) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد ملی معتبر نمی باشد") :  Exit Function
				End if
			End if	
	     Next
	 End If
End if	
	
'94242 if TypeObjMain=4 Then 
'94242	Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
'94242	Info8.ParameterList.Add("Revisit_Apartment_NidBase", a.RApartment.NidBase.ToString)
'94242	tmpOwner = Info8.GetRevisit_Apartment.GetBase_Owner
'94242End if 
'94242 If Not tmpOwner.ANY Then
'94242    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا اطلاعات مالک را وارد نمایید")
'94242 Else
'94242    For I = 0 To tmpOwner.COUNT - 1
'94242        If tmpOwner(I).OwnerName = "" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "نام مالک را وارد نمایید")
'94242        If tmpOwner(I).EumOwnerCharacter = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "نوع شخصیت مالک را وارد نمایید")
'94242        if tmpOwner(I).EumOwnerCharacter <>2 Then
'94242		    If isdbnull(tmpOwner(I).NationalCode) Then 
'94242				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا کد ملی را وارد کنید") 
'94242			ElseIf tmpOwner(I).NationalCode ="" Then 
'94242				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا کد ملی را وارد کنید") 
'94242			Else
'94242				if Not CheckMeli( tmpOwner(I).NationalCode) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد ملی معتبر نمی باشد")
'94242			End if
'94242		End if	
'94242     Next
'94242 End If
'----------------------------------------------------------------

End if
  ' 106170  گ132130
  ' Modir_Daramad=True
  If Info8.GetRevisit_House.GetBase_NosaziCode.District <> 80  Then
'sp 378519 CI_ResourceManagerConfirm=5  CI_ResourceManagerConfirm=16
        If TmpListManger.EumManagerConfirmLicence = 1 And (TmpListManger.CI_ResourceManagerConfirm = 3 or TmpListManger.CI_ResourceManagerConfirm = 5 or TmpListManger.CI_ResourceManagerConfirm = 16 ) And Fiche() = 0 Then
        	if 	Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"4BFCFAE5-ACF9-4159-9BF1-1B2C0781ED0E" And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring) <>"35CFFB97-67A3-488E-8A7A-BEC93C459B99" _
        	And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"B6A28C29-6761-4719-8B62-C8575F88F572" And ucase(NidWDeff7)<>"35CFFB97-67A3-488E-8A7A-BEC93C459B99" _
        	And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" _
        	And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB" _
        	And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"E10EDCBA-7945-4DDD-8306-9BAA37529AE2" _
        	And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
        	And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"3157211F-C484-48E9-B65C-6BF3271A9F15" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
        	And Ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"BDD563C8-1EB2-40EC-8811-19DB03908488" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"0FC9224B-94DA-4156-89C2-C9E7C7ACDFDA" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"25F1F6FA-354F-46B4-BE0F-3497E1F82E20" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"D1CDEBF9-EE74-4994-9942-877BF3D2ECCB" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>("a06e7139-acc1-46a1-97f1-d4c92563e55e") _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>("5EC6DB49-B836-474B-950D-876816C82262") _
        	And TaeedSabegheMojaz=False Then
        	 'D1CDEBF9-EE74-4994-9942-877BF3D2ECCB  sp 393636
        	 if TmpListManger.CI_ResourceManagerConfirm = 16 And Modir_Daramad=True And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="3708270C-B7D9-495E-B72A-20D072D26D52" Then 'SP 367183 ...serfan nokte sampa tavasote ghovati  ok shod
        	    'استعلام محضر - دفترخانه
        	   'در این نوع درخواست امکان تایید تقسیط در فرم تایید مدیریان صرفا برای گروه "مدیر درآمد " بایستی فعال گردد 
        	 Else
        	 	if Info8.GetRequest.Info.NidWorkItem<>10892332 Then   'sp 510992     
        	    	if  LstUserGroupId(LstUserDaramad)<> 1 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر ** ", "فیش تایید از بانک  درآمد ندارد") :  Exit Function 'new 2249811----E10EDCBA-7945-4DDD-8306-9BAA37529AE2   sp 200032
        	 	End if    'end sp 510992    
        	 End if
        	 
        	End if
        	
        End if	
 ' 106170 132130
  End If
	
    If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 5 Then
        Dim NidBaseTo As New Guid
        Dim tmpNidRevisit As guid
       
        tmpNidRevisit = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit

        Dim b As BIZ.SC.ClsNidLib
        
        b = Info8.GetFunctions.GetNidLib(tmpNidRevisit)
       
        NidBaseTo = b.RMovafeghatOsooli.NidBase()
        
        Info8.ParameterList.Remove("MovafeghatOsooli_NidBase")
        Info8.ParameterList.Add("MovafeghatOsooli_NidBase", NidBaseTo.ToString)
        Info8.ParameterList.Remove("NidProc")
        
        Info8.ParameterList.Add("NidProc", Info8.GetManagerConfirm.NidProc.tostring)
	
        Info8.GetMovafeghatOsooli().CopyFromMovafeghatOsooliToMapControl(Info8.FindParameter("NidProc").tostring, Info8.GetMovafeghatOsooli.GetBase_Using _
                   , Info8.GetMovafeghatOsooli.GetBase_Front, Info8.GetMovafeghatOsooli.Base_Parking, Info8.GetMovafeghatOsooli.Base_SupplyParking _
                      , Info8.GetMovafeghatOsooli.Base_OtherEquipment, Info8.GetMovafeghatOsooli.Base_Bezel, Info8.GetMovafeghatOsooli.Base_Gap _
                      , Info8.GetMovafeghatOsooli.Base_Wall, Info8.GetMovafeghatOsooli.Base_Door,Info8.GetMovafeghatOsooli.Base_BuildingInfo , Info8.Security_RequestGuid, Info8.Security_EncryptCode)

    End If

    ''''''''''''''''''''''''''''''''''''''''''
    
    Try
        Info8.ParameterList.Remove("NidProc")
        Info8.ParameterList.Add("NidProc", Info8.GetManagerConfirm.NidProc.tostring)
        Info8.ParameterList.Remove("NidRevisit")
        Info8.ParameterList.Add("NidRevisit", Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring)

        Dim tayeedparvaneh As Boolean = False
        Dim tayeedpayankar As Boolean = False

        If Info8.GetRequest.Info.NidWorkflowDeff.ToString = "008df040-922b-41a6-a3b2-7f35b1c870bb" _
         or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString )="79903151-AF7C-4A73-9F18-6EB915F3293F" Then
            tayeedparvaneh = True

        ElseIf Info8.GetRequest.Info.NidWorkflowDeff.ToString = "8b4af4ad-a7e7-4330-bd2c-f39b3a07adfd" _
        or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString )= "83F0FAC4-2268-4005-8D52-34266DD9F595" Then
            tayeedpayankar = True
        End If

        If tayeedpayankar = True Or tayeedparvaneh = True Then

            Dim NidBaseTo As Guid


            Dim dto_Savabegh, TmpSavabegh As New List(Of DtoSC.Base_ShahrsaziArchive)
			Dim TmpParent_S As New List(Of Global.Biz.SA.ClsNosaziCodeType)
		
		    TmpParent_S = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
		    
            If Not (aj.RDastgah Is Nothing) Then
				TmpParent_S=TmpParent_S.where(Function (f) f.EumNosaziCodeObjType=5).tolist
                Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
                Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", TmpParent_S(0).NidBase.ToString)
				
                '143885 dto_Savabegh = Info8.GetRevisit_HouseSharing.GetBase_ShahrsaziArchive()
                dto_Savabegh =Info8.GetParvandeh_HouseSharing.GetBase_ShahrsaziArchive()
                TypeObj = 5
            ElseIf Not (aj.RBuilding Is Nothing) Or Not (aj.RHouse Is Nothing) Then
				TmpParent_S=TmpParent_S.where(Function (f) f.EumNosaziCodeObjType=2).tolist
                Info8.ParameterList.Remove("Parvandeh_House_NidBase")
                Info8.ParameterList.Add("Parvandeh_House_NidBase", TmpParent_S(0).NidBase.ToString)
                '143885dto_Savabegh = Info8.GetRevisit_House.GetBase_ShahrsaziArchive()
				dto_Savabegh = Info8.GetParvandeh_House.GetBase_ShahrsaziArchive()
                TypeObj = 3
            End If
            If TypeObj = 5 Or TypeObj = 3 Then

                If dto_Savabegh.count = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "AA تایید مدیر", "با توجه به عدم وجود سابقه شهرسازی امکان تایید سابقه وجود ندارد.") :  Exit Function
                If dto_Savabegh.count > 0 Then
                    If tayeedparvaneh = True Then
                        TmpSavabegh = dto_Savabegh.where(Function(f) f.Ci_ShahrsaziArchiveGroup = 1).tolist()
                        If TmpSavabegh.count = 0 Then
                            Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "در سوابق شهرسازی، پروانه وجود ندارد.") :  Exit Function
                        Else
                            Dim DateMax As String = ""
                            DateMax = iif(isdbnull(TmpSavabegh.Max(Function(f) f.CertificateDate)), "", TmpSavabegh.Max(Function(f) f.CertificateDate))
                            If trim(DateMax) = "" Or DateMax = "____/__/__" Or DateMax.Length <> 10 Then
                                Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "تاریخ پروانه را در سوابق شهرسازی اصلاح نمایید.") :  Exit Function
                            End If
                        End If
                    End If

                    If tayeedpayankar = True Then
                        TmpSavabegh = dto_Savabegh.where(Function(f) f.Ci_ShahrsaziArchiveGroup <> 1 And f.Ci_ShahrsaziArchiveGroup <> 5 And f.Ci_ShahrsaziArchiveGroup <> 6).tolist()
                        If TmpSavabegh.count = 0 Then
                            Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "در سوابق شهرسازی، درخواستی در گروه پایانکار وجود ندارد.") :  Exit Function
                        Else
                            Dim DateMax As String = ""
                            DateMax = iif(isdbnull(TmpSavabegh.Max(Function(f) f.CertificateDate)), "", TmpSavabegh.Max(Function(f) f.CertificateDate))
                            If trim(DateMax) = "" Or DateMax = "____/__/__" Or DateMax.Length <> 10 Then
                                Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "تاریخ پایانکار را در سوابق شهرسازی اصلاح نمایید.") :  Exit Function
                            End If
                        End If
                    End If

                End If
            End If
        End If
    Catch
    End Try

    '-
    If TaeedMGavahi() = 0 Then Call ComfirmFicheDiscount()


    '--------------------------
    If (TypeObjMain = 2 Or TypeObjMain = 3 Or TypeObjMain = 5) And (TmpListManger.CI_ResourceManagerConfirm = 3 Or TmpListManger.CI_ResourceManagerConfirm = 7) And TmpListManger.EumManagerConfirmLicence = 1 Then
        'if Info8.User.UserGuid.tostring = "6f99c240-c92b-4fb3-99db-6760750167eb" Then
        Dim M_OtherEQ, M_OtherEQMap As New List(Of Dtosc.Base_OtherEquipment)
        If Not (aj.RHouse Is Nothing) Then
            If Not (aj.RDastgah Is Nothing) Then
                Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
                Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", aj.RDastgah.NidBase.ToString)
            End If
            If Not (aj.RBuilding Is Nothing) Then
                Info8.ParameterList.Remove("Revisit_Building_NidBase")
                Info8.ParameterList.Add("Revisit_Building_NidBase", aj.RBuilding.NidBase.ToString)
            End If
            If Not (aj.RMapControl Is Nothing) Then
                Info8.ParameterList.Remove("MapControl_NidBase")
                Info8.ParameterList.Add("MapControl_NidBase", aj.RMapControl.NidBase.ToString)
            End If
            Dim MaxIncomedate, TahlilDate, TahlilTime, DateMapControl, TimeMapControl, DateBazdid, TimeBazdid As String
            M_OtherEQ = Info8.GetRevisit_House.GetBase_OtherEquipment()
            If Not (aj.RMapControl Is Nothing) Then M_OtherEQMap = Info8.GetMapControl.GetBase_OtherEquipment()
            If Not (aj.RBuilding Is Nothing) And typeobj = 3 Then M_OtherEQ = M_OtherEQ.Concat(Info8.GetRevisit_Building.GetBase_OtherEquipment).Tolist
            If Not (aj.RDastgah Is Nothing) And typeobj = 5 Then M_OtherEQ = M_OtherEQ.Concat(Info8.GetRevisit_HouseSharing.GetBase_OtherEquipment).Tolist


            TahlilDate = Analys()
            TahlilTime = AnaliysParvaneh_Time

            If M_OtherEQMap.any Then
                Dim TmpList As New List(Of Dtosc.Base_OtherEquipment)
                TmpList = M_OtherEQMap.where(Function(f) f.CI_OtherEquipmentType = 250).tolist()
                If TmpList.count > 0 Then
                    DateMapControl = iif(isdbnull(TmpList.Max(Function(f) f.OtherEquipmentValue)), "1300/00/00", TmpList.Max(Function(f) f.OtherEquipmentValue))
                    TmpList = TmpList.where(Function(f) f.OtherEquipmentValue = DateMapControl).tolist()
                    If TmpList.count > 0 Then TimeMapControl = iif(isdbnull(TmpList.Max(Function(f) f.Comments)), "", TmpList.Max(Function(f) f.Comments))
                End If
            End If


            If (DateMapControl > TahlilDate Or (DateMapControl = TahlilDate And TimeMapControl > TahlilTime)) And TahlilDate > "1300/00/00" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "با توجه به تغییر اطلاعات کنترل نقشه لطفا تحلیل را مجدد محاسبه نمایید") :  Exit Function


            If M_OtherEQ.any Then
                Dim TmpList As New List(Of Dtosc.Base_OtherEquipment)
                TmpList = M_OtherEQ.where(Function(f) f.CI_OtherEquipmentType = 250).tolist()
                If TmpList.count > 0 Then
                    DateBazdid = iif(isdbnull(TmpList.Max(Function(f) f.OtherEquipmentValue)), "1300/00/00", TmpList.Max(Function(f) f.OtherEquipmentValue))

                    TmpList = TmpList.where(Function(f) f.OtherEquipmentValue = DateBazdid).tolist()
                    If TmpList.count > 0 Then TimeBazdid = iif(isdbnull(TmpList.Max(Function(f) f.Comments)), "", TmpList.Max(Function(f) f.Comments))
                End If
            End If
            
            
            Msgbox("x","DateBazdid:" & DateBazdid & "TahlilDate:"  & TahlilDate & "TimeBazdid:" & TimeBazdid & "TahlilTime:" & TahlilTime )
            If (DateBazdid > TahlilDate Or (DateBazdid = TahlilDate And TimeBazdid > TahlilTime)) And TahlilDate > "1300/00/00" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه++", "با توجه به تغییر اطلاعات بازدید لطفا تحلیل را مجدد محاسبه نمایید") :  Exit Function

            '''''''''''''''''''''''''''''''''
            If TahlilDate > "1300/00/00" Then
                Dim ListIncome As New List(Of DtoSC.Income)
                ListIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
                If ListIncome.Count > 0 Then MaxIncomedate = IIf(IsDBNull(ListIncome.Max(Function(f) f.CreateDate)), "1300/00/00", ListIncome.Max(Function(f) f.CreateDate))
                'Msgbox("ListIncome" & ListIncome.Count, "Income:" & MaxIncomedate & " kNew:" & Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc.tostring)

                If TahlilDate > MaxIncomedate And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"2C1EE596-5D29-4C0A-802A-1C2492B77A9F" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "با توجه به تغییر اطلاعات تحلیل لطفا درآمد را مجدد محاسبه نمایید") :  Exit Function
            End If

            'Msgbox("tayeed Date", "Income:" & MaxIncomedate & "TahlilDate:" & TahlilDate & "TahlilTime :" & TahlilTime & " DateMapControl:" & DateMapControl & "DateBazdid:" & DateBazdid & " TimeBazdid:" & TimeBazdid)

        End If

        'End if 
    End If

Msgbox("A10","A10")
'If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","aaaa")  

    ''''''''''''''''''''''''''''''''''''''''''''''''

    If Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "A3B24BD2-4CB2-4011-9900-3070AD088A6F" And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "87783CD5-DCF5-40A6-B4B5-5228B62C4D11" _
    And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "008DF040-922B-41A6-A3B2-7F35B1C870BB" And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "8DCD2E0C-8E5E-48A4-BAD6-8C2A1B4901B1" _
    And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "AE758A56-C5AC-4FC7-AEE9-C24C252F758C" And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "5DC809DF-16C5-459B-B6F8-C28A049A32AC" _
    And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "8B4AF4AD-A7E7-4330-BD2C-F39B3A07ADFD" And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "BB0854BE-4002-4C0C-975C-04B4B8485334" _
    And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB" And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> "B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" Then ' BB0854BE-4002-4C0C-975C-04B4B8485334   2583427

        If TmpListManger.EumManagerConfirmLicence = 1 And (TmpListManger.CI_ResourceManagerConfirm = 3 Or TmpListManger.CI_ResourceManagerConfirm = 7) And GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString) = 1 Then
            Dim NidBaseTo As New Guid
            Dim tmpNidRevisit As guid
            tmpNidRevisit = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit

            Dim b As BIZ.SC.ClsNidLib
            b = Info8.GetFunctions.GetNidLib(tmpNidRevisit)
            NidBaseTo = b.RMovafeghatOsooli.NidBase()
            Info8.ParameterList.Remove("MovafeghatOsooli_NidBase")
            Info8.ParameterList.Add("MovafeghatOsooli_NidBase", NidBaseTo.ToString)
            Info8.ParameterList.Remove("NidProc")
            Info8.ParameterList.Add("NidProc", Info8.GetManagerConfirm.NidProc.tostring)
            Dim TmpList, ListBase_Using As New List(Of DtoSC.Base_Using)
            ListBase_Using = Info8.GetMapControl.GetBase_Using()
            If ListBase_Using.count > 0 Then '66465
                Dim SumUnitCount, MaxUnitNo As Integer
                'Dim Str_UnitMaskooni as String="(401 , 402, 403, 404, 405 , 505,456 ,975)" 
                'TmpList=ListBase_Using.where (Function (f) f.CI_UsingGroup=11 And (f.CI_UsingType =401 or f.CI_UsingType =402 or f.CI_UsingType =403 or f.CI_UsingType = 404 or f.CI_UsingType = 405 or f.CI_UsingType = 505 or f.CI_UsingType =456 or f.CI_UsingType =975 )).tolist ()
                TmpList = ListBase_Using.where(Function(f) f.CI_UsingType = 401 Or f.CI_UsingType = 402 Or f.CI_UsingType = 403 Or f.CI_UsingType = 404 Or f.CI_UsingType = 405 Or f.CI_UsingType = 505 Or f.CI_UsingType = 456 Or f.CI_UsingType = 975 Or f.CI_UsingType = 411 Or f.CI_UsingType = 412 Or f.CI_UsingType = 2025 Or f.CI_UsingType = 420).tolist()
                If TmpList.count > 0 Then
                    SumUnitCount = iif(isdbnull(TmpList.sum(Function(f) f.UnitCount)), 0, TmpList.sum(Function(f) f.UnitCount))

                    Dim listShenasnamefani As New List(Of DtoSC.Sh_ShenasnameFani)
                    'listShenasnamefani = Info8.ShenasnameFani.Sh_ShenasnameFani.tolist()
                    'MaxUnitNo= iif (isdbnull (listShenasnamefani.Max(Function(f) f.FloorNo)) , 0 , listShenasnamefani.Max(Function(f) f.FloorNo))

                    MaxUnitNo = iif(isdbnull(ListBase_Using.max(Function(f) f.unitNo)), 0, ListBase_Using.Max(Function(f) f.unitNo))
                    'Hif SumUnitCount>0 And SumUnitCount<> MaxUnitNo Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی","اطلاعات شناسنامه فنی در فرم کنترل نقشه تکمیل نگردیده است." )

                End If
                'Msgbox(MaxUnitNo, " SumUnitCountNew shenasname:  " & SumUnitCount)

                Dim NoNeedSaze As Boolean
                Dim flag as Boolean=False
                If TmpListManger.EumManagerConfirmLicence = 1 And (TmpListManger.CI_ResourceManagerConfirm = 5 or  TmpListManger.CI_ResourceManagerConfirm=7 or TmpListManger.CI_ResourceManagerConfirm=3) And GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString) = 1 And GuidWorkFlow=112 Then  flag=True
                
                

                For i = 0 To ListBase_Using.Count - 1
                    NoNeedSaze = False
                    If ListBase_Using.Item(i).CI_UsingType = 438 Or ListBase_Using.Item(i).CI_UsingType = 429 Or ListBase_Using.Item(i).CI_UsingType = 585 Or ListBase_Using.Item(i).CI_UsingType = 1016 Then NoNeedSaze = True
                    if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="097C8C41-BA8C-4B60-8B2F-6BED0221F573" or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="2635FE18-FE4C-4A2D-9D05-993756751262" or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="8D20BE44-78AB-4001-BB94-F064D22B9CE1" Then NoNeedSaze = True 'RE 10018028 sara10

                    If flag=False And ListBase_Using.Item(i).CI_SazehType = 0 And NoNeedSaze = False Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد نوع سازه رديف " & i + 1 & " فرم کنترل نقشه وارد نشده است") :  Exit Function 'CI_BuildingType
                    If flag=False And ListBase_Using.Item(i).CI_RoofType = 0 And NoNeedSaze = False Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد نوع سقف رديف " & i + 1 & " فرم کنترل نقشه وارد نشده است") :  Exit Function '
                    If flag=False And ListBase_Using.Item(i).CI_BuildingKind = 0 And NoNeedSaze = False Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد نوع ساختمان رديف " & i + 1 & " فرم کنترل نقشه وارد نشده است") :  Exit Function
                Next
            End If
        End If
    End If

    If Info8.GetRevisit_House.GetBase_NosaziCode.District = 80 And Info8.GetRevisit_House.GetBase_NosaziCode.Region = 80 And TmpListManger.EumManagerConfirmLicence = 1 And (TmpListManger.CI_ResourceManagerConfirm = 3 Or TmpListManger.CI_ResourceManagerConfirm = 7) Then
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "امکان صدور گواهی بر روی کد موقت وجود ندارد ") :  Exit Function
    End If
   
    If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 3 And GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString) = 1 Then
    	Dim District as Integer  =Info8.GetRevisit_House.GetBase_NosaziCode.District
    	Dim Region as Integer  = Info8.GetRevisit_House.GetBase_NosaziCode.Region 
    	if District=12 And ( Region =15 or Region=16 or Region=17 or Region=18 or Region=19 or Region=20 )Then '1657768
    	Else 
        '105581	if	TaeedMShenasnameFani()<> 0  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی","با توجه به عدم تایید شناسنامه فنی امکان تایید گواهی وجود ندارد ") 
        End if 
	End If

	If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 3  Then
    	Call MapHarim() 'sampa 83012
    End if 
  
    If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 9 Then
        'If GhaleCom100() = False Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فاقد شرایط احراز می باشد. ")
		Call GhaleCom100()
    End If
    
    If TmpListManger.EumManagerConfirmLicence = 1 And (TmpListManger.CI_ResourceManagerConfirm = 10 or TmpListManger.CI_ResourceManagerConfirm = 13)  Then
        Dim IsOk as Integer=0
        '269020 '299988
        If UserList7.Any Then
	        For ii = 0 To UserList7.Count - 1
				If Ucase(UserList7(ii).GroupGuid.tostring) = "D0119423-DA84-48E4-9588-6BA84E7DB1BD" or Ucase(UserList7(ii).GroupGuid.tostring) = "3EA579D1-68D0-406B-BDD5-C5FBCFD1C1E8" _
				or Ucase(UserList7(ii).GroupGuid.tostring) = "61892BC0-C5DF-4D0A-B1DA-C0F10B659878" or Ucase(UserList7(ii).GroupGuid.tostring) = Ucase("08d20b53-d68a-4d22-a0e0-b43eec51461e") _
				or Ucase(UserList7(ii).GroupGuid.tostring) =  Ucase("05aed9ee-74d6-4300-a1b3-c7d37cfb15af") or  Ucase(UserList7(ii).GroupGuid.tostring) =  Ucase("3a718066-1c33-445e-9b03-e0277f6cc756") _ 
				or Ucase(UserList7(ii).GroupGuid.tostring) =  Ucase("8c4c134c-864d-4757-8e34-b734e226c162") or Ucase(UserList7(ii).GroupGuid.tostring) ="C0331976-2235-4688-B8A5-37EB4517FE8A" Then ' 
	                IsOk=1
	            End If
	        Next ii
	    End If
	    
	    if IsOk=0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "@@شما مجاز به تاييد نمی باشید ") :  Exit Function
    End If
    '-	
    	  
	If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 1 Then
    	if  GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString) = 1 Then 
    		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "با توجه به نوع درخواست نوع تایید انتخابی نادرست است. ") :  Exit Function
    	Else
    	'187627
    	'187627
    		Dim TmpFineHelp As New System.Collections.Generic.List(Of DtoSC.Commission_FinePenalty)
			
	    	TmpFineHelp = Info8.GetFine.Commission_FinePenalty
	        Dim kkj as Integer=0
	        'Sp457353 حذف نظر شهردار برای مشمول سرقفلی
	        For kkj = 0 To TmpFineHelp.Count - 1
	        	If TmpFineHelp(kkj).CI_CommissionUrbanIdea = 0 And TmpFineHelp(kkj).FramePrice<>0   And TmpFineHelp(kkj).CI_CommissionFinePenalty <> 3141 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "در تمامی ردیف های جریمه لایحه ، نظر شهردار باید مقدار داشته باشد") :  Exit Function
	        Next
	    End if
    End if 
 
  ' If Info8.User.UserGuid.tostring = "6f99c240-c92b-4fb3-99db-6760750167eb" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر test1 ", "Stop")'''hessami 95-07-15
''''''''''''''''''''''''''''''''''''100487
Msgbox("A5","A5")


If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 3 Then
	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("4BFCFAE5-ACF9-4159-9BF1-1B2C0781ED0E") Then
		Dim LBaseUsing As List(Of Global.DtoSC.Base_Using)
		
	Dim currentArea as Double=0	'new sampa 2142833
	currentArea=Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea
		
	
		if 	TypeObjMain =4 Then
			LBaseUsing=Info8.GetRevisit_Apartment.GetBase_Using
		Else
		
			LBaseUsing=Info8.GetRevisit_Building.GetBase_Using
		End if 	
		LBaseUsing=LBaseUsing.where(Function(f) f.GenerateDate<"1395/10/01").tolist
	
'sp 495221	if currentArea>=60 And currentArea<=500 And LBaseUsing.count>0 Then 'new sampa 2142833
'sp 495221	Else
'sp 495221		if Info8.Getrequest.info.nidworkitem <> 5831733 And Info8.Getrequest.info.nidworkitem <> 5868264 _
'sp 495221		   And Info8.Getrequest.info.nidworkitem <> 5867707 And Info8.Getrequest.info.nidworkitem <> 5872991 Then  'sp 488805  -  5867707(sp490951) - 5868264(sp 491846) - 5872991(sp 493594) به دلیل قدیمی بودن سمپا فعلا موردی رفع میکنن
'sp 495221		  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", " ملک مذکور فاقذ شرایط احراز مصوبه 2543 / 96 /4 /ش شورای اسلامی شهر می باشد ") :  Exit Function
'sp 495221		End if  
'sp 495221	End if
		
	'	if  Info8.GetRevisit_House.GetBase_HouseInfo.DocArea<60 or Info8.GetRevisit_House.GetBase_HouseInfo.DocArea>500 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", " ملک مذکور فاقذ شرایط احراز مصوبه 2543 / 96 /4 /ش شورای اسلامی شهر می باشد ")
		
	'	Info8.AddError(BIZ.SA.EumErrorAction.warning, "Info8.GetRevisit_House.GetBase_HouseInfo.DocArea2", Info8.GetRevisit_House.GetBase_HouseInfo.DocArea)
		
	'	if 	TypeObjMain =4 Then
	'		LBaseUsing=Info8.GetRevisit_Apartment.GetBase_Using
	'	Else	
	'		LBaseUsing=Info8.GetRevisit_Building.GetBase_Using
	'	End if 	
	'	LBaseUsing=LBaseUsing.where(Function(f) f.GenerateDate>"1395/10/01").tolist
	'	if LBaseUsing.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", " ملک مذکور فاقذ شرایط احراز مصوبه 2543 / 96 /4 /ش شورای اسلامی شهر می باشد ")
	End if 


 	
 	
	if GetSara8Workflow(Info8.getRequest.Info.NidWorkflowDeff.tostring)=1 Then 
		if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="A3B24BD2-4CB2-4011-9900-3070AD088A6F" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="83F0FAC4-2268-4005-8D52-34266DD9F595" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="87783CD5-DCF5-40A6-B4B5-5228B62C4D11" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="79903151-AF7C-4A73-9F18-6EB915F3293F" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="008DF040-922B-41A6-A3B2-7F35B1C870BB" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="BB0854BE-4002-4C0C-975C-04B4B8485334" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="8B4AF4AD-A7E7-4330-BD2C-F39B3A07ADFD" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="097C8C41-BA8C-4B60-8B2F-6BED0221F573" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="2635FE18-FE4C-4A2D-9D05-993756751262" _
		   or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" Then 
		   'sp430671    097C8C41-BA8C-4B60-8B2F-6BED0221F573
		   'sampa 177254 "BB0854BE-4002-4C0C-975C-04B4B8485334" 
		   
		Else  
		
			Dim tmpusing as List(Of DtoSC.Base_Using)
			Dim tmpusing1 as List(Of DtoSC.Base_Using)
			Dim tmpusing2 as List(Of DtoSC.Base_Using)
			Dim tmpFront as List(Of DtoSC.Base_Front)
			Dim tmpArea as Double 
			tmpusing= Info8.GetMovafeghatOsooli.GetBase_Using()
			tmpFront= Info8.GetMovafeghatOsooli.GetBase_Front()
				
			tmpArea = tmpusing.Sum(Function(f) f.UsingArea)
		    tmpusing1=tmpusing.where(Function (f) f.CI_UsingType = 968 or  f.CI_UsingType = 923  or f.CI_UsingType = 816 or f.CI_UsingType =820 or f.CI_UsingType =838 or f.CI_UsingType =827 or f.CI_UsingType =806 or f.CI_UsingType =640 or f.CI_UsingType =438 or f.CI_UsingType =418 ).tolist
		                     
		    tmpusing2=tmpusing.where(Function (f) f.CI_UsingType = 437).tolist
		    tmpArea =tmpArea - tmpusing1.Sum(Function(f) f.UsingArea) - (tmpusing2.Sum(Function(f) f.UsingArea) * 0.5)
		    tmpArea+ = tmpFront.Sum(Function(f) f.FrontArea)
		   
		   
		  
		     '               -------------------------
		   
			Dim tmpAreaP as Double 
			tmpusing= Info8.GetMapControl.GetBase_Using()	
			tmpFront= Info8.GetMapControl.GetBase_Front()
			
			tmpAreaP = tmpusing.Sum(Function(f) f.UsingArea)
		    tmpusing1=tmpusing.where(Function (f) f.CI_UsingType = 968 or  f.CI_UsingType = 923  or f.CI_UsingType = 816 or f.CI_UsingType =820 or f.CI_UsingType =838 or f.CI_UsingType =827 or f.CI_UsingType =806 or f.CI_UsingType =640 or f.CI_UsingType =438 or f.CI_UsingType =418 ).tolist
		    
		                     
		    tmpusing2=tmpusing.where(Function (f) f.CI_UsingType = 437).tolist
		    tmpAreaP =tmpAreaP - tmpusing1.Sum(Function(f) f.UsingArea) - (tmpusing2.Sum(Function(f) f.UsingArea) * 0.5)
		    	    
		    tmpAreaP+ = tmpFront.Sum(Function(f) f.FrontArea)
		    
		    
		   
		    
		  
		 
		 
		 
		    tmpAreaP=math.round(tmpAreaP+0.1 ,2)' 2409323
		    tmpArea=math.round(tmpArea+0.1 ,2) ' 2409323
		  
		 
		   msgbox("tmpAreaP@",tmpAreaP)
		   msgbox("tmpArea@",tmpArea)
		   
		   'if Info8.User.UserGuid.tostring = "540d119d-14f1-4f65-94d1-7f39fc2b3b48" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "yyy", "sss")  :  Exit Function            
		    
		    if (tmpAreaP <> tmpArea) And math.max(tmpAreaP-tmpArea,0)>0.1 Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "متراژ کنترل نقشه با متراژ موافقت اصولی یکسان نمی باشد")  :  Exit Function            
		End if	    
	 End if
End if	    
	''''''''''''''100487  end
    '  If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","ذذذ")  
 
  '221363      Try
'221363if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B6A28C29-6761-4719-8B62-C8575F88F572" or _
'221363((Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="E530B5D8-6283-4E95-B655-6DB83377A6E7" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C83F6228-987B-4B5E-8906-56F7CD87EA7C" ) And Info8.GetRequest.Info.RequestDate>"1396/06/04") Then
    'D84FA3D1-54FD-4D24-8000-758A300B2441   9A15158D-0369-452C-B73D-E8AC3295FE10   EE64A610-9142-4E2B-953F-6E65B522A9E1
'197308
'221363	if TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm=3 Then 
		
'221363		Dim tmpFire As New Global.Biz.Communication.ClsFireFighting
'221363		Dim ReturnFire as Integer=0
'221363		Dim TmpCom as String =""
		
	'*	ReturnFire=tmpFire.GetInstanceState (Info8.GetRequest.Info.NidWorkItem)
'221363	    ReturnFire=tmpFire.GetInstanceState (Info8.GetRequest.Info.NidWorkItem.ToString,Info8.User.UserGuid,Info8.User.UserName.ToString)
	
'221363		if ReturnFire= 1 Then 
'221363			TmpCom="پرونده در حال پیگیری می باشد"
'221363		ElseIf ReturnFire= 2 Then 
'221363			TmpCom="پرونده تایید شده است "
'221363		ElseIf ReturnFire= 3 Then 
'221363			TmpCom="پرونده رد شده است "
'221363	    ElseIf ReturnFire= 5 Then 
'221363			TmpCom="پرونده در حال پیگیری می باشد"
'221363		Else
'221363			TmpCom="شماره پرونده پیدا نشد"
'221363		End if
'221363		TmpListManger.Comments =TmpListManger.Comments & "مقدار ارسال شده از اتش نشانی : " & TmpCom & "#" &  ReturnFire
'221363		if ReturnFire=1 or ReturnFire=5 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "پاسخی از اتش نشانی دریافت نشده") :  Exit Function
'221363		if ReturnFire=4 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "پرونده به اتش نشانی ارسال نشده است") :  Exit Function
'221363	End if 	
'221363End if	
'221363Catch ex2 As Exception
'221363	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "توجه", "مشکل در دریافت اطلاعات از اتش نشانی")
'221363End Try	
'''''''''''''GHAFARIAN
Msgbox("A4","A4")

	
if ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99"  or ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="BDD563C8-1EB2-40EC-8811-19DB03908488" Then 
If TmpListManger.EumManagerConfirmLicence = 1 And (TmpListManger.CI_ResourceManagerConfirm = 14 or TmpListManger.CI_ResourceManagerConfirm = 3 ) Then	
	 Dim p_NosziCode As String = ""
	 Dim p_RequestNo as String=""
	 p_NosziCode= Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) 
	 p_RequestNo = Info8.GetRequest.Info.NidWorkItem
	 Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
	 tmpSMS.BizCode = p_NosziCode.tostring()
	 tmpSMS.Number = Info8.GetRequest.Info.Cellphone
	 tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
	 tmpSMS.SMSType = Info8.GetRequest.Info.WorkflowTitel
	' tmpSMS.Text ="شهروند محترم جهت پرداخت عوارض انتقال و دریافت گواهی به سایت مراجعه فرمایید"
	tmpSMS.Text ="شهروند محترم جهت دریافت گواهی به سایت مراجعه فرمایید"
	
	 tmpSMS.UserID = Info8.User.UserGuid.ToString
	 tmpSMS.UserName = Info8.User.UserName
	
	
	  Dim smsErrorResult As New BIZ.SA.ClsErrorResult
	                    ' Send SMS
	  
	  	smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
	
	                    ' Check Error
	 	If smsErrorResult.BizErrors.Any Or smsErrorResult.Requirements.Any Then
		 	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "SMS", "اختلال در ارسال پیامک")
	 	Else
	    	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "SMS", "ارسال پیامک با موفقیت انجام شد")
	 	End If
	
End IF
End if 

Msgbox("A11","A11")
if ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="356BAAAB-2627-417A-9AE3-A815CAF505C1" Then 
If TmpListManger.EumManagerConfirmLicence = 1 And (TmpListManger.CI_ResourceManagerConfirm = 24 ) Then	
	 Dim p_NosziCode As String = ""
	 Dim p_RequestNo as String=""
	 p_NosziCode= Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) 
	 p_RequestNo = Info8.GetRequest.Info.NidWorkItem
	 Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
	 tmpSMS.BizCode = p_NosziCode.tostring()
	 tmpSMS.Number = Info8.GetRequest.Info.Cellphone
	 tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
	 tmpSMS.SMSType = Info8.GetRequest.Info.WorkflowTitel
	' tmpSMS.Text ="شهروند محترم جهت پرداخت عوارض انتقال و دریافت گواهی به سایت مراجعه فرمایید"
'	tmpSMS.Text ="شهروند گرامی پرونده شما تائید شده است لطفا جهت دریافت گواهی به سایت ec مراجعه نمایید.  شهرداری مشهد "
     '294677
     tmpSMS.Text ="شهروند گرامی پرونده شما تائید شده است لطفا جهت دریافت گواهی به لینک زیر http://shahr1.mashhad.ir:8072/Default.aspx منوی خدمات اختصاصی گزینه درخواست های پیشین مراجعه نمائید. شهرداری مشهد"
	
	 tmpSMS.UserID = Info8.User.UserGuid.ToString
	 tmpSMS.UserName = Info8.User.UserName
	
	
	  Dim smsErrorResult As New BIZ.SA.ClsErrorResult
	                    ' Send SMS
	  
	  	smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
	
	                    ' Check Error
	 	If smsErrorResult.BizErrors.Any Or smsErrorResult.Requirements.Any Then
		 	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "SMS", "اختلال در ارسال پیامک")
	 	Else
	    	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "SMS", "ارسال پیامک با موفقیت انجام شد")
	 	End If
	
End IF
End if 

'----------------------------------------------------------------------107114

'''''''''''''Solh
'Info8.AddError(BIZ.SA.EumErrorAction.warning, "300", "300")  
'plogfile("EumManagerConfirmLicence:" , TmpListManger.EumManagerConfirmLicence)
'plogfile("CI_ResourceManagerConfirm:" , TmpListManger.CI_ResourceManagerConfirm)
Msgbox("A3","A3") 


   
If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 6 Then	
Try	 

 
	Dim Peacelst As New List(Of DtoSC.Sh_PeaceLetter)
	Dim TmpPeacelst As New List(Of DtoSC.Sh_PeaceLetter)
	
	Dim NidBaseTo As New Guid
    Dim tmpNidRevisit As guid
   
    tmpNidRevisit = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit
	Dim Agreement as Integer=0
    Dim b As BIZ.SC.ClsNidLib
    b = Info8.GetFunctions.GetNidLib(tmpNidRevisit)
    
    Try
    	NidBaseTo = b.RPeace.NidBase()
   	Catch ex1 As Exception
   		Try
   			NidBaseTo = b.RAgreement.NidBase()
   			Agreement=1
   		Catch ex2 As Exception
   			Info8.AddError(BIZ.SA.EumErrorAction.Warning, "توجه", "لطفا ابتدا صلح یا توافق اعمال شود")
   		End Try	
   	End Try	
   	
   '	 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Agreement", Agreement)  'new comment shod 2150435
'plogfile("Agreement:" , Agreement)
   	if Agreement=0 Then
	    Info8.ParameterList.Remove("Peace_NidBase")
	    Info8.ParameterList.Add("Peace_NidBase", NidBaseTo.ToString)
	    Info8.ParameterList.Remove("NidProc")
	    
	    Info8.ParameterList.Add("NidProc", Info8.GetManagerConfirm.NidProc.tostring)
	    Peacelst = Info8.GetPeace.GetSh_PeaceLetter()
	    TmpPeacelst=Peacelst.where(Function(f) f.CI_PeaceType=2 or f.CI_PeaceType=3 or f.CI_PeaceType=4 or f.CI_PeaceType=5 or f.CI_PeaceType=6 or f.CI_PeaceType=7 or f.CI_PeaceType=20 or f.CI_PeaceType=21 ).tolist 
	  '  plogfile("TmpPeacelst.count0000:" , TmpPeacelst.count)
	    if TmpPeacelst.count >0 Then
	    	if Info8.GetRevisit_House.GetBase_CommonEstate.CI_DocType=8 or Info8.GetRevisit_House.GetBase_CommonEstate.CI_DocType=18 Then Info8.AddError(BIZ.SA.EumErrorAction.Warning, "توجه","با توجه به اینکه نوع سند قولنامه ای می باشد لطفا وضعیت مالکیت متراژ در مسیر کنترل گردد") 
	    End if 
	  	TmpPeacelst=Peacelst.where(Function(f) f.CI_PeaceType=2).tolist 
	  '	plogfile("TmpPeacelst.count 2222:" , TmpPeacelst.count)
	 	if TmpPeacelst.count >0 Then Info8.AddError(BIZ.SA.EumErrorAction.Warning, "توجه", "چنانچه تراکم تشویقی ناشی از عرصه در مسیر به صورت پیشروی طولی اعمال شده است، رعایت حقوق مجاورین الزامی است")
	 	
	 	TmpPeacelst=Peacelst.where(Function(f) f.CI_PeaceType=18).tolist
	 	
	 	if TmpPeacelst.count >0 Then Info8.AddError(BIZ.SA.EumErrorAction.Warning, "توجه", "چنانچه ضابطه مسجد یا حسینیه اعمال شده است رعایت حقوق مجاورین الزامی است")
 	End if	

 Catch ex As Exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "خطا : تایید مدیر صلح" & ex.Message)
 End Try	
End IF
Msgbox("A2","A2")
'If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","سسس")  




'---------------------------------363206
'---------------------------------------------------------------------------------------
If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 3 Then
	if ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="9D2031E0-8ACE-4CB3-A563-05FC1D42C8DF" or _
	   ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="82FBD9BC-EBA4-495C-99FF-22B035FB97FF" or _
       ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B0602DD4-FEF9-4AEB-A14A-251B3E86A6D4" or _
	   ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="01AAA0B7-8193-41D6-84F9-437FA39306A7" or _
	   ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="04DF58F1-731B-4371-8D53-47435F681362" or _
       ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="AB69C90F-FBAF-42FC-9414-7DAE032A1182" or _
       ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA" or _
	   ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="6F00FA9D-02A3-43F9-9365-A7F42E9039E9" or _
	   ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="677FBEE4-B05C-4FA6-AB19-CE897739195A" Then
	   		Dim Floor  as Double =0
	   		
 	   		
	   		
	   		
	   		Dim LstBaseUsing  As New List(Of DtoSC.Base_Using)
	   		' sp 377508 
	   		if  TypeObjMain=5 Then
	   			LstBaseUsing = Info8.GetRevisit_HouseSharing.GetBase_Using
	   		Else
		   		Try
		   			LstBaseUsing = Info8.GetRevisit_Building.GetBase_Using
		   		Catch
		   			LstBaseUsing = Info8.GetRevisit_HouseSharing.GetBase_Using
		   		End Try
		   	End if 	
	   		Floor=LstBaseUsing.Max(Function(f) f.FloorNo) - LstBaseUsing.Min(Function(f) f.FloorNo) + 1
	   		
        	 'sp 487119
        		Dim GetAllRequest1 as New List(of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
        		GetAllRequest1 = Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)        	 
        		GetAllRequest1 = GetAllRequest1.where(Function (f) ucase(f.nidworkflowdeff.tostring) = ucase("097C8C41-BA8C-4B60-8B2F-6BED0221F573") And _
        											 			 ucase(f.nidworkflowdeff.tostring) = ucase("2635fe18-fe4c-4a2d-9d05-993756751262") And  _
	        											 		 ucase(f.nidworkflowdeff.tostring) = ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") And  _
        											 			 ucase(f.nidworkflowdeff.tostring) = ucase("2f67d8af-89e3-4738-9ecb-b51d4501d640") And  _
        											 			 ucase(f.nidworkflowdeff.tostring) = ucase("22F8F7D9-71D9-4FBA-90F5-E43AD0EDC746") ).tolist 
    			
        		        		    		        		
        		
        	'	If GetAllRequest(0).RequestDate.tostring > "1400/03/06" Then
        		
        		if GetAllRequest1.Max(Function(d) d.RequestDate)>"1400/03/06" Then
        			if TaeedMKaifiat()= 1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "ابتدا باید تایید تضمین کیفیت داشته باشید" ) : Exit Function
        		End if	
        		
			'end 'sp 487119       	   		
	   		
if 1 = 2 Then	   		
	   		
	   		if Floor>5 And LstBaseUsing.sum(Function(f) f.UsingArea)> 799.9 Then 
	   		    '
	   		    '
	   		    'RE 4709016
	   		    Dim Base_ShahrsaziArchive As New List(Of Global.DtoSC.Base_ShahrsaziArchive)
	   			Base_ShahrsaziArchive=Info8.GetRevisit_House.GetBase_ShahrsaziArchive
	   		    if TypeObjMain=2 or TypeObjMain=3 Then  
			   		Base_ShahrsaziArchive = Info8.GetParvandeh_House.GetBase_ShahrsaziArchive()				   		
			   	ElseIf TypeObjMain=5 Then
			   		Base_ShahrsaziArchive = Info8.GetParvandeh_HouseSharing.GetBase_ShahrsaziArchive()
			   	End if
	   		    
	   		    
	   		    
	   			'Dim Base_ShahrsaziArchive As New List(Of Global.DtoSC.Base_ShahrsaziArchive)
	   			'Base_ShahrsaziArchive=Info8.GetRevisit_House.GetBase_ShahrsaziArchive
	   			
	   			'
	   			'
	   			'
	   			'sp 377440  'And f.CI_ShahrsaziArchiveKind<> 213
        		Base_ShahrsaziArchive = Base_ShahrsaziArchive.Where(Function(f) f.CI_ShahrsaziArchiveGroup = 1 And f.CI_ShahrsaziArchiveKind<> 213 And f.CI_ShahrsaziArchiveKind<> 6 And f.CI_ShahrsaziArchiveKind<> 140 And f.CI_ShahrsaziArchiveKind<> 203 And f.CI_ShahrsaziArchiveKind<> 202).ToList()
        	

        		if Base_ShahrsaziArchive.count>0 Then 
        '	msg("C: ",Base_ShahrsaziArchive.Max(Function(f) f.CertificateDate))	
        '	msgbox("CertificateDate:",Base_ShahrsaziArchive.Max(Function(f) f.CertificateDate))
        
        'sp 487119
        		Dim SH_NidProc as String
        		Dim SH_Row As New List(Of Global.DtoSC.Base_ShahrsaziArchive)
        		SH_Row = Base_ShahrsaziArchive.where(Function(f) f.CertificateDate = Base_ShahrsaziArchive.Max(Function(d) d.CertificateDate)).tolist()
        		SH_NidProc = SH_Row(0).nidproc.tostring
	      		        		
        		Dim GetAllRequest as New List(of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
        		GetAllRequest = Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
        		GetAllRequest = GetAllRequest.where(Function (f) f.NidProc = guid.parse(SH_NidProc)).tolist    
        		
    		
        		        		    		        		
        		
        		If GetAllRequest(0).RequestDate.tostring > "1400/03/06" Then
        			if TaeedMKaifiat()= 1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "ابتدا باید تایید تضمین کیفیت داشته باشید" ) : Exit Function
        		End if		
        		
        'sp 487119 			If Base_ShahrsaziArchive.Max(Function(f) f.CertificateDate)>"1400/03/06" Then
         'sp 487119				if TaeedMKaifiat()= 1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "ابتدا باید تایید تضمین کیفیت داشته باشید" ) :Exit Function
        				
        			'sp 363206			
	   	 'End sp 487119			End if
	   	 
	   			End if 	
	   		End if
	   		
	   			
End if '1=2	   		
	   		
	End if 
End if


If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 4 Then	
	Try	 
	
	
	'sp257801-------------------------------------------------------------------
	if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>1 Then
	   Call ComfirmFiche()
	End if
	'------------------------------------------------------------------- 	
	 	
	 	
	if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=1 And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"BB0854BE-4002-4C0C-975C-04B4B8485334" _
	And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>ucase("2635fe18-fe4c-4a2d-9d05-993756751262") Then 	
				Dim LstMangerCon As New List(Of Global.DtoSC.Sh_ManagerConfirm)
			    LstMangerCon = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
			    If LstMangerCon.Count > 0 Then LstMangerCon = LstMangerCon.Where(Function(f) f.CI_ResourceManagerConfirm = 5).ToList
			    If LstMangerCon.Count = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "لطفا ابتدا موافقت اصولی را تایید کنید ") : Exit Function
			    
			    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
	End if
		
 '190192 
 
                   if  Taghsid(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc,0)=1 Then
						Dim LstMangerCon As New List(Of Global.DtoSC.Sh_ManagerConfirm)
						LstMangerCon = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
						If LstMangerCon.Count > 0 Then LstMangerCon = LstMangerCon.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence=1).ToList
						If LstMangerCon.Count = 0  And NoContorol_TaeedTaghsid=False   Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", " به دلیل داشتن تقسیط ، پرونده باید تایید تقسیط داشته باشد ") : Exit Function
					
					End if
					'190192
'---------------------------
	  
		Dim Mant As Integer = 0
        Select Case Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District
            Case 1
                Mant = 19
            Case 2
                Mant = 20
            Case 3
                Mant = 21
            Case 4
                Mant = 22
            Case 5
                Mant = 23
            Case 6
                Mant = 24
            Case 7
                Mant = 25
            Case 8
                Mant = 26
            Case 9
                Mant = 27
            Case 10
                Mant = 28
            Case 11
                Mant = 29
            Case 12
                Mant = 30
            Case 80
                Mant = 31

        End Select
        
     
  
        
 if Sara10=False Then     
if User137(Info8.User.UserGuid.tostring,TmpArchiveGroup)=1 Then
	if  (TmpArchiveGroup=1877 or TmpArchiveGroup=1878 or TmpArchiveGroup=1879) Then '
'	(Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=9  or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=8 or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=3 or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=12 _
	  '  or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District=5) And (ArchiveGroup=1877 or ArchiveGroup=1878 or ArchiveGroup=1879)add 8-3 -9  161246-161249-161250
	   
	  
'187054		  Dim Fava137 As New Biz.Communication.ClsFava137
'187054		  Dim Fava137Message As New Biz.Communication.ClsFava137MessageContract
		  
'187054		  Fava137Message.key = "5tgb6yhn^YHN%TGB"
		  
'187054		  Fava137Message.Name = Info8.GetRequest.Info.RequesterName 
		  
'187054		  Fava137Message.LastName = " "
'187054		  Fava137Message.ReferenceNum = Info8.GetRequest.Info.NidWorkItem
		 
		  
'187054		  Fava137Message.TypeOfReq=ArchiveGroup
		 
'187054		  Dim TmpWorkflowTaskList137 As New List(Of Global.Biz.SC.SrvTaskService.Task)
		 
'187054	      TmpWorkflowTaskList137 = Info8.GetFunctions().GetRequestLog(Guid.Parse(Info8.GetRequest.Info.NidProc.ToString))
	     
	
'187054	      TmpWorkflowTaskList137 = TmpWorkflowTaskList137.Where(Function(f) f.TaskTitel = "بررسي درخواست" ).ToList()
	       
'187054	       Dim joblocationNull as Boolean=False
	       
'187054	       if Info8.GetFunctions.GetUser(TmpWorkflowTaskList137(0).AssingTo).User.JobLocations Is Nothing  Then 
'187054	      	 joblocationNull=True
'187054	       End if
	      
'187054	      if joblocationNull=True Then 
'187054	      	Fava137Message.OrgCellPhon="-"
'187054	      Else
'187054	     	 Fava137Message.OrgCellPhon=Info8.GetFunctions.GetUser(TmpWorkflowTaskList137(0).AssingTo).User.JobLocations.Name.tostring
'187054	      End if
	      
	
'187054	      If ArchiveGroup=1877 or ArchiveGroup=1878 or ArchiveGroup=1879 Then 
'187054	      		Fava137Message.LevelNum = 2
'187054	      		Fava137Message.LevelExplain =	  "شماره گام: 2" + VbCrLf _
'187054				  + "شماره دفتر :" + Fava137Message.OrgCellPhon + VbCrLf _
'187054				  + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
'187054				  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +"سابقه مجاز ملک شما تایید شد."  
			
'187054				  Fava137Message.Regdate = Date.Now.Date
'187054				  Fava137Message.TrackingDate = AddDateForHolidays(GetShamsiDate(Date.Now.Date), 1) 'Date.Now.Date.AddDays(1)
'187054				  Fava137Message.CellPhone = Info8.GetRequest.Info.Cellphone
	      
'187054	   End IF
'187054		   Fava137Message.CityCode=Info8.GetFunctions.GetUser(TmpWorkflowTaskList137(0).AssingTo).User.JobLocations.Name.tostring
		   
'187054		   Fava137Message.Subject="1134"
'187054		   Fava137Message.SubSubject=ArchiveGroup
		 
'187054		   Fava137Message.Zone=Mant
'187054		  Dim NosaziLst As List(Of Biz.SA.ClsNosaziCodeType)
'187054	 	  NosaziLst = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
'187054		  NosaziLst = NosaziLst.Where(Function(f) f.EumNosaziCodeObjType = 2).tolist
	
'187054		  if NosaziLst.any Then 
		  	
'187054	 		  Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
'187054	 		  Info8.ParameterList.Add( "Parvandeh_House_NidBase" , Info8.GetFunctions.GetNidBase(NosaziLst(0).NidNosaziCode, Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000")).ToString)
'187054	 	 Else
'187054	 	 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "137اعتبارسنجی", "137 مشکل در ارسال به 137 ")
'187054	 	 End if 	
	
'187054	Try
	
'187054		  Fava137Message.Adress=Info8.GetParvandeh_House.GetBase_AddressInfo.MainAddress.ToString 
'187054		 Catch ex As Exception
'187054					Info8.AddError(BIZ.SA.EumErrorAction.warning, "اعتبارسنجی"  , "ادرس ملک برای ارسال به 137 معتبر نمی باشد")
'187054	        End Try	  
		 
'187054		  if Info8.GetParvandeh_House.GetBase_PreCodeInfo.Count>0 Then 
'187054	 	  	Fava137Message.RenovationCode=Info8.GetParvandeh_House.GetBase_PreCodeInfo(0).PreCode	
'187054	 	  Else
'187054		  	Fava137Message.RenovationCode="0"	
'187054	 	  End if 	
	 	  
'187054	 	  Fava137Message.RenovationCodeNew=Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)	
	
	
'187054		  If Fava137.SaveRequestnew(Fava137Message) Then
		 
		  	Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
				 tmpSMS.BizCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
			
				 tmpSMS.Number = Info8.GetRequest.Info.Cellphone
				 
				 tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
				 ' + "شماره دفتر :" + Fava137Message.OrgCellPhon + VbCrLf 
				 
				 tmpSMS.SMSType = "شماره گام: 2" + VbCrLf _
				  + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
				  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +"سابقه مجاز ملک شما تایید شد."  
		
				 tmpSMS.UserID = Info8.User.UserGuid.ToString
			
				 tmpSMS.UserName = Info8.User.UserName
				
				
				  Dim smsErrorResult As New BIZ.SA.ClsErrorResult
				  smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
	'187054	  Else
		 '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "137اعتبارسنجی", "137 مشکل در ارسال به 137 ")
'187054		  End If
'187054		  
	   End if 
  End if
End if
	
	Catch ex As Exception
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "خطا : تایید سابقه مجاز" )
	End Try
End IF 

''''''''''''''''''''''''''''''''''''''''''''''''''
Msgbox("A1","A1")
	 If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 3 Then
	 	
	 	Dim Mant137 As Integer = 0
        Select Case Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).District
            Case 1
                Mant137 = 1
            Case 2
                Mant137 = 2
            Case 3
                Mant137 = 3
            Case 4
                Mant137 = 4
            Case 5
                Mant137 = 5
            Case 6
                Mant137 = 6
            Case 7
                Mant137 = 7
            Case 8
                Mant137 = 8
            Case 9
                Mant137 = 9
            Case 10
                Mant137 = 10
            Case 11
                Mant137 = 11
            Case 12
                Mant137 = 12
            Case 80
                Mant137 = 13

        End Select
        Dim sus_id as String=100000
        
       Select Case ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)
            Case "BB0854BE-4002-4C0C-975C-04B4B8485334" 'صدور پروانه ساختمانی
                sus_id = 102091
            Case "7D38106F-7362-4283-BDD9-0AC4E4BE2384","8AEAD964-2A11-4072-9F17-0BAB8E4511C5","20286CC7-9313-4363-8C70-0C7814B79CC9" _
             ,"CB2A1920-7B97-4D20-9BBE-12D50F40BB5A","18313B50-F76D-428A-BAE4-134A918B2600","3411EFED-3AEF-4619-8DED-1C4D845572C1" _
             ,"CBB8C800-70E6-432B-B338-1F229A365712","17FD1A79-AC10-4B37-8D4F-1FAD7550F2C4","457475B0-E461-475E-89FC-1FB85FD41FEC" _
             ,"26ADD6E3-9EEF-4757-87F2-2E9091AB730C","E1AC3D8B-4B4E-4837-A7EE-302CB70F5FA3","2497B40D-9861-445E-96AD-3588A1613682" _
			 ,"7321AE5D-C65E-48DD-A8D5-3BF4773923E7","CC1FBC44-7EDD-41E7-A64B-3D6DF21E855E","B96261EE-E426-45D1-9118-41D02210CC14" _
			 ,"50E0B7EF-E10C-4727-BF96-4399B6A37196","8ECF01D4-A93C-44A7-B52B-4685634C54C6","88E98ADA-C0F2-48B5-A724-4B455E445303" _
			 ,"A24A966E-0BCB-432F-8474-4ED76CF93A79","27CFF1AD-5936-407E-A420-5B8B71F94566","D8A215F8-2E28-45A0-A826-5ED886F16CCF" _
			 ,"69C923A3-4908-4828-92E4-5F66712974BA","1661EA82-19A6-4ED6-9EE1-63B16C5FDA8D","5BD4AD80-D3A5-4F87-81CB-652363740C1D" _
			 ,"0C2B0730-504F-44AD-A067-6AF8AA1A3132","E33F2C9A-C0A7-46F5-B4A1-6CBDA0FE419D","79903151-AF7C-4A73-9F18-6EB915F3293F" _
			 ,"3DBF0B9B-7A1A-4459-81F2-6FC6FC208FCF","D62680B7-5038-4066-B651-73B3F94C225F","9660D3B0-B41A-4B2A-BA3F-75F0E5B78085" _
			 ,"68C87B74-097E-4EC0-83D1-7723C403B1F8","86D5016B-AB5D-4577-B318-791A083AFC5F","55C9F836-E48B-4BAA-AC92-7B1C208F6831" _
			 ,"16C77278-30A1-4375-80DC-7EE8367B39FD","008DF040-922B-41A6-A3B2-7F35B1C870BB","D9E55CD5-922B-4D56-80C8-7FE5F7178C95" _
			 ,"3CA97718-4AFE-4752-A892-8B5EF88F176C","7B60BD7D-5116-4B6A-8B6C-8EE303345AD9","F32B14FF-E1E8-479B-8A9B-979BF74F7579" _
			 ,"679C82EB-4227-45D5-966D-A62C7FE580EB","AA2E5691-CD31-452C-A019-ADE4494EC258","307E2C2F-99DC-4AB3-A6A4-AF7A5D731581" _
			 ,"27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7","5A659F4C-8DC6-4283-A496-BD20C952A810","0B53D213-CB83-4E5C-9319-C551CCB187E9" _
			 ,"102D761E-C698-4C9B-A5A9-C6D1B57EBBCC","08C2BE75-FAF3-457E-9B33-C97681FA419D","A1C05500-1D9F-4CDF-B3C5-D26B115CC2D5" _
			 ,"AB164972-4C84-445E-A8FD-D576578E24B1","710510E5-D8B3-459D-9CAC-DBE13C7DB8DE","C6230C76-3DEB-41E3-8F68-DEEA920763D1" _
			 ,"F57C96A0-697F-410C-AA37-DFDE36A29C47","22F8F7D9-71D9-4FBA-90F5-E43AD0EDC746","E5F5C5A4-F51E-4ED1-80E5-E6A11853C746" _
			 ,"938EE557-4E3B-43EE-A9EE-ED12D17351DC","AA458943-252C-444F-802E-F038156A208C","214CE534-3326-4C0F-9CF5-F6EC3844B847" _
			 ,"30F756C2-2AAF-4543-B98D-FBAB4BC916FD"'صدور پروانه و تغییرات بنا/توسعه بنا/تغییربهره برداری/تمدید/تجدید	
                sus_id = 102092
            Case "F5B285CC-8792-498C-A6C0-0D1571EEB793","50E0B7EF-E10C-4727-BF96-4399B6A37196","27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7" _
				 ,"710510E5-D8B3-459D-9CAC-DBE13C7DB8DE","F57C96A0-697F-410C-AA37-DFDE36A29C47" 'صدور پروانه دیوارکشی
                sus_id = 102445
            Case "9D2031E0-8ACE-4CB3-A563-05FC1D42C8DF","2FBD9BC-EBA4-495C-99FF-22B035FB97FF","B0602DD4-FEF9-4AEB-A14A-251B3E86A6D4" _
				,"01AAA0B7-8193-41D6-84F9-437FA39306A7","04DF58F1-731B-4371-8D53-47435F681362","AB69C90F-FBAF-42FC-9414-7DAE032A1182" _
				,"11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA","677FBEE4-B05C-4FA6-AB19-CE897739195A" 'صدور گواهی  پایانکار بهره برداری
                sus_id = 102446
            Case "E6C9DB23-6CEA-4161-813E-078193082DD9","5F07A635-B569-47F7-A97F-2AA2C36AFE37","FBE3AD14-934C-4F8F-8BA2-5C07BFB4696D" _
				,"53E7F83A-59A2-4C0D-805A-7BE86E1DC56D","80134AAF-F070-4706-AC6A-E82778B7029C","DE10D8ED-3989-4E6C-AD37-F5D915BC73D6" 'صدور گواهی  عدم خلاف	
                sus_id = 102447
            Case "5AEC0F88-6381-41F3-A5B6-636C8941886E" 'صدور گواهی  مفاصاحساب
                sus_id = 102448
            Case "EF8464E4-38F5-4AAB-8DB5-094D6B5E1398","032DD2F4-DC70-44F4-88FD-119580EC9884","2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
				,"3157211F-C484-48E9-B65C-6BF3271A9F15","7318F0DB-13DD-40E4-BB89-6C4C7A23FF62","1426A8E6-68BC-4B2F-8E82-7C478D5CAB14" _
			    ,"CDAA2D6E-1184-4983-A516-A8A2FF92795C" 'صدور گواهی  انتقال ملک
                sus_id = 102449
            Case "E5F7F86A-0D02-4A01-A01C-19F13F6BEDB9","297949D0-9E25-419E-AD7C-62A7034910A1","EBCCF491-D89D-46E6-BEE7-6F4459C01329" _
				,"3EBB3B53-972C-425F-9939-80E76AAB6BFB","3636B5FF-0BF6-4CBC-BBD3-875DB4A1BB11","6C67F07C-84DA-41CB-A454-BAC84078EA9F" _
				,"D0127432-AA14-4A16-BA07-D6A6C7DF5C5D" 'صدور گواهی  پایان کار تفکیکی آپارتمان
                sus_id = 102450
            Case "C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB","B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" 'مجوز شروع عملیات ساختمانی
                sus_id = 102451
            Case "35CFFB97-67A3-488E-8A7A-BEC93C459B99","BDA49CCE-DFE8-4ECC-8EEC-C6D6A1EB6A76","BDD563C8-1EB2-40EC-8811-19DB03908488" 'گواهی انتقال آپارتمان  شهروند سپاری	
                sus_id = 102452
            Case "06E28FB5-E844-45E4-9FBA-631C1C324081","577CB127-0C16-4F4D-B645-A7B4D0EC713B","5482C28A-025D-40FB-A928-B62EAD670ACE" _
				,"676E0202-F41E-44E9-98C7-E586072F8C17" 'صدور گواهی افراز
                sus_id = 102454
            Case "BCE823DF-A854-406F-A881-4983C8447DDD","51209265-3386-4E15-9CF3-91449BD22AE8","9E7565F4-21C3-4497-887F-A751274EDEEF 8012EFB5-5C8C-4BF4-8F3D-E25168B17A24" _
				,"E34D362D-D8F9-4481-B1C2-E9F880A08582","7FF7C349-A773-4A45-8F52-F8593B467A96" 'صدور گواهی تفکیک
                sus_id = 102455
            Case "5437358A-F86B-48D8-A823-168388815F79" 'درخواست هیئت پارکینگ
                sus_id = 102456
            Case "F5F8B45F-A8D0-4384-9ED2-42DC747A1A20" 'درخواست ایجاد کدآپارتمان
                sus_id = 102457    
		'	Case '"b15c254e-4dcb-4801-9845-5f3f70acd6e3" 'درخواست کانورت پرونده
         '       sus_id = 102458 
         '   Case '"F5F8B45F-A8D0-4384-9ED2-42DC747A1A20" 'درخواست اعتراض(هيئت فني و کمیسیون ماده 100)
                sus_id = 102459  
            Case "53379B94-1411-4DC8-AEE1-012D4A9B43A7","C28A7410-B48A-4199-BA3A-B5254A20C1F5" 'صدور مجوز آماده سازی
                sus_id = 102460 
            Case "E10EDCBA-7945-4DDD-8306-9BAA37529AE2" 'درخواست هیئت فنی
                sus_id = 102461
            Case "6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9","B3407F6F-967D-47DE-A8EB-9A7AB98BDA75" 'درخواست اصلاح نقشه
                sus_id = 102462 
          '  Case '"6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9" 'درخواست صدور کد نوسازی	
          '      sus_id = 102463 
         '   Case '"6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9" 'درخواست صدور فیش(نوسازی، صنفی، اشغال معبر، فیش درآمدی و کلیه عوارضات)
         '       sus_id = 102464 
          '  Case '"6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9" 'درخواست تصوير سوابق شهرسازي	
          '      sus_id = 102465                                                    
        End Select 

	 	Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
		tmpSMS.BizCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
		tmpSMS.Number = Info8.GetRequest.Info.Cellphone
		tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
		tmpSMS.SMSType = "رضایت مندی شهروندان"
		tmpSMS.Text =   " شهروند گرامی خواهشمند است جهت ارائه بهبود خدمات شهرداری مشهد میزان رضایت خود را ازخدمت دریافت شده با کلیک بر روی لینک زیر  "  + VbCrLf _
		+ "https://eservice.mashhad.ir/fa/index.php?module=web_directory&wd_id=102040&sus_type=1&sus_id="+sus_id.tostring+"&sus_location=" + Mant137.tostring + VbCrLf _
		+" اعلام نمایید "  
		tmpSMS.UserID = Info8.User.UserGuid.ToString
		tmpSMS.UserName = Info8.User.UserName
		
		Dim smsErrorResult As New BIZ.SA.ClsErrorResult
		smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
	 	
	 End if
	Msgbox("A","A")
'''''''''''''''''''''''''''''''''''''''''''''''''''''	
 if ChekCi=1 And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 Then Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=23
 Dim StopTmp as Integer=0
 Dim z as Integer=0
 if Info8.ErrorResult.BizErrors.count>0 Then 
 	For z=0 to Info8.ErrorResult.BizErrors.count-1
 		If Info8.ErrorResult.BizErrors(z).ErrorAction = BIZ.SA.EumErrorAction.Stop Then StopTmp = 1
 	Next
 End if 
 Msgbox("A","B")
'------------------275950
if  (ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241")  And StopTmp=0 Then 
		Dim tmpSMSMelk As New BIZ.SC.SrvSMSService.ClsParameters
		tmpSMSMelk.BizCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
		tmpSMSMelk.Number = Info8.GetRequest.Info.Cellphone
		tmpSMSMelk.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
		tmpSMSMelk.SMSType = "تایید مدیر/انتقال ملک"
		tmpSMSMelk.Text =   "شهروند محترم درخواست شما به شماره  " + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf + "توسط منطقه تائید شد.برای پرداخت عوارض نوسازی و صدور گواهی به درخواست پیشین در منوی خدمات اختصاصی مراجعه نمائید. شهرداری مشهد " 
		tmpSMSMelk.UserID = Info8.User.UserGuid.ToString
		tmpSMSMelk.UserName = Info8.User.UserName
		Dim smsErrorResult As New BIZ.SA.ClsErrorResult
		smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMSMelk)
End if

If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 4 Then

  RptMojaz = True
  	Dim districtA as String 
	
	Dim tmpDistrictA() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	if tmpNosaziNickName<>"" Then 
		tmpDistrictA = tmpNosaziNickName.Split("-")
		districtA=tmpDistrictA(0)
	End if	

	Call Rpt_Check()
	if (Info8.GetRequest.Info.NidWorkflowDeff.ToString) = "0fc9224b-94da-4156-89c2-c9e7c7acdfda" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) = "25f1f6fa-354f-46b4-be0f-3497e1f82e20" Then
	
	  	if RptMojaz=True Then  InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptTaiidSabegheMojaz" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "تایید سابقه مجاز", "/Sara8Reports/RptTaiidSabegheMojaz", "1", districtA,1001)
		
    End if
End if
 

Msgbox("A","C")
If TmpListManger.EumManagerConfirmLicence = 1 And TmpListManger.CI_ResourceManagerConfirm = 3 Then

	 Rpt_Parvaneh=True
	 Rpt_Zabete=True
	 RptComments=True
	 Rpt_iIncome=True
	 Rpt_RevisitHouse=True
	 Rpt_RevisitBuilding=True
	 Rpt_RevisitApartment=True
	 Rpt_RevisitShop=True
	 Rpt_Khalaf=True
	 Rpt_Movafeghat=True
	
	 UP_Parvandeh =True
	 UP_Arc_Melk =True
	 UP_Arc_Dastghah =True
	 UP_Arc_Apar=True
	 UP_Arc_Senf=True
	
	ControlORMova=False
	
	Dim districtA as String 
	
	Dim tmpDistrictA() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	if tmpNosaziNickName<>"" Then 
		tmpDistrictA = tmpNosaziNickName.Split("-")
		districtA=tmpDistrictA(0)
	End if	

	Call Rpt_Check()
	if (Info8.GetRequest.Info.NidWorkflowDeff.ToString) = "0fc9224b-94da-4156-89c2-c9e7c7acdfda" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) = "25f1f6fa-354f-46b4-be0f-3497e1f82e20" Then
	
	  '	if Rpt_iIncome=True Then  InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptTaiidSabegheMojaz" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "تایید سابقه مجاز", "/Sara8Reports/RptTaiidSabegheMojaz", "1", districtA,1001)
		
    End if
	if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB" And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" _
	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A"   Then
		
		
		'InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "SabteDarkhast_Sara10" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "ثبت درخواست", "/Sara8Reports/SabteDarkhast_Sara10", "1", districtA,66)
		''InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptRevisitBuilding" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "بازدید ساختمان", "/Sara8Reports/RptRevisitBuilding", "1", districtA,66)
		
		'تقسیط
		'302512
		if Rpt_iIncome=True Then  InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptInstallment_IncomeForArchive" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "تقسیط", "/Sara8Reports/RptInstallment_IncomeForArchive", "1", districtA,66)
		
		'تحلیل تعیین خلاف
		if Rpt_Khalaf=True Then  InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptTaiinKhalaf_Khalaf" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "تحلیل تعیین خلاف", "/Sara8Reports/RptTaiinKhalaf_Khalaf", "1", districtA,66)
		'موافقت اصولی
		if Rpt_Movafeghat=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptMovafeghatOsooli" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "موافقت اصولی", "/Sara8Reports/RptMovafeghatOsooli", "1", districtA,66)
		
		'تحلیل پروانه
		if Rpt_Parvaneh=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptTaiinKhalaf_Parvaneh" & Info8.GetRequest.Info.NidWorkItem.tostring & "_", "تحلیل پروانه", "/Sara8Reports/RptTaiinKhalaf_Parvaneh", "1", districtA,66)
		'ضابطه

		if Rpt_Zabete=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptZabete_Active" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "ضابطه", "/Sara8Reports/RptZabete_Active", "1", districtA,66)
		'توضیحات درخواست
		if RptComments=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptWorkflowInstance_Comments" & Info8.GetRequest.Info.NidWorkItem.tostring & "_", "توضیحات درخواست", "/Sara8Reports/RptWorkflowInstance_Comments", "1", districtA,66)
		'فاکتور درآمد
		if Rpt_iIncome=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptShahrsaziIncomeFactor_Last" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "فاکتور درآمد", "/Sara8Reports/RptShahrsaziIncomeFactor_Last", "1", districtA,66)
		'بازدید ملک
	
		if Rpt_RevisitHouse=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptRevisitHouse" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "بازدید ملک", "/Sara8Reports/RptRevisitHouse", "1", districtA,66)
		'بازدید ساختمان
		
		if Rpt_RevisitBuilding=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptRevisitBuilding" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "بازدید ساختمان", "/Sara8Reports/RptRevisitBuilding", "1", districtA,66)
		'بازدید آپارتمان
		
		if Rpt_RevisitApartment=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptRevisitApartment" & Info8.GetRequest.Info.NidWorkItem.tostring & "_", "بازدید آپارتمان", "/Sara8Reports/RptRevisitApartment", "1", districtA,66)
		'بازدید صنفی
		if Rpt_RevisitShop=True Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptRevisitShop" & Info8.GetRequest.Info.NidWorkItem.tostring & "_", "بازدید صنفی", "/Sara8Reports/RptRevisitShop", "1", districtA,66)
		''فاکتور خدماتی
		''184506
	if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="A3B24BD2-4CB2-4011-9900-3070AD088A6F" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="83F0FAC4-2268-4005-8D52-34266DD9F595" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="87783CD5-DCF5-40A6-B4B5-5228B62C4D11" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="79903151-AF7C-4A73-9F18-6EB915F3293F" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="008DF040-922B-41A6-A3B2-7F35B1C870BB" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="8B4AF4AD-A7E7-4330-BD2C-F39B3A07ADFD" _ 
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="6B26CCDD-4FF5-4B9D-BF15-128312E7A08A" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F75B08E0-6F1A-4762-8D1F-4271F648DEA0" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="8DCD2E0C-8E5E-48A4-BAD6-8C2A1B4901B1" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="AE758A56-C5AC-4FC7-AEE9-C24C252F758C" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="1E386CF1-F6BC-4C10-99B5-03F50EEF8104" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="5DC809DF-16C5-459B-B6F8-C28A049A32AC" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="5437358A-F86B-48D8-A823-168388815F79" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F5CAE479-5301-47B9-A7FB-845B1FBCB2C2" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F5F8B45F-A8D0-4384-9ED2-42DC747A1A20" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="356BAAAB-2627-417A-9AE3-A815CAF505C1" _
	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" Then
	Else
	 '	if districtA <> "80" Then InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptFaktor" & Info8.GetRequest.Info.NidWorkItem.tostring , "فاکتور خدماتی", "/Sara8Reports/RptFaktor", "1", districtA,66)
	End if	
	End if 	
	'89118End if 	
End if
'
'sp 319776 

Info8.GetManagerConfirm.Sh_ManagerConfirm.Comments=tmpListManger.Comments
  '  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a", "Comments:" & Info8.GetManagerConfirm.Sh_ManagerConfirm.Comments & "ReportArchive:" &  ReportArchive)
'If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a3", time1.ElapsedMilliseconds()) 
'azar if ReportArchive =1 Then 
'azar	Dim Nidnosazimelak as guid
'azar	Dim ArcWrrap As New List(Of Global.DtoSC.Archive_Wrapper)
'azar	Nidnosazimelak=Info8.GetRequest.Info.NidNosaziCode
'azar	Dim NosaziCNwe = Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0,0,Guid.Parse("00000000-0000-0000-0000-000000000000"))
'azar	If NosaziCNwe.EumNosaziCodeObjType = 3 Then
'azar	   Dim Plist As New List(Of Global.Biz.SA.ClsNosaziCodeType)
'azar	   Plist = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NosaziCNwe.NidNosaziCode, 0)
'azar	   Plist = Plist.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
'azar	   Nidnosazimelak=Plist(0).NidNosaziCode
'azar	End If
'azar	 ArcWrrap = Info8.GetFunctions.GetArchiveBizCode(Nidnosazimelak)
'azar	Dim districtNew as String 
'azar	Dim tmpDistrictNew() as String
'azar	Dim tmpNosaziNickNameNew As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
'azar	if tmpNosaziNickNameNew<>"" Then 
'azar		tmpDistrictNew = tmpNosaziNickNameNew.Split("-")
'azar		districtNew=tmpDistrictNew(0)
'azar	End if	
'azar	 Dim TmpUrlFromBizCodeResult As New List(Of Global.Biz.SC.SrvArchive.SrvGetUrlFromBizCodeResult)
'azar	 Dim LstArchive As New List(Of Global.BIZ.SC.SrvArchive.ClsImageItem)
'azar'	 If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a4", time1.ElapsedMilliseconds()) 
'azar	 if ArcWrrap.count >0 Then  
'azar		ArcWrrap=ArcWrrap.where (Function (f) f.CI_ArchiveGroup =1 ).tolist ()
'azar		if ArcWrrap.count >0 Then
'azar			Dim tmpSerchArchive As New Global.Biz.SC.SrvArchive.ClsSearchDataFile
'azar			Dim tmpSerchArchiveOut As New  Global.Biz.SC.SrvArchive.ClsSearchDataFile
'azar			Dim TmpG As New List(Of Guid) 
'azar		'	TmpG.Add(Guid.Parse("540D119D-14F1-4F65-94D1-7F39FC2B3B48"))
'azar			tmpSerchArchive.BizCode = ArcWrrap.min(Function (f) f.BizCode) 'ArcWrrap(0).BizCode.tostring
'azar			
'azar			tmpSerchArchive.DomainName = districtNew
'azar			tmpSerchArchive.AllGuids = TmpG
'azar			tmpSerchArchive.UserID ="540D119D-14F1-4F65-94D1-7F39FC2B3B48"
'azar			tmpSerchArchive.UserName ="tofighnezhad"
'azar			'	 If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a5", time1.ElapsedMilliseconds()) 
'azar			tmpSerchArchiveOut = Info8.GetFunctions.GetArchiveDataFileByBizCode(tmpSerchArchive, districtNew)		
'azar			'	 If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a6", time1.ElapsedMilliseconds()) 
'azar			TmpUrlFromBizCodeResult = tmpSerchArchiveOut.Result.GetFile
'azar				' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a7", time1.ElapsedMilliseconds()) 
'azar		    TmpUrlFromBizCodeResult = TmpUrlFromBizCodeResult.Where(Function(f) (f.NidEntity = 66 )And f.isdeleted=0 And f.CreateDate = BIZ.SA.ClsCommon.CurrentShamsiDateString).ToList
'azar			if Not TmpUrlFromBizCodeResult.any() Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "دائم", "با توجه به اینکه آرشیو نمودن گزارش، با خطا مواجه شده است، لطفا چند دقیقه دیگر مجدد تلاش نمایید "): Exit Function
'azar		 End if 
'azar	 End if 

'azarEnd if 

'sp WebService Made14 or Elzam


if ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString)=ucase("B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A") Then
	if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
	
    
	'	Call ElzamWebService()
					
	End if
End if
'end sp 


'sp 508549
 if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=4  And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then
   	Dim districtB as String 
	
	Dim tmpDistrictB() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	if tmpNosaziNickName<>"" Then 
		tmpDistrictB = tmpNosaziNickName.Split("-")
		districtB=tmpDistrictB(0)
	End if
 	if {"097C8C41-BA8C-4B60-8B2F-6BED0221F573","8d20be44-78ab-4001-bb94-f064d22b9ce1","2635fe18-fe4c-4a2d-9d05-993756751262","d1b85def-3f07-4b29-b99b-bf47d0a283f8"}.Contains(ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString)) Then  	
	   		InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptTaiidSabegheMojaz" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "فرم اطلاعات مجاز درخواست صادر نشده", "/Sara8Reports/RptTaiidSabegheMojaz", "1", districtB,1001)	   
 	End if 
 End if  ' eEnd sp 508549
 
 'sp 508549  -  471106
if( Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=4 And Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirmdetails=82) _
	And Info8.GetManagerConfirm.Sh_ManagerConfirm.EumManagerConfirmLicence=1 Then 
   	Dim districtB as String 
	
	Dim tmpDistrictB() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	if tmpNosaziNickName<>"" Then 
		tmpDistrictB = tmpNosaziNickName.Split("-")
		districtB=tmpDistrictB(0)
	End if
 	if WorkFlowArchive=1 or WorkFlowArchive=2 Then 
	   		InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptTaiidSabegheMojaz" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "فرم اطلاعات مجاز درخواست صادر نشده", "/Sara8Reports/RptTaiidSabegheMojaz", "1", districtB,1001)	   
 	End if 
 End if  'end  'sp 508549  -  471106

'---------------------------------------------------------------------------------------
Catch ex As Exception
	'	if ucase(Info8.GetRequest.info.NidWorkFlowDeff)<>ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then
			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "TaeedModirD"  & ex.Message)
	'	End if
End Try 
End Function

Private Function ComfirmFiche() As Integer
    Try
    
   
        Dim MaxD As String = "1301/01/01"
        Dim PaymentDate As String = "1301/01/01"
        Dim BankPaymentDate As String = "1301/01/01"
        Dim Mohlat_Pardakht as String=""
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        Dim AllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFicheExport As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFicheLast As List(Of DtoSC.Income_Fiche)
        Info8.Income_Fiche=Nothing
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        ' sp 343748
        'sp 434430
        if  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="083062F2-6C96-4F21-8955-0FAE1F367704" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="5EC6DB49-B836-474B-950D-876816C82262" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="BDD563C8-1EB2-40EC-8811-19DB03908488" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="E5F7F86A-0D02-4A01-A01C-19F13F6BEDB9" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="F5F8B45F-A8D0-4384-9ED2-42DC747A1A20" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="297949D0-9E25-419E-AD7C-62A7034910A1" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="EBCCF491-D89D-46E6-BEE7-6F4459C01329" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="3EBB3B53-972C-425F-9939-80E76AAB6BFB" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="3636B5FF-0BF6-4CBC-BBD3-875DB4A1BB11" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="4351C20C-643A-4A50-A4BD-AEBCEC5EADF8" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="6C67F07C-84DA-41CB-A454-BAC84078EA9F" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="BDA49CCE-DFE8-4ECC-8EEC-C6D6A1EB6A76" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="D0127432-AA14-4A16-BA07-D6A6C7DF5C5D" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="1E386CF1-F6BC-4C10-99B5-03F50EEF8104" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="6B26CCDD-4FF5-4B9D-BF15-128312E7A08A" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="A3B24BD2-4CB2-4011-9900-3070AD088A6F" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="83F0FAC4-2268-4005-8D52-34266DD9F595" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="F75B08E0-6F1A-4762-8D1F-4271F648DEA0" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="87783CD5-DCF5-40A6-B4B5-5228B62C4D11" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="79903151-AF7C-4A73-9F18-6EB915F3293F" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="008DF040-922B-41A6-A3B2-7F35B1C870BB" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="8DCD2E0C-8E5E-48A4-BAD6-8C2A1B4901B1" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="AE758A56-C5AC-4FC7-AEE9-C24C252F758C" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="5DC809DF-16C5-459B-B6F8-C28A049A32AC" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="8B4AF4AD-A7E7-4330-BD2C-F39B3A07ADFD" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="EF8464E4-38F5-4AAB-8DB5-094D6B5E1398" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="083062F2-6C96-4F21-8955-0FAE1F367704" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="032DD2F4-DC70-44F4-88FD-119580EC9884" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="BDD563C8-1EB2-40EC-8811-19DB03908488" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="70C0E402-67CC-4174-991C-41E62DBB6409" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="2BAF4A84-73B2-4270-8B9F-507238635542" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="3157211F-C484-48E9-B65C-6BF3271A9F15" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="7318F0DB-13DD-40E4-BB89-6C4C7A23FF62" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="EE64A610-9142-4E2B-953F-6E65B522A9E1" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="1426A8E6-68BC-4B2F-8E82-7C478D5CAB14" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="F9DE191A-959F-4C2E-8092-931FEB3589EB" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="CDAA2D6E-1184-4983-A516-A8A2FF92795C" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="31747A10-4CF4-477D-8950-BB8A4D114C55" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="BDA49CCE-DFE8-4ECC-8EEC-C6D6A1EB6A76" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="97394C4C-F5DC-4BF0-828C-DA9236A2B8D8" or _ 
			ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" or _
			Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="0FC9224B-94DA-4156-89C2-C9E7C7ACDFDA" or _
        	Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="25F1F6FA-354F-46B4-BE0F-3497E1F82E20" or _
        	Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=Ucase("a06e7139-acc1-46a1-97f1-d4c92563e55e") or _  
        	Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=Ucase("854895e0-185d-4ee2-ae4e-aab1be49fa50") Then  'sp 482034
		Else	
			  if TmpAllFiche.count=0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", " پرونده هیچ فیش ندارد")   :  Exit Function 
		End if	   
        TmpAllFicheExport=TmpAllFiche.where(Function (f) f.EumFicheStatus=0 or f.EumFicheStatus=1).tolist
        if Info8.GetRequest.Info.NidWorkItem<>10892332 Then   'sp 510992             
			if TmpAllFicheExport.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "برخی از فیش ها تعیین تکلیف نشده و در حال صدور می باشد  .")   :  Exit Function 
		End if    'end sp 510992        
		Dim LstIncome As List(Of Global.DtoSC.Income)
        LstIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        Dim Tmp As String
        tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
       '250860
		if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" Then Exit Function
        '203982Dim kk = Info8.GetFunctions.GetNidLib(Guid.Parse(tmp))
       '203982 msgbox("tmp*:",kk.RHouse.NidBase.ToString )

        '203982Info8.ParameterList.Remove("Revisit_House_NidBase")
        '203982Info8.ParameterList.Add("Revisit_House_NidBase", kk.RHouse.NidBase.ToString)

        If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District <> 80 Then

            Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116,143,163"
        Else
            Str_Income = "1,7,10,11,143,162,163"
        End If

        Dim TmpArr() As String
        Dim intList_Income As New list(Of Integer)

        TmpArr = Str_Income.Split(",")
        For i = 0 To TmpArr.Count - 1
            intList_Income.Add(TmpArr(i))
        Next i
        '	Dim AllFiche,Tmp_AllFiche as New list(of DtoSc.Income_Fiche) 
 
        '	AllFiche = M_AllFiche'Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse(tmStrP))
         
        
         
        'If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) And (f.ExportPermanentDate > "1393/09/01" or f.ExportTemporaryDate > "1393/09/01") ).ToList()
        
        '**942854
        'تاریخی که باید بعد از این تاریخ تائید بانک بزنن  1393/09/01
        'تائید دستی بعد از این تاریخ قابل قبول نمیباشد  1393/09/01
        Dim M_CreateDate as String
        If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.EumFicheStatus = 3 And (f.ExportPermanentDate > "1393/09/01" or f.ExportTemporaryDate > "1393/09/01")) or f.EumFicheStatus = 5  ).ToList()
        TmpAllFicheLast=TmpAllFiche
        'msgbox("TmpAllFiche*:",TmpAllFiche.count )
        '247865
        M_CreateDate = LstIncome.Max(Function(f) f.CreateDate)
        LstIncome = LstIncome.Where(Function(f) f.CreateDate = M_CreateDate).ToList
        TmpAllFiche=TmpAllFiche.where(Function (f) f.NidIncome=LstIncome(0).nidincome).tolist 
        '247865
        
        if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=Ucase("854895e0-185d-4ee2-ae4e-aab1be49fa50") Then  
        	 'sp 482034  'کلا تایید سابفه مجاز بدون گواهی فیش ندارن
        Else
        If TmpAllFiche.Any Then
            TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
   
            TmpAllFiche = TmpAllFiche.Distinct().ToList()
  
            If TmpAllFiche.any Then
                AllFiche = TmpAllFiche.Distinct().ToList()
                If AllFiche.Any Then
                   ' MaxD = AllFiche.max(Function(f) f.ExportPermanentDate)

                  
                     MaxD = AllFiche.max(Function(f) f.ExportPermanentDate)
                    
                    If MaxD = "" Then MaxD="1300/01/01"
                    If MaxD < AllFiche.max(Function(f) f.ExportTemporaryDate) Then MaxD =AllFiche.max(Function(f) f.ExportTemporaryDate)
                    
					If MaxD = "" or MaxD="1300/01/01"  Then MaxD = BIZ.SA.ClsCommon.CurrentShamsiDateString().tostring
                   
                    AllFiche = TmpAllFiche.Where(Function(f) f.ExportPermanentDate = MaxD).ToList()
					if Not AllFiche.Any Then  AllFiche = TmpAllFiche.Where(Function(f) f.ExportTemporaryDate = MaxD).ToList()
					
                    If AllFiche.Any Then
						Dim payableMaxdate as Long 
                        PaymentDate = AllFiche.max(Function(f) f.PaymentDate)
                        payableMaxdate = AllFiche.max(Function(f) f.payable)
                        
                        BankPaymentDate = AllFiche.max(Function(f) f.BankPaymentDate)
                        Mohlat_Pardakht=AllFiche.max(Function(f) f.PaymentBreakDate) 'melak tarikhMohlate darj shode dar fiche mibashad 3584648
                  '      msgbox("Mohlat_Pardakht:",Mohlat_Pardakht)
                  '      msgbox("BankPaymentDate:",BankPaymentDate)
                  '      msgbox("PaymentDate:",PaymentDate)
                        If String.IsNullOrEmpty(PaymentDate)  = True Then PaymentDate = "1301/01/01"
                        If String.IsNullOrEmpty(BankPaymentDate)  = True Then BankPaymentDate = "1301/01/01"
                         'msgbox("aaaaaaaa:","tttt"  & "dt:" & GetDiffDate(MaxD, PaymentDate, 3) & "MaxD:" & MaxD  & "BankPaymentDate:" & BankPaymentDate & "PaymentDate:" & PaymentDate )
                       
                        PaymentDate = BankPaymentDate
                         'msgbox("w","PaymentDate:" & PaymentDate & "GetDiffDate:" & GetDiffDate(MaxD, PaymentDate, 3)  & "AddDateForHolidays:" & AddDateForHolidays(MaxD, 10)  )
                      '  ucase(Info8.User.UserGuid.tostring)=ucase("ca742134-44bb-4ba6-b0c4-e5e186600a0a")
                      'fj1403 sp481575
                      'SP 502850 برای گروه پروانه غیر فعال شد
                         if   Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm=3 And GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>1  Then 
                           'sp481575 فیش هایی که توسط گروه ناظران ارشد محاسبه شده را نمی توان تایید کرد
                            Dim AllCalculation As New List(Of DtoSC.Income_Calculation)
                            AllCalculation.Clear()
                            Dim UserList As List(Of Biz.SA.SrvSecurityWCF.UserGroup)
                            AllCalculation = Info8.GetIncome().GetIncome_Calculation_InNidIncome(AllFiche.max(Function(f) f.NidIncome))
                            Dim b =  AllCalculation(0).NidUser
                            UserList = Info8.GetFunctions.GetUserGroups(b)
                              For i = 0 To UserList.count - 1
                                 If Ucase(UserList(i).GUID.tostring) =Ucase("85873e46-6558-41fa-ab16-529eada31fcf")  Then 
                                   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فیش تایید شده شهرداری در آخرین ردیف درامدی مربوط به اجرای ثبت می باشد - sp481575")    
                                   Exit Function 
                                 End if
                              Next
                             
                         End if
                         
                         'aaaa
                       'ghovati1401/01/08 ؟؟؟  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "MaxD", MaxD)  :  Exit Function 
		                         if  MaxD>="1398/12/16" And MaxD<="1399/01/31" Then '3623285-sp  268951
		                               msgbox("Holidays:",Holidays(MaxD, 10))
		                               
		                               if String.IsNullOrEmpty(Mohlat_Pardakht)=True Then Mohlat_Pardakht=Holidays(MaxD, 10) ' 3584648
		                         
		                               If PaymentDate > Mohlat_Pardakht And payableMaxdate>0 And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
		                               And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"C83F6228-987B-4B5E-8906-56F7CD87EA7C" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"045BEA33-0A29-4F35-8CC1-672417F49DC1") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی++++", " 1  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  :  Exit Function 
                                'ElseIf BIZ.SA.ClsCommon.CurrentShamsiDateString>="1399/10/22" And BIZ.SA.ClsCommon.CurrentShamsiDateString<"1399/12/11" Then
                                 ElseIf MaxD>="1399/10/22" And MaxD<"1399/12/11" Then
		                              
		                               if String.IsNullOrEmpty(Mohlat_Pardakht)=True Then Mohlat_Pardakht=Holidays(MaxD, 10) ' 3584648
		                         
		                               If PaymentDate > Mohlat_Pardakht And payableMaxdate>0 And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
		                               And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"C83F6228-987B-4B5E-8906-56F7CD87EA7C" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"045BEA33-0A29-4F35-8CC1-672417F49DC1") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی++++", " 2  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  :  Exit Function '1048779 فیش پرداخت شده در مهلت ندارید لطفا ردیف جدید درامد بزنید و فیش جدید صادر نمایید
		                            
		                            
		                            
		                            
		                            
		                         Else '3623285
		                         
		                               'GetDiffDate(MaxD, PaymentDate, 3) > 10
		                               'sp518690
		                               if String.IsNullOrEmpty(Mohlat_Pardakht)=True Then Mohlat_Pardakht=Holidays(MaxD, 10)
		                               If  PaymentDate > Mohlat_Pardakht  And payableMaxdate>0 And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
		                               And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"C83F6228-987B-4B5E-8906-56F7CD87EA7C" _
		                               And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"045BEA33-0A29-4F35-8CC1-672417F49DC1") And  BankPaymentDate > Mohlat_Pardakht  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی++", " 3  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد . " & VbCrLf & " مهلت پرداخت: " & Mohlat_Pardakht & " | تاریخ پرداخت بانک: " & BankPaymentDate & " | تاریخ پرداخت: " & PaymentDate )  :  Exit Function '1048779 فیش پرداخت شده در مهلت ندارید لطفا ردیف جدید درامد بزنید و فیش جدید صادر نمایید---- And  BankPaymentDate > Mohlat_Pardakht sp 408382
                        
                                End if
                        
                        
                        
                    Else
                    	if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" _
                    	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
                    	And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1"  ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*&", " 4  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")   :  Exit Function 
                    End If
				Else
					if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" _
					And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
					And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" ) And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی#", "آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .") :  Exit Function
                End If
            Else
            	if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" _
            	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
            	And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"E10EDCBA-7945-4DDD-8306-9BAA37529AE2" _
            	And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" )  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی@@@+", " 5  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .") :  Exit Function   'E10EDCBA-7945-4DDD-8306-9BAA37529AE2  200032
            End If
        Else
       
        'msgbox("","TaeedSabegheMojaz:" & TaeedSabegheMojaz & "NotaeedMojaz:" & NotaeedMojaz )
        'sp 383361  And NotaeedMojaz=True
        if (TaeedSabegheMojaz=False ) And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" _
        And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704"  And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1") _
        And  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" Then	
        	'246904
          	if ((Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District=8 or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District=12 or _
        		Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District=1 or Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District=11 or _
        		Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District=10) _
        	And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="E10EDCBA-7945-4DDD-8306-9BAA37529AE2") or (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="1472660E-7313-4FC8-8B1D-455A81478F20") or  (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="C83F6228-987B-4B5E-8906-56F7CD87EA7C") or  (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="3157211F-C484-48E9-B65C-6BF3271A9F15") or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="5EC6DB49-B836-474B-950D-876816C82262" or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="D0127432-AA14-4A16-BA07-D6A6C7DF5C5D"  Then '356516'RE 447541
        	Else
        		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی%%", " 6  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  :  Exit Function  'new TaeedSabegheMojaz 2249811
        	End if	
        End if	
        End If
        End if  
        
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "خطا فیش های تایید شده شهرداری") 

    End Try


End Function

Private Function ComfirmFicheAmozesh() As Integer
    Try
    
        Dim MaxD As String = "1301/01/01"
        Dim PaymentDate As String = "1301/01/01"
        Dim BankPaymentDate As String = "1301/01/01"
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        Dim AllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim TaeenTaklifKol As New List(Of DtoSC.Income_Fiche)
        Info8.Income_Fiche=Nothing
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)

        Dim Tmp As String
        tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring

       '203982 Dim kk = Info8.GetFunctions.GetNidLib(Guid.Parse(tmp))
       '203982 Info8.ParameterList.Remove("Revisit_House_NidBase")
       '203982 Info8.ParameterList.Add("Revisit_House_NidBase", kk.RHouse.NidBase.ToString)
	
        If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District <> 80 Then

            Str_Income = "3,80,17,59,31,66,38,73,10,24,45,52"
        Else
            Str_Income = "3"
        End If

        Dim TmpArr() As String
        Dim intList_Income As New list(Of Integer)
		Dim Taeed as Integer =1
        TmpArr = Str_Income.Split(",")
        For i = 0 To TmpArr.Count - 1
            intList_Income.Add(TmpArr(i))
        Next i
        '	Dim AllFiche,Tmp_AllFiche as New list(of DtoSc.Income_Fiche) 
 
        '	AllFiche = M_AllFiche'Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse(tmStrP))
        
         
       
       
        
        If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.ExportPermanentDate > "1397/05/01" or f.ExportTemporaryDate > "1397/05/01") ).ToList()
        
        if TmpAllFiche.Any Then TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList

		'
        If TmpAllFiche.Any Then TaeenTaklifKol = TmpAllFiche.Where(Function(f) (f.EumFicheStatus =0 Or f.EumFicheStatus =1 or f.EumFicheStatus =2)).ToList()'movaghat,Ghatee,Chap   217502
        
        If TaeenTaklifKol.Any  Then
        Msgbox("Modir_Daramad",Modir_Daramad)
	        if Info8.GetRequest.Info.NidWorkItem<>10892332 Then   'sp 510992     
		          if Modir_Daramad=True  Then
		             'RE 10090159
		             Info8.AddError(BIZ.SA.EumErrorAction.warning, "اعتبارسنجی*", "کلیه فیش های آموزش و پرورش بایستی تعیین تکلیف شده باشد ")   :  Exit Function 
		          Else
		             Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*", "کلیه فیش های آموزش و پرورش بایستی تعیین تکلیف شده باشد ")   :  Exit Function 
		          End if
	        End if
       	End if    'end sp 510992    
        
			
		If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) ).ToList()
		
        If TmpAllFiche.Any Then
               
            TmpAllFiche = TmpAllFiche.Distinct().ToList()
  
	            If TmpAllFiche.any Then
	           
	                   AllFiche = TmpAllFiche.Distinct().ToList()
	                  
		                If AllFiche.Any Then
		                   			' MaxD = AllFiche.max(Function(f) f.ExportPermanentDate)
		
		                   
		                     		MaxD = AllFiche.max(Function(f) f.ExportPermanentDate)
		                     		 
				                    If MaxD = "" Then MaxD="1300/01/01"
				                    If MaxD < AllFiche.max(Function(f) f.ExportTemporaryDate) Then MaxD =AllFiche.max(Function(f) f.ExportTemporaryDate)
		                    
									If MaxD = "" or MaxD="1300/01/01"  Then MaxD = BIZ.SA.ClsCommon.CurrentShamsiDateString().tostring
		                    
		                    		AllFiche = TmpAllFiche.Where(Function(f) f.ExportPermanentDate = MaxD).ToList()
									if Not AllFiche.Any Then  AllFiche = TmpAllFiche.Where(Function(f) f.ExportTemporaryDate = MaxD).ToList()
									
				                    If AllFiche.Any Then
										Dim payableMaxdate as Long 
				                        PaymentDate = AllFiche.max(Function(f) f.PaymentDate)
				                        payableMaxdate = AllFiche.max(Function(f) f.payable)
				                        
				                        BankPaymentDate = AllFiche.max(Function(f) f.BankPaymentDate)
				                        If PaymentDate = "" Then PaymentDate = "1301/01/01"
				                        If BankPaymentDate = "" Then BankPaymentDate = "1301/01/01"
				
				                       '''1965672 If BankPaymentDate > PaymentDate Then 
				                        PaymentDate = BankPaymentDate
				                       'sp 211444 If GetDiffDate(MaxD, PaymentDate, 3) > 10 And payableMaxdate>0 And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "آخرين رديف درآمد بايستي داراي فيش اموزش و پرورش با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  :  Exit Function '1048779 فیش پرداخت شده در مهلت ندارید لطفا ردیف جدید درامد بزنید و فیش جدید صادر نمایید
				                       
				                    Else
				                    	if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" _
				                    	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
				                    	And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*&", "آخرين رديف درآمد بايستي داراي فيش اموزش و پرورش با مبلغ صفر و يا فيش پرداخت شده باشد .")   :  Exit Function 
				                    	
				                    End If
						Else
						
							if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" _
							And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
							And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی#", "آخرين رديف درآمد بايستي داراي فيش اموزش و پرورش با مبلغ صفر و يا فيش پرداخت شده  باشد .") :  Exit Function
		                End If
	            Else
	            
	            	if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" _
	            	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
	            	And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی222", "آخرين رديف درآمد بايستي داراي فيش اموزش و پرورش با مبلغ صفر و يا فيش پرداخت شده  باشد .") :  Exit Function   
	            End If
            
        Else
        
        if Taeed=False And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" ) Then	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی%%", "آخرين رديف درآمد بايستي داراي فيش اموزش و پرورش با مبلغ صفر و يا فيش پرداخت شده  باشد .")  :  Exit Function  'new TaeedSabegheMojaz 2249811
        End If
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "خطا فیش های تایید شده اموزش" & ex.message ) 

    End Try


End Function

Private Function Taghsid(ByVal nidpro as guid,ByVal Check as Integer) as Integer
    Dim kk As New Global.Biz.SC.ClsNidLib
   ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, " Message", "D" )
    Try
    
 
   
    	Dim RtnResult as Integer=0 
        Dim MaxD As String = "1301/01/01"
        Dim PaymentDate As String = "1301/01/01"
        Dim BankPaymentDate As String = "1301/01/01"
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        Dim AllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        
        'TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        Info8.Income_Fiche=Nothing
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(nidpro)
msgbox("y:",1)
 if nidpro=Info8.getRequest.Info.NidProc And Check = 1 Then Return RtnResult
  
        Dim Tmptt As String
        Try
        	tmptt = Info8.GetFunctions.GetActiveRevisitBaseInfo(nidpro).NidRevisit.tostring
        Catch ex As Exception
        	Return RtnResult
    	End Try
        
	      '  kk = Info8.GetFunctions.GetNidLib(Guid.Parse(tmptt))
	   ' if kk.RHouse is  Nothing  Then 
	    	'	Return RtnResult    
        ' Else
       
	      '  Info8.ParameterList.Remove("Revisit_House_NidBase")
	        
	     '   Info8.ParameterList.Add("Revisit_House_NidBase", kk.RHouse.NidBase.ToString)
	      Dim strCodeNew as String
	      Dim tmpDistrict as Integer=0
	      
			strCodeNew = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
		    tmpDistrict =  strCodeNew.Split("-")(0)
	        If tmpDistrict <> 80 Then
	
	            Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116,163"
	        Else
	            Str_Income = "1,7,10,11,162,163"
	        End If
	
	        Dim TmpArr() As String
	        Dim intList_Income As New list(Of Integer)
	
	        TmpArr = Str_Income.Split(",")
	        For i = 0 To TmpArr.Count - 1
	            intList_Income.Add(TmpArr(i))
	        Next i
	      
	        If TmpAllFiche.Any Then
	            TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
	
	   
	            TmpAllFiche = TmpAllFiche.Distinct().ToList()
	  
	            If TmpAllFiche.any Then
	                AllFiche = TmpAllFiche.Distinct().ToList()
	                
	                AllFiche = AllFiche.Where(Function(f) f.EumFicheStatus = 3 Or f.EumFicheStatus = 5).ToList()'*1129298 
	                 
	                If AllFiche.Any Then
	                	Dim NidIncome= Info8.FindParameter("NidIncome")
						tarikhFich =AllFiche.Max(Function(f) f.BankPaymentDate)
						if tarikhFich="" Then tarikhFich=AllFiche.Max(Function(f) f.ExportPermanentDate) '239893
							msgbox("tarikhFich pyyy:",tarikhFich)
						For i=0 to AllFiche.count-1 
	                	
	                	
						'*204176	if AllFiche(i).NidIncome.tostring=NidIncome.tostring Then 
	                	'*204176   	HasFicheTaeed=True
	                	'*204176   	Else
	                	   	'faghed fiche
	                	'*204176   	HasFicheTaeed=False
	                	'*204176   	End if
	                	
	                	'Info8.GetRequest.Info.ActiveNidIncome
	                	
	                    	Info8.Installment = Nothing'*2544139
	                		Info8.GetInstallment.LoadObj (AllFiche(i).NidFiche,1)
	                		Dim LstInstal As New List(Of Global.DtoSC.Installment_List)
				            LstInstal = Info8.GetInstallment.GetInstallment_List()
				            ''*2544139 LstInstal = LstInstal.Where(Function(f) f.EumCheckStatus = 0 or  f.EumCheckStatus = 2 or f.EumCheckStatus = 9 ).tolist
				           '  msgbox("LstInstal.count*:",LstInstal.count ) 'mpAllFiche(0).InsatllmentValue
				           
				          '  msgbox("RtnResult:","AllFiche(i).NidIncome:" & AllFiche(i).NidIncome.tostring  & "Payable:" & AllFiche(i).Payable & "ActiveNidIncome:" & Info8.GetRequest.Info.ActiveNidIncome.tostring )
				          '  if LstInstal.count>0 And AllFiche(i).NidIncome.tostring=NidIncome.tostring And AllFiche(i).Payable=0 Then
				          '     RtnResult =3
				          '  End if
				          
				          
				            
				           
				           
				            if LstInstal.count>0 Then 
				           '187912 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "به علت داشتن چک نامشخص مجاز به تایید نمی باشید")
				            	RtnResult  =1
				            End if	 
				            if RtnResult  =1 And Check=1 Then 
				            
				            	 LstInstal=LstInstal.where(Function(f) f.EndStateCode<>11 And  f.EndStateCode<>17 And f.CI_InstallmentStatus<18).tolist
				            	 
				            	 If LstInstal.Max(Function(f) f.PaymentDate) > BIZ.SA.ClsCommon.CurrentShamsiDateString Then 
				            	 	
				            	 	if AllFiche(i).ExportPermanentDate>"1397/10/00" Then RtnResult = 2
				            	 End if 
				            End if   
				           
	                	Next	
	                	
	                End If
	            End if
	        End if
       
      '  End if 
      
       Return RtnResult       
    
Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی Taghsid",  ex.Message )

    End Try

End Function

Private Function TaghsidTaeed()
   Try
       
    	Dim RtnResult as Integer=0 
        Dim MaxD As String = "1301/01/01"
        Dim PaymentDate As String = "1301/01/01"
        Dim BankPaymentDate As String = "1301/01/01"
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        Dim Hastaghsid as Boolean=False
        Dim AllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFiche,TaenTaklifKol,FicheHavale As List(Of DtoSC.Income_Fiche)
        Info8.Income_Fiche=Nothing
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
 		if TmpAllFiche.count =0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "در صورت نداشتن هیچ فیشی مجاز به تایید تقسیط نمی باشید")   :  Exit Function 
        Dim Tmp As String
        tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring

      '203982  Dim kk = Info8.GetFunctions.GetNidLib(Guid.Parse(tmp))
      '203982  Info8.ParameterList.Remove("Revisit_House_NidBase")
      '203982  Info8.ParameterList.Add("Revisit_House_NidBase", kk.RHouse.NidBase.ToString)

        If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District <> 80 Then
            Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116,152"
        Else
            Str_Income = "1,7,10,11,162"
        End If

        Dim TmpArr() As String
        Dim intList_Income As New list(Of Integer)

        TmpArr = Str_Income.Split(",")
        For i = 0 To TmpArr.Count - 1
           intList_Income.Add(TmpArr(i))
        Next i        
        
        If TmpAllFiche.Any Then
            TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList

            TmpAllFiche = TmpAllFiche.Distinct().ToList()
            
            '-------
            FicheHavale=TmpAllFiche.where(Function(f) (f.EumFicheStatus=2 or f.EumFicheStatus=3)And f.CI_IncomeAccountGroup=152).tolist	'sp199266
            
            TmpAllFiche=TmpAllFiche.where(Function(f) (f.CI_IncomeAccountGroup<> 152 )).tolist	'sp199266
            '-------
            '189923 
 
            TaenTaklifKol=TmpAllFiche.where(Function(f) (f.EumFicheStatus=0 or f.EumFicheStatus=1 or f.EumFicheStatus=2)).tolist	'sp217502
           
            If TaenTaklifKol.Any  Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کلیه فیش های شهرداری بایستی تعیین تکلیف شده باشد ")   :  Exit Function 
  			TmpAllFiche = TmpAllFiche.where(Function(f) f.EumFicheStatus<>4 And f.EumFicheStatus<>2).tolist	
  		
            If TmpAllFiche.any Then
                AllFiche = TmpAllFiche.Distinct().ToList()
                '----
                    MaxD = AllFiche.max(Function(f) f.ExportPermanentDate)
                    If MaxD = "" Then MaxD="1300/01/01"
                    If MaxD < AllFiche.max(Function(f) f.ExportTemporaryDate) Then MaxD =AllFiche.max(Function(f) f.ExportTemporaryDate)
                    
					If MaxD = "" or MaxD="1300/01/01"  Then MaxD = BIZ.SA.ClsCommon.CurrentShamsiDateString().tostring
                   
                    AllFiche = TmpAllFiche.Where(Function(f) f.ExportPermanentDate = MaxD).ToList()
					if Not AllFiche.Any Then  AllFiche = TmpAllFiche.Where(Function(f) f.ExportTemporaryDate = MaxD).ToList()
                   
                   AllFiche = AllFiche.where(Function(f) f.EumFicheStatus=3 or f.EumFicheStatus=5 or f.EumincomepaymentType=4).tolist	
                   
                  
                   If AllFiche.Any Then
						Dim payableMaxdate as Long 
						 Dim MohlatPardakht as String=""
                        PaymentDate = AllFiche.max(Function(f) f.PaymentDate)
                        payableMaxdate = AllFiche.max(Function(f) f.payable)
                       
                      
                        BankPaymentDate = AllFiche.max(Function(f) f.BankPaymentDate)
                        If PaymentDate = "" Then PaymentDate = "1301/01/01"
                        If BankPaymentDate = "" Then BankPaymentDate = "1301/01/01"
                        'sp518690 مهلت فیش از PaymentBreakDate خوانده شود
                          MohlatPardakht=AllFiche.max(Function(f) f.PaymentBreakDate) 
                        if String.IsNullOrEmpty(MohlatPardakht)=True Then MohlatPardakht=Holidays(MaxD, 10) 
                                               
                         PaymentDate = BankPaymentDate
                                                                          
                         if  MaxD>="1398/12/16" And MaxD<="1399/01/31" Then 'RE 2007221 sp 272581
                              'PaymentDate > Holidays(MaxD, 10) 
                               If FicheHavale.count=0 And PaymentDate > MohlatPardakht And payableMaxdate>0 And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
                         		And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی+", " 7  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  :  Exit Function  'ad 211464  186472  band 4  ghovati-- FicheHavale.count=0 ghovati 199266
                              'PaymentDate > Holidays(MaxD, 10) 
                         ElseIf MaxD>="1399/10/22" And MaxD<"1399/12/11" Then 'RE 2472156
                                If FicheHavale.count=0 And PaymentDate > MohlatPardakht And payableMaxdate>0 And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
                         		And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی+", " 8  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  :  Exit Function  'ad 211464  186472  band 4  ghovati-- FicheHavale.count=0 ghovati 199266
                         
                         Else
                             'GetDiffDate(MaxD, PaymentDate, 3) > 10
	                         If FicheHavale.count=0 And PaymentDate > MohlatPardakht And payableMaxdate>0 And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
	                         And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی+", " 9  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  :  Exit Function  'ad 211464  186472  band 4  ghovati-- FicheHavale.count=0 ghovati 199266
                         
                         End if
                         
                     Else 
                     
                     if FicheHavale.count=0 Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی..", " 10  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  :  Exit Function  'ad 211464      186472  band 4  ghovati---FicheHavale.count=0 ghovati 199266
                 End if  
                '---
                
                If TmpAllFiche.Any Then
               
                	For i=0 to TmpAllFiche.count-1 
               	   
                		Info8.Installment= Nothing
                		Info8.GetInstallment.LoadObj (TmpAllFiche(i).NidFiche,1)
                		Dim LstInstal As New List(Of Global.DtoSC.Installment_List)
			            LstInstal = Info8.GetInstallment.GetInstallment_List()
			          
			            if LstInstal.count>0  Then Hastaghsid=True '*2411165	
   			            	            
 
 	                     
			            LstInstal= LstInstal.Where(Function(f) (f.Endstatecode = 0 or f.Endstatecode = 9 ) And f.CI_InstallmentStatus<>21 And f.CI_InstallmentStatus<>20  And f.CI_InstallmentStatus<>19 And f.CI_InstallmentStatus<>23 And f.CI_InstallmentStatus<>22 And f.CI_InstallmentStatus<>24 And f.CI_InstallmentStatus<>25 ).tolist '<>21  197817 ----2785022 20  =====953524  19 == 23 216273 --24 25 tell mosalman zade 3368512--22 sp453379
			           '2411165  if LstInstal.count>0  Then Hastaghsid=True 
			 			 	       
			            if LstInstal.count>0  And TmpAllFiche(i).ExportPermanentDate>"1397/10/00" Then
				            Dim LstAccounting As List(Of Global.DtoSC.Accounting_DocHeader)
				            
				            LstAccounting = Info8.GetFunctions.GetAccountingDocHeader(TmpAllFiche(i).NidFiche, 1)
	            			LstAccounting=LstAccounting.where(Function(f) f.PhasType =4 or f.PhasType =12 ).tolist
	            	
	            			if LstAccounting.count <  LstInstal.count  And Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District <> 80 And LstUserGroupId(LstUserDaramad)<>1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "به علت عدم ارسال به سیستم مالی مجاز به تایید نمی باشید") ' 3025670 <>80 195064  2
            			End if
            		
            			  ' if LstInstal.count=0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "پرونده مذکور فاقد تقسیط می باشد ") 
            			   ' 241725 or  f.Endstatecode = 2
			           ' LstInstal = LstInstal.Where(Function(f) (f.Endstatecode = 0 or f.Endstatecode = 9 )And f.CI_InstallmentStatus<>19 And f.CI_InstallmentStatus<>20  And f.CI_InstallmentStatus<>21 And f.CI_InstallmentStatus<>22 And f.CI_InstallmentStatus<>23 And f.CI_InstallmentStatus<>24).tolist '<>21  197817 - --23  216273--22 2988862 
			          
			            if LstInstal.count>0 Then 
			            	If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District<>80 And TmpAllFiche(i).ExportPermanentDate>"1397/10/00" Then 
			            	    if LstUserGroupId(LstUserDaramad)<>1 Then    	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "به علت داشتن چک نامشخص مجاز به تایید نمی باشید")
			            	End if   
			            End if
                	Next     		
                	
                if Hastaghsid=False Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*اعتبارسنجی++:","پرونده مذکور فاقد تقسیط می باشد " ) ''sp201964
                	
                End If
            End if
        End if
       
        
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی**", "خطا تایید تقسیط  : " & ex.Message.ToString)

    End Try

End Function

Private Function TaghsidTaeed28() as Boolean
Return True
   Try
       
    	Dim RtnResult as Integer=0 
        Dim MaxD As String = "1301/01/01"
        Dim PaymentDate As String = "1301/01/01"
        Dim BankPaymentDate As String = "1301/01/01"
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        Dim Hastaghsid as Boolean=False
        Dim AllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFiche,TaenTaklifKol,FicheHavale As List(Of DtoSC.Income_Fiche)
 
        
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        
 		if TmpAllFiche.count =0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "در صورت نداشتن هیچ فیشی مجاز به تایید تقسیط نمی باشید")   :  Exit Function 
        Dim Tmp As String
        tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
    
        
        If TmpAllFiche.Any Then
        
        
            FicheHavale=TmpAllFiche.where(Function(f) (f.EumFicheStatus=5 or f.EumFicheStatus=3)And f.CI_IncomeAccountGroup=152).tolist	
            
  		
            If FicheHavale.any Then
            
        	For i=0 to TmpAllFiche.count-1 
       	   
        		Info8.Installment= Nothing
        		Info8.GetInstallment.LoadObj (TmpAllFiche(i).NidFiche,1)
        		Dim LstInstal As New List(Of Global.DtoSC.Installment_List)
	            LstInstal = Info8.GetInstallment.GetInstallment_List()	          	
   			            	             	                     
	            LstInstal = LstInstal.where(Function (f) f.EumCheckStatus=BIZ.SA.ClsEnum.EumCheckStatus.Return And f.CI_InstallmentStatus = 28).tolist	 			 	       
				
				if LstInstal.any Then Return False
	        Next   
            End if
        End if
        
        Return True
        
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی - sp 482598", "خطا تایید تقسیط  : " & ex.Message.ToString)

    End Try

End Function


Private Function ComfirmFicheSepordeh() As Integer
    Try
        Dim MaxD As String = "1301/01/01"
        Dim PaymentDate As String = "1301/01/01"
        Dim BankPaymentDate As String = "1301/01/01"
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        Dim AllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Info8.Income_Fiche=Nothing
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)

        Dim Tmp As String
        tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring

        Dim kk = Info8.GetFunctions.GetNidLib(Guid.Parse(tmp))
        Info8.ParameterList.Remove("Revisit_House_NidBase")
        Info8.ParameterList.Add("Revisit_House_NidBase", kk.RHouse.NidBase.ToString)

       

            Str_Income = "129,130,131,132,133,134,135,136,137,138,139,140,141,142"
       

        Dim TmpArr() As String
        Dim intList_Income As New list(Of Integer)

        TmpArr = Str_Income.Split(",")
        For i = 0 To TmpArr.Count - 1
            intList_Income.Add(TmpArr(i))
        Next i
        '	Dim AllFiche,Tmp_AllFiche as New list(of DtoSc.Income_Fiche) 
 
        '	AllFiche = M_AllFiche'Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse(tmStrP))
        If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) And f.ExportPermanentDate > "1396/08/06").ToList()

        If TmpAllFiche.Any Then
            TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList


            TmpAllFiche = TmpAllFiche.Distinct().ToList()

            If TmpAllFiche.any Then
                AllFiche = TmpAllFiche.Distinct().ToList()
                If AllFiche.Any Then
                    MaxD = AllFiche.max(Function(f) f.ExportPermanentDate)

                    If MaxD = "" Then MaxD = AllFiche.max(Function(f) f.ExportTemporaryDate)

                    If MaxD = "" Then MaxD = BIZ.SA.ClsCommon.CurrentShamsiDateString().tostring
                    AllFiche = TmpAllFiche.Where(Function(f) f.ExportPermanentDate = MaxD).ToList()

                    If AllFiche.Any Then
						Dim payableMaxdate as Long 
                        PaymentDate = AllFiche.max(Function(f) f.PaymentDate)
                        payableMaxdate = AllFiche.max(Function(f) f.payable)
                        
                        BankPaymentDate = AllFiche.max(Function(f) f.BankPaymentDate)
                        If PaymentDate = "" Then PaymentDate = "1301/01/01"
                        If BankPaymentDate = "" Then BankPaymentDate = "1301/01/01"

                       '''1965672 If BankPaymentDate > PaymentDate Then 
                        PaymentDate = BankPaymentDate
                       ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "payableMaxdate", payableMaxdate) 

                        If  payableMaxdate>0 And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
                        And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", " 11  آخرين رديف درآمد بايستي داراي فيش سپرده مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")  '1048779 فیش پرداخت شده در مهلت ندارید لطفا ردیف جدید درامد بزنید و فیش جدید صادر نمایید
                    Else
                    	if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
                    	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*", " 12  آخرين رديف درآمد بايستي داراي فيش سپرده با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")    
                    End If
				Else
					if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
					And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی#", " 13  آخرين رديف درآمد بايستي داراي فيش سپرده با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")
                End If
            Else
            	if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
            	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی14", " 14  آخرين رديف درآمد بايستي داراي فيش سپرده با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")    
            End If
        Else
        	if (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"356BAAAB-2627-417A-9AE3-A815CAF505C1" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F9DE191A-959F-4C2E-8092-931FEB3589EB" _
        	And Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"C4F9A87D-2FA8-4E42-9B74-4480C47B1241" And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" And  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"083062F2-6C96-4F21-8955-0FAE1F367704" ) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی##", " 15  آخرين رديف درآمد بايستي داراي فيش شهرداري با مبلغ صفر و يا فيش پرداخت شده در مهلت باشد .")    
        End If
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "خطا فیش های تایید شده  سپرده" )

    End Try


End Function

Private Function GetDiffDate(ByVal Date1 As String, ByVal Date2 As String, ByVal Mood As Integer) As Integer
    'Mood : 1 ماه .... 2  سال
    Dim D1, D2, M1, M2, Y1, Y2, Out As Integer
    Dim tmpstr(), tmpstr2() As String
    tmpstr = Date1.Split("/")
    D1 = tmpstr(2)
    M1 = tmpstr(1)
    Y1 = tmpstr(0)


    tmpstr2 = Date2.Split("/")
    D2 = tmpstr2(2)
    M2 = tmpstr2(1)
    Y2 = tmpstr2(0)

    If Mood = 1 Then
        If Y2 >= Y1 Then
            If M2 <= M1 And Y2 = Y1 Then
                Return 0
            ElseIf M2 > M1 And Y2 = Y1 Then
                Out = M2 - M1
                '	if D2<D1 Then Out-=1
                If D2 > D1 Then Out += 1
                Return out
            ElseIf Y2 > Y1 Then
                If M2 >= M1 Then
                    Out = (Y2 - Y1) * 12
                    Out += (M2 - M1)
                    '	if D2<D1 Then Out-=1
                    If D2 > D1 Then Out += 1
                    Return out
                ElseIf M2 < M1 Then
                    Out = (Y2 - Y1) * 12
                    Out = ((M2 + Out) - M1)
                    '	if D2<D1 Then Out-=1
                    If D2 > D1 Then Out += 1
                    Return out
                End If
            End If

        End If

    ElseIf Mood = 2 Then
        If Y2 > Y1 Then
            Out = Y2 - Y1
            If M2 <= M1 Then Out -= 1
            Return out
        End If

    ElseIf Mood = 3 Then

        Dim Day1 As Long
        Select Case Y1
            Case 1388
                Day1 = 0
            Case 1389
                Day1 = 365
            Case 1390
                Day1 = 365 * 2
            Case 1391
                Day1 = 365 * 3
            Case 1392
                Day1 = (365 * 3) + 366
            Case 1393
                Day1 = (365 * 4) + 366
            Case 1394
                Day1 = (365 * 5) + 366
            Case 1395
                Day1 = (365 * 6) + 366 
            Case 1396
                Day1 = (365 * 6) + (2*366 ) 
            Case 1397
                Day1 = (365 * 7) + (2*366 )
            Case 1398
                Day1 = (365 * 8) + (2*366 ) 
            Case 1399
                Day1 = (365 * 9) + (2*366 )  
            Case 1400
                Day1 = (365 * 9) + (3*366 )                            
        End Select
        If M1 < 7 Then
            Day1 += (M1 - 1) * 31
        Else
            Day1 += (6 * 31) + (M1 - 7) * 30
        End If
        Day1 += D1

        '-----------------
        Dim Day2 As Long
        Select Case Y2
            Case 1388
                Day2 = 0
            Case 1389
                Day2 = 365
            Case 1390
                Day2 = 365 * 2
            Case 1391
                Day2 = 365 * 3
            Case 1392
                Day2 = (365 * 3) + 366
            Case 1393
                Day2 = (365 * 4) + 366
            Case 1394
                Day2 = (365 * 5) + 366
            Case 1395
                Day2 = (365 * 6) + 366 
            Case 1396
                Day2 = (365 * 6) + (2*366 )  
            Case 1397
                Day2 = (365 * 7) + (2*366 )       
            Case 1398
                Day2 = (365 * 8) + (2*366 ) 
            Case 1399
                Day2 = (365 * 9) + (2*366 ) 
            Case 1400
                Day2 = (365 * 9) + (3*366 )                                   
        End Select
        If M2 < 7 Then
            Day2 += (M2 - 1) * 31
        Else
            Day2 += (6 * 31) + (M2 - 7) * 30
        End If
        Day2 += D2

        Return iif(Day2 - Day1 < 0, 0, Day2 - Day1)
    End If
    Return 0


End Function

Private Function ComfirmFicheDiscount() As Integer

    Try

        Dim F_Insert As Boolean = False
        Dim Dt_ForRequest As datatable

        Dim Str_Income As String
        Dim i, j As Integer

        Dim AllFiche As List(Of DtoSC.Income_Fiche)
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Info8.Income_Fiche=Nothing
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)

        Dt_ForRequest = New DataTable("Dt_ForRequest")
        Dt_ForRequest.Columns.Add(New DataColumn("CI_incomeCalculation", Type.GetType("System.Int32")))
        Dt_ForRequest.Columns.Add(New DataColumn("Value", Type.GetType("System.Double")))
        Dim Tmpdr As System.Data.DataRow
        Dim AllCalculation As New List(Of DtoSC.Income_Calculation)
        Dim Tmp As String
        tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring

      ' 203982 Dim kk = Info8.GetFunctions.GetNidLib(Guid.Parse(tmp))
     '  203982 Info8.ParameterList.Remove("Revisit_House_NidBase")
      ' 203982 Info8.ParameterList.Add("Revisit_House_NidBase", kk.RHouse.NidBase.ToString)

        If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District <> 80 Then

            Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116,163"
        Else
            Str_Income = "1,7,10,16,162,163"
        End If

        Dim TmpArr() As String
        Dim intList_Income As New list(Of Integer)

        TmpArr = Str_Income.Split(",")
        For i = 0 To TmpArr.Count - 1
            intList_Income.Add(TmpArr(i))
        Next i

        If TmpAllFiche.Any Then TmpAllFiche = TmpAllFiche.Where(Function(f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) And f.ExportPermanentDate > "1393/09/01").ToList()


        If TmpAllFiche.Any Then
            TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList

            TmpAllFiche = TmpAllFiche.Distinct().ToList()
        End If


        If TmpAllFiche.Any Then

            For i = 0 To TmpAllFiche.count - 1

                AllCalculation.Clear()

                Info8.ParameterList.REMove("NidIncome")

                Info8.ParameterList.Add("NidIncome", TmpAllFiche(i).NidIncome.tostring)


                AllCalculation = Info8.GetIncome().GetIncome_Calculation_InNidIncome(TmpAllFiche(i).NidIncome)

                If AllCalculation.Any Then

                    For j = 0 To AllCalculation.Count - 1
                        If AllCalculation(j).SysValue <> 0 Then
                            Tmpdr = Dt_ForRequest.NewRow
                            Tmpdr.Item("Value") = AllCalculation(j).SysValue
                            Tmpdr.Item("CI_incomeCalculation") = iif(isdbnull(AllCalculation(j).CI_IncomeCalculation), 0, AllCalculation(j).CI_IncomeCalculation)
                            Dt_ForRequest.Rows.Add(Tmpdr)
                        End If
                    Next j
                End If
            Next i
        End If



        If Dt_ForRequest.rows.count > 0 Then
            For i = 0 To Dt_ForRequest.rows.count - 1

                If Dt_ForRequest.rows(i).Item("CI_incomeCalculation") = 100051 Then

                    If iif(isdbnull(Dt_ForRequest.rows(i).Item("Value")), 0, Dt_ForRequest.rows(i).Item("Value")) < 0 Then
                        F_Insert = True
                    End If
                End If
            Next i
        End If


        Dim mo As Biz.SC.ClsNidLib = Info8.GetFunctions.GetNidLib(Guid.Parse(tmp))


        Dim M_D, M_R, M_BL, M_H, M_BU, M_A, M_SH As Integer

        M_D = Info8.GetRevisit_House.GetBase_NosaziCode.District
        M_R = Info8.GetRevisit_House.GetBase_NosaziCode.Region
        M_BL = Info8.GetRevisit_House.GetBase_NosaziCode.Block
        M_H = Info8.GetRevisit_House.GetBase_NosaziCode.House
        M_BU = mo.RMainCodeInfo.Building
        M_A = mo.RMainCodeInfo.Apartment
        M_SH = mo.RMainCodeInfo.Shop

        Dim TmpDiscount As New List(Of DtoDbSaraAnls.Sara8Sys_DiscountDetails)
        TmpDiscount = Info8.GetAllDiscount.GetSara8Sys_DiscountDetails_InNosaziCode(M_D, M_R, M_BL, M_H, M_BU, M_A, M_SH)


        Dim F_Takhfif As Boolean = False


        If TmpDiscount.any Then
            If TmpDiscount.count > 0 Then

                For i = 0 To TmpDiscount.Count - 1

                    If TmpDiscount(i).CI_Sara8Sys_ExemptionType = 1 And TmpDiscount(i).IsDone.tostring = "" Then
                        F_Takhfif = True

                    Else
                        F_Takhfif = False
                    End If

                Next


                If F_Takhfif = True And F_Insert = True Then

                    For i = 0 To TmpDiscount.Count - 1

                        Info8.GetAllDiscount.NidD = TmpDiscount(i).NidD
                        Info8.AllDiscount = Nothing
                        Info8.GetAllDiscount.LoadObj(TmpDiscount(i).NidD)
                        Info8.GetAllDiscount.GetSara8Sys_DiscountDetails(0).IsDone = True
                        Info8.GetAllDiscount.Save()

                    Next
                End If
            End If
        End If

        F_Takhfif = False
        F_Insert = False

    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "خطا فیش های تایید شده -ایثارگران  ")

    End Try


End Function
Private Function Analys() As String
    Try
        Dim tmpAllAnaliys As New List(Of DtoSC.AnalysisBuilding)
        Dim FlagL As Integer

        tmpAllAnaliys = Info8.GetAnalysisBuilding.AnalysisBuildingList

        Dim AnaliysParvaneh_Date As String
        'Dim AnaliysParvaneh_Time as String
        AnaliysParvaneh_time = ""
        Dim TmpAnaliysSayer As List(Of DtoSC.AnalysisBuilding)
        Dim Cur_AnaliysParvaneh As New List(Of DtoSC.AnalysisBuilding_Details)
        Dim Cur_AnaliysSayer As New List(Of DtoSC.AnalysisBuilding_Details)
        FlagL = 0
        If Not (tmpAllAnaliys.Any) Then

        End If
        TmpAnaliysSayer = tmpAllAnaliys

        If TmpAnaliysSayer.Any Then
            AnaliysParvaneh_Date = TmpAnaliysSayer.Max(Function(F) F.PenaltyDate)
            TmpAnaliysSayer = TmpAnaliysSayer.Where(Function(F) F.PenaltyDate = AnaliysParvaneh_Date).ToList
            If TmpAnaliysSayer.Any Then AnaliysParvaneh_time = TmpAnaliysSayer.Max(Function(F) F.PenaltyTime)
            TmpAnaliysSayer = TmpAnaliysSayer.Where(Function(f) f.Penaltytime = AnaliysParvaneh_Time).ToList
            If TmpAnaliysSayer.Any Then FlagL = TmpAnaliysSayer(0).EnumAnalysisBuildingType
            
        End If

        Return AnaliysParvaneh_Date
    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Analys", ex.message)
    End Try
End Function
Private Function GhaleCom100()
    Try
    	Info8.AddError(BIZ.SA.EumErrorAction.warning, "هشدار", "کنترل شرایط احراز مصوبه الزامی می باشد. ")
		Dim UserIdGroup as Integer 
		'UserIdGroup= Set_UserGroupId()
		
		Dim UserList As New List(Of BIZ.SA.ClsUserGroup)
		Dim ii as Integer 
    	UserList = Info8.User.UserGroups
'299988
		If UserList.Any Then
	        For ii = 0 To UserList.Count - 1
				If Ucase(UserList(ii).GroupGuid.tostring) = "D0119423-DA84-48E4-9588-6BA84E7DB1BD"  or Ucase(UserList(ii).GroupGuid.tostring) = "61892BC0-C5DF-4D0A-B1DA-C0F10B659878" _
				or Ucase(UserList(ii).GroupGuid.tostring) = "3EA579D1-68D0-406B-BDD5-C5FBCFD1C1E8"  or Ucase(UserList(ii).GroupGuid.tostring) = ucase("08d20b53-d68a-4d22-a0e0-b43eec51461e") _
				or Ucase(UserList(ii).GroupGuid.tostring) = ucase("05aed9ee-74d6-4300-a1b3-c7d37cfb15af") or Ucase(UserList(ii).GroupGuid.tostring) = ucase("3a718066-1c33-445e-9b03-e0277f6cc756") _
				or Ucase(UserList(ii).GroupGuid.tostring) = ucase("8c4c134c-864d-4757-8e34-b734e226c162") or Ucase(UserList(ii).GroupGuid.tostring) ="C0331976-2235-4688-B8A5-37EB4517FE8A" Then
	                UserIdGroup=1
	            End If
	        Next ii
	    End If
		if Not  (UserIdGroup=1 ) Then
	
			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "شما مجاز به تاييد نمی باشید .. ")
		End if 
		
        Dim List_NidProc As New List(Of DtoSC.SrvGetCommissionList_InNidProcResult)
        Dim DLIST As New List(Of DtoSC.Commission_Vote)

        List_NidProc = Info8.GetCommission_List().CommissionList_InNidProc
        Info8.ParameterList.Remove("Commission_NidCommission")
        If List_NidProc.count = 0 Then 
        	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "امکان تایید وجود ندارد. وجود رای کمیسیون ماده 100 را بررسی نمایید  ")
        End if
        Info8.ParameterList.Add("Commission_NidCommission", List_NidProc(0).NidCommission.ToString)

        Dim tmps = Info8.FindParameter("NidProc")
        Info8.GetCommission.GetAllVote(Guid.Parse(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc.tostring))
        DLIST = Info8.GetCommission.Commission_Vote_All


        Dim MaxVoteDate As String
        If DLIST.count > 0 Then
        	DLIST=DLIST.where (Function (f) f.CI_CommissionType<> 5 or (f.CI_CommissionType=5 And f.VoteValue.tostring<>"" )).tolist()
        	if DLIST.count >0 Then DLIST=DLIST.where (Function (f) f.CI_CommissionVoteType<> 5 or (f.CI_CommissionVoteType=5 And f.VoteValue.tostring<>"" )).tolist()
        	if DLIST.count >0 Then 
        		MaxVoteDate = iif(isdbnull(DLIST.max(Function(f) f.VoteDate)), "", DLIST.max(Function(f) f.VoteDate))
        	End if
        End If

        
        'if MaxVoteDate>"1392/05/15" or MaxVoteDate<"1300/00/00 "  Then  '1683777
        '	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "با توجه به تاریخ آخرین رای صادره، فاقد شرایط احراز می باشد ")
        'End if

        ''''''''''''''''''''''''''''''''''''''''''
        Dim Base_Using,TmpBase_Using As New List(Of DtoSC.Base_Using)
        
        If TypeObjMain = 3 Then
            Base_Using = Info8.GetRevisit_Building.GetBase_Using()
        ElseIf TypeObjMain = 5 Then
           ' Base_Using = Info8.GetParvandeh_HouseSharing.GetBase_Using()
            Base_Using = Info8.GetRevisit_HouseSharing.GetBase_Using
        End If

        Dim MaxGeneratDate As String
        If Base_Using.count > 0 Then
            MaxGeneratDate = iif(isdbnull(Base_Using.max(Function(f) f.GenerateDate)), "", Base_Using.max(Function(f) f.GenerateDate))
        End If
        
        If MaxGeneratDate >= "1398/12/29" Or MaxGeneratDate < "1300/00/00 " Then 'sp 178697 > "1392/05/15"----new >= "1396/12/29"---- 1396/12/29--> sp 328476 1398/12/29
        	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "با توجه به تاریخ احداث، فاقد شرایط احراز می باشد ")
        End if 


		Dim M_MaxF as Integer 
		if Base_Using.count>0 Then 
			TmpBase_Using=Base_Using.where (Function (f) f.FloorNo <90 And f.CI_UsingType <>806 And f.CI_UsingType <>845 And f.CI_UsingType <>801 And f.CI_UsingType <>831 And f.CI_UsingType <>802 And f.CI_UsingType <>803 And f.CI_UsingType <>804 ).tolist ()     
			if TmpBase_Using.count >0 Then M_MaxF=iif (isdbnull (TmpBase_Using.Max (Function (F) f.FloorNo)) , 0 , TmpBase_Using.Max (Function (F) f.FloorNo))
			if M_MaxF>=9 Then 
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "با توجه به اینکه بلند مرتبه می باشد، فاقد شرایط احراز است ")
        	End if 
		End if
	

    Catch ex As exception
       	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "بروز خطا" & ex.tostring)
     End Try
End Function

Private Function MapHarim()
Try
	Dim typemap as Integer=2
	
	Dim NidBaseTo As Guid
	Dim tmpNidR As Guid
	Dim NidNosaziCode As Guid
	NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
	
	Dim Key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
 	Dim P_LapList As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
 	Dim LayerListId as String =""
 	Dim gu As New Guid
    Dim gu2 as New DtoSC.Base_NosaziCode 
    
    Dim district,house,Region as Integer 
    district=Info8.GetRevisit_House.GetBase_NosaziCode.District
    house=Info8.GetRevisit_House.GetBase_NosaziCode.house
    Region=Info8.GetRevisit_House.GetBase_NosaziCode.Region
						        
    If TypeObjMain = 5 Or TypeObjMain = 2 Then
        NidBaseTo = Info8.GetFunctions.GetNidBase(NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, BIZ.SA.ClsEnum.EumRevisitGroup.Revisit, tmpNidR)
        If TypeObjMain = 5 Then
        	Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
            Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", NidBaseTo.ToString)
            typemap=5
        Else
        	Info8.ParameterList.Remove("Parvandeh_House_NidBase")
            Info8.ParameterList.Add("Parvandeh_House_NidBase", NidBaseTo.ToString)

        End If
    Else
        Dim ListParent,tmpListParent As New List(Of BIZ.SA.ClsNosaziCodeType)
        ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NidNosaziCode, 0)
        tmpListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
        if tmpListParent.count >0 Then 
        	Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
            Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", tmpListParent(0).NidBase.tostring())
            typemap=5
	    Else
	        ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
	        Info8.ParameterList.Remove("Parvandeh_House_NidBase")
	        Info8.ParameterList.Add("Parvandeh_House_NidBase", ListParent(0).NidBase.tostring())
	    End if
    
    End If

    If typemap = 5 Then
        gu2 = Info8.GetParvandeh_HouseSharing.GetBase_NosaziCode
    Else
        gu2 = Info8.GetParvandeh_House.GetBase_NosaziCode
    End If
    gu = gu2.NidNosaziCode

    Dim strnosazicode As String = ""
    strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
    Dim KeyObj As Object
    
    If district <> "80" And house > "10000" Then
        Key_List = Info8.GetMap().GetMapFind(strnosazicode, "1991")
        KeyObj = Key_List(0).Guid
    ElseIf district = "80" Then
    
    
           if house > 10000 Then 
			
			key_List =Info8.GetMap().GetMapFind(strnosazicode,"3710")
 			keyObj= Key_List(0).Guid
 																			
		   ElseIf Region = 80 Then
			
			key_List =Info8.GetMap().GetMapFind(strnosazicode,"3706")
 			keyObj= Key_List(0).Guid 
			 
           Else
    
	        Key_List = Info8.GetMap().GetMapFind(strnosazicode, "5")
	        KeyObj = Key_List(0).Guid
           End if
        
    Else
        Try
            Key_List = Info8.GetMap().GetMapFind(strnosazicode, "5")
            KeyObj = Key_List(0).Guid
        Catch
            Key_List = Info8.GetMap().GetMapFind(strnosazicode, "839")
            KeyObj = Key_List(0).Guid
        End Try

    End If
    LayerListId = "10088"

    P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring, LayerListId)
    
    
    
    
    If P_LapList.count > 0 Then
        P_LapList = P_LapList.where(Function(f) f.LayerID = 10088 And f.code=1).tolist()
        If P_LapList.count > 0 Then Info8.AddError(BIZ.SA.EumErrorAction.warning, "تایید مدیر", "رعایت مفاد نامه محرمانه معاونت معماری و شهرسازی به شماره 95/م/729/219 - 95/5/27 در خصوص پاسخگویی در حریم شهر الزامی میباشد .")
    End If
Catch ex as exception 
	msgbox ("exsss" , ex.tostring )
End Try 
End Function

Private Function MapGosal() as String 
Try
	Dim typemap as Integer=2
	Dim tmp as String ="0"
	
	Dim NidBaseTo As Guid
	Dim tmpNidR As Guid
	Dim NidNosaziCode As Guid
	NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
	
	Dim Key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
 	Dim P_LapList As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
 	Dim LayerListId as String =""
 	Dim gu As New Guid
    Dim gu2 as New DtoSC.Base_NosaziCode 
    
   
						        
    If TypeObjMain = 5 Or TypeObjMain = 2 Then
        NidBaseTo = Info8.GetFunctions.GetNidBase(NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, BIZ.SA.ClsEnum.EumRevisitGroup.Revisit, tmpNidR)
        If TypeObjMain = 5 Then
        	Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
            Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", NidBaseTo.ToString)
            typemap=5
        Else
        	Info8.ParameterList.Remove("Parvandeh_House_NidBase")
            Info8.ParameterList.Add("Parvandeh_House_NidBase", NidBaseTo.ToString)

        End If
    Else
        Dim ListParent,tmpListParent As New List(Of BIZ.SA.ClsNosaziCodeType)
        ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NidNosaziCode, 0)
        tmpListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
        if tmpListParent.count >0 Then 
        	Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
            Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", tmpListParent(0).NidBase.tostring())
            typemap=5
	    Else
	        ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
	        Info8.ParameterList.Remove("Parvandeh_House_NidBase")
	        Info8.ParameterList.Add("Parvandeh_House_NidBase", ListParent(0).NidBase.tostring())
	    End if
    
    End If

    If typemap = 5 Then
        gu2 = Info8.GetParvandeh_HouseSharing.GetBase_NosaziCode
    Else
        gu2 = Info8.GetParvandeh_House.GetBase_NosaziCode
    End If
    gu = gu2.NidNosaziCode

    Dim strnosazicode As String = ""
    strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
    Dim KeyObj As Object
    
    
        Try
            Key_List = Info8.GetMap().GetMapFind(strnosazicode, "5")
            KeyObj = Key_List(0).Guid
        Catch
            Key_List = Info8.GetMap().GetMapFind(strnosazicode, "839")
            KeyObj = Key_List(0).Guid
        End Try

    
    LayerListId = "10695"

    P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring, LayerListId)
    
    if P_LapList.count>0 Then 
		Dim ff as Integer =0
		For ff=0 to P_LapList.count-1
			if P_LapList(ff).OverLapArea > key_List(0).Area*0.5 Then
				Tmp=P_LapList(ff).OverLapArea
			 	Exit For 
			Else
				Tmp="0"
			End if 	
		Next	
	Else
		Tmp="0"
	End if	 
    
    
 Return   Tmp
Catch ex as exception 
	msgbox ("exsss" , ex.tostring )
End Try 
End Function

Private Function DifDate(Str as String) as Integer

 Try

  Dim out as Integer 
  Dim ArrayStr=Split(Str , "/")

     if ArrayStr(1) <=  6 Then
      Out= ArrayStr(2) + ((ArrayStr(1)-1)*31)+( (ArrayStr(0)-1320)*365)
     Else 
      Out= ArrayStr(2) + (186 + ((ArrayStr(1)-7) * 30) )+ ((ArrayStr(0)-1320)*365)
     End if 
      
     Return out
 Catch
  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "DifDate" , "اجرای فرمول درآمد-DifDate")
 End Try

End  Function


Private Function  FnUsed2Density() as Boolean  
 Dim IsUsed as Boolean =False
 Dim SumUse as Double =0


Dim M_NrequestNo as Long
M_NrequestNo= Info8.GetFunctions.GetSh_RequestInfo_FromRevisit(Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit).NidWorkItem.ToString()
 
Dim Reqno3D as Long 
	
 Dim tmStrPMain  as String 
 tmStrPMain = Info8.FindParameter("NidProc") 
  
if tmStrPMain="" Then
 		Info8.ParameterList.REMove("NidProc")
    	Info8.ParameterList.Add("NidProc", Info8.GetFunctions.GetSh_RequestInfo_FromRevisit (Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit).nidproc.tostring )
    	tmStrPMain=Info8.FindParameter("Nidproc")
End if

 '-----------
  
 	Dim TmpA As String
    TmpA = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
     
    Dim M_ObjectType As Integer
    M_ObjectType = 0
    Dim q = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))
    
    If Not (q.RDastgah Is Nothing) Then M_ObjectType = 5
    If Not (q.RBuilding Is Nothing) Then M_ObjectType = 3
    If Not (q.RHouse Is Nothing) Then M_ObjectType = 3

    If Not (q.RHouse Is Nothing) Then M_ObjectType = 2
    If Not (q.RBuilding Is Nothing) Then M_ObjectType = 3
    If Not (q.RDastgah Is Nothing) Then M_ObjectType = 5
    If Not (q.RApartment Is Nothing) Then M_ObjectType = 4
    If Not (q.RSenfi Is Nothing) Then M_ObjectType = 6
 
 Try
    Info8.ParameterList.Remove("Revisit_House_NidBase")
   
    Info8.ParameterList.Add("Revisit_House_NidBase", q.RHouse.NidBase.ToString)
   
    if  Not (q.RDastgah Is Nothing) Then
	    Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
	    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", q.RDastgah.NidBase.ToString)
    End if
     
    if  Not (q.RBuilding Is Nothing) Then
    
		Info8.ParameterList.Remove("Revisit_Building_NidBase")
	    Info8.ParameterList.Add("Revisit_Building_NidBase", q.RBuilding.NidBase.ToString)
	End if
	 
	if  Not (q.RApartment Is Nothing) Then

		Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
	    Info8.ParameterList.Add("Revisit_Apartment_NidBase", q.RApartment.NidBase.ToString)
	End if
	
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "a", "zzz")
End Try
 
 
 '---------
 
 Try 
	 	
	 
	 
	 
	 Dim gu As New Guid
	 Dim gu2 As New DtoSC.Base_NosaziCode
	 Dim NIDProc_Melak As Guid
	
	 If M_ObjectType = 5 Then
     	gu2 = Info8.GetRevisit_HouseSharing.GetBase_NosaziCode()
     ElseIf M_ObjectType = 3 Then
   
       gu2 = Info8.GetRevisit_Building.GetBase_NosaziCode()

     End If
     gu = gu2.NidNosaziCode
        
        
       
                   
     Dim TmpListReq As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
     Dim TmpReq As DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult
      
     TmpListReq =Info8.GetRequest.GetAllRequestList(gu)
          
 
     if TmpListReq.any Then TmpListReq=TmpListReq.where (Function (f) f.eumrequeststatus =2).tolist
     
     For Each TmpReq In TmpListReq
        if M_NrequestNo<>TmpReq.NidWorkItem And  TmpReq.RequestDate>"1394/00/00" Then 
        	NIDProc_Melak = TmpReq.NidProc
        	If UCase(NIDProc_Melak.ToString) <> "" And UCase(NIDProc_Melak.ToString) <> "00000000-0000-0000-0000-000000000000" Then
	         	 
	             tmpStr=NIDProc_Melak.tostring
	             Dim TmpInfo8 As New Biz.SC.ClsObjectFactory
        		' TmpInfo8.ManagerConfirm = Nothing
        		  
        		 TmpInfo8.ParameterList.Add("NidProc", tmpStr)
        		 
        		 Dim ListMangerConfim As New List(Of DtoSC.Sh_ManagerConfirm)
        		 
        		 TmpInfo8.GetManagerConfirm.LoadObj(Guid.Parse(tmpStr) )
        		                            
        		 ListMangerConfim = TmpInfo8.GetManagerConfirm.GetSh_ManagerConfirm_List
 	                  
		         'Info8.GetManagerConfirm.LoadObj(NIDProc_Melak)
		         if Taeed3darsadcode13(TmpInfo8.GetManagerConfirm.GetSh_ManagerConfirm_List)=0 or Taeed3darsad(TmpInfo8.GetManagerConfirm.GetSh_ManagerConfirm_List)=0 Then IsUsed=True
	         	  msgbox("X","Taeed3darsadcode13-->:" & Taeed3darsadcode13(TmpInfo8.GetManagerConfirm.GetSh_ManagerConfirm_List) & "Taeed3darsad-->:" & Taeed3darsad(TmpInfo8.GetManagerConfirm.GetSh_ManagerConfirm_List) & "NidWorkItem:" & TmpReq.NidWorkItem & "IsUsed:" & IsUsed )
	              
	    	End if 
    	End If
	Next 
	 
      ''''''''''''''''''''''
   	Info8.ParameterList.REMove("NidProc")
    Info8.ParameterList.Add("NidProc" ,tmStrPMain.ToString)
    
Catch ex as Exception
	Info8.ParameterList.REMove("NidProc")
    Info8.ParameterList.Add("NidProc" ,tmStrPMain.ToString)
         	

End Try

       msgbox("IsUsed:", IsUsed)
 Return IsUsed 
 End Function

Private Function  FnUsed2DensityK() as Integer 
 Dim IsUsed as Integer=1
 Dim SumUse as Double 
 Dim HasTaedMng as Boolean =False
 '1735427
' Return 0
  Dim seDarsad as Double=0
 ''if Info8.User.UserGuid.tostring<>"6f99c240-c92b-4fb3-99db-6760750167eb" Then Return False
	
 Dim tmStrPMain  as String 
 tmStrPMain = Info8.FindParameter("NidProc") 
 Try 
	
	if tmStrPMain="" Then
 		Info8.ParameterList.REMove("NidProc")
    	Info8.ParameterList.Add("NidProc", Info8.GetFunctions.GetSh_RequestInfo_FromRevisit (Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit).nidproc.tostring )
    	tmStrPMain=Info8.FindParameter("Nidproc")
    End if
	
	
			
Dim M_NrequestNo as Long
M_NrequestNo= Info8.GetFunctions.GetSh_RequestInfo_FromRevisit(Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit).NidWorkItem.ToString()
 

'RE  4760211 if M_NrequestNo=4777410 or M_NrequestNo=4760211 Then Exit Function 'Dastoor HozeMoavenat Edalati
 if M_NrequestNo=4777410 Then Exit Function 'Dastoor HozeMoavenat Edalati
 msgbox("M_NrequestNo#:",M_NrequestNo)
 
 
 
Dim Reqno3D as Long 
			
	
 '-----------
  
 	Dim TmpA As String
    TmpA = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
     
    Dim M_ObjectType As Integer
    M_ObjectType = 0
    Dim q = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))
    
    If Not (q.RDastgah Is Nothing) Then M_ObjectType = 5
    If Not (q.RBuilding Is Nothing) Then M_ObjectType = 3
    If Not (q.RHouse Is Nothing) Then M_ObjectType = 3

    If Not (q.RHouse Is Nothing) Then M_ObjectType = 2
    If Not (q.RBuilding Is Nothing) Then M_ObjectType = 3
    If Not (q.RDastgah Is Nothing) Then M_ObjectType = 5
    If Not (q.RApartment Is Nothing) Then M_ObjectType = 4
    If Not (q.RSenfi Is Nothing) Then M_ObjectType = 6
 
 Try
    Info8.ParameterList.Remove("Revisit_House_NidBase")
   
    Info8.ParameterList.Add("Revisit_House_NidBase", q.RHouse.NidBase.ToString)
   
    if  Not (q.RDastgah Is Nothing) Then
	    Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
	    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", q.RDastgah.NidBase.ToString)
    End if
     
    if  Not (q.RBuilding Is Nothing) Then
    
		Info8.ParameterList.Remove("Revisit_Building_NidBase")
	    Info8.ParameterList.Add("Revisit_Building_NidBase", q.RBuilding.NidBase.ToString)
	End if
	 
	if  Not (q.RApartment Is Nothing) Then

		Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
	    Info8.ParameterList.Add("Revisit_Apartment_NidBase", q.RApartment.NidBase.ToString)
	End if
	
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "a", "zzz")
End Try			
		

 '-----------------------------read 3% paravneh or payankar or  zabeteh	
  Dim NidBaseTo As Guid
  Dim tmpNidR As Guid
    Dim M_TmpPayankarUsing,M_TmpParvanehUsing, TmpListBU,M_TmpPayankarUsingT As New List(Of DtoSc.Base_Using)
        Dim M_TmpPayankarFront, M_TmpParvanehFront As New List(Of DtoSc.Base_Front)

        Dim NidPayankar, HouseNidPayankar As Guid
        Dim ReadParvaneh As Boolean = False
        Dim ReadPayankar As Boolean = False
        Dim Tmpdto_Savabegh, dto_Savabegh ,Dto_PayankarSavabegh As New List(Of DtoSC.Base_ShahrsaziArchive)
        Dim M_lastCertificateDate As String
        Dim M_LastCertificateGroup As Integer
         Dim NidNosaziCode As Guid
        NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
   
       If M_ObjectType = 5 Then
            NidBaseTo = Info8.GetFunctions.GetNidBase(NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, BIZ.SA.ClsEnum.EumRevisitGroup.Revisit, tmpNidR)
            
            Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", NidBaseTo.ToString)
            dto_Savabegh = Info8.GetParvandeh_HouseSharing.GetBase_ShahrsaziArchive
       ElseIf M_ObjectType = 3 Then
           
            Dim ListParent As New List(Of BIZ.SA.ClsNosaziCodeType)
            ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NidNosaziCode, 0)
            ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList

            Info8.ParameterList.Add("Parvandeh_House_NidBase", ListParent(0).NidBase.tostring())
 			dto_Savabegh = Info8.GetParvandeh_House.GetBase_ShahrsaziArchive()
      End If
  
  'msgbox("dto_Savabegh*:", dto_Savabegh.count)	
  
           If dto_Savabegh.Any Then
                       
            M_lastCertificateDate = IIf(IsDBNull(dto_Savabegh.Max(Function(f) f.CertificateDate)), "", dto_Savabegh.Max(Function(f) f.CertificateDate))
            Tmpdto_Savabegh = dto_Savabegh.where(Function(f) f.CertificateDate = M_lastCertificateDate).tolist
            If Tmpdto_Savabegh.any Then
                M_LastCertificateGroup = IIf(IsDBNull(Tmpdto_Savabegh.Max(Function(f) f.Ci_ShahrsaziArchiveGroup)), 0, Tmpdto_Savabegh.Max(Function(f) f.Ci_ShahrsaziArchiveGroup))
                If M_LastCertificateGroup = 1 Then ReadParvaneh = True
                If M_LastCertificateGroup = 2 Or M_LastCertificateGroup = 3 Or M_LastCertificateGroup = 4 Then ReadParvaneh = True
                
                'sp 307513
                If M_LastCertificateGroup = 2 Or M_LastCertificateGroup = 3 Then
                    Dto_PayankarSavabegh= dto_Savabegh.where(Function(f) f.Ci_ShahrsaziArchiveGroup =2 or f.Ci_ShahrsaziArchiveGroup=3).tolist
                    if Dto_PayankarSavabegh.any Then  Date_payankar_Adamkhalaf=IIf(IsDBNull(Dto_PayankarSavabegh.Max(Function(f) f.CertificateDate)), "", Dto_PayankarSavabegh.Max(Function(f) f.CertificateDate))
                End if
                 
                 msgbox("*:", "ReadParvaneh:" & ReadParvaneh & "ReadPayankar:" & ReadPayankar )	
            Else
                'Exit Function
            End If


        Else
            'Exit Function
        End If
  
        
        
    'sp 307513==================================================================== 
      
     if String.IsNullOrEmpty(Date_payankar_Adamkhalaf)=False  Then      
          Dim tmpClsNidLibT = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))
          Dim NidpaYan As Guid
	        NidpaYan = Info8.GetFunctions.GetNidBase(tmpClsNidLibT.RHouse.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazPayankar, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidR)
	        Info8.ParameterList.REMove("MojazPayankar_House_NidBase")
	    	Info8.ParameterList.Add("MojazPayankar_House_NidBase", NidpaYan.ToString)
    		    		    	
            If M_ObjectType = 3 Then
                NidpaYan = Info8.GetFunctions.GetNidBase(tmpClsNidLibT.RBuilding.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazPayankar, BIZ.SA.ClsEnum.EumRevisitGroup.None, tmpNidR)
                Info8.ParameterList.REMove("MojazPayankar_Building_NidBase")
                Info8.ParameterList.Add("MojazPayankar_Building_NidBase", NidpaYan.ToString)
            ElseIf M_ObjectType = 5 Then
                NidpaYan = Info8.GetFunctions.GetNidBase(tmpClsNidLibT.RDastgah.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazPayankar, BIZ.SA.ClsEnum.EumRevisitGroup.None, tmpNidR)
                Info8.ParameterList.REMove("MojazPayankar_HouseSharing_NidBase")
                Info8.ParameterList.Add("MojazPayankar_HouseSharing_NidBase", NidpaYan.ToString)
            Else
                Exit Function
            End If
	    	
	        Info8.ParameterList.REMove("MojazPayankar_House_LoadFunc")
	    	Info8.ParameterList.Add("MojazPayankar_House_LoadFunc", "")
    	    	
	        Try
	        
                If NidpaYan.ToString <> "00000000-0000-0000-0000-000000000000" Then
                    If M_ObjectType = 3 Then
                        M_TmpPayankarUsingT = Info8.GetMojazPayankar_Building.GetBase_Using
                    ElseIf M_ObjectType = 5 Then
                        M_TmpPayankarUsingT = Info8.GetMojazPayankar_HouseSharing.GetBase_Using
                    End If
                End If
            Catch
            End Try
	        'msgbox("#:",  "M_TmpPayankarUsingT:" & M_TmpPayankarUsingT.count )
	        'RE 3910188
	        if M_TmpPayankarUsingT.any Then	Date_payankar_Adamkhalaf=M_TmpPayankarUsingT.max(Function(f) f.GenerateDate)
     End if
     '=======================================================================================
      



       Dim tmpClsNidLib = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))

      If  ReadParvaneh = True Then 
                		
	 	    Dim Nidparvaneh As Guid
	        Nidparvaneh = Info8.GetFunctions.GetNidBase(tmpClsNidLib.RHouse.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazParvaneh, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidR)
	        Info8.ParameterList.REMove("MojazParvaneh_House_NidBase")
	    	Info8.ParameterList.Add("MojazParvaneh_House_NidBase", Nidparvaneh.ToString)
    	
	    	If M_ObjectType=5 Then 
				Nidparvaneh = Info8.GetFunctions.GetNidBase(tmpClsNidLib.RDastgah.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazParvaneh, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidR)
	        	Info8.ParameterList.REMove("MojazParvaneh_HouseSharing_NidBase")
	    		Info8.ParameterList.Add("MojazParvaneh_HouseSharing_NidBase", Nidparvaneh.ToString)
	        End if	
    	
	        Info8.ParameterList.REMove("MojazParvaneh_House_LoadFunc")
	    	Info8.ParameterList.Add("MojazParvaneh_House_LoadFunc", "")
    	
	        if Nidparvaneh.ToString<>"00000000-0000-0000-0000-000000000000" Then 
			    If M_ObjectType=5 Then
			        M_TmpParvanehUsing = Info8.GetMojazParvaneh_HouseSharing.GetBase_Using 
			        MY_ParvanehUsing=M_TmpParvanehUsing
			        M_TmpParvanehFront = Info8.GetMojazParvaneh_HouseSharing.GetBase_Front
			    Else
			        M_TmpParvanehUsing = Info8.GetMojazParvaneh_House.GetBase_Using
			        MY_ParvanehUsing=M_TmpParvanehUsing
			        M_TmpParvanehFront = Info8.GetMojazParvaneh_House.GetBase_Front
			    End if
		 	End if
	          
        
       End If
       
       
       'msgbox("M_TmpParvanehUsing*:", M_TmpParvanehUsing.count)	
      ' msgbox("M_TmpParvanehFront*:", M_TmpParvanehFront.count)	
       '---------------3%parvaneh or payankar
       Dim ZirbanaParvane as Double=0
       Dim Zirbanapayankar as Double=0
      
       if M_TmpParvanehUsing.any Then ZirbanaParvane=M_TmpParvanehUsing.sum(Function(f) f.UsingArea)
       if M_TmpParvanehUsing.any Then ZirbanaParvane+=M_TmpParvanehFront.sum(Function(f) f.frontarea)
       if M_TmpPayankarUsing.any Then  Zirbanapayankar=M_TmpPayankarUsing.sum(Function(f) f.UsingArea)
        Info8.AddError(BIZ.SA.EumErrorAction.Warning, "ZirbanaParvane", ZirbanaParvane*0.03)
       Dim sedarsadprvaneh as Double=0
       Dim sedarsadZabeteh as Double=0
       if ZirbanaParvane>0 Then seDarsad=ZirbanaParvane*0.03
       sedarsadprvaneh=seDarsad
       
       msgbox("#*:", "ZirbanaParvane:" &  ZirbanaParvane  & "seDarsad:" & seDarsad)
       
       '---------------3%chideman peace
        Dim  TotalChidemanSolh as Double=0
        Dim DtSolh as New list(of Dtosc.Base_using)
	    DtSolh=Info8.GetPeace.Base_Using
        if DtSolh.any Then  TotalChidemanSolh=DtSolh.sum(Function(f) f.UsingArea)
        msgbox("#:", "DtSolh:" & DtSolh.count)
       '---------------3%chideman Zabeteh
       Dim  TotalChidemanZabateh as Double=0
       Dim Tmpdtz as New list(of Dtosc.Base_using)
	   Dim Tmpdtzfront as New list(of Dtosc.Base_Front)

	   Tmpdtz = Info8.GetZabetehChideman.GetBase_Using
	   Tmpdtzfront=Info8.GetZabetehChideman.GetBase_Front
	   if Tmpdtz.any Then TotalChidemanZabateh=Tmpdtz.sum(Function(f) f.UsingArea)
	  	   
	   if Tmpdtzfront.any Then TotalChidemanZabateh+=Tmpdtzfront.sum(Function(f) f.FrontArea)
	   if TotalChidemanSolh>0 Then TotalChidemanZabateh=TotalChidemanSolh 'RE 3530938 DarsooratVojood SOlh Chideman solhname Lahaz Migardad
	    Info8.AddError(BIZ.SA.EumErrorAction.Warning, "TotalChidemanZabateh", TotalChidemanZabateh*0.03)
	   if  ZirbanaParvane=0 Then seDarsad=TotalChidemanZabateh*0.03
	   sedarsadZabeteh=TotalChidemanZabateh*0.03
	   seDarsad=math.max(sedarsadZabeteh,sedarsadprvaneh) 'RE 4587673  نا بر صورتجلسه   44661  مور خ 9/ 3 / 1396  لطفا 3% زيربناي پروانه و يا ضابطه ( هر کدام بيشتر باشد ) ملاک تعيين 3% قرار بگيرد 
	    msgbox("seDarsad:", seDarsad)
     
       'msgbox("Tmpdtz*:", Tmpdtz.count)	
      ' msgbox("Tmpdtzfront*:", Tmpdtzfront.count)	
'---------------------------faghat 3% parvaneh ya 3%chideman zabate ya 3% Solhnameh

			
	 Dim gu As New Guid
	 Dim gup As New Guid
	 
	 Dim gu2 As New DtoSC.Base_NosaziCode
	 Dim gu3 As New DtoSC.Base_NosaziCode
	 Dim NIDProc_Melak As Guid
	
	 If M_ObjectType = 5 Then
     	gu2 = Info8.GetRevisit_HouseSharing.GetBase_NosaziCode()
     ElseIf M_ObjectType = 3 Then
   
       gu2 = Info8.GetRevisit_Building.GetBase_NosaziCode()
     End If
     gu = gu2.NidNosaziCode
     
       
        
       
       
       
       
       
       'new 2283129        
       ' Dim Mylist As New List(Of DtoSC.SrvGetCommissionVote_InNidNosaziCodeResult)
       ' Mylist = Info8.GetCommission.GetCommissionVoteList_InNidNosaziCode(gup) 
                   
     Dim TmpListReq As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
     Dim TmpReq As DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult
     TmpListReq =Info8.GetRequest.GetAllRequestList(gu)
       
       Dim TakhlofatGhable as Double=0 
     if TmpListReq.any Then TmpListReq=TmpListReq.where (Function (f) f.eumrequeststatus =2 or f.NidWorkItem=M_NrequestNo).tolist
     For Each TmpReq In TmpListReq
     
 	     
        if   TmpReq.RequestDate>"1394/00/00" Then 
        	NIDProc_Melak = TmpReq.NidProc
        	If UCase(NIDProc_Melak.ToString) <> "" And UCase(NIDProc_Melak.ToString) <> "00000000-0000-0000-0000-000000000000" Then
	         	'Info8.GetFine.Commission_FinePenalty = Nothing
	         	Info8.Fine = Nothing
	         	Info8.ParameterList.REMove("NidProc")
	         	Info8.ParameterList.Add("NidProc" ,NIDProc_Melak.ToString)
	         	
	         	Dim FinePenalti_List,TmpFinePenalti_List,TotalList As New List(Of DtoSC.Commission_FinePenalty)
	         	FinePenalti_List = Info8.GetFine.GetCommission_FinePenalty
		            
	         	If FinePenalti_List.Any  Then
	         		         		         	
	         	TotalList=FinePenalti_List.Where(Function(f) f.CI_CommissionFinePenalty = 3019 or f.CI_CommissionFinePenalty = 3207 or f.CI_CommissionFinePenalty = 3021 or f.CI_CommissionFinePenalty = 3022 or f.CI_CommissionFinePenalty = 3024 or f.CI_CommissionFinePenalty = 3046 or f.CI_CommissionFinePenalty = 2252  or f.CI_CommissionFinePenalty = 700030 ).ToList()
	         		Dim TmpVal as Double 
	         		Dim TmpValDesc as String
	         		TmpFinePenalti_List=FinePenalti_List.Where(Function(f) f.CI_CommissionFinePenalty = 1000).ToList()
					if TmpFinePenalti_List.count>0 Then 
						TmpVal=iif ( isdbnull (TmpFinePenalti_List(0).Area ),0,TmpFinePenalti_List(0).Area)
						TmpValDesc=iif ( isdbnull (TmpFinePenalti_List(0).Comments ),0,TmpFinePenalti_List(0).Comments)
						Msgbox("z*","TmpVal:" & TmpVal & "TmpValDesc:" & TmpValDesc & "TmpReq.NidWorkItem:" & TmpReq.NidWorkItem  & "TotalList:" & TotalList.count )
					
						
						
						'------------------------------------------------------------------------
						  
				             Dim TmpInfo8 As New Biz.SC.ClsObjectFactory
			        					        		  
			        		 TmpInfo8.ParameterList.Add("NidProc", NIDProc_Melak.tostring)
			        		 
			        		 Dim ListMangerConfim As New List(Of DtoSC.Sh_ManagerConfirm)
			        		 
			        		 TmpInfo8.GetManagerConfirm.LoadObj(Guid.Parse(NIDProc_Melak.tostring) )
			        		                            
			        		 ListMangerConfim = TmpInfo8.GetManagerConfirm.GetSh_ManagerConfirm_List
			 	                  		         
					         if Taeed3darsadcode13(TmpInfo8.GetManagerConfirm.GetSh_ManagerConfirm_List)=0 or Taeed3darsad(TmpInfo8.GetManagerConfirm.GetSh_ManagerConfirm_List)=0 Then
					            if TmpReq.NidWorkItem<>M_NrequestNo Then HasTaedMng=True
					         Else
					           HasTaedMng=False
					         End if
						    '------------------------------------------------------------------------
						    
						    
							if Instr(TmpValDesc,"%" ) <>0 Then 
								SumUse+=iif ( isdbnull (TmpFinePenalti_List(0).maxprice ),0,TmpFinePenalti_List(0).maxprice)
								Reqno3D=TmpReq.NidWorkItem
								
								if TmpReq.NidWorkItem=M_NrequestNo Then
								    Msgbox("xxxx:" & M_NrequestNo, "Area:" &  TotalList.Sum(Function(f) f.Area))
							  	   TakhlofatGhable+= TotalList.Sum(Function(f) f.Area)
								Else
								   Msgbox("yyyyy:" & M_NrequestNo, "Area:" &  TotalList.Sum(Function(f) f.Area))
								   if HasTaedMng=True Then TakhlofatGhable+= TotalList.Sum(Function(f) f.Area)	
								End if
								Msgbox("A:", "TakhlofatGhable" & TakhlofatGhable & "HasTaedMng:" & HasTaedMng & "NidWorkItem:" & TmpReq.NidWorkItem & "M_NrequestNo:" & M_NrequestNo)
							Else
							    if TmpReq.NidWorkItem=M_NrequestNo Then
								   TakhlofatGhable+= TotalList.Sum(Function(f) f.Area)	
								Else
								   if HasTaedMng=True Then TakhlofatGhable+= TotalList.Sum(Function(f) f.Area)	
								End if
								Msgbox("B", "TakhlofatGhable" & TakhlofatGhable & "HasTaedMng:" & HasTaedMng & "NidWorkItem:" & TmpReq.NidWorkItem & "M_NrequestNo:" & M_NrequestNo)
							End if
							
							 
							 
						End if 
					'End if
	         	End if 
	        
	    	End if 
    	End If
	Next 
	'takhalofat darkhasthaye ghabl + takhalofat jarime darkhast jadid kamtar az 3% bashad nemitavanad az taeed modir estefade konad
	Msgbox("x", "seDarsad:" & seDarsad & "TakhlofatGhable" & TakhlofatGhable)
	
	
	seDarsad=math.round(seDarsad+0.01 ,2)' RE 3899375
	TakhlofatGhable=math.round(TakhlofatGhable+0.01 ,2) ' RE 3899375
	
	Msgbox("y", "seDarsad:" & seDarsad & "TakhlofatGhable" & TakhlofatGhable)
	if seDarsad<>0 And TakhlofatGhable<=seDarsad Then IsUsed=0 : Msgbox("g", "IsUsed:" & IsUsed )
      ''''''''''''''''''''''
   	Info8.ParameterList.REMove("NidProc")
    Info8.ParameterList.Add("NidProc" ,tmStrPMain.ToString)
    
Catch ex as Exception
	Info8.ParameterList.REMove("NidProc")
    Info8.ParameterList.Add("NidProc" ,tmStrPMain.ToString)
         	

End Try
Msgbox("p", "IsUsed:" & IsUsed )
if IsUsed =1 Then Return IsUsed 
'------------------------------------------------------------------------------
   
 Try
 	 Dim List_NidProc As New List(Of DtoSC.SrvGetCommissionList_InNidProcResult)
        Dim DLIST As New List(Of DtoSC.Commission_Vote)

        List_NidProc = Info8.GetCommission_List().CommissionList_InNidProc
        Info8.ParameterList.Remove("Commission_NidCommission")
        If List_NidProc.count > 0 Then 
        	
        	Info8.ParameterList.Add("Commission_NidCommission", List_NidProc(0).NidCommission.ToString)

        	Dim tmps = Info8.FindParameter("NidProc")
        	Info8.GetCommission.GetAllVote(Guid.Parse(tmps.tostring))
        	DLIST = Info8.GetCommission.Commission_Vote_All
  msgbox("DLIST:",DLIST.count )
        	If DLIST.count > 0 Then IsUsed=2
        	msgbox("IsUsed:",IsUsed )
	End if         
	Info8.ParameterList.REMove("NidProc")
    Info8.ParameterList.Add("NidProc" ,tmStrPMain.ToString)
    
Catch ex as Exception
	Info8.ParameterList.REMove("NidProc")
    Info8.ParameterList.Add("NidProc" ,tmStrPMain.ToString)
         	

End Try 
if IsUsed =2 Then Return IsUsed 
'---------------------------------------------------------------
Try
	Dim ListPenalty As New List(Of DtoSC.AnalysisBuilding_Details)
	Dim TmpListPenalty As New List(Of DtoSC.AnalysisBuilding_Details)
	ListPenalty = Info8.GetAnalysisBuilding.GetAnalysisBuilding_Details()
	'Eslah ghovati RE 4606550 TmpListPenalty=ListPenalty.where(Function(f) (f.CI_Penalty =46 And f.valuedesc Like "%بالکن%" ) or f.CI_Penalty =146 or f.CI_Penalty = 345 or ( f.CI_Penalty = 119 And  f.valuedesc Like "%خلوت%" )).tolist 
	TmpListPenalty=ListPenalty.where(Function(f) ((f.CI_Penalty =46 ) And f.valuedesc Like "%بالکن%" )  or f.CI_Penalty = 345 or ( (f.CI_Penalty = 119 or f.CI_Penalty =146) And  f.valuedesc Like "%خلوت%" )).tolist 
	Dim DumPHayat=TmpListPenalty.sum(Function(f) f.PenaltyValue)
	'Eslah ghovati RE 4606550 TmpListPenalty=ListPenalty.where(Function(f)   f.CI_Penalty = 349 or f.CI_Penalty =246   or  ( f.CI_Penalty = 219 And  f.valuedesc Like "%خلوت%" )).tolist  
	TmpListPenalty=ListPenalty.where(Function(f)   f.CI_Penalty = 349 or ((f.CI_Penalty = 219 or f.CI_Penalty =246) And  f.valuedesc Like "%خلوت%" )).tolist  
	 msgbox("DLIST:","DumPHayat:" & DumPHayat & "TmpListPenalty:" & TmpListPenalty.sum(Function(f) f.PenaltyValue) & "seDarsad:" & seDarsad )
	if DumPHayat-TmpListPenalty.sum(Function(f) f.PenaltyValue)>seDarsad Then IsUsed=3
Catch ex as Exception
	Info8.ParameterList.REMove("NidProc")
    Info8.ParameterList.Add("NidProc" ,tmStrPMain.ToString)
        
End Try
if IsUsed =3 Then Return IsUsed  
'---------------------------------------------------------------------------

 End Function
 

 
 Public Function UnTaeedTaghsit() as Integer
'188685'
'Info8.AddError(Biz.SA.EumErrorAction.Stop, "GuidWorkFlow",GuidWorkFlow)

Dim Check_com as Integer =1
Try

Dim tmpStr as String = Info8.GetRequest.Info.NidProc.ToString

 Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
 Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
 Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)
 
 ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
  
 ListMangerConfimTaeed = ListMangerConfim.Where(Function(f)  f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence = 1).ToList
 
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

Else
	Check_com=1
End if
'Info8.AddError(Biz.SA.EumErrorAction.Stop, "Check_com",Check_com)
Catch ex As Exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار**","اطلاعات وارد شده در فرم تایید مدیران برای گواهی نادرست است") 
 'msgbox("ملک - بازدید	",ex.tostring)
 Throw ex
End Try 
Return Check_com
End Function

Private Function Rpt_Check ()
Try 

 
	
	Select Case Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)
 		 Case "B6A28C29-6761-4719-8B62-C8575F88F572"  'گواهي اتحاديه صنفي
 			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "83406869-9A8B-4B80-9A13-F74F946FF84F"  'استعلام الکترونيک - شهروند سپاري
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		 Case "06E28FB5-E844-45E4-9FBA-631C1C324081"  'گواهي افراز
			 Rpt_Parvaneh=False
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=True
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		 Case "DAACD02A-5B8D-4C40-9FFB-DE24B605D084"  'گواهي تفکيک
			 Rpt_Parvaneh=False
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=True
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		 Case "8F844ABD-2642-4854-9A33-8E61B6F67BA7"  'بررسي اطلاعات ملک
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "BC07498D-CB42-4D1A-A0C6-BDF66F50065A"  'برگشت از درآمد
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "C28A7410-B48A-4199-BA3A-B5254A20C1F5"     'آماده سازي	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False 
			 Rpt_Movafeghat=False
		Case "5AEC0F88-6381-41F3-A5B6-636C8941886E" 'گواهي مفاصا حساب
			 Rpt_Parvaneh=False
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=True
			 Rpt_Movafeghat=False
		Case "677FBEE4-B05C-4FA6-AB19-CE897739195A"  'گواهي پايانکار بهره برداري
			 Rpt_Parvaneh=False
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=True																			'گواهي پايانکار بهره برداري 	
			 Rpt_Movafeghat=False	
		Case "83F0FAC4-2268-4005-8D52-34266DD9F595"	'تاييد سابقه مجاز پايانکار	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "1E386CF1-F6BC-4C10-99B5-03F50EEF8104" 'تاييد سابقه قبل از 52	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "F75B08E0-6F1A-4762-8D1F-4271F648DEA0" 'تاييد سابقه تصويه حساب (بدون گواهي)	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "79903151-AF7C-4A73-9F18-6EB915F3293F" 'تاييد سابقه مجاز پروانه
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False	
			 Rpt_Movafeghat=False
		Case "C83F6228-987B-4B5E-8906-56F7CD87EA7C"  'مجوز صدور بهره برداري موقت - صنفي
	 		 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=True
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "F5F8B45F-A8D0-4384-9ED2-42DC747A1A20"  'ايجاد کد آپارتمان 											'ايجاد کد آپارتمان 	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
		     Rpt_Movafeghat=False
		Case "307E2C2F-99DC-4AB3-A6A4-AF7A5D731581" 'پروانه توسعه بنا/ تغيير بهره برداري
			 Rpt_Parvaneh=True
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False	
	         Rpt_Movafeghat=False 
		Case "D0127432-AA14-4A16-BA07-D6A6C7DF5C5D"  'گواهي پايانکار تفکيکي آپارتمان
			 Rpt_Parvaneh=True
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=True
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False	
			 Rpt_Movafeghat=False	
		Case "35CFFB97-67A3-488E-8A7A-BEC93C459B99","BDD563C8-1EB2-40EC-8811-19DB03908488"  'گواهي انتقال آپارتمان - شهروندسپاري
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=True
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "032DD2F4-DC70-44F4-88FD-119580EC9884","3157211F-C484-48E9-B65C-6BF3271A9F15","F9DE191A-959F-4C2E-8092-931FEB3589EB","2C1EE596-5D29-4C0A-802A-1C2492B77A9F","3157211F-C484-48E9-B65C-6BF3271A9F15"  'گواهي انتقال
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=True
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=True
			 Rpt_Movafeghat=False
		Case "EE64A610-9142-4E2B-953F-6E65B522A9E1"  'گواهي انتقال صنفي
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=True
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "FFF07F64-7B35-43BA-B363-95332389273B"  'مجوز خدماتي
		     Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "CB2A1920-7B97-4D20-9BBE-12D50F40BB5A"  'پروانه و تغییرات بنا/تجدید
			 Rpt_Parvaneh=True
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "8EBA8AAB-A256-4EEC-B1E5-DA115686A9B0"		'تعيين بروکف	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "2CA83747-919F-4FC2-B3DB-D37BA6272320"	  'گواهي تجميع 											'گواهي تجميع 	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=True
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "2AE54B83-03DB-4DF5-A983-01A761395252"	'بهره برداري موقت
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=True																			'بهره برداري موقت	
	         Rpt_Movafeghat=False
		Case "27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7"	'پروانه ديوارکشي	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=True
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False		
			 Rpt_Movafeghat=False																
		Case "DE10D8ED-3989-4E6C-AD37-F5D915BC73D6","E6C9DB23-6CEA-4161-813E-078193082DD9" 'گواهي عدم خلاف	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=True
			 Rpt_Movafeghat=False
		Case "4BFCFAE5-ACF9-4159-9BF1-1B2C0781ED0E"  'استعلام املاک فاقد سند آستانه قدس
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "D84FA3D1-54FD-4D24-8000-758A300B2441" 'گواهي مفاصا بهره برداري صنفي	
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=True
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False				
		Case "BB0854BE-4002-4C0C-975C-04B4B8485334"  'پروانه ساختماني
		 	 Rpt_Parvaneh=True
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=True
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
			
		Case "7B60BD7D-5116-4B6A-8B6C-8EE303345AD9"  'پروانه و تغییرات بنا/تمدید
			 Rpt_Parvaneh=True
			 Rpt_Zabete=True
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=True
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
		     Rpt_Movafeghat=False
		Case "08BB48AD-8AA1-4BE1-8D81-EA966DAA5035" 'استعلام تجديد اجاره
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
	    Case "4351C20C-643A-4A50-A4BD-AEBCEC5EADF8" 'استعلام تجديد اجاره آپارتمان/صنف
	         Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=True
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=True
			 Rpt_RevisitShop=True
			 Rpt_Khalaf=False
		     Rpt_Movafeghat=False
		Case "C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB","B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A"   'مجوز شروع عمليات ساختماني
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "F5CAE479-5301-47B9-A7FB-845B1FBCB2C2"  'هيئت پارکينگ
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
		Case "E10EDCBA-7945-4DDD-8306-9BAA37529AE2"  'هيئت فني
			 Rpt_Parvaneh=False
			 Rpt_Zabete=False
			 RptComments=True
			 Rpt_iIncome=False
			 Rpt_RevisitHouse=False
			 Rpt_RevisitBuilding=False
			 Rpt_RevisitApartment=False
			 Rpt_RevisitShop=False
			 Rpt_Khalaf=False
			 Rpt_Movafeghat=False
	End Select
  
Catch   ex As Exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"GetSara8Workflow", "اجرای فرمول برای گزارش ")
	Throw ex
End Try
End Function

Private Function InsertReportA(ByVal pNidNosaziCode As Guid, ByVal pEngTitle As String, ByVal pPerTitle As String, ByVal pReportPath As String, ByVal pOutputNo As String, ByVal pDistrict As String,ByVal Group as Integer)
Try
           ReportArchive =1
            Dim tmpExportLicence As New DtoDbSaraAnls.Sara8Sys_ExportLicence
            
            tmpExportLicence.ExportDate = Biz.SA.ClsCommon.CurrentShamsiDateString
            
            tmpExportLicence.ExportTime = Date.Now.Hour.ToString.PadLeft(2, "0") & ":" & Date.Now.Minute.ToString.PadLeft(2, "0") & ":" & Date.Now.Second.ToString.PadLeft(2, "0")
            tmpExportLicence.ExportNidUser = Info8.User.UserGuid
            tmpExportLicence.ExportUserName = Info8.User.UserName
            
            Dim NosaziC = Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(pNidNosaziCode,0,0,Guid.Parse("00000000-0000-0000-0000-000000000000"))
	       
	         
	        If NosaziC.EumNosaziCodeObjType = 3 Then
	            Dim Plist As New List(Of Global.Biz.SA.ClsNosaziCodeType)
	            Plist = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NosaziC.NidNosaziCode, 0)
	            Plist = Plist.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
	            pNidNosaziCode=Plist(0).NidNosaziCode
	        End If
            
            tmpExportLicence.NidNosaziCode = pNidNosaziCode
           
            ' BizCode
            Dim TmpList As New List(Of DtoSC.Archive_Wrapper)
            TmpList = Info8.GetFunctions.GetArchiveBizCode(pNidNosaziCode)
            TmpList=TmpList.where(Function(f) f.CI_ArchiveGroup=1).tolist
            
           
            If TmpList.Any Then
            
                If TmpList.Count = 1 Then
                
                   tmpExportLicence.ArchiveBizCode = TmpList.max(Function(f) f.BizCode).tostring 'TmpList.First.BizCode
                ElseIf TmpList.Count > 0 Then
                
                    Dim mm As Integer = 0
                    For mm = 0 To TmpList.Count - 1
                    
                        If TmpList.Item(mm).BizCode.Length < 32 Then
                         
                            tmpExportLicence.ArchiveBizCode = TmpList.Item(mm).BizCode.tostring
                           
                            Exit For
                        Else 
                        	tmpExportLicence.ArchiveBizCode = TmpList.Min(Function(f) f.BizCode).tostring    
                        End If
                    Next
                End If
               
            Else
            
            		Dim TmpNidNosaziCodeList As list(of BIZ.SA.ClsNosaziCodeType)
                	TmpNidNosaziCodeList=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(pNidNosaziCode,0)
                	Dim i as Integer
                	
                	For i =0 to TmpNidNosaziCodeList.count-1
                		if TmpNidNosaziCodeList(i).EumNosaziCodeObjType=2  Then 	pNidNosaziCode =TmpNidNosaziCodeList(i).NidNosaziCode
                	Next
                	tmpExportLicence.NidNosaziCode = pNidNosaziCode
                	TmpList = Info8.GetFunctions.GetArchiveBizCode(pNidNosaziCode)
                	
            		TmpList=TmpList.where(Function(f) f.CI_ArchiveGroup=1).tolist
            		If TmpList.Any Then
		                If TmpList.Count = 1 Then
		                    tmpExportLicence.ArchiveBizCode = TmpList.max(Function(f) f.BizCode).tostring' TmpList.First.BizCode
		                ElseIf TmpList.Count > 0 Then
		                    Dim mm As Integer = 0
		                    For mm = 0 To TmpList.Count - 1
		                    
		                        If TmpList.Item(mm).BizCode.Length < 32 Then
		                            tmpExportLicence.ArchiveBizCode = TmpList.Item(mm).BizCode.tostring
		                            Exit For
		                        Else 
		                        	 tmpExportLicence.ArchiveBizCode = TmpList.max(Function(f) f.BizCode).tostring
		                        End If
		                    Next
		                End If
		             End if  
            		
            End If
 
            tmpExportLicence.NidProc = Info8.GetRequest.Info.NidProc  
           
            tmpExportLicence.OutputEngTitle=pEngTitle 
            tmpExportLicence.OutputDate = Biz.SA.ClsCommon.CurrentShamsiDateString
            tmpExportLicence.OutputNo = pOutputNo
            
            tmpExportLicence.ArchiveDomain = pDistrict
            tmpExportLicence.OutputPerTitle =pPerTitle
            tmpExportLicence.ReportPath = pReportPath
            
            
            Dim ClsErrorResult As New List(Of Global.Biz.SC.SrvReportService.ClsRenderName)
            Try
           ' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " ClsErrorResult.Count " ,   tmpExportLicence.ReportPath.tostring ) 
          '   If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Exit Function 

              ClsErrorResult = Info8.GetFunctions.ExportLicenceInCentral(tmpExportLicence, pDistrict,Group,Info8.GetRequest.Info.NidWorkItem, Info8.Security_RequestGuid, Info8.Security_EncryptCode)
            ' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " ClsErrorResult.Count " ,   Info8.GetFunctions.ErrorResult.BizErrors.Count ) 
            ' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Exit Function 

          '  Info8.AddError(BIZ.SA.EumErrorAction.Stop, " ClsErrorResult.Count " ,  ClsErrorResult(0).FileName.ToString )  
                       ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, " ClsErrorResult.Count1111 " ,  ClsErrorResult(1).FileName.ToString )  
            Catch ex As Exception
    			Throw ex
    			 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مکاتبات" , ex.message.tostring) 
			End Try
          
Catch ex As Exception
    Info8.AddError(Biz.SA.EumErrorAction.Stop, "insertReport", ex.ToString)
    Throw ex
End Try
End Function

Private Function FicheBazAfrini_No_Taeed() As Integer
    Try
        ' در شرایطی که فیش بازآفرینی در وضعیت صدور و یا صدور قطعی است ( تایید /ابطال نشده است) امکان تایید گواهی وجود نداشته باشد band 1 sp299297
                
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim Fiche_BazAfrini As New List(Of DtoSC.Income_Fiche)
        Dim M_CreateDate as String
        Info8.Income_Fiche=Nothing
        
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        
        
        'fichehaye Last Record
        Dim LstIncome As List(Of Global.DtoSC.Income)
        LstIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        M_CreateDate = LstIncome.Max(Function(f) f.CreateDate)
        LstIncome = LstIncome.Where(Function(f) f.CreateDate = M_CreateDate).ToList
        TmpAllFiche=TmpAllFiche.where(Function (f) f.NidIncome=LstIncome(0).nidincome).tolist 
        
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus<> 4) ).tolist
                
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus< 3) ).tolist
        
        if TmpAllFiche.count>0 Then Fiche_BazAfrini=TmpAllFiche.where(Function (f) f.CI_IncomeAccountGroup=156).tolist 
        
        if Fiche_BazAfrini.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فیش تخفیف بازآفرینی تائید نشده بررسی فرمائید."  ) 
               
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "تائید مدیر" & ex.message ) 

    End Try


End Function

Private Function FicheP_Taeed() As Integer
    Try
        ' در شرایطی که فیش بازآفرینی در وضعیت صدور و یا صدور قطعی است ( تایید /ابطال نشده است) امکان تایید گواهی وجود نداشته باشد band 1 sp299297
                
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim Fiche_BazAfrini As New List(Of DtoSC.Income_Fiche)
        Dim M_CreateDate as String
        Info8.Income_Fiche=Nothing
        
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        
        
        'fichehaye Last Record
        Dim LstIncome As List(Of Global.DtoSC.Income)
        LstIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        M_CreateDate = LstIncome.Max(Function(f) f.CreateDate)
        LstIncome = LstIncome.Where(Function(f) f.CreateDate = M_CreateDate).ToList
        TmpAllFiche=TmpAllFiche.where(Function (f) f.NidIncome=LstIncome(0).nidincome).tolist 
        
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus<> 4) ).tolist
                
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus< 3) ).tolist
        
        if TmpAllFiche.count>0 Then Fiche_BazAfrini=TmpAllFiche.where(Function (f) f.CI_IncomeAccountGroup=150 or f.CI_IncomeAccountGroup=151).tolist 
        
        if Fiche_BazAfrini.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "در ردیف اخر فیش پرداخت قطعی و یا برگشت از سپرده تایید نشده دارید."  ) 
               
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "تائید مدیر )پرداخت قطعی( " & ex.message ) 

    End Try


End Function
Private Function Fiche_Train_No_Taeed() As Integer
    Try
        'فیش قطار شهری
                
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim Fiche_Train As New List(Of DtoSC.Income_Fiche)
        Dim M_CreateDate as String
        Info8.Income_Fiche=Nothing
        
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        
        
        'fichehaye Last Record
        Dim LstIncome As List(Of Global.DtoSC.Income)
        LstIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        M_CreateDate = LstIncome.Max(Function(f) f.CreateDate)
        LstIncome = LstIncome.Where(Function(f) f.CreateDate = M_CreateDate).ToList
        TmpAllFiche=TmpAllFiche.where(Function (f) f.NidIncome=LstIncome(0).nidincome).tolist 
        
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus<> 4) ).tolist
                
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus< 3) ).tolist
        
        if TmpAllFiche.count>0 Then Fiche_Train=TmpAllFiche.where(Function (f) f.CI_IncomeAccountGroup=153).tolist 
        
        if Fiche_Train.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فیش قطار شهری تائید نشده بررسی فرمائید."  ) 
               
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "تائید مدیر" & ex.message ) 

    End Try


End Function
Private Function Fiche_BarghashtAzSeporde_No_Taeed() As Integer
    Try
        'فیش برگشت از سپرده
                
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim Fiche_Train As New List(Of DtoSC.Income_Fiche)
        Dim M_CreateDate as String
        Info8.Income_Fiche=Nothing
        
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        
        
        'fichehaye Last Record
        Dim LstIncome As List(Of Global.DtoSC.Income)
        LstIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        M_CreateDate = LstIncome.Max(Function(f) f.CreateDate)
        LstIncome = LstIncome.Where(Function(f) f.CreateDate = M_CreateDate).ToList
        TmpAllFiche=TmpAllFiche.where(Function (f) f.NidIncome=LstIncome(0).nidincome).tolist 
        
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus<> 4) ).tolist
                
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus< 3) ).tolist
        
        if TmpAllFiche.count>0 Then Fiche_Train=TmpAllFiche.where(Function (f) f.CI_IncomeAccountGroup=151).tolist 
        
        if Fiche_Train.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فیش برگشت از سپرده تائید نشده بررسی فرمائید."  ) 
               
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "تائید مدیر" & ex.message ) 

    End Try


End Function
Private Function Fiche_payAvaleGhatee_No_Taeed() As Integer
    Try
        'فیش پرداختی اول قطعی
                
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim Fiche_Train As New List(Of DtoSC.Income_Fiche)
        Dim M_CreateDate as String
        Info8.Income_Fiche=Nothing
        
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        
        
        
        
        'fichehaye Last Record
        Dim LstIncome As List(Of Global.DtoSC.Income)
        LstIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        M_CreateDate = LstIncome.Max(Function(f) f.CreateDate)
        LstIncome = LstIncome.Where(Function(f) f.CreateDate = M_CreateDate).ToList
        TmpAllFiche=TmpAllFiche.where(Function (f) f.NidIncome=LstIncome(0).nidincome).tolist 
        
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus<> 4) ).tolist
                
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus< 3) ).tolist
        
        if TmpAllFiche.count>0 Then Fiche_Train=TmpAllFiche.where(Function (f) f.CI_IncomeAccountGroup=150).tolist 
        
        if Fiche_Train.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فیش پرداختی اول قطعی تائید نشده بررسی فرمائید."  ) 
               
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "تائید مدیر" & ex.message ) 

    End Try


End Function
Private Function Fiche_EshghalMabar_No_Taeed() As Integer
    
    'فیش ودیعه اشغال معبر
    
    Try
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim Fiche_Train As New List(Of DtoSC.Income_Fiche)
        Dim M_CreateDate as String
        Info8.Income_Fiche=Nothing
        
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        
        
        Dim Tmp As String
        tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring

     
	
        If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District <> 80 Then

            Str_Income ="7,14,21,28,35,42,49,56,63,70,77,84,85,102,120,124"
        Else
            Str_Income = "124"
        End If

        Dim TmpArr() As String
        Dim intList_Income As New list(Of Integer)
		Dim Taeed as Integer =1
        TmpArr = Str_Income.Split(",")
        For i = 0 To TmpArr.Count - 1
            intList_Income.Add(TmpArr(i))
        Next i
        
        
        if TmpAllFiche.Any Then TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
        
        'fichehaye Last Record
        Dim LstIncome As List(Of Global.DtoSC.Income)
        LstIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        M_CreateDate = LstIncome.Max(Function(f) f.CreateDate)
        LstIncome = LstIncome.Where(Function(f) f.CreateDate = M_CreateDate).ToList
        TmpAllFiche=TmpAllFiche.where(Function (f) f.NidIncome=LstIncome(0).nidincome).tolist 
        
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus<> 4) ).tolist
                
        if TmpAllFiche.count>0 Then TmpAllFiche=TmpAllFiche.where(Function (f) (f.EumFicheStatus< 3) ).tolist
        
        if TmpAllFiche.count>0 Then Fiche_Train=TmpAllFiche.where(Function (f) f.CI_IncomeAccountGroup=124).tolist 
        
        if Fiche_Train.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فیش ودیعه اشغال معبر  تائید نشده بررسی فرمائید."  ) 
               
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "تائید مدیر" & ex.message ) 

    End Try

End Function
Private Function Fiche_EshghalMabar_OutOfBreakDate() As Integer
    'sp 352888
    'کنترل فیش پرداختی خارج از مهلت
    
    Try
        Dim Str_Income As String
        Dim i As Integer
        Dim chek As Integer = 0
        
        Dim TmpAllFiche As List(Of DtoSC.Income_Fiche)
        Dim Fiche_Train As New List(Of DtoSC.Income_Fiche)
        Dim M_CreateDate as String
        Info8.Income_Fiche=Nothing
        
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
                         
        Dim Tmp As String
            Tmp = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
            
        If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Guid.Parse(tmp)).District <> 80 Then
            Str_Income ="7,14,21,28,35,42,49,56,63,70,77,84,85,102,120,124"
        Else
            Str_Income = "124"
        End If

        Dim TmpArr() As String
        Dim intList_Income As New list(Of Integer)
		Dim Taeed as Integer =1
        TmpArr = Str_Income.Split(",")
        For i = 0 To TmpArr.Count - 1
            intList_Income.Add(TmpArr(i))
        Next i
        
        
        if TmpAllFiche.Any Then TmpAllFiche = (From A In TmpAllFiche Join B In intList_Income On A.CI_IncomeAccountGroup Equals B Select A).ToList
        
        'fichehaye Last Record
        '
        Dim LstIncome As List(Of Global.DtoSC.Income)
        LstIncome = Info8.GetIncome.GetIncomeList(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
        M_CreateDate = LstIncome.Max(Function(f) f.CreateDate)
        LstIncome = LstIncome.Where(Function(f) f.CreateDate = M_CreateDate).ToList
        TmpAllFiche=TmpAllFiche.where(Function (f) f.NidIncome=LstIncome(0).nidincome).tolist 
        '
        '
        
        if TmpAllFiche.count>0 Then TmpAllFiche = TmpAllFiche.where(Function(f) f.EumFicheStatus=3 or f.EumFicheStatus=5 or f.EumincomepaymentType=4).tolist	
        Dim PaymentDate as String=""
        Dim BankPaymentDate as String=""
        Dim Mohlat_Pardakht as String=""
            PaymentDate = TmpAllFiche.max(Function(f) f.PaymentDate)
            BankPaymentDate = TmpAllFiche.max(Function(f) f.BankPaymentDate)
            Mohlat_Pardakht=TmpAllFiche.max(Function(f) f.PaymentBreakDate) 
         
	        If String.IsNullOrEmpty(Mohlat_Pardakht) = False Then
		           if Mohlat_Pardakht>"1400/08/10" Then 'az rooze Emal sampa 352888  chon ghabl az in tarikh vadieh bedoon mohlat boode
		              If String.IsNullOrEmpty(BankPaymentDate) = False Then
		                 if BankPaymentDate>Mohlat_Pardakht Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فیش ودیعه اشغال معبر  درمهلت پرداخت نشده بررسی فرمائید."  ) 
		              ElseIf  String.IsNullOrEmpty(PaymentDate) = False Then
		                 if PaymentDate>Mohlat_Pardakht Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "فیش ودیعه اشغال معبر  درمهلت پرداخت نشده بررسی فرمائید."  ) 
		              End if
		          End if
	        End if
	                              
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "تائید مدیر" & ex.message ) 

    End Try

End Function
Private Function Khalaf() As List(Of DtoSC.AnalysisBuilding_Details)
 Try
 		
 		Dim tmpAllAnaliys As New  List(Of DtoSC.AnalysisBuilding)
 	
 		 		
 		tmpAllAnaliys =Info8.GetAnalysisBuilding.AnalysisBuildingList
 		
 		Dim AnaliysParvaneh_Date as String
 		Dim AnaliysParvaneh_Time,TahlilTime as String
 	
 		Dim TmpAnaliysSayer As  List(Of DtoSC.AnalysisBuilding)
        Dim Cur_AnaliysParvaneh As New List(Of DtoSC.AnalysisBuilding_Details )
        Dim Cur_AnaliysSayer ,TmpPrnaltyL As New  List(Of DtoSC.AnalysisBuilding_Details)
        Dim TmpCur_AnaliysSayer As New  List(Of DtoSC.AnalysisBuilding_Details)
        
	    
	    TmpAnaliysSayer = tmpAllAnaliys
		  	 
		If TmpAnaliysSayer.count>0 Then 
				 AnaliysParvaneh_Date = TmpAnaliysSayer.Max(Function(F) F.PenaltyDate)
				 TmpAnaliysSayer = TmpAnaliysSayer.Where(Function(F) F.PenaltyDate=AnaliysParvaneh_Date).ToList
				 
				 If TmpAnaliysSayer.count>0 Then  AnaliysParvaneh_time= TmpAnaliysSayer.Max(Function(F) F.PenaltyTime)
				 TmpAnaliysSayer = TmpAnaliysSayer.Where(Function(f)  f.Penaltytime= AnaliysParvaneh_Time).ToList
				 

			  	 
			  	 Dim tmStr As String
				 tmStr = Info8.GetManagerConfirm.NidProc.ToString
				' if String.IsNullOrEmpty(tmStr)=True Then tmStr=Info8.GetRequest.Info.NidRevisit.tostring
				
				 msgbox("tmStr:", "NidRevisit:" &  tmStr   & "NidAnalysisBuilding:" & TmpAnaliysSayer.First.NidAnalysisBuilding.tostring )
				 
			  	 Info8.GetAnalysisBuilding.LoadObj(TmpAnaliysSayer.First.NidAnalysisBuilding,tmStr,Info8.Security_RequestGuid, Info8.Security_EncryptCode,True)
			     Cur_AnaliysSayer= Info8.GetAnalysisBuilding.AnalysisBuilding_Details
			     msgbox("Cur_AnaliysSayer:", Cur_AnaliysSayer.count)
		End if	    			     
 	Return Cur_AnaliysSayer
 	
 Catch Ex as Exception
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Khalaf", ex.message )
End Try 
End Function 


'RE 4335352 
<DisplayName("AllVot_InTotalcodا")> Private Function AllVot_InTotalcod() as Boolean 
  Try    
  
  	 Dim HasVote as Boolean=False
  	 
  	 
  	 
  	 Dim VoteCount,i,l,CI_CommissionType, CI_PrintType,Ci_VoteType,ii,kk,j as Integer
  	 Dim TmpVoteDate as String 
	 Dim gu As New Guid
	 Dim gu2 As New DtoSC.Base_NosaziCode
	 Dim NIDProc_Melak As Guid
	 Dim TotallVoteList As New List(Of DtoSC.Commission_Trepass )
	 
	 '
	 '
	 	Dim TmpA As String
	    TmpA = Info8.GetFunctions.GetActiveRevisitBaseInfo(Info8.GetManagerConfirm.NidProc).NidRevisit.tostring
	    Dim TypeObjT As Integer
	    TypeObjT = 0
	    Dim q = Info8.GetFunctions.GetNidLib(Guid.Parse(TmpA))
	    If Not (q.RDastgah Is Nothing) Then TypeObjT = 5
	    If Not (q.RBuilding Is Nothing) Then TypeObjT = 3
	    If Not (q.RHouse Is Nothing) Then TypeObjT = 3
	    If Not (q.RHouse Is Nothing) Then TypeObjMain = 2
	    If Not (q.RBuilding Is Nothing) Then TypeObjMain = 3
	    If Not (q.RDastgah Is Nothing) Then TypeObjMain = 5
	    If Not (q.RApartment Is Nothing) Then TypeObjMain = 4
	    If Not (q.RSenfi Is Nothing) Then TypeObjMain = 6
	 '
	 '
	 
	 
	 
	 
	 If TypeObjMain = 5 Then
     	gu2 = Info8.GetRevisit_HouseSharing.GetBase_NosaziCode()
     ElseIf TypeObjT = 3 Then
        gu2 = Info8.GetRevisit_Building.GetBase_NosaziCode()
     End If
     gu = gu2.NidNosaziCode
      
                  
     Dim TmpListReq As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
     Dim TmpReq As DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult
     TmpListReq =Info8.GetRequest.GetAllRequestList(gu)
          
 
     if TmpListReq.any Then TmpListReq=TmpListReq.where (Function (f) f.eumrequeststatus =2).tolist
     
      Dim  ListReq = From f In TmpListReq Order By f.RequestDate Descending.ToList
      TmpListReq=ListReq
      'msgbox("TmpListReq",TmpListReq.count)
     Dim DLIST As New List(Of DtoSC.Commission_Vote)
     For Each TmpReq In TmpListReq
       
        	NIDProc_Melak = TmpReq.NidProc
        	If UCase(NIDProc_Melak.ToString) <> "" And UCase(NIDProc_Melak.ToString) <> "00000000-0000-0000-0000-000000000000" Then
	         	
	             '
	             '
	             Info8.ParameterList.Remove("Commission_NidCommission")
	             Info8.ParameterList.Add("Commission_NidCommission", "55592676-5F75-4C36-9CB6-0000120DE186")
	        	 '
	        	 '
	        	    Dim tmpStr as String=""
		            tmpStr=NIDProc_Melak.tostring
		            'msgbox("NIDProcxx",tmpStr)
		            			            	
	        		Info8.GetCommission.GetAllVote(Guid.Parse(tmpStr.tostring))
	        		
	        	
	        		DLIST.Clear()
	        		DLIST = Info8.GetCommission.Commission_Vote_All
	        		Info8.ParameterList.Remove("Commission_NidCommission")
	        		if DLIST.any Then   Info8.ParameterList.Add("Commission_NidCommission", DLIST(0).NidCommission.ToString)
	        		 'Msgbox("DLIST ",DLIST.count)
	        		if DLIST.any Then
			             Dim AllVoteList As New List(Of DtoSC.Commission_Trepass )
			                 AllVoteList = Info8.GetCommission.Commission_Trepass_All
			                 'Msgbox("AllVoteList ",AllVoteList.count)
			                 if AllVoteList.any  Then 
								      AllVoteList = AllVoteList.Where(Function(f) f.CI_CommissionFinePenalty=700028 ).ToList() 
								      if AllVoteList.any  Then
								        HasVote=True 
                                        Return HasVote
								      End if
							End if
					Else
					  HasVote=False		
			        End if	
		              	              
	    	End if 
    	
    	'Msgbox("NidWorkItem  xxx:" & HasVote,"------------------------------" & TmpReq.NidWorkItem  )
	Next 
	 								       '
  Catch  ex as exception
  	Info8.AddError(BIZ.SA.EumErrorAction.Stop, " AllVot_InTotalcod", ex.message )
  End Try
End Function

Public Function NodeType() as String
Try
	Dim Str as String=""
	Dim i as Integer 
	Dim NodeT = Info8.GetFunctions.WF_GetTaskByNidProc(Info8.GetRequest.Info.NidProc.tostring)
    For i=0 to NodeT.data.Count-1 
	    if NodeT.data(i).EumTaskStatus<>3 Then 
	    	
	    	Str= NodeT.data(i).TaskTitel
	    	if Str="مالک" Then Str=""
	    	if Str="تاييد مديران" Then Str=""
	    	if Str="املاک" Then Str=""
	    	if Str<>"" Then Return Str
		End if 
		 
	Next
	Return Str
 Catch Ex as Exception
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "NodeType", ex.message )
End Try 
End Function 

Private Function MapTarhJame3()  
Try
	msgbox ("MapTarhJame3" , "a")
	Dim typemap as Integer=2
	Dim tmp as String ="0"
	
	Dim NidBaseTo As Guid
	Dim tmpNidR As Guid
	Dim NidNosaziCode As Guid
	NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
	
	Dim Key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
 	Dim P_LapList As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
 	Dim LayerListId as String =""
 	Dim gu As New Guid
    Dim gu2 as New DtoSC.Base_NosaziCode 
    
   
						        
    If TypeObjMain = 5 Or TypeObjMain = 2 Then
        NidBaseTo = Info8.GetFunctions.GetNidBase(NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, BIZ.SA.ClsEnum.EumRevisitGroup.Revisit, tmpNidR)
        If TypeObjMain = 5 Then
        	Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
            Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", NidBaseTo.ToString)
            typemap=5
        Else
        	Info8.ParameterList.Remove("Parvandeh_House_NidBase")
            Info8.ParameterList.Add("Parvandeh_House_NidBase", NidBaseTo.ToString)

        End If
    Else
        Dim ListParent,tmpListParent As New List(Of BIZ.SA.ClsNosaziCodeType)
        ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NidNosaziCode, 0)
        tmpListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
        if tmpListParent.count >0 Then 
        	Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
            Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", tmpListParent(0).NidBase.tostring())
            typemap=5
	    Else
	        ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
	        Info8.ParameterList.Remove("Parvandeh_House_NidBase")
	        Info8.ParameterList.Add("Parvandeh_House_NidBase", ListParent(0).NidBase.tostring())
	    End if
    
    End If

    If typemap = 5 Then
        gu2 = Info8.GetParvandeh_HouseSharing.GetBase_NosaziCode
    Else
        gu2 = Info8.GetParvandeh_House.GetBase_NosaziCode
    End If
    gu = gu2.NidNosaziCode

    Dim strnosazicode As String = ""
    strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
    Dim KeyObj As Object
    
    
        Try
            Key_List = Info8.GetMap().GetMapFind(strnosazicode, "5")
            KeyObj = Key_List(0).Guid
        Catch
            Key_List = Info8.GetMap().GetMapFind(strnosazicode, "839")
            KeyObj = Key_List(0).Guid
        End Try

    
    LayerListId = "10239"

    P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring, LayerListId)
    
    if P_LapList.count>0 Then 

				ISTarhJame3=True

	
	Else

		ISTarhJame3=False
	End if	 
    
    

Catch ex as exception 
	msgbox ("MapTarhJame3" , ex.tostring )
End Try 
End Function

'sp 462674
Public Function ObligationBime(ByVal TmpListMangerConfimKh as List(Of DtoSC.Sh_ManagerConfirm)) As Integer

    Dim Check_com As Integer = 1
    Try

        If tmpStr <> "" Then

            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = TmpListMangerConfimKh
 
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 30 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 30 And f.EumManagerConfirmLicence = 2).ToList
                      
            If ListMangerConfimTaeed.Any Then
                If ListMangerConfimCancel.Any Then
	                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
	                        Check_com = 0
	                    Else
	                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
	                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString
	
	                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 30 And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
	                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 30 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList
	
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
    
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "هشدار", "اطلاعات وارد شده در فرم تایید مدیران برای تعهد ارائه تاییدیه بیمه صحیح نمیباشد")
        Throw ex

    End Try
    Return Check_com
End Function

Public function ElzamWebService() 
       Try
            Dim Clspermit As New Biz.Communication.ClsPermit

            Dim ClsPRMC As New Biz.Communication.Elzam.ClsPermitResponseMessageContract
            Dim clsIputBodyModel As New Biz.Communication.Elzam.InputBodyModel

            '   Dim PieceType  As New Biz.Communication.Elzam.PieceType
            '  Dim PermitType As New Biz.Communication.Elzam.PermitType


            Dim PermitType As New Biz.Communication.Elzam.PermitType
            Dim permit As New Biz.Communication.Elzam.Permit            
            
            '--------
            Dim LstParent As List(Of Global.Biz.SA.ClsNosaziCodeType)
            Dim LstParent_TMP As List(Of Global.Biz.SA.ClsNosaziCodeType)
            LstParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)              

            Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
            TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))

            

            Dim tmpNidR As Guid = Guid.Parse("00000000-0000-0000-0000-000000000000")
            Dim NidParvaneh,NidParvanehTMP As Guid

            Dim licenseIssuanceDate, LicenseNo As String

            If TmpNosazi.EumNosaziCodeObjType = 2 Or TmpNosazi.EumNosaziCodeObjType = 3 Then
                  
                If LstParent.Any Then LstParent_TMP = LstParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList    
                   
                NidParvaneh = Info8.GetFunctions.GetNidBase(LstParent_TMP(0).NidNosaziCode, 1, 0, tmpNidR)    
                'NidParvanehTMP = NidParvaneh                         
                               
                Info8.ParameterList.Remove("Parvandeh_House_NidBase")
                Info8.ParameterList.Add("Parvandeh_House_NidBase", NidParvaneh.ToString)
                Info8.ParameterList.Remove("Parvandeh_House_LoadFunc")
                Info8.ParameterList.Add("Parvandeh_House_LoadFunc", "")

            ElseIf TmpNosazi.EumNosaziCodeObjType = 5 Then
                If LstParent.Any Then LstParent_TMP = LstParent.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
                                                 
                NidParvaneh = Info8.GetFunctions.GetNidBase(LstParent_TMP(0).NidNosaziCode, Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Biz.SA.ClsEnum.EumRevisitGroup.None, tmpNidR)

                Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
                Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", NidParvaneh.ToString)
                Info8.ParameterList.Remove("Parvandeh_HouseSharing_LoadFunc")
                Info8.ParameterList.Add("Parvandeh_HouseSharing_LoadFunc", "")
                
				If LstParent.Any Then LstParent_TMP = LstParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList    
                   
                NidParvaneh = Info8.GetFunctions.GetNidBase(LstParent_TMP(0).NidNosaziCode, 1, 0, tmpNidR)                 
                
                Info8.ParameterList.Remove("Parvandeh_House_NidBase")
                Info8.ParameterList.Add("Parvandeh_House_NidBase", NidParvaneh.ToString)
                Info8.ParameterList.Remove("Parvandeh_House_LoadFunc")
                Info8.ParameterList.Add("Parvandeh_House_LoadFunc", "")                  
                                                
            End If
            
            
           
            

            Dim Base_ShahrsaziArchive As New List(Of Global.DtoSC.Base_ShahrsaziArchive)            

            if TmpNosazi.EumNosaziCodeObjType = 2 or TmpNosazi.EumNosaziCodeObjType = 3 Then
            	Base_ShahrsaziArchive = Info8.GetParvandeh_House.GetBase_ShahrsaziArchive()
            Else	
           		Base_ShahrsaziArchive = Info8.GetParvandeh_HouseSharing.GetBase_ShahrsaziArchive()
            End If   
            
                                           
            
            Base_ShahrsaziArchive = Base_ShahrsaziArchive.Where(Function(f) f.CI_ShahrsaziArchiveGroup = 1).ToList ' parvaneh Group
            Base_ShahrsaziArchive = Base_ShahrsaziArchive.OrderByDescending(Function(f) f.CertificateDate).ToList

            If Base_ShahrsaziArchive.Any Then licenseIssuanceDate = Base_ShahrsaziArchive(0).CertificateDate.ToString : LicenseNo = Base_ShahrsaziArchive(0).CertificateNo.ToString
            '-----------

            Dim BaseUsing,BaseUsing_TMP As New List(Of DtoSC.Base_Using)
            BaseUsing = Info8.GetMapControl.Base_Using
            BaseUsing_TMP = BaseUsing
            if BaseUsing.any Then
            	permit.allowedOccupancyLevel = BaseUsing_TMP.sum(Function (f) f.UsingArea).tostring
            Else
            	permit.allowedOccupancyLevel = "0"	
            End if	
         

            '	PermitType = New Biz.Communication.Elzam.PermitType	
            permit.type = 1 ' PermitType.Survey  

            permit.no = LicenseNo
            permit.issuanceDate = licenseIssuanceDate
            permit.centroid = "0"
            
            permit.province = "خراسان رضوی"
            permit.town = "مشهد"
            permit.registerationUnit = If(String.IsNullOrEmpty(Info8.GetParvandeh_House.GetBase_CommonEstate_House.RegisterSection.tostring),"0",Info8.GetParvandeh_House.GetBase_CommonEstate_House.RegisterSection.tostring)


            Dim plaq As New List(Of DtoSC.Base_RegisterPlack)
            Dim plaqMain, plaqSub As String
          
            Try
	            if TmpNosazi.EumNosaziCodeObjType = 2 or TmpNosazi.EumNosaziCodeObjType = 3 Then
	            	plaq = Info8.GetParvandeh_House.GetBase_RegisterPlack
	            Else	
	           		plaq = Info8.GetParvandeh_HouseSharing.GetBase_RegisterPlack
	            End If   
	            
            Catch 
            	plaq = Info8.GetMapControl.GetBase_RegisterPlack
            End Try	
                                        
            
            If plaq.Any Then            	
                For Each X As DtoSC.Base_RegisterPlack In plaq
                    If X.CI_RegisterPlackType = 1 Then plaqMain = X.RegisterPlackTypeValue.ToString
                    If X.CI_RegisterPlackType = 2 Then plaqSub = X.RegisterPlackTypeValue.ToString
                Next
            Else
                plaqMain = "0"
                plaqSub = "0"
            End If

            permit.primary = If(String.IsNullOrEmpty(plaqMain),"0",plaqMain)
            permit.secondary = If(String.IsNullOrEmpty(plaqSub),"0",plaqSub) 

            '----------
            permit.attachment = "Null"
            permit.electronicEstateNoteNo = "0"
            permit.timeLimit = "0000/00/00"
            permit.postalCode = "0"

            permit.section = "Null"
            permit.subSection = "Null"
            '-------


			Dim Jamcode as String  'sp 493246

            Dim Applicant1 As New List(Of Biz.Communication.Elzam.Applicant)

            Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
            lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Info8.GetRequest.Info.NidProc)
            If lstOwner.Count > 0 Then
                Dim Fname, Mobail, Lname, code, BDay As String
                
                For Each u As DtoDbSaraAnls.UGP_AuthenticationHistory In lstOwner
                    '	For i=0 to i<lstOwner.Count 

                    Dim Applicant As New Biz.Communication.Elzam.Applicant

                    '	if Not String.IsNullOrEmpty(u.LawyerFirstName.tostring) Then
                    Mobail = u.LawyerMobile  'lstOwner(i).LawyerMobile ' u.LawyerMobile  '
                    Fname = u.LawyerFirstName  'lstOwner(i).LawyerFirstName  ' u.LawyerFirstName  '
                    Lname = u.LawyerLastName ' lstOwner(i).LawyerLastName '	u.LawyerLastName
                    code = u.LawyerNationalCode  '	lstOwner(i).LawyerNationalCode ' u.LawyerNationalCode  '
                    BDay = u.BirthDate
                    
                    'sp 493246 
                    if String.IsNullOrEmpty(JamCode) Then 
                    
                    	Try 
                    		JamCode = u.JamCode.tostring
                    	Catch 
                    		JamCode = ""
                    	End Try	
                    End if  'end sp 493246
                    'continue For
                    '	End if	 	  

                    If Fname = Nothing Then Fname = u.OwnerFirstName    ' lstOwner(i).OwnerFirstName
                    If Lname = Nothing Then Lname = u.OwnerLastName 'lstOwner(i).OwnerLastName
                    If Mobail = Nothing Then Mobail = u.OwnerMobile     'lstOwner(i).OwnerMobile
                    If code = Nothing Then code = u.OwnerNationalCode
                    If BDay = Nothing Then BDay = u.BirthDate
                    if JamCode = Nothing Then JamCode = u.JamCode

                    Applicant.nationalCode = code
                    Applicant.birthDate = If(String.IsNullOrEmpty(BDay),"0000/00/00",BDay)
                    Applicant.firstName = Fname
                    Applicant.lastName = Lname
                    Applicant.fatherName = ""

                    Applicant1.Add(Applicant)

                Next
            End If
            
            
            '=========================== 'sp 493246 ===================================
              permit.electronicEstateNoteNo = JamCode.tostring
			'=========================== ' end sp 493246 ===================================

            Dim Floor1 As New List(Of Biz.Communication.Elzam.Floor)

            If BaseUsing.Any Then
                For Each BaseUsingTMP As DtoSC.Base_Using In BaseUsing
                    Dim Floor As New Biz.Communication.Elzam.Floor

                    Floor.floorNumber = BaseUsingTMP.FloorNo
                    Floor.totalPieces = BaseUsingTMP.UnitCount
                    Floor.area = BaseUsingTMP.UsingArea
                    Floor.utilization = BaseUsingTMP.CI_UsingGroup.ToString
                    Floor.usage = BaseUsingTMP.CI_UsingType.ToString

                    '-----
                    Floor.attachment = "Null"
                    '------

                    Floor1.Add(Floor)
                Next
            End If


            Dim PieceType As New Biz.Communication.Elzam.PieceType
            'Dim piece As New Biz.Communication.Elzam.Piece
            Dim piece1 As New List(Of Biz.Communication.Elzam.Piece)


            If BaseUsing.Any Then
                For Each BaseUsingTMP As DtoSC.Base_Using In BaseUsing
                    Dim piece As New Biz.Communication.Elzam.Piece

                    If BaseUsingTMP.CI_UsingType.ToString <> "801" Or BaseUsingTMP.CI_UsingType.ToString <> "902" Then
                    	Continue For
					Else                    	
	                    piece.type = iif(BaseUsingTMP.CI_UsingType.ToString = "801", 1, 2)
	
	
	
	                    piece.usage = BaseUsingTMP.CI_UsingType.ToString
	
	
	                    piece.plan = "0"
	                    '----- 
	                    piece.floorNumber = 0
	                    piece.pieceNumber = 0
	                    piece.roofLimit = "0"
	                    piece.floorLimit = "0"
	                    piece.samt = "0"
	                    piece.jahat = "0"
	
	                    piece.area = 0.0
	                    piece.northLimit = "Null"
	                    piece.eastLimit = "Null"
	                    piece.westLimit = "Null"
	                    piece.southLimit = "Null"
	                    piece.contour = "Null"
	                    piece.utilization = BaseUsingTMP.CI_UsingGroup.ToString
	                    ''-----
	
	                    piece1.Add(piece)
	               End if     
                Next

            End If


            clsIputBodyModel.applicants = New List(Of Biz.Communication.Elzam.Applicant)
            clsIputBodyModel.floors = New List(Of Biz.Communication.Elzam.Floor)
            clsIputBodyModel.pieces = New List(Of Biz.Communication.Elzam.Piece)

            clsIputBodyModel.permit = permit
            clsIputBodyModel.applicants = Applicant1
            clsIputBodyModel.floors = Floor1
            clsIputBodyModel.pieces = piece1

            Dim clsUser As New Biz.SA.ClsUser
            clsUser = Info8.User

		'	Dim clsTrackingCode As New Biz.Communication.Elzam.SuccessData
			Dim newGuid As Guid = Guid.NewGuid()
            ' Info8.AddError(Biz.SA.EumErrorAction.warning, "newGuid: ",  newGuid.tostring)
            Try

                ClsPRMC = Clspermit.SendPermit(clsIputBodyModel, Info8.GetRequest.Info.NidProc, newGuid, clsUser) ' Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), clsUser)
              '  Info8.AddError(Biz.SA.EumErrorAction.Stop, "AF", Clspermit.ErrorResult.BizErrors.Count)
                If Clspermit.ErrorResult.BizErrors.Count > 0 Then
                   ' Info8.AddError(Biz.SA.EumErrorAction.Warning, "AFF", Clspermit.ErrorResult.BizErrors(0).ErrorTitel)
                End If
                If ClsPRMC.ResCode = 1 Then
                	ClsPRMC.Data = New Biz.Communication.Elzam.SuccessData
                    Info8.AddError(Biz.SA.EumErrorAction.warning, "اداره ثبت ",  " درخواست با موفقیت به اداره ثبت ارسال شد  ")
                  '  Exit Function
                Else
                    Info8.AddError(Biz.SA.EumErrorAction.warning, "اداره ثبت", "ارسال نشد : " + ClsPRMC.ResCode.ToString + " | message: " + ClsPRMC.ResMessage)
                  '  Exit Function
                End If


            Catch e As Exception
                Info8.AddError(Biz.SA.EumErrorAction.warning, "Error Send to Elzam", "Elzam: " + e.Message)
            End Try

        Catch e As Exception
            Info8.AddError(Biz.SA.EumErrorAction.warning, "NOO", "Elzam:" + e.Message)
        End Try

End  function

Public Function ExecSp(ByVal NameSp as String)  as Boolean
 Try
 '	Sms("09155193570",Info8.GetRequest.Info.NidWorkItem.ToString,"M")
 
' Info8.AddError(BIZ.SA.EumErrorAction.warning, "Ret", "1")
 
	Dim p_Connection As String = "Data Source=172.16.8.136;Initial Catalog=Sara8M03;Persist Security Info=True;User ID=esup92;Password=zxc@789-sf"
	Dim intRowsAffected As String = 0
	
' Info8.AddError(BIZ.SA.EumErrorAction.warning, "Ret", "2")	
	
	Dim con As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(p_Connection)
	
' Info8.AddError(BIZ.SA.EumErrorAction.warning, "Ret", "3")
 	
	Dim TextSms as String =""
	Dim cmd As New System.Data.SqlClient.SqlCommand(NameSp, con)
	cmd.CommandType = CommandType.StoredProcedure
	cmd.Parameters.AddWithValue("@NidProc", Info8.GetRequest.Info.NidProc)
	cmd.Connection = con
	con.Open()
    Dim adp As New SqlDataAdapter(cmd)
    Dim ds As DataSet = New DataSet()

   
 '   Info8.AddError(BIZ.SA.EumErrorAction.warning, "Ret", "4")
   
    cmd.ExecuteNonQuery()
	adp.Fill(ds, NameSp)
	Dim BuildingGroupTitle As String 
Try
	BuildingGroupTitle = ds.Tables(0).Rows(0).Item(1).ToString
 Catch
End Try 	
 '	Sms("09155193570",Info8.GetRequest.Info.NidWorkItem.ToString,"M1")
	con.Close()
	con = Nothing
	
	

       
       if  {"الف","ب"}.contains(BuildingGroupTitle) Then
       		Return False
       		
       Else       	       
        	Return True
       End if 	
Catch e as exception

	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Ret", e.message)

End Try

End function


Public function WorkFlowArchive() as Integer
    Try
    	Dim p_WorkflowGuid As String
        Try
        	p_WorkflowGuid = UCase(Info8.DtoWorkflowData.WorkflowGuid.ToString)
        	if p_WorkflowGuid.tostring = "00000000-0000-0000-0000-000000000000" Then 
        		p_WorkflowGuid = UCase(Info8.GetRequest.info.NidWorkFlowDeff.ToString)
        	End if
        Catch
        	p_WorkflowGuid = UCase(Info8.GetRequest.info.NidWorkFlowDeff.ToString)
        End Try
               
        
        Dim jj As Integer = 0
        Dim p_NidWorkflowDeff As Guid
        Dim p_WorkflowTitel As String = ""
        Dim tmpSh_WorkflowList As New List(Of DtoSA.Sh_Workflow_SA)
        tmpSh_WorkflowList = Info8.GetWorkFlow.GetSh_WorkflowList_All 'Info8.GetWorkFlow.GetSh_WorkflowList_InNosaziCodeObjType(Info8.GetParvandeh_House.GetBase_NosaziCode.NidNosaziCode)             
 
      'sp 509299
       if sp509299=True Then tmpSh_WorkflowList.REMoveAll(Function(f) ucase(f.NidWorkflowDeff.ToString())=ucase("25f1f6fa-354f-46b4-be0f-3497e1f82e20"))  'تایید سابقه مجاز پروانه
      'end sp 509299
      
        If tmpSh_WorkflowList.Any Then        
			tmpSh_WorkflowList = tmpSh_WorkflowList.where(Function (f) ucase(f.NidWorkFLowDeff.tostring)=p_WorkflowGuid).tolist		
			if 	tmpSh_WorkflowList.any Then			
			Return tmpSh_WorkflowList(0).EumShahrsaziArchiveGroup
			End if
        End If
		Return 2
    Catch ex As Exception
        Info8.AddError(Biz.SA.EumErrorAction.Stop, "WorkflowArchive_تایید مدیر", "اجرای فرمول" & ex.Message)
        Throw ex
    End Try
End function
    
Private Function ElzamNewWebServices 
   
Try
	Dim NidNosaziCode = Info8.GetRequest().Info.NidNosaziCode
	Dim NidRevisit = Info8.GetRequest().Info.NidRevisit
	
	Dim NickN =Info8.GetFunctions.GetNosaziNickName(NidNosaziCode)
	Dim tmpDistrict as Integer = CInt(NickN.Split("-")(0))

    Dim Elzam as New BIZ.Communication.ClsElzam(tmpDistrict)
    
    Dim E_ResponseModel As New Biz.Communication.ElzamResponseModel
    Dim E_GSBElzamResponseModel As New Biz.Communication.GSBElzamResponseModel
    Dim Elazm_AP As New  BIZ.Communication.ElzamApprovePlan
    
    Dim E_Block as New List(Of Biz.Communication.ElzamBlock)
    Dim E_Owner as New List(Of Biz.Communication.ElzamOwnerInfo)
	Dim E_mainPlan as New Biz.Communication.ElzamMainPlan
	Dim E_UnitPlanList as New List(Of Biz.Communication.ElzamUnitPlan)
	


    

	
	Dim ElzamNidBase = Info8.GetFunctions.GetNidBase(NidNosaziCode,0,7,NidRevisit)	
    
 ' 	Info8.ParameterList.REMove("Parvandeh_Block_NidBase")
 ' 	Info8.ParameterList.Add("Parvandeh_Block_NidBase", ElzamNidBase.tostring)      
 
 Dim jamCode , CurrentArea as String
 
Try	
   	jamCode = Info8.GetMapControl_Engineer.GetBase_HouseInfo.JamCode.tostring
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Elzam "," کد جام وارد نشده است ") : Exit Function 
End Try
   

Try		
   	CurrentArea = Info8.GetMapControl_Engineer.GetBase_HouseInfo.CurrentArea.tostring
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Elzam "," مساحت سند وارد نشده است ") : Exit Function 
End Try
      	
  	
   	
   	'======== IssueNo, IssueDate, IssueTime 
   	Dim GetLastParvandehInfo = GetLastParvandehInfoFunc
   	
   	'=========  SubSection , Basic, Secondary ========
        Dim plaq As New List(Of DtoSC.Base_RegisterPlack)
        Dim plaqBasic, plaqSecondary As String
      
        Try  
        	plaq = Info8.GetMapControl_Engineer.GetBase_RegisterPlack
        Catch
        	
        End Try	
                                    
        
        If plaq.Any Then            	
            For Each X As DtoSC.Base_RegisterPlack In plaq
                If X.CI_RegisterPlackType = 1 Then plaqBasic = X.RegisterPlackTypeValue.ToString
                If X.CI_RegisterPlackType = 2 Then plaqSecondary = X.RegisterPlackTypeValue.ToString
            Next
        Else
            plaqBasic = "0"
            plaqSecondary = "0"
        End If   
        
        
        
     '=========  Address, TotalBlocks  ========   	
   		Dim Address = Info8.GetRevisit_House.GetBase_AddressInfo.MainAddress
   		
		Dim BaseUsing  as New List (Of DtoSC.Base_Using)
		BaseUsing = Info8.GetMapControl_Engineer.GetBase_Using   		
   		Dim TotalBlock = BaseUsing.Select(Function (f) f.BuildingNo).distinct.count
   		
   		
	
    
    E_Block = ElzamBlockSection 
  '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElzamBlockSection", E_Block.count) : Exit Function 
    E_Owner = ElzamOwnerSection
    E_UnitPlanList = ElzamUnitPlan
    E_mainPlan = ElzamMainPlan
      
      
     
      
    Elazm_AP.organId = "14002764962"
    Elazm_AP.elzam14 = True
    Elazm_AP.requestUniqueId = Info8.Getrequest.info.Nidworkitem.tostring        
    Elazm_AP.IssueNo = GetLastParvandehInfo.Item1    
    Elazm_AP.IssueDate = GetLastParvandehInfo.Item2
    Elazm_AP.IssueTime = GetLastParvandehInfo.Item3
    Elazm_AP.ActionReferenceType = 1
    Elazm_AP.ActionReferenceNo = "14002764962"
    Elazm_AP.EstateTypeCode = 2
    Elazm_AP.Province = 13
    Elazm_AP.Unit = "6271" 
    Elazm_AP.Section = Info8.GetRevisit_House.GetBase_CommonEstate_House.RegisterSection
    Elazm_AP.SubSection = "0"
    Elazm_AP.Basic = plaqBasic
    Elazm_AP.Secondary = plaqSecondary
    Elazm_AP.JamCode = jamCode
    Elazm_AP.Address = Address
    Elazm_AP.Area = CurrentArea
  '  Elazm_AP.PostalCode = "0"
    Elazm_AP.TotalBlockNo = TotalBlock
  '  Elazm_AP.TotalFloorNo=7      
    Elazm_AP.Block = E_Block
    Elazm_AP.OwnersInfo = E_Owner
    Elazm_AP.MainPlan = E_mainPlan
    Elazm_AP.UnitPlans = E_UnitPlanList
    
  
  
    If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
    '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", "Done")
    '   Exit Function
   End If 
  
  
       
    Try
        Dim pNidproc as Guid = Info8.GetRequest().Info.NidProc
       E_GSBElzamResponseModel = Elzam.ApprovePlan(pNidproc,Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"),"ESUP",Elazm_AP,False)
       
    '   If Elzam.ErrorResult.BizErrors.Count > 0 Then  Info8.AddError(Biz.SA.EumErrorAction.Stop, "AFF", Elzam.ErrorResult.BizErrors(0).ErrorTitel)
        if Not E_GSBElzamResponseModel.result.data.owTrakingCode is Nothing Then
            Info8.AddError(BIZ.SA.EumErrorAction.Stop,"ارسال به ماده: ",E_GSBElzamResponseModel.result.data.owTrakingCode)' & " | Code: " & E_ResponseModel.Data.TrackingCode)
        Else    
            Info8.AddError(BIZ.SA.EumErrorAction.Stop,"Failure: " ,"Dont Send: "& E_GSBElzamResponseModel.result.data.msg)
        End If
    Catch e as Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop,"Exceptions: ",e.Message)
    End Try  
    
Catch e as Exception
    Info8.AddError(BIZ.SA.EumErrorAction.Stop,"Ex-Elzam: ",e.Message)
End Try     
End Function

Private Function ElzamMainPlan() As Biz.Communication.ElzamMainPlan

    Dim E_mainPlan as New Biz.Communication.ElzamMainPlan
    
    Dim Elazm_Map as New list (Of BIZ.SC.ClsMapControl.ElzamMap)
    Elazm_Map = Info8.GetMapControl_Engineer.GetBase_ElzamPlan()
    
    
 
 Try   
    	
        Dim coordinates As New List(Of List(Of List(Of Double)))
        Dim polygon As New List(Of List(Of Double))

        polygon.Add(New List(Of Double) From {48.2225397, 36.1125056})
        polygon.Add(New List(Of Double) From {48.2225397, 36.1125056})

        coordinates.Add(polygon)	
' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "WKT", WKT) : Exit Function
 
 	if Elazm_Map.any Then
 	
 '	For Each x as BIZ.SC.ClsMapControl.ElzamMap in Elazm_Map
 	
      	
		 
 		Dim Base_ElzamPlan as New DtoSC.Base_ElzamPlan
 		Dim ElazmPlan_MAP as New DtoSC.ElzamPlan_Map
 	'	Base_ElzamPlan = Base_ElzamPlan.where(Function (f) f.EumElzamPlanType=1).toList()
 		
		Base_ElzamPlan = Elazm_Map.where(Function (f) f.Base_ElzamPlan.EumElzamPlanType=1).Select(Function(f) f.Base_ElzamPlan).FirstOrDefault()
		ElazmPlan_MAP = Elazm_Map.where(Function (f) f.Base_ElzamPlan.EumElzamPlanType=1).Select(Function(f) f.ElzamPlan_Map).FirstOrDefault()
		
   If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
   '		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElazmPlan_MAP", ElazmPlan_MAP.NidElzamPlan.tostring)
   '		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElazmPlan_MAP", ElazmPlan_MAP.NidElzamPlan_Map.tostring)
   '    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Base_ElzamPlan", Base_ElzamPlan.NidElzamPlan.tostring) : Exit Function
   End If			
'		For Each item In Elazm_Map
'		    If item.Base_ElzamPlan IsNot Nothing AndAlso item.Base_ElzamPlan.EumElzamPlanType = 1 Then
'		        Base_ElzamPlan = item.Base_ElzamPlan  
'		        ElazmPlan_MAP = item.ElzamPlan_Map 
'		        Exit For      
'		    End If
'		Next		
 		
' 	if Base_ElzamPlan.any Then 
 	 		
    
	    'Dim ElazmPlan_MAP as New DtoSC.ElzamPlan_Map
	    'ElazmPlan_MAP = Elazm_Map(0).ElzamPlan_Map 
	
	'	E_mainPlan = Info8.GetMapControl_Engineer.GetBase_Using
	

	    
	    E_mainPlan.Type= Base_ElzamPlan.Type ' "FeatureCollection"
	    E_mainPlan.FileName= Base_ElzamPlan.FileName  '""
	    E_mainPlan.File=""
	       
	        Dim MainPlan_EstatMap as New Biz.Communication.geojson
	        
	        MainPlan_EstatMap.Type = Base_ElzamPlan.Type ' Elazm_Map(0).Base_ElzamPlan.Type
	        MainPlan_EstatMap.Name = ElazmPlan_MAP.Name  ' Elazm_Map(0).ElzamPlan_Map.Name   
	        		           
		        Dim MP_EstatMap_CoorDinateRF As New Biz.Communication.CoordinateReferenceSystem
		        MP_EstatMap_CoorDinateRF.Type=ElazmPlan_MAP.CoordinateReferenceSystemType  ' Elazm_Map(0).ElzamPlan_Map.CoordinateReferenceSystemType 
		        
			        Dim MP_EstatMap_CoorDinateRSP As New Biz.Communication.CoordinateReferenceSystemProperty
			        MP_EstatMap_CoorDinateRSP.Name=ElazmPlan_MAP.CoordinateReferenceSystemPropertiesName  'iif(String.isNullorEmpty(Elazm_Map(0).ElzamPlan_Map.CoordinateReferenceSystemPropertiesName),"EPSG:4326",Elazm_Map(0).ElzamPlan_Map.CoordinateReferenceSystemPropertiesName)    
		        
		        MP_EstatMap_CoorDinateRF.Properties= MP_EstatMap_CoorDinateRSP  
	    	        
	        MainPlan_EstatMap.CoordinateReferenceSystem=MP_EstatMap_CoorDinateRF	  
	                        
		       
	    
	    Dim NidElzamPlan_Map = ElazmPlan_MAP.NidElzamPlan_Map.tostring
	    
		Info8.GetMapControl_Engineer()._NidElzamPlan_Map = Guid.Parse(NidElzamPlan_Map.tostring) 
		
		Dim ElzamPlanMap_Features as New List (Of DtoSC.ElzamPlanMap_Features)	 
			 	
	    ElzamPlanMap_Features = Info8.GetMapControl_Engineer.GetElzamPlanMap_Features
	    
	    Dim Coordinates_New = ElzamPlanMap_Features(0).Geometrycoordinates	    
	    
	    ' Convert to LIst Feature
		    Dim result As New List(Of Double())()
		    
		    ' حذف فاصله‌های اضافی و جدا کردن با کاما
		    Dim parts = Coordinates_New.Split(","c)
		    
		    For i As Integer = 0 To parts.Length - 1 Step 2
		        ' پاک کردن براکت‌ها و فاصله‌ها
		        Dim latStr = parts(i).Replace("[", "").Replace("]", "").Trim()
		        Dim lonStr = parts(i + 1).Replace("[", "").Replace("]", "").Trim()
		        
		        ' تبدیل به Double
		        Dim lat As Double = Double.Parse(latStr, System.Globalization.CultureInfo.InvariantCulture)
		        Dim lon As Double = Double.Parse(lonStr, System.Globalization.CultureInfo.InvariantCulture)
		        
		        result.Add(New Double() {lat, lon})
		    Next   
		       
	        
		        Dim MP_EstatMap_Feature As New List(Of Biz.Communication.Feature)
		        
		        '	For i as Integer = 0 to 1
			            Dim Feature_tmp1 As New BIZ.Communication.Feature
			            Feature_tmp1.Type="Feature"
			            Feature_tmp1.Properties="J1WD25KLC2"
			            
					        Dim MP_EstatMap_Features_Geometry as New Biz.Communication.Geometry
					        MP_EstatMap_Features_Geometry.type="Polygon"
					        MP_EstatMap_Features_Geometry.coordinates = result   'coordinates  'coordinates'   {centerX + "," + CenterY}  '{48.2225397,36.1125056}   'New Object	
					       ' MP_EstatMap_Features_Geometry.coordinates = coordinates  'coordinates'   {centerX + "," + CenterY}  '{48.2225397,36.1125056}   'New Object	
					              
			            Feature_tmp1.Geometry = MP_EstatMap_Features_Geometry
			            
			            MP_EstatMap_Feature.Add(Feature_tmp1)
			    '     Next   		        		

	        MainPlan_EstatMap.Features=MP_EstatMap_Feature
	    
		E_mainPlan.EstateMap = MainPlan_EstatMap  
'	Next	  
    End if   
Catch e as exception 
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElzamMainPlan", e.message)
    Exit Function   
End Try 
    
         Return E_mainPlan
end function

Private Function ElzamUnitPlan_Old() As List(Of Biz.Communication.ElzamUnitPlan) 
Try  

	 Dim E_UnitPlanList as New List(Of Biz.Communication.ElzamUnitPlan) 

	Dim BaseUsing as New List (Of DtoSC.Base_Using)
	
       Dim coordinates As New List(Of List(Of List(Of Double)))
       Dim polygon As New List(Of List(Of Double))

        polygon.Add(New List(Of Double) From {48.222508, 36.1134061})
        polygon.Add(New List(Of Double) From {48.2225397, 36.1125056})
        coordinates.Add(polygon)
	
	   
        Dim UnitPlansTMP as New Biz.Communication.ElzamUnitPlan
        
        UnitPlansTMP.BlockNo=1
        UnitPlansTMP.ClassNo=1
        UnitPlansTMP.EstateUnitNo=1
        UnitPlansTMP.Type="UNIT"
        UnitPlansTMP.FileName="PMF6A1U2HJ.pdf"
        UnitPlansTMP.File="VEVTVCBVTklUIEZJTEU="
               
           
            Dim UnitPlans_UnitMap as New Biz.Communication.geojson
        
                Dim UP_UnitMap_Features_Geometry as New Biz.Communication.Geometry
                UP_UnitMap_Features_Geometry.type="Polygon"
                UP_UnitMap_Features_Geometry.coordinates= coordinates 'New Object
        
                Dim UP_UnitMap_Feature As New List(Of Biz.Communication.Feature)
                    Dim Feature_tmp as New Biz.Communication.Feature
                        Feature_tmp.Type="Feature"
                        Feature_tmp.Properties="PMF6A1U2HJ"
                        Feature_tmp.Geometry = UP_UnitMap_Features_Geometry
                UP_UnitMap_Feature.Add(Feature_tmp)

                Dim UP_UnitMap_CoorDinateRSP As New BIZ.Communication.CoordinateReferenceSystemProperty
                UP_UnitMap_CoorDinateRSP.Name="EPSG:4326"
           
                Dim UP_UnitMap_CoorDinateRF As New BIZ.Communication.CoordinateReferenceSystem
                UP_UnitMap_CoorDinateRF.Type="Feature"
                UP_UnitMap_CoorDinateRF.Properties= UP_UnitMap_CoorDinateRSP  
        
            UnitPlans_UnitMap.Type="FeatureCollection"
            UnitPlans_UnitMap.Name="PMF6A1U2HJ"
            UnitPlans_UnitMap.CoordinateReferenceSystem=UP_UnitMap_CoorDinateRF
            UnitPlans_UnitMap.Features=UP_UnitMap_Feature
            
            
        UnitPlansTMP.UnitMap = UnitPlans_UnitMap     
  
    	E_UnitPlanList.Add(UnitPlansTMP)
    	
    	Return E_UnitPlanList
Catch e as exception 
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElzamUnitPlan", e.message)
    Exit Function   
End Try     	
End function

Private Function ElzamUnitPlan() As List(Of Biz.Communication.ElzamUnitPlan) 
Try  

 	Dim E_UnitPlanList as New List(Of Biz.Communication.ElzamUnitPlan) 

	Dim BaseUsing as New List (Of DtoSC.Base_Using)

	BaseUsing = Info8.GetMapControl_Engineer.GetBase_Using
	Dim BNumber = BaseUsing.Select(Function(f) f.BuildingNo).Distinct().Count()		'شماره بلوک



   ' دریافت اطلاعات سربرگ نقشه در فرم تقسیم نامه    =============================      
   	
	    Dim Elazm_Map as New list (Of BIZ.SC.ClsMapControl.ElzamMap)
	    Elazm_Map = Info8.GetMapControl_Engineer.GetBase_ElzamPlan()	
	    
			
   '    Dim coordinates As New List(Of List(Of List(Of Double)))
   '    Dim polygon As New List(Of List(Of Double))

   '     polygon.Add(New List(Of Double) From {48.222508, 36.1134061})
   '     polygon.Add(New List(Of Double) From {48.2225397, 36.1125056})
   '     coordinates.Add(polygon)
	
	   
        Dim UnitPlansTMP as New Biz.Communication.ElzamUnitPlan
        
        
        For Each x as BIZ.SC.ClsMapControl.ElzamMap in Elazm_Map
        
		 	Dim Base_ElzamPlan as New DtoSC.Base_ElzamPlan
		 	Dim ElazmPlan_MAP as New DtoSC.ElzamPlan_Map		 	
			
			if x.Base_ElzamPlan.EumElzamPlanType = 1 Then 
				Continue For
			Else
				
		        Base_ElzamPlan = x.Base_ElzamPlan  
		        ElazmPlan_MAP = x.ElzamPlan_Map 
		       
			End if
			
   If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
   '    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "BlockNo", Base_ElzamPlan.BlockNo)
   '    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ClassNo", Base_ElzamPlan.ClassNo)
   '    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "EstateUnitNo", Base_ElzamPlan.EstateUnitNo)
   '    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "FileName", Base_ElzamPlan.FileName)
   '    Continue For 
   End If					
			
			
	    	Info8.GetMapControl_Engineer()._NidElzamPlan_Map = Guid.Parse(ElazmPlan_MAP.NidElzamPlan_Map.tostring) 
	    	
	    	Dim ElzamPlanMap_Features as New List (Of DtoSC.ElzamPlanMap_Features)	 
	    		 	
		    ElzamPlanMap_Features = Info8.GetMapControl_Engineer.GetElzamPlanMap_Features
		    
		    Dim Coordinates_New = ElzamPlanMap_Features(0).Geometrycoordinates
		    
		    ' Convert to LIst Feature
			    Dim result As New List(Of Double())()
			    
			    ' حذف فاصله‌های اضافی و جدا کردن با کاما
			    Dim parts = Coordinates_New.Split(","c)
			    
			    For i As Integer = 0 To parts.Length - 1 Step 2
			        ' پاک کردن براکت‌ها و فاصله‌ها
			        Dim latStr = parts(i).Replace("[", "").Replace("]", "").Trim()
			        Dim lonStr = parts(i + 1).Replace("[", "").Replace("]", "").Trim()
			        
			        ' تبدیل به Double
			        Dim lat As Double = Double.Parse(latStr, System.Globalization.CultureInfo.InvariantCulture)
			        Dim lon As Double = Double.Parse(lonStr, System.Globalization.CultureInfo.InvariantCulture)
			        
			        result.Add(New Double() {lat, lon})
			    Next
			    
			    		    		    		    									   	        
	        UnitPlansTMP.BlockNo=Base_ElzamPlan.BlockNo
	        UnitPlansTMP.ClassNo=Base_ElzamPlan.ClassNo
	        UnitPlansTMP.EstateUnitNo=Base_ElzamPlan.EstateUnitNo
	        UnitPlansTMP.Type="UNIT"
	        UnitPlansTMP.FileName=Base_ElzamPlan.FileName
	        UnitPlansTMP.File="VEVTVCBVTklUIEZJTEU="	'فایل نقشه 
               
           
	            Dim UnitPlans_UnitMap as New Biz.Communication.geojson
	        
	                Dim UP_UnitMap_Features_Geometry as New Biz.Communication.Geometry
	                UP_UnitMap_Features_Geometry.type="Polygon"
	                UP_UnitMap_Features_Geometry.coordinates= result  'New Object
	        
	                Dim UP_UnitMap_Feature As New List(Of Biz.Communication.Feature)
	                    Dim Feature_tmp as New Biz.Communication.Feature
	                        Feature_tmp.Type="Feature"
	                        Feature_tmp.Properties="PMF6A1U2HJ"
	                        Feature_tmp.Geometry = UP_UnitMap_Features_Geometry
	                UP_UnitMap_Feature.Add(Feature_tmp)
	
	                Dim UP_UnitMap_CoorDinateRSP As New BIZ.Communication.CoordinateReferenceSystemProperty
	                UP_UnitMap_CoorDinateRSP.Name="EPSG:4326"
	           
	                Dim UP_UnitMap_CoorDinateRF As New BIZ.Communication.CoordinateReferenceSystem
	                UP_UnitMap_CoorDinateRF.Type="Feature"
	                UP_UnitMap_CoorDinateRF.Properties= UP_UnitMap_CoorDinateRSP  
	        
	            UnitPlans_UnitMap.Type="FeatureCollection"
	            UnitPlans_UnitMap.Name=Base_ElzamPlan.FileName
	            UnitPlans_UnitMap.CoordinateReferenceSystem=UP_UnitMap_CoorDinateRF
	            UnitPlans_UnitMap.Features=UP_UnitMap_Feature
                        
        	UnitPlansTMP.UnitMap = UnitPlans_UnitMap     
  
    		E_UnitPlanList.Add(UnitPlansTMP)
    	Next
    	
    	Return E_UnitPlanList
Catch e as exception 
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElzamUnitPlan", e.message)
    Exit Function   
End Try     	
End function

Private Function ElzamBlockSection() As List(Of Biz.Communication.ElzamBlock)
	
	Dim BaseUsing , BaseUsingTmp, BaseUsingTmp2 as New List (Of DtoSC.Base_Using)
	Dim ELzamBlockList as New List(Of Biz.Communication.ElzamBlock)	
Try
'	BaseUsing = Info8.GetParvandeh_Block().GetBase_Using
	BaseUsing = Info8.GetMapControl_Engineer.GetBase_Using


Dim i as Integer = 0
Dim j as Integer = 0

	

Dim StructType as String = BaseUsing.Select(Function(f) f.BuildingNo).Distinct().Count()

Dim CenterX , CenterY as String
Dim TmpParent,Parent As New list(of BIZ.SA.ClsNosaziCodeType)	
	
Parent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.Getrequest.info.nidNosaziCode,0)		
TmpParent = Parent.Where(Function(f) f.EumNosaziCodeObjType =2).toList

if TmpParent.any Then
	Dim strnosazicode = Info8.GetFunctions.GetNosaziNickName(TmpParent(0).NidNosaziCode)
	
	Dim MapF As New Biz.SC.ClsSettings_Map
	Dim codeMapF As New Biz.SC.SrvMap.MapService_SrvFindResult
	codeMapF = Info8.GetMap.GetMapLatAndLong(MapF, strnosazicode, False)
	
		
		CenterX=codeMapF.CenterX.tostring
		CenterY=codeMapF.CenterY.tostring
Else
	   CenterX = "0"   
	   CenterY = "0"   
End if	   

   

	if BaseUsing.count > 0 Then 


	
		Dim BNumber = BaseUsing.Select(Function(f) f.BuildingNo).Distinct().Count()
		Dim FloorCount = BaseUsing.Select(Function(f) f.FloorNo).Distinct().Count()
		
 		
		
    	Try
    		    								
			For j=0 to  BNumber-1
			
		
		'	For Each x as DtoSC.Base_Using in BaseUsing    		
			    
			    Dim ElzamBlock As New Biz.Communication.ElzamBlock
			        ElzamBlock.BlockNo= j
			        ElzamBlock.BlockName=""
			        ElzamBlock.StructureType = 1 'BaseUsing(j).CI_SazehType
			        ElzamBlock.Limitation= BaseUsing(j).BuildingLimitation
			        					        	   
			        
					    Dim ElzamClassList as New List(Of Biz.Communication.ElzamApprovePlanClass)
					   ' Dim BNumber as Integer = CInt(x.BuildingNo)
					   ' if BNumber = 0 Then  BNumber = 1
					   
					    
					    For i=0 to FloorCount - 1
					        Dim ElzamClass as New Biz.Communication.ElzamApprovePlanClass
					        
					         
					          
					        ElzamClass.ClassNo = i
					        ElzamClass.ClassUnitNo = BaseUsing.where(Function (f) f.FloorNo = i).count
					        ElzamClass.ClassArea = BaseUsing.where(Function (f) f.FloorNo = i).Sum(Function (f) f.UsingArea)
					        
					       
					        	
					        	Dim ElzamJointList as New List(Of Biz.Communication.ElzamJoint)
							    Dim ElzamStateUnitList as New List(Of Biz.Communication.ElzamEstateUnit)
							    Dim UnitNumber as Integer = CInt(ElzamClass.ClassUnitNo)
							    
							    BaseUsingTmp = BaseUsing.where(Function (f) f.FloorNo = i).tolist
							    BaseUsingTmp2 = BaseUsing.where(Function (f) f.FloorNo = i And Not{401}.contains(f.CI_UsingType)).tolist
							    
							   '  Info8.AddError(BIZ.SA.EumErrorAction.Stop,BaseUsingTmp2.count ,BaseUsingTmp.count) : Exit Function
							   
							   		
							    
							    For Each x as DtoSC.Base_Using in BaseUsingTmp
							    
							    	Dim CIUsingType as New DtoSC.CI_UsingType
							    	CIUsingType = Info8.GetFunctions.GetCI_UsingType(x.CI_UsingType)
							    	
							        Dim ElzamStateUnit As New Biz.Communication.ElzamEstateUnit
							        ElzamStateUnit.ClassNo= x.FloorNo
							        ElzamStateUnit.EstateUnitNo= x.UnitNo
							        ElzamStateUnit.UnitArea= x.UsingArea  'WOW
							        ElzamStateUnit.Usage= CIUsingType.Elzam_ID     'ONLINE
							        ElzamStateUnit.Limitation= x.Limitation
							        
							    	ElzamStateUnitList.Add(ElzamStateUnit)
							    	
								'////////////////////////////   بخش ملضمات
									if BaseUsingTmp2.count > 0 Then 
								        Dim ElzamJoint As New Biz.Communication.ElzamJoint
								        ElzamJoint.BlockNo= iif(j=0,1,j+1)	'N
								        ElzamJoint.Code="1"   'Online
								        ElzamJoint.Class="1"
								        ElzamJoint.Area=x.UsingArea
								        ElzamJoint.Usage=CIUsingType.Elzam_ID 	'online
								        ElzamJoint.Sector = x.UnitNo
								        ElzamJoint.Limitation=x.Limitation  'GPS
								        							        
							    		ElzamJointList.Add(ElzamJoint)	
							    	End if		        
					        	Next
					        
					        ElzamClass.EstateUnits=ElzamStateUnitList
					        ElzamClass.Joint=ElzamJointList
					        ElzamClassList.Add(ElzamClass)
					        			        
			        	Next
			        	
			        	i = 0
			        
			        ElzamBlock.Class=ElzamClassList
			
				ELzamBlockList.Add(ElzamBlock)
			Next	
		Catch 
		
		End Try
    Else
    
    End if 
    
    Return ELzamBlockList
    
Catch e as exception 
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElzamBlockSection", e.message)
    Exit Function   
End Try     
    
    Return ElzamBlockList
    
End Function
    
Private Function ElzamOwnerSection() As List(Of Biz.Communication.ElzamOwnerInfo)


    '======================== Father Name Section
    Dim BaseOwner as New List (Of DtoSC.Base_Owner) 

    BaseOwner = Info8.GetMapControl_Engineer.GetBase_Owner
  
	if BaseOwner.count <=0 Then   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "مالکین","مشخصات مالکین لود نشده است") : Exit Function  
        
    Dim FatherName as String = ""
                    
    '======================== Address Section
    
    	Dim Address as String
   Try 
    	Address = Info8.GetRevisit_House.GetBase_AddressInfo.MainAddress
	Catch
   		Address = Info8.GetRevisit_HouseSharing.GetBase_AddressInfo.MainAddress
    End Try	
    
    Dim ReuesterNumber = Info8.GetRequest().Info.Cellphone.ToString()
    
    Dim OwnerInfoList As New List(Of Biz.Communication.ElzamOwnerInfo)
    
    
Try    
	if BaseOwner.count > 0 Then 
		For Each x as  DtoSC.Base_Owner in BaseOwner		
        Dim Fname, Mobail, Lname, code, BDay, uniqueidentifier, Type As String
        
            Fname = x.OwnerName    
            Lname = x.OwnerLastName
            Mobail = iif(String.isNullOrEmpty(x.CellPhone),"",x.CellPhone)
            code = x.NationalCode
          
          '  BDay = x.BirthDate   
			FatherName = iif(String.isNullOrEmpty(x.OwnerFatherName),"",x.OwnerFatherName)
			
			uniqueidentifier = iif(String.isNullOrEmpty(x.uniqueidentifier.tostring),"",x.uniqueidentifier) ' 
			
		'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "L",Lname) : Exit Function  
			
			Type = x.EumOwnerCharacter
			 
    
	        Dim Owner As New Biz.Communication.ElzamOwnerInfo
	        Owner.Type=Type
	        Owner.NationalNo=code
	        Owner.Name=Fname
	        Owner.Family=Lname
	        Owner.BirthDate="1300/01/01"
	        Owner.FatherName=FatherName
	        Owner.OwnersAddress=Address
	        Owner.ElectronicEstateNoteNo=uniqueidentifier
	        Owner.ContactNo=Mobail
	    	OwnerInfoList.Add(Owner)
	    Next	
	 End if   
	 
	 if OwnerInfoList.any(Function (f) String.isNullOrEmpty(f.ContactNo)) Then
	 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElzamOwnerSection", "شماره مالکین وارد نشده است")
	    Exit Function 	 
	 End if
	 
    Return OwnerInfoList
    
Catch e as exception 
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ElzamOwnerSection1", e.message)
    Exit Function   
End Try     
    
End Function

Private function GetLastParvandehInfoFunc 

Try

	Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	TmpListReq_N= Info8.GetRequest.GetAllRequestList(Info8.Getrequest.info.NidNosaziCode)
	
	if TmpListReq_N.count > 0 Then
	
	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "A2",TmpListReq_N(0).NidWorkItem) : Exit Function 
		 
		TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus=2 And _
					 {"2635fe18-fe4c-4a2d-9d05-993756751262","8d20be44-78ab-4001-bb94-f064d22b9ce1","097C8C41-BA8C-4B60-8B2F-6BED0221F573" _
					  ,"e7a80e33-172e-4e3c-8ba5-88a5eb69f146"}.contains(Lcase(f.NidWorkFlowDeff.tostring))).tolist 'جاری	
										  
		if TmpListReq_N.count > 0 Then
		
			Dim NidWorkItem, TaeedModirDate, TaeedModirTime as String
		
			TmpListReq_N = TmpListReq_N.OrderByDescending(Function (f) f.RequestDate).tolist
			NidWorkItem = TmpListReq_N(0).NidWorkItem.tostring
			
			Dim ListMangerConfim As New BIZ.SC.ClsManagerConfirm
			ListMangerConfim.LoadObj(TmpListReq_N(0).nidProc)
			
			 Dim ListMangerConfimTaeed As New List(Of DtoSC.Sh_ManagerConfirm)
			 ListMangerConfimTaeed = ListMangerConfim.GetSh_ManagerConfirm_List()
			 if ListMangerConfimTaeed.count > 0 Then
			 	ListMangerConfimTaeed = ListMangerConfimTaeed.where(Function (f) f.CI_ResourceManagerConfirm = 3 And f.EumManagerConfirmLicence=1).tolist
			 	if ListMangerConfimTaeed.count > 0 Then 
			 		ListMangerConfimTaeed = ListMangerConfimTaeed.OrderByDescending(Function(f) f.ConfirmationDate).ToList	 
			 		TaeedModirDate = ListMangerConfimTaeed(0).ConfirmationDate.tostring
			 		TaeedModirTime = ListMangerConfimTaeed(0).ConfirmationTime.tostring
			 	End if
			 End if
			 
			
			
			Return New Tuple(Of String, String, String)(NidWorkItem, TaeedModirDate, TaeedModirTime)
		End if
			Return New Tuple(Of String, String, String)("12345678", "1405/03/13", "12:12")
	End IF								  


Catch e as exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "GetLastParvandehInfoFunc",e.message) : Exit Function 
End Try

End function

Private Sub Tamin  
Try    
    '=============  Base_Owner
    Dim BuildOwner As New List(Of BIZ.Communication.ClsTaminConstructionOwnerInputDto)
	Dim BaseOwner as New List (Of DtoSC.Base_Owner) 
	
    Dim plaq As New List(Of DtoSC.Base_RegisterPlack)
    Dim MainPlaq, SecPlaq As String	
    Dim UniqueID as String
	

	Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
	TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))	
	
'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "UniqueID", Info8.GetRequest.Info.NidNosaziCode.tostring) : Exit Sub  
	
	If TmpNosazi.EumNosaziCodeObjType = 5 Then
		Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
	    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    BaseOwner = Info8.GetRevisit_HouseSharing.GetBase_Owner	
	    UniqueID = Info8.GetRevisit_HouseSharing.GetBase_NosaziCode().UniqueID
	    plaq = Info8.GetRevisit_HouseSharing.GetBase_RegisterPlack
	    
	ElseIf TmpNosazi.EumNosaziCodeObjType = 2 Then
	    Info8.ParameterList.Remove("Revisit_House_NidBase")
	    Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    BaseOwner = Info8.GetRevisit_House.GetBase_Owner
	    UniqueID = Info8.GetRevisit_House.GetBase_NosaziCode().UniqueID
	    plaq = Info8.GetRevisit_House.GetBase_RegisterPlack
	    	
	ElseIf TmpNosazi.EumNosaziCodeObjType = 3 Then   
		Dim LstNosaziCode_T As New List(Of Global.Biz.SA.ClsNosaziCodeType)
		LstNosaziCode_T = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode, 0)
	    Dim LstNosaziCode_B As New List(Of Global.Biz.SA.ClsNosaziCodeType)	 
	    LstNosaziCode_B = LstNosaziCode_T.Where(Function(f)  f.EumNosaziCodeObjType = 2).ToList
	    Info8.ParameterList.Remove("Revisit_House_NidBase")
	    Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(LstNosaziCode_B(0).NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    BaseOwner = Info8.GetRevisit_House.GetBase_Owner	    
	    plaq = Info8.GetRevisit_House.GetBase_RegisterPlack	    
	    
	    UniqueID = Info8.GetRevisit_Building.GetBase_NosaziCode().UniqueID
    End if	  
    
	    
	    if BaseOwner.any Then   
	    	For Each x as DtoSC.Base_Owner in BaseOwner
		        Dim TaminOwner As New BIZ.Communication.ClsTaminConstructionOwnerInputDto
		        
		        TaminOwner.nationalCode = x.NationalCode
		        TaminOwner.name=x.OwnerName 
		        TaminOwner.lastName=x.OwnerLastName
		        TaminOwner.mobile=iif(String.isNullOrEmpty(x.CellPhone),"",x.CellPhone)
		        TaminOwner.ownerType=x.EumOwnerCharacter
		        
		      BuildOwner.Add(TaminOwner)
		    
		    Next
		    
		 End if   

		 
		' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "BaseOwner", BaseOwner.count.tostring) : Exit Sub    
		
		 
		'============   plak 
                                    
        
        If plaq.Any Then            	
            For Each X As DtoSC.Base_RegisterPlack In plaq
                If X.CI_RegisterPlackType = 1 Then MainPlaq = X.RegisterPlackTypeValue.ToString
                If X.CI_RegisterPlackType = 2 Then SecPlaq = X.RegisterPlackTypeValue.ToString
            Next
        Else
            MainPlaq = "0"
            SecPlaq = "0"
        End If 		
        
        	Dim tmpSh_WorkflowList As New List(of DtoSA.Sh_NewWorkflow_SA)
        	Dim Eum_groupKind as Integer
		   	tmpSh_WorkflowList = Info8.GetWorkFlow.GetSh_NewWorkflowList   	
		   	Dim p_WorkflowGuid As String = Info8.GetRequest.Info.NidWorkFlowDeff.ToString
		   	
		    If tmpSh_WorkflowList.Any Then        
        		tmpSh_WorkflowList = tmpSh_WorkflowList.where(Function (f) f.NidWorkFlowDeff.tostring = p_WorkflowGuid).tolist
        		if tmpSh_WorkflowList.any Then Eum_groupKind = tmpSh_WorkflowList(0).EumShahrsaziArchiveGroup
         	End if
         	
         	
         	Dim fondation , buildingprominence, article100, fencescharges as Decimal 
         	fondation = 0
         	buildingprominence = 0
         	article100 = 0
         	fencescharges = 0
         	
      		Dim IncomeCalculation as New List(of DtoSC.Income_Calculation)
      		IncomeCalculation = Info8.GetIncome.GetIncome_Calculation_InNidIncome(Info8.GetRequest.Info.ActiveNidIncome) 
		       		if IncomeCalculation.any Then
			       		For Each x as DtoSC.Income_Calculation in IncomeCalculation
			       			if Eum_groupKind = 1 Then
			       				if x.CI_IncomeCalculation = 1267 Then fondation += x.SysValue
			       				if x.CI_IncomeCalculation = 1268 Then fondation += x.SysValue
			       				if x.CI_IncomeCalculation = 1270 Then fondation += x.SysValue
			       				if x.CI_IncomeCalculation = 1272 Then fondation += x.SysValue
			       				if x.CI_IncomeCalculation = 1272 Then buildingprominence = x.SysValue
			       			Else
			       				if x.CI_IncomeCalculation = 1269 Then article100 += x.SysValue
			       				if x.CI_IncomeCalculation = 1271 Then article100 += x.SysValue
			       				if x.CI_IncomeCalculation = 1273 Then article100 += x.SysValue
			       				if x.CI_IncomeCalculation = 1275 Then article100 += x.SysValue
			       				if x.CI_IncomeCalculation = 1277 Then article100 += x.SysValue		       			
			       			
			       			End if
			       			
			       				if x.CI_IncomeCalculation = 1276 Then fencescharges = x.SysValue 
			       		Next	
		       		End if	     
		       		
		'  Dim NidNosaziCode = Info8.Getrequest.info.NidNosaziCode    
		'  Dim NidRevisit = Info8.Getrequest.info.NidRevisit   		 	
		'  Dim Nidbase = Info8.GetFunction.GetNidBase(NidNosaziCode, 1,3,NidRevisit)   
		  
		  Dim BaseUsing as New list (Of DtoSc.Base_Using)
		  BaseUsing = Info8.GetMovafeghatOsooli.GetBase_Using		 

	 


    Dim buildRequest As New BIZ.Communication.ClsTaminConstructionRequestInputDto
    buildRequest.buildRequestNo=Info8.GetRequest.info.NidWorkItem.tostring
    buildRequest.buildFileNo=UniqueID.tostring
    
    buildRequest.mainPelak=MainPlaq
    buildRequest.partPelak="0"
    buildRequest.subPelak=SecPlaq.tostring 
    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", fondation) : Exit Sub
    buildRequest.fondation= CInt(fondation)
    buildRequest.buildingProminence=CInt(buildingprominence)    
    buildRequest.densityFinance=0
    buildRequest.optimalCharges=0
    buildRequest.postulate=0
    buildRequest.block=0
    buildRequest.estate=0
    buildRequest.apartment=0
    buildRequest.trade=0
    buildRequest.thirdItemCharges=0
    buildRequest.fencesCharges=CInt(fencescharges)
    buildRequest.article100=CInt(article100)
    buildRequest.metrage= CInt(BaseUsing.sum(Function (f) f.UsingArea))
    
 
    
    Dim TaminBody As New BIZ.Communication.ClsTaminConstructionInputDto 
    TaminBody.workshopAddress="test-testy"
    TaminBody.provinceId="8590"
    TaminBody.postCode=""
    TaminBody.character = "01"
    TaminBody.legalId=""
    TaminBody.buildOwner=BuildOwner
    TaminBody.buildRequest=buildRequest
    
 
  '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "metrage", buildRequest.metrage) 
  '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "fencescharges", fencescharges.tostring) 
  '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "article100", article100.tostring) 
  '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "fondation", fondation.tostring) : Exit Sub
    
    
    Dim pNidproc = Info8.GetRequest().Info.NidProc.ToString()
    Dim pNidnosaziCode = Info8.GetRequest().Info.NidNosaziCode.ToString()
    Dim ClsTamin As New BIZ.Communication.ClsTamin

    Try
        
        Dim TaminSend = ClsTamin.TaminConstructionSiteDebtRegistration(TaminBody,pNidnosaziCode,pNidproc)
        if ClsTamin.ErrorResult.BizErrors.count > 0 Then
            Info8.AddError(BIZ.SA.EumErrorAction.Stop,"Error-Tamin","پرونده به وبسرویس تامین اجتماعی ارسال نشد - " & ClsTamin.ErrorResult.BizErrors(0).ErrorTitel)
        Else
        	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"تامین اجتماعی - کد رهگیری",TaminSend)    
        End If
    
    Catch e as Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop,"Tamin",e.Message)
    End Try  
    
Catch e as Exception
    Info8.AddError(BIZ.SA.EumErrorAction.Stop,"Tamin-Func",e.Message)
End Try       
End Sub

Private function GetCreatedTaskDate(ByVal NIdNod as String) as String

	Try
	
		Dim TaskByNidProcResponse As New Global.Biz.SC.ClsNewWorkFlow.ClsGetTaskByNidProcResponse
		Dim strClose as String =""
		Dim OpStr as String = ""
		Dim CLStr as String = ""
	
		TaskByNidProcResponse=Info8.GetFunctions.WF_GetTaskByNidProc(Info8.Getrequest.info.NidProc.tostring)
		Dim counter as Integer=0
		Dim Colescounter as Integer=0
		Dim Opencounter as Integer=0
		Dim ik as Integer=0
		For ik = 0 To TaskByNidProcResponse.data.Count - 1
		    If TaskByNidProcResponse.data(ik).NidNode = NIdNod Then
		    	Return TaskByNidProcResponse.data(ik).TaskStartDate
		    Else
		    	Return "1405/01/01"
		    End if
		Next
			
		'	Return BIZ.SA.ClsCommon.CurrentShamsiDateString
	Catch e as exception
		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبار سنجی - GetCreatedTaskDate " , e.message) 
	End Try	

End function

Public Function ControlCheck ( ByVal NidCheckList as String) as Integer 

 '  0   پیدا نشد
 '1 تایید نشده 
 '2  تایید نشده مالک 
 '3 تایید شده 
 
 Try
	Dim Resultint as Integer =0
    Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
    LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.Getrequest.info.NidProc.tostring)
       Dim i as Integer
	   Dim j as Integer=0
	   Dim k as Integer=0
       if LstObj.data.count>0 Then 	       
		  	For i = 0 To LstObj.data.count - 1					
			   if ucase(LstObj.data(i).NidCheckList)= ucase(NidCheckList) Then
			         If String.IsNullOrEmpty( LstObj.data(i).CI_CheckListValue) Then
			             Resultint=1
			         ElseIf  LstObj.data(i).CI_CheckListValue <> 1 Then
							Resultint=1
							if LstObj.data(i).NeedToConfirm  Then
					            Dim OwnerConfirm as Boolean = 0
					            Try
					                OwnerConfirm= LstObj.data(i).OwnerConfirm
							 	Catch
									OwnerConfirm= 0
							 	End Try
							   if OwnerConfirm =0 Then   Resultint=2
							End if   
			         Else
			         	  Resultint=3
			              if LstObj.data(i).NeedToConfirm  Then
				            Dim OwnerConfirm as Boolean = 0
				            Try
				                OwnerConfirm= LstObj.data(i).OwnerConfirm
						 	Catch
								OwnerConfirm= 0
						 	End Try
						   if OwnerConfirm =0 Then   Resultint=2
								
						End if	
					End if 	
				 End if    		         
	       Next    
		 End if     
			    
	Return Resultint	    
Catch e as exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی - ControlCheck",e.message) 
	Exit Function
End Try			     
End Function

Public Function ExecSp_Made14(ByVal NidProc as guid)  as Boolean
 Try
'درخواست های شروع عملیاتی که مشمول ماده 14 قانون الزام هستند ( تاریخ صدور  پروانه آنها 1404/11/14 به بعد می باشد ) 
'در زمان تایید مدیر شروع عملیات ساختمانی کنترل گردد که حتما کد پیگیری فرآیند ماده 14 قانون الزام را گرفته باشد.
 
 	Dim NameSp as String = "[dbo].[GetTrackingCode]"
	Dim p_Connection As String = "Data Source=172.16.8.136;Initial Catalog=Sara8M03;Persist Security Info=True;User ID=esup92;Password=zxc@789-sf"
	Dim intRowsAffected As String = 0
	
' Info8.AddError(BIZ.SA.EumErrorAction.warning, "Ret", "2")	
	
	Dim con As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(p_Connection)
	
' Info8.AddError(BIZ.SA.EumErrorAction.warning, "Ret", "3")
 	
	Dim TextSms as String =""
	Dim cmd As New System.Data.SqlClient.SqlCommand(NameSp, con)
	cmd.CommandType = CommandType.StoredProcedure
	cmd.Parameters.AddWithValue("@NidProc", NidProc)
	cmd.Connection = con
	con.Open()
    Dim adp As New SqlDataAdapter(cmd)
    Dim ds As DataSet = New DataSet()

   
 '   Info8.AddError(BIZ.SA.EumErrorAction.warning, "Ret", "4")
   
    cmd.ExecuteNonQuery()
	adp.Fill(ds, NameSp)
	Dim BuildingGroupTitle As String 
	Try
		BuildingGroupTitle = ds.Tables(0).Rows(0).Item(1).ToString
	Catch 
		Return False
	End Try	
	con.Close()
	con = Nothing
	
	if String.IsNullOrEmpty(BuildingGroupTitle) Then Return False
	
	Return True

Catch e as exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Ret", e.message)
End Try		
End function	


Private function FireFighting_Func(ByVal TmpListManger As DtoSC.Sh_ManagerConfirm, ByVal CCount as Integer)  as String
   
Try  
   
   Dim RequestSubjectFire as Integer
   RequestSubjectFire=GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)
   RequestSubjectFire=0
Dim i as Integer
Dim Maskoni as Boolean 
Dim TmpFloor as Integer=0  
Dim groupNumbers As New List(Of Integer)


	if {ucase("f16712d1-9938-4692-ab59-3cd090d70b8f"),ucase("36d59b70-b2bc-4793-932e-7376877025e0")}.contains(ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)) Then
		'"درخواست های مفاصا حساب ارسال به آتش نشانی ارسال نمیشوند - sp 504734 " 
		Return ""
	End if

           	if  (Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="677FBEE4-B05C-4FA6-AB19-CE897739195A")  And  CCount > 0 Then
           '	 If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","aaaa") 
           	Else
           '	If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1","bbbb") 
           	'220880 '220880
                ' sp  406867
               if Info8.getRequest.Info.RequestDate >"1397/03/08"  And (TmpArchiveGroup=1873 or TmpArchiveGroup=1839 or TmpArchiveGroup=1881 or TmpArchiveGroup=1874 or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="36D59B70-B2BC-4793-932E-7376877025E0" ) Then TmpArchiveGroup=0
               '313755
         
               if TmpArchiveGroup =1871 or TmpArchiveGroup=1873 or TmpArchiveGroup=1839 or TmpArchiveGroup=1881 or TmpArchiveGroup=1874 or TmpArchiveGroup=1872 or TmpArchiveGroup=1877 or TmpArchiveGroup=1882 Then TmpArchiveGroup=0
                   
                   if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B6A28C29-6761-4719-8B62-C8575F88F572" or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="E530B5D8-6283-4E95-B655-6DB83377A6E7" or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C83F6228-987B-4B5E-8906-56F7CD87EA7C" or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="EE64A610-9142-4E2B-953F-6E65B522A9E1" or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" Or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" Or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" Or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="D84FA3D1-54FD-4D24-8000-758A300B2441" Or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="EE64A610-9142-4E2B-953F-6E65B522A9E1" Or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="CBAD978D-3764-4E51-903C-9A8831B18760" Or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="4351C20C-643A-4A50-A4BD-AEBCEC5EADF8" Or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B6A28C29-6761-4719-8B62-C8575F88F572" Or _
					  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="BDD563C8-1EB2-40EC-8811-19DB03908488"  Then '3074550 EE64A610-9142-4E2B-953F-6E65B522A9E1 
                   	   '387529 Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="3157211F-C484-48E9-B65C-6BF3271A9F15"
                   	   Msgbox("B3","B3")
                   	    
                   	   Dim ReturnFire as Integer=0
                   	   Dim tmp As New Global.Biz.Communication.ClsFireFighting
				       ReturnFire=0
				       Dim ApartmentTejari as Integer=1
				          '271542
				       Dim Lstu  As New List(Of DtoSC.Base_Using)
				       if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" _
				       or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" _
				       or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
				       or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
				       or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="BDD563C8-1EB2-40EC-8811-19DB03908488" _
				        Then
				        '387529 or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="3157211F-C484-48E9-B65C-6BF3271A9F15"
				       		    
				       			if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" _
				       			or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
				       			or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
				       			Then 
				       			'387529 or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="3157211F-C484-48E9-B65C-6BF3271A9F15" 
				       			
				       			'272109
				       				
						            If Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.Request.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).EumNosaziCodeObjType = 3 Then
										
										Lstu=Info8.GetRevisit_Building.GetBase_Using
									ElseIf Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.Request.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).EumNosaziCodeObjType = 5 Then
										Lstu=Info8.GetRevisit_HouseSharing.GetBase_Using
								
				       				ElseIf Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.Request.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000")).EumNosaziCodeObjType = 4 Then
				       					Lstu=Info8.GetRevisit_Apartment.GetBase_Using
				       				End if 
										       				
				       				
				       			Else
				       				Lstu=Info8.GetRevisit_Apartment.GetBase_Using
				       			End if 
				       		    
				       			Lstu=Lstu.where(Function (f) f.ci_usinggroup<>11).tolist
				       		   
				       			if Lstu.count>0 Then
				       				ApartmentTejari=1
				       			Else 
				       				ApartmentTejari=0
				       			End if
				       	'	Else
				       	'		ApartmentTejari=0
				       		End if
				       
				        
				          
				        
				       Dim ResualtFire2 As Biz.Communication.SrvFireFighting.resualt2
					
				        Try
				         
				       	 ReturnFire=tmp.GetInstanceState(Info8.GetRequest.Info.NidWorkItem.ToString,Info8.User.UserGuid,Info8.User.UserName.ToString)
				           
					    Catch ex As Exception
					     
							ReturnFire=0      
					    End Try
					    
					    Dim TmpCom as String =""
					 
							'sp 521050  sp 304696	'sp 303549 'sp 303530 sp 303018   'sp 289920  299201    'ghovati sp 299999 RE 3858162
				   if Info8.GetRequest.Info.NidworkItem ="3787284" or Info8.GetRequest.Info.NidworkItem ="3760788" or Info8.GetRequest.Info.NidworkItem ="3759776" or Info8.GetRequest.Info.NidworkItem ="3760757" _
				   or Info8.GetRequest.Info.NidworkItem ="3776986" or Info8.GetRequest.Info.NidworkItem ="3858162" or Info8.GetRequest.Info.NidworkItem ="3877581" or Info8.GetRequest.Info.NidworkItem ="3893024" _
				   or Info8.GetRequest.Info.NidworkItem ="3894554" or Info8.GetRequest.Info.NidworkItem ="3899454" or Info8.GetRequest.Info.NidworkItem ="3880950" or Info8.GetRequest.Info.NidworkItem ="10229856" _
				   or Info8.GetRequest.Info.NidworkItem = "10976836"  Then ReturnFire=2
					   
					   
					  if Info8.GetRequest.Info.NidworkItem ="2830546" Then 
					  	TmpCom="مطابق سمپا 512608 آتش نشانی بررسی نشود"
					  	ReturnFire= 2
					  	
					  	StrComments =TmpListManger.Comments & "توجه : " & TmpCom & "#" &  ReturnFire
					  	
					  Else 
						    
						    if ReturnFire= 1 Then 
								TmpCom="پرونده در اتش نشانی در حال پیگیری می باشد"
							ElseIf ReturnFire= 2 Then 
								TmpCom="پرونده در اتش نشانی تایید شده است "
							ElseIf ReturnFire= 3 Then 
								TmpCom="پرونده در اتش نشانی رد شده است "
						   ' ElseIf ReturnFire= 5 Then 
							'	TmpCom="پرونده در حال پیگیری می باشد"
							Else
								TmpCom="شماره پرونده در اتش نشانی پیدا نشد"
							End if
							
						StrComments =TmpListManger.Comments & "مقدار ارسال شده از اتش نشانی : " & TmpCom & "#" &  ReturnFire	
							
					   End if	
							
						
					
					' 295096	TmpListManger.Comments =TmpListManger.Comments & "مقدار ارسال شده از اتش نشانی : " & TmpCom & "#" &  ReturnFire
						'	msgbox("ReturnFire:",ReturnFire)    
					   ' 265461	if ReturnFire=0   Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*توجه*", "برای پرونده پاسخی از اتش نشانی دریافت نشد") :  Exit Function 
					  
				' 295096	  	if ReturnFire=0   Then 
				' 295096	  	    if (Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" _
				' 295096	  	    or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" or  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F") Then
				' 295096	       		 if  ApartmentTejari=1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*توجه*", "*برای پرونده پاسخی از اتش نشانی دریافت نشد") :  Exit Function 
				' 295096	      Else
				' 295096	  	      Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*توجه*", "برای پرونده پاسخی از اتش نشانی دریافت نشد") :  Exit Function 
				' 295096	  	    End if   
				' 295096	  	End if
					   '******************** amin 
					      
					
				' 295096		if ReturnFire = 1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*توجه *", "پرونده در حال پیگیری در اتش نشانی می باشد تا پاسخ اتش نشانی اجازه تایید ندارید " & "#" &  ReturnFire ) :  Exit Function 
				'220880	or ReturnFire = 5
				'226714	
				   ' msgbox("d:","AA")'or ReturnFire = 4
				  
				  ' 295096      if  (ReturnFire = 3 ) And (Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" or  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
				   ' 295096     or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="EE64A610-9142-4E2B-953F-6E65B522A9E1" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" _
				   ' 295096     or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "پرونده در اتش نشانی تایید نشد اجازه تایید ندارید ") :  Exit Function
						
					
						Dim TmpAllFiche_Tmp As New Global.Biz.SC.ClsDto_IncomeConfirmFiche
			            Dim TmpAllFiche As New  List(Of DtoSC.Income_Fiche)
			            Dim TmpAllFiche_F As New  List(Of DtoSC.Income_Fiche)
			            Info8.Income_Fiche=Nothing
			            TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetManagerConfirm.Sh_ManagerConfirm.NidProc)
			            TmpAllFiche_F = TmpAllFiche.where( Function(f) f.CI_IncomeAccountGroup=129  ).tolist  
			            
			            TmpAllFiche = TmpAllFiche.where( Function(f) f.CI_IncomeAccountGroup=129 And(f.EumIncomePaymentType<> 1 or f.Payable=0) ).tolist  
			 	   ' msgbox("e1:",TmpAllFiche.count)
			 	   ' msgbox("e2:",ApartmentTejari)
			 	    ' msgbox("e3:",ReturnFire)
			 	    '313755 if TmpAllFiche.count =0 And TmpAllFiche_F.count>0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*توجه *" , "فیش تایید شده اتش نشانی ندارد") :  Exit Function 
			 	  '313755   TmpAllFiche_Tmp = Info8.GetConfirmIncomeFicheParameter.IncomeConfirmFiche
			 	    
			 	    	   
			 	    	if (TmpAllFiche.count> 0 or ApartmentTejari=1)  Then ' 295096 And ReturnFire<>2
 
							Dim tmpBase_JobInfo_Fire as  New list(of DtoSC.Base_JobInfo)
							if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" _
							or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" _
							or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
							or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
							or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="BDD563C8-1EB2-40EC-8811-19DB03908488" _
							Then
							 '387529	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="3157211F-C484-48E9-B65C-6BF3271A9F15" 
						
						
				  			Else 
				  		
								Info8.ParameterList.REMove("Revisit_Shop_NidBase")
								    
								Info8.ParameterList.Add("Revisit_Shop_NidBase",Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Info8.Request.Info.NidRevisit).tostring())
								 Dim tmStr As String
			 					Dim tmpClsNidLib As BIZ.SC.ClsNidLib
			 
	                  			tmStr = Info8.GetRequest.Info.NidRevisit.tostring
								Info8.ParameterList.REMove("NidRevisit")
								Info8.ParameterList.Add("NidRevisit",tmStr)
					
						 
								tmpClsNidLib = Info8.GetFunctions.GetNidLib(Guid.Parse(tmStr))
								 	Info8.ParameterList.REMove("Revisit_House_NidBase")
									Info8.ParameterList.Add("Revisit_House_NidBase", tmpClsNidLib.RHouse.NidBase.ToString)

								 
								Try
								 tmpBase_JobInfo_Fire =Info8.GetRevisit_Shop.GetBase_JobInfo  '133680 Info8.GetParvandeh_Shop.GetBase_JobInfo  'Info8.GetRevisit_Shop.GetBase_JobInfo  
						           
						      	Catch
									Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید فیش", "job Fire")
								End Try
						    End if		
							Dim Fi_Fire , Job_Fire as Integer 
					
					        For Fi_Fire=0 to tmpBase_JobInfo_Fire.count -1  
					        	If tmpBase_JobInfo_Fire(Fi_Fire).JobDeActivateDate = "" Or  tmpBase_JobInfo_Fire(Fi_Fire).JobDeActivateDate is Nothing  Or tmpBase_JobInfo_Fire(Fi_Fire).JobDeActivateDate > BIZ.SA.ClsCommon.CurrentShamsiDateString  Then
					               Job_Fire=tmpBase_JobInfo_Fire(Fi_Fire).CI_JobName
					            End if      
					      	Next 
							
							Dim Map As New Biz.SC.ClsSettings_Map
					 		Dim codeMap As Biz.SC.SrvMap.MapService_SrvFindResult
					 		
					 		Dim tmStrFire As String
					 		tmStrFire = Info8.GetRequest.Info.NidRevisit.tostring
					 		Dim tmpClsNidLibFire As BIZ.SC.ClsNidLib
					 		
							tmpClsNidLibFire = Info8.GetFunctions.GetNidLib(Guid.Parse(tmStrFire))
							    
								Try  
						 		codeMap = Info8.GetMap.GetMapLatAndLong(Map, Info8.GetFunctions.GetNosaziNickName(tmpClsNidLibFire.RHouse.NidNosaziCode), False)
								Catch ex As Exception
									Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "ملک روی نقشه جانمایی نشده ") : Exit Function      
								End Try
							
							Try
							
							Dim tmpMap as Decimal
								tmpMap=codeMap.CenterX.tostring
							Catch ex As Exception
							
						'	
									Try	
									codeMap = Info8.GetMap.GetMapLatAndLong(Map, Info8.GetFunctions.GetNosaziNickName(tmpClsNidLibFire.RDastgah.NidNosaziCode), False)
									Catch 
								
										Info8.AddError(BIZ.SA.EumErrorAction.Stop, "* توجه", "ملک روی نقشه جانمایی نشده ") : Exit Function      
									End Try
					    
							End Try
						
					        Dim pMessage As New Biz.Communication.SrvFireFighting.RequestDataObjectType
					       	
					       Try
					       		pMessage.address = Info8.GetRequest.Info.RequesterAddress.tostring
					       Catch ex As Exception
								Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "اطلاعات ادرس را اصلاح نمایید ")      
							End Try 
					
					        Dim lstUsing As New List(Of DtoSC.Base_Using) 
					        Dim TmplstUsing As New List(Of DtoSC.Base_Using) 
					        if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99"  or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="BDD563C8-1EB2-40EC-8811-19DB03908488" Then
					        	lstUsing=Lstu '272888 Info8.GetRevisit_Apartment.GetBase_Using 
					        ElseIf Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" _
					        or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
					        or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="3157211F-C484-48E9-B65C-6BF3271A9F15" Then 
				  				lstUsing=Lstu '272888  Info8.GetRevisit_Building.GetBase_Using 
				  			Else 
					        	lstUsing=Info8.GetRevisit_Shop.GetBase_Using '133680 Info8.GetParvandeh_Shop.GetBase_Using
				        	End if
				        	
				        
				        
					        if lstUsing.count>0 Then 
					        	pMessage.area = lstUsing.Sum(Function(f) f.UsingArea)
					        	pMessage.accounttypeid =lstUsing(0).CI_UsingGroup
					        	TmplstUsing=lstUsing.where(Function(f) f.FloorNo<0).tolist
					        	Dim FloornoCount as Integer=0
					        	if TmplstUsing.any Then FloornoCount= (-1) * TmplstUsing.Min(Function(f) f.FloorNo)
					        	TmplstUsing=lstUsing.where(Function(f) f.FloorNo<100 And  f.FloorNo>-1).tolist
					        	if TmplstUsing.any Then FloornoCount = FloornoCount+TmplstUsing.Max(Function(f) f.FloorNo)
					        	TmplstUsing=lstUsing.where(Function(f) f.FloorNo>100 ).tolist
					        	if TmplstUsing.any Then FloornoCount = FloornoCount+(100-TmplstUsing.Max(Function(f) f.FloorNo)+1)
					        	FloornoCount=FloornoCount+1
					        	pMessage.floors =FloornoCount
					        Else
					        	pMessage.area =0
					        	pMessage.floors =0
					       End if 	
				     
				     		if TmpAllFiche.count>0 Then
					       		pMessage.bills_id = TmpAllFiche(0).billid.tostring + TmpAllFiche(0).PaymentID.tostring  '125934 
					       	Else
					       		pMessage.bills_id =""
					       	End if		
					        
					   
					        '271068
					       Dim TmpBaseUsingTyp_M As New List(Of DtoSC.Base_Using) 
					       TmpBaseUsingTyp_M= lstUsing 
				           Dim TmpBaseUsingTyp_MP As New List(Of DtoSC.Base_Using) 
				           TmpBaseUsingTyp_MP= lstUsing 
				           Dim UsingType as Integer=0
				           Dim TmpUsingType as Integer=0
				           Dim SumArea as Double=0
				           For i = 0 To lstUsing.GroupBy(Function(f) f.CI_UsingType).Count - 1
				           		TmpUsingType=lstUsing.GroupBy(Function(f) f.CI_UsingType)(i)(0).CI_UsingType
				                TmpBaseUsingTyp_MP =TmpBaseUsingTyp_M.where(Function(f) f.CI_UsingType= TmpUsingType).tolist
				                if TmpBaseUsingTyp_MP.sum(Function (f) f.UsingArea)>SumArea Then 
				                	SumArea=TmpBaseUsingTyp_MP.sum(Function (f) f.UsingArea)
				                	UsingType=TmpUsingType
				                End if 
				            Next
				            pMessage.requestplaceid =UsingType
					        
					        Msgbox("B777","B6")
						 	 '   msgbox("f:","AA")
					        pMessage.fname =Info8.GetRequest.Info.RequesterName.tostring
					        pMessage.lname = Info8.GetRequest.Info.RequesterName.tostring
					 
					        pMessage.latitude = codeMap.CenterX.tostring
					     
					        pMessage.longtitude = codeMap.CenterY.tostring
					  
					        pMessage.modernizationcode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
					 
					        
					        pMessage.mobile = Info8.GetRequest.Info.Cellphone 
					        
						If String.IsNullOrWhiteSpace(pMessage.mobile.tostring) Then 
							Info8.AddError(BIZ.SA.EumErrorAction.Stop, "شماره مالک ", "شماره همراه مالک بدرستی پر نشده است -0") : Exit Function
						End if						        
				
					        pMessage.nationalcode =Info8.GetRequest.Info.RequesterNationalCode
				
					        if TmpAllFiche.count>0 Then 
					        	pMessage.paymentdate =  TmpAllFiche(0).PaymentDate
					        
					        	pMessage.payment_Code =TmpAllFiche(0).FicheNo
					        	
					        	pMessage.payType_id = TmpAllFiche(0).EumIncomePaymentType
					        	pMessage.returnsnum =TmpAllFiche(0).Payable
					        Else
					        	 pMessage.paymentdate = ""
					        
					        	pMessage.payment_Code =""
					        	
					        	pMessage.payType_id = 0
					        	pMessage.returnsnum =0
					        End if
					    
					      'sp 434323
					      
					     	Dim LstOwner As New List(Of Global.DtoSC.Base_Owner)
						    LstOwner = Info8.GetRevisit_House.GetBase_Owner()
				
						    LstOwner = LstOwner.Where(Function(f) Len(f.CellPhone) > 9).ToList
    
					       if LstOwner.count>0 Then  pMessage.phone = LstOwner(0).CellPhone 'Info8.GetRequest.Info.TelephoneNo
					
					       'sp 405421 pMessage.requestplaceid = 3
					         
					        pMessage.shomareerja =Info8.GetRequest.Info.NidWorkItem
				        				       
					        pMessage.useitems = Job_Fire
					       
					        ReturnFire=0
					       ' If TmpAllFiche(0).Payable > 1000  Then
					           
			 				        Try
			 				   
							        '239939
							       '    if ApartmentTejari=1 Then
							       
							      
							         ReturnFire=tmp.GetInstanceState (Info8.GetRequest.Info.NidWorkItem.ToString,Info8.User.UserGuid,Info8.User.UserName.ToString)
								   Catch ex As Exception
							
										ReturnFire=0      
									End Try 
							
								   
								   Try
								    Dim TmpComN as String =""
								    if ReturnFire= 1 Then 
										TmpComN="پرونده در اتش نشانی  در حال پیگیری می باشد"
									ElseIf ReturnFire= 2 Then 
										TmpComN="پرونده در اتش نشانی تایید شده است "
									ElseIf ReturnFire= 3 Then 
										TmpComN="پرونده در اتش نشانی  رد شده است "
								   ' ElseIf ReturnFire= 5 Then 
									'	TmpCom="پرونده در حال پیگیری می باشد"
									Else
										TmpComN="شماره در اتش نشانی پرونده پیدا نشد"
									End if
								
							StrComments =TmpListManger.Comments & "مقدار ارسال شده از اتش نشانی : " & TmpComN & "#" &  ReturnFire & "$$" & pMessage.requestplaceid
						'SP 351595
					
								   if ReturnFire =1 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر &", "پرونده  در اتش نشانی در حال پیگیری می باشد لطفا منتظر جواب بمانید") : Exit Function
								   '309093
								   if (ReturnFire = 3 ) And (Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F9DE191A-959F-4C2E-8092-931FEB3589EB" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C4F9A87D-2FA8-4E42-9B74-4480C47B1241" _
								   or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" _
								   or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="EE64A610-9142-4E2B-953F-6E65B522A9E1" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" _
				                   or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="2C1EE596-5D29-4C0A-802A-1C2492B77A9F" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="BDD563C8-1EB2-40EC-8811-19DB03908488" _
				                   or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="D84FA3D1-54FD-4D24-8000-758A300B2441" _
				                   or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="EE64A610-9142-4E2B-953F-6E65B522A9E1" _
				                   or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="CBAD978D-3764-4E51-903C-9A8831B18760" _
				                   or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="4351C20C-643A-4A50-A4BD-AEBCEC5EADF8" _
				                   or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B6A28C29-6761-4719-8B62-C8575F88F572" ) And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "پرونده در اتش نشانی تایید نشد اجازه تایید ندارید ") :  Exit Function
				                   'sp 327791
				                   'SP 351595
						'387529 or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="3157211F-C484-48E9-B65C-6BF3271A9F15" _
								  'if ReturnFire =3 Then 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "در اتش نشانی رد شده است مجاز به تایید نمی باشید") : Exit Function
								   ''239939 
								   'SP 351595
						        		if ReturnFire<> 1 And ReturnFire<> 2 And  ReturnFire<> 3 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then 
						        			tmp.Pishgiri_Start(pMessage)
						        			if ApartmentTejari=1 Then 
						        				Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
												tmpSMS.BizCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
												tmpSMS.Number = Info8.GetRequest.Info.Cellphone
												tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
												tmpSMS.SMSType = "اتش نشانی"
											'	tmpSMS.Text =   " مالک محترم پرونده شماره  " + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf + "به سازمان آتش نشانی ارسال جهت پیگیری به سازمان آتشنشانی واقع در  ابتدای بلوار دانشجو مراجعه نمایید. شهرداری مشهد " 
												tmpSMS.Text ="مالک محترم ، پرونده شماره " + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf + " به سازمان آتشنشانی ارسال لذا منتظر پیامک فیش بازدید آتشنشانی باشید .پس از پرداخت غیر حضوری قبض و دریافت پیامک بازدید در تاریخ ذکر شده در محل حضور داشته باشد."									
												tmpSMS.UserID = Info8.User.UserGuid.ToString
												tmpSMS.UserName = Info8.User.UserName
												
												
												Dim smsErrorResult As New BIZ.SA.ClsErrorResult
												smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
						        			End if 
						        			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تایید مدیر", "به اتش نشانی ارسال شد لطفا منتظر جواب بمانید")
						        			Exit Function
						        		
						        		End if	
						        	Catch ex As Exception
						        
									Info8.AddError(BIZ.SA.EumErrorAction.Stop, "خطای تایید فیش", ex.message) 
								End Try
					'	End if 		 
				        
				End if
			End if	'metodPesh	
			
																		
	
                   Dim tmp222 As New Global.Biz.Communication.ClsFireFighting
                   '281387 Dim ResualtFire222 As Biz.Communication.SrvFireFighting.resualt2
                   '281387  ResualtFire222=tmp222.GetRequestStateBySubject(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode),TmpArchiveGroup)
                   'sp 377602  or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A"
'if Info8.Getrequest.info.Nidworkitem=4794230 Then 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "aa*","aa") :  Exit Function


                   if TmpArchiveGroup=1838 or TmpArchiveGroup=1839 or TmpArchiveGroup= 1869 or TmpArchiveGroup= 1870 or TmpArchiveGroup= 1871 or TmpArchiveGroup= 1872  or TmpArchiveGroup=1874 _							 
                   or TmpArchiveGroup=1881 or TmpArchiveGroup=1882 or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB"  or _
                   Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="3157211F-C484-48E9-B65C-6BF3271A9F15" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=Ucase("c4afbbd2-5b4d-4451-8a5d-a50d4a1260ce")  or _
                    Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=Ucase("d1b85def-3f07-4b29-b99b-bf47d0a283f8") or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)=Ucase("b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a")  Then 
               	
                  	Try
                  	 
                  		Dim tmStr As String
			 			Dim tmpClsNidLib As BIZ.SC.ClsNidLib
			 
	                  	tmStr = Info8.GetRequest.Info.NidRevisit.tostring
						Info8.ParameterList.REMove("NidRevisit")
						Info8.ParameterList.Add("NidRevisit",tmStr)
					
						 
						tmpClsNidLib = Info8.GetFunctions.GetNidLib(Guid.Parse(tmStr))
						Info8.ParameterList.REMove("Revisit_House_NidBase")
						Info8.ParameterList.Add("Revisit_House_NidBase", tmpClsNidLib.RHouse.NidBase.ToString)
					    if  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="35CFFB97-67A3-488E-8A7A-BEC93C459B99" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="BDD563C8-1EB2-40EC-8811-19DB03908488"  Then
					    	Info8.ParameterList.REMove("Revisit_Apartment_NidBase")
							Info8.ParameterList.Add("Revisit_Apartment_NidBase", tmpClsNidLib.RApartment.NidBase.ToString)
					    End if
		        		
                
	                	Dim Fire As New Global.Biz.Communication.ClsFireFighting
				        Dim FireReqMessage As New Global.Biz.Communication.ClsFireFightingRequestMessageContract
				          

				  Try 
				  
				  If Not (tmpClsNidLib.RDastgah Is Nothing) Then
				  
						Info8.ParameterList.REMove("Revisit_HouseSharing_NidBase")
						Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", tmpClsNidLib.RDastgah.NidBase.ToString)
						
						
						
						FireReqMessage.OwnerName = Info8.GetRevisit_HouseSharing.GetBase_Owner(0).OwnerName +" - " + Info8.GetRevisit_HouseSharing.GetBase_Owner(0).OwnerLastName
		
						iif(String.IsNullOrEmpty(Info8.GetRevisit_HouseSharing.GetBase_Owner(0).OwnerFatherName) , FireReqMessage.OwnerFather = "Unknown" , FireReqMessage.OwnerFather = Info8.GetRevisit_HouseSharing.GetBase_Owner(0).OwnerFatherName)
						
						FireReqMessage.NationalCode = Info8.GetRevisit_HouseSharing.GetBase_Owner(0).NationalCode
						
						Dim LstOwner,LstOwner_tmp As New List(Of Global.DtoSC.Base_Owner)
					    LstOwner = Info8.GetRevisit_HouseSharing.GetBase_Owner()
					    
					    FireReqMessage.mobile = ""
					    
					    LstOwner_tmp = LstOwner.Where(Function(f) Len(f.CellPhone) > 9).ToList
				    					     
				        if LstOwner_tmp.count>0 Then  FireReqMessage.mobile = LstOwner_tmp(0).CellPhone  ' Info8.GetRequest.Info.TelephoneNo 			        			        
				        
				        if LstOwner_tmp.count<=0 Then				        					        				        
				        	LstOwner_tmp = LstOwner.Where(Function(f) Not String.IsNullOrWhiteSpace(f.Telephone)).ToList
				        	if LstOwner_tmp.count>0 Then  FireReqMessage.mobile = LstOwner_tmp(0).Telephone				        	
				        End if	

						if LstOwner_tmp.count<=0  Then 
					            Dim lstOwner_UGP As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
					            lstOwner_UGP = Info8.GetFunctions.GetUGP_AuthenticationHistory(Info8.GetRequest.Info.NidProc)
					            if 	lstOwner_UGP.any Then 
					            	lstOwner_UGP = lstOwner_UGP.where(Function (f) Not String.IsNullOrWhiteSpace(f.LawyerMobile)).tolist
					            	if lstOwner_UGP.any Then
					            		FireReqMessage.mobile = lstOwner_UGP(0).LawyerMobile
					            	End if	
					            End if
						End if	
						
									         
				        
						If FireReqMessage.Mobile = "" Then 
							Info8.AddError(BIZ.SA.EumErrorAction.Stop, "شماره مالک ", "شماره همراه مالک بدرستی پر نشده است -1 ") : Exit Function
						End if		
									        		  
				 	End if
				 Catch e as exception
	   				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*","مشکل در خواندن اطلاعات مالک (بازدید دستگاه)" & Info8.ParameterList("Revisit_HouseSharing_NidBase").tostring) :  Exit Function
	   		 	End Try
				 
					Try	
					
				 If (tmpClsNidLib.RDastgah Is Nothing) Then	
				 				 
				        FireReqMessage.OwnerName = Info8.GetRevisit_House.GetBase_Owner(0).OwnerName +" - " + Info8.GetRevisit_House.GetBase_Owner(0).OwnerLastName
				        
				        FireReqMessage.OwnerFather = Info8.GetRevisit_House.GetBase_Owner(0).OwnerFatherName
				         
				        FireReqMessage.NationalCode = Info8.GetRevisit_House.GetBase_Owner(0).NationalCode
				        'sp 434323
						Dim LstOwner,LstOwner_tmp As New List(Of Global.DtoSC.Base_Owner)
					    LstOwner = Info8.GetRevisit_House.GetBase_Owner()
					    
					    FireReqMessage.mobile = ""
					    
					    LstOwner_tmp = LstOwner.Where(Function(f) Len(f.CellPhone) > 9).ToList
				    					     
				        if LstOwner_tmp.count>0 Then  FireReqMessage.mobile = LstOwner_tmp(0).CellPhone  ' Info8.GetRequest.Info.TelephoneNo 			        			        
				        
				        if LstOwner_tmp.count<=0 Then				        					        				        
				        	LstOwner_tmp = LstOwner.Where(Function(f) Not String.IsNullOrWhiteSpace(f.Telephone)).ToList
				        	if LstOwner_tmp.count>0 Then  FireReqMessage.mobile = LstOwner_tmp(0).Telephone				        	
				        End if	

						if LstOwner_tmp.count<=0  Then 
					            Dim lstOwner_UGP As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
					            lstOwner_UGP = Info8.GetFunctions.GetUGP_AuthenticationHistory(Info8.GetRequest.Info.NidProc)
					            if 	lstOwner_UGP.any Then 
					            	lstOwner_UGP = lstOwner_UGP.where(Function (f) Not String.IsNullOrWhiteSpace(f.LawyerMobile)).tolist
					            	if lstOwner_UGP.any Then
					            		FireReqMessage.mobile = lstOwner_UGP(0).LawyerMobile
					            	End if	
					            End if
						End if	
						
									         
				        
						If FireReqMessage.Mobile = "" Then 
							Info8.AddError(BIZ.SA.EumErrorAction.Stop, "شماره مالک ", "شماره همراه مالک بدرستی پر نشده است -2 ") : Exit Function
						End if					        
				        
				   End if   
				     
				 Catch e as exception
	   				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*","مشکل در خواندن اطلاعات مالک (بازدید ملک)|| " & " NidBase_house: " & tmpClsNidLib.RHouse.NidBase.ToString ) :  Exit Function
	   		 	 End Try
            
				        FireReqMessage.Address =Info8.GetRevisit_House.GetBase_AddressInfo.MainAddress + " پلاک ابی " +Info8.GetRevisit_House.GetBase_CommonEstate.Plack
				        
				        if Info8.GetRevisit_House.GetBase_AddressPostCode.count >0 Then 
				        	FireReqMessage.PostalCode=Info8.GetRevisit_House.GetBase_AddressPostCode(0).PostCode 
				        Else
				        	FireReqMessage.PostalCode=" "
				        End if		
				        
				        
				        
				  '   Try   
				        FireReqMessage.RequestorName =Info8.GetRequest.Info.RequesterName.tostring
				        
				        FireReqMessage.RequestorType = Info8.GetRequest.Info.CI_Requester.tostring
				'     Catch
	   			'			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Dتوجه","GetError") :  Exit Function
	   			'	 End Try	
				        
				        
				        
				        FireReqMessage.BuildingAddress = Info8.GetRevisit_House.GetBase_AddressInfo.MainAddress 
				        if Info8.GetRevisit_House.GetBase_AddressPostCode.count >0 Then 
				        	FireReqMessage.BuildingPostalCode = Info8.GetRevisit_House.GetBase_AddressPostCode(0).PostCode 
				        Else
				        	FireReqMessage.BuildingPostalCode=" "
				        End if		
				        
				        FireReqMessage.ModernizationCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
				        FireReqMessage.CaseNumber = Info8.GetRequest.Info.NidWorkItem 
				        
				        Dim TmpBaseUsing As New List(Of DtoSC.Base_Using)
				        Dim TmpBaseUsingTyp As New List(Of DtoSC.Base_Using)
				        
				        Dim TmpBaseFront As New List(Of DtoSC.Base_Front)
				        Try 
				      	 
					       
		            		If TmpListManger.CI_ResourceManagerConfirm =5 Then '172373
	                			Info8.ParameterList.REMove("MovafeghatOsooli_NidBase")
								Info8.ParameterList.Add("MovafeghatOsooli_NidBase", tmpClsNidLib.RMovafeghatOsooli.NidBase.ToString)
		            			TmpBaseUsing = Info8.GetMovafeghatOsooli.GetBase_Using 
		            			TmpBaseFront = Info8.GetMovafeghatOsooli.GetBase_Front
		            			FireReqMessage.RequestSubject = 1
		            			RequestSubjectFire=1	
		            		ElseIf TmpListManger.CI_ResourceManagerConfirm =3 or TmpListManger.CI_ResourceManagerConfirm =20 Then 
		            		'sp 377602 or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" )
		            		
		            			 If (TmpArchiveGroup=1839 or TmpArchiveGroup=1871 or TmpArchiveGroup=1872 or TmpArchiveGroup=1874 or TmpArchiveGroup=1882 _
		            			    or TmpArchiveGroup=1881  or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB" _
		            			    or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A")  And (TmpListManger.CI_ResourceManagerConfirm =3 or TmpListManger.CI_ResourceManagerConfirm =20 ) Then '172373
		            			    
		                			Info8.ParameterList.REMove("MapControl_NidBase")
									Info8.ParameterList.Add("MapControl_NidBase", tmpClsNidLib.RMapControl.NidBase.ToString)
			            			TmpBaseUsing = Info8.GetMapControl.GetBase_Using 
			            			TmpBaseFront = Info8.GetMapControl.GetBase_Front
			            			FireReqMessage.RequestSubject = 1
			            			RequestSubjectFire=1
			            		Else	
			            			Try			        
									    Info8.ParameterList.REMove("Revisit_Building_NidBase")
										Info8.ParameterList.Add("Revisit_Building_NidBase", tmpClsNidLib.RBuilding.NidBase.ToString)
										TmpBaseUsing = Info8.GetRevisit_Building.GetBase_Using 
										TmpBaseFront = Info8.GetRevisit_Building.GetBase_Front
										FireReqMessage.RequestSubject = TmpArchiveGroup
										RequestSubjectFire=TmpArchiveGroup
									Catch
										Info8.ParameterList.REMove("Revisit_HouseSharing_NidBase")
										Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", tmpClsNidLib.RDastgah.NidBase.ToString)
										TmpBaseUsing = Info8.GetRevisit_HouseSharing.GetBase_Using 
										TmpBaseFront = Info8.GetRevisit_HouseSharing.GetBase_Front
										FireReqMessage.RequestSubject = TmpArchiveGroup
										RequestSubjectFire=TmpArchiveGroup
									End Try
									
									
			            			if TmpArchiveGroup=1838 Then 
			            				FireReqMessage.RequestSubject = 3
			            				RequestSubjectFire=3
			            			ElseIf RequestSubjectFire<> 1 Then
			            				FireReqMessage.RequestSubject = 2
			            				RequestSubjectFire=2
			            			End if
			            		End if		
		            		End if
		            		
		            	Catch
	   						Info8.AddError(BIZ.SA.EumErrorAction.Stop, "fتوجه","مشکل در اطلاعات کاربری ساختمان") :  Exit Function
	   					End Try	   					
	   					
	   					Dim TesTMP as Integer=0
	   					if TmpBaseUsing.count =0 Then 	TesTMP=1
				        if TesTMP=0 Then
				          Dim TmpBaseUsingNew_M As New List(Of DtoSC.Base_Using)
				          TmpBaseUsingNew_M=TmpBaseUsing
				          TmpBaseUsingNew_M=TmpBaseUsingNew_M.where(Function(f)  f.ci_usingtype <> 833 And   f.ci_usingtype <> 816).tolist 
				       ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a: " & TmpBaseFront.Sum (Function(f) f.FrontArea ) ,TmpBaseUsing.Sum (Function(f) f.UsingArea )) 
				        FireReqMessage.Metrage =TmpBaseUsingNew_M.Sum (Function(f) f.UsingArea )+ TmpBaseFront.Sum (Function(f) f.FrontArea )
		'=============================== 'sp 459756 =====	
			
   			Dim tejfloor as Integer = 0
			Dim TmpFlorBaseUsing_GhM As New List(Of DtoSC.Base_Using)  		
			Dim SumGheirMaskooni as Double =0 
			Dim SumGheirMaskooni2 As New List(Of DtoSC.Base_Using)	'sajedi
			SumGheirMaskooni2=TmpBaseUsingNew_M
   			SumGheirMaskooni2=SumGheirMaskooni2.where(Function(f)  f.ci_UsingGroup = 12).tolist
   			Try
				SumGheirMaskooni = SumGheirMaskooni2.sum(Function(f) f.UsingArea )'+ TmpBaseFront.Sum (Function(f) f.FrontArea )
			Catch	
			End Try
		'====================================				        
				        'FireReqMessage.Floors = TmpBaseUsing.Max(Function(f) f.FloorNo)+1
				        
				        Msgbox("B8","B8")
				       
						 i=0
						Dim TmpBaseUsingNew As New List(Of DtoSC.Base_Using)
						
						TmpBaseUsingNew=TmpBaseUsing.where(Function(f) f.FloorNo<80).tolist
			

		Dim floormanfiYek as Integer = 0							
	'=====================================		
	'sp 479080 - 479088
	if ucase(Info8.GetRequest.info.NidWorkFlowDeff.ToString) = ucase("b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a") And Info8.GetRequest.info.nidworkitem <> 10363971 Then  
		Try
		'	if TmpBaseUsingNew.Min(Function(f) f.FloorNo) = 0 Then
		 floormanfiYek=TmpBaseUsingNew.Min(Function(f) f.FloorNo)	'sp 459756  تشخیص اینکه طبقه زیر همکف داره یانه در صورت داشتن بی قیدو شرط ارسال به آتش نشانی
		Catch ex As Exception
			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "خطا کنترل نقشه","کاربری کنترل نقشه خالی میباشد - " & ex.message)
			Exit Function
		End Try	
	End if	
	'=========================================	
		
						
					
					
						For  i=TmpBaseUsingNew.Min(Function(f) f.FloorNo) to TmpBaseUsingNew.Max(Function(f) f.FloorNo) 					
						 	
							Dim TmpFlorBaseUsing As New List(Of DtoSC.Base_Using)
							Dim TmpFlorBaseUsing20 As New List(Of DtoSC.Base_Using)
							Dim TmpFlorBaseUsing_M As New List(Of DtoSC.Base_Using)
							Dim UnitCol as Integer =0 
							Dim SumAnbar as Double =0 
							
							TmpFlorBaseUsing=TmpBaseUsingNew.where(Function(f) (f.ci_usingtype =709 or  f.ci_usingtype =810 or  f.ci_usingtype =833 or  f.ci_usingtype =842 _
							or f.ci_usingtype =850 or  f.ci_usingtype =853 or  f.ci_usingtype =2036 or  f.ci_usingtype =2038 or  f.ci_usingtype =2042 or  f.ci_usingtype =2047 _
							or f.ci_usingtype =805 or  f.ci_usingtype =804 or  f.ci_usingtype =802 or  f.ci_usingtype =829 or  f.ci_usingtype =831 or  f.ci_usingtype =845 _
							or f.ci_usingtype =817 or  f.ci_usingtype =933 or  f.ci_usingtype =843 or  f.ci_usingtype =843 or  f.ci_usingtype =930 or  f.ci_usingtype =803) And f.FloorNo=i ).tolist
			'sp 459756		or f.ci_usingtype =843   
				
							if TmpFlorBaseUsing.count>0 Then 
	
								TmpFlorBaseUsing20= TmpBaseUsingNew.where(Function(f)  f.FloorNo=i  And f.ci_usingtype=801).tolist
								SumAnbar=TmpFlorBaseUsing20.Sum (Function(f) f.UsingArea )
								TmpFlorBaseUsing20= TmpBaseUsingNew.where(Function(f) f.ci_usingtype=401).tolist
								if TmpFlorBaseUsing20.count>0 Then 
									UnitCol=TmpFlorBaseUsing20.sum(Function(f) f.UnitCount)
										Else
											UnitCol=0
										End if	
										TmpFlorBaseUsing20= TmpBaseUsingNew.where(Function(f)  f.FloorNo=i  And f.ci_usingtype=806).tolist			
										TmpFlorBaseUsing_M= TmpBaseUsingNew.where(Function(f)  f.FloorNo=i ).tolist
										if TmpFlorBaseUsing_M.Sum (Function(f) f.UsingArea )- TmpFlorBaseUsing20.Sum (Function(f) f.UsingArea )-(UnitCol*4)- TmpFlorBaseUsing.Sum (Function(f) f.UsingArea )<2 _
											And TmpFlorBaseUsing20.Sum (Function(f) f.UsingArea )<26 And SumAnbar<=(UnitCol*4) Then 
										Else
											TmpFloor=TmpFloor+1
											TmpFlorBaseUsing_GhM = TmpBaseUsingNew.where(Function(f)  f.FloorNo=i  And f.CI_UsingGroup=12).tolist 'sp 459756
											if TmpFlorBaseUsing_GhM.count>0 Then tejfloor +=1	 'sp 459756
					   					End if
						   			Else
										TmpFlorBaseUsing20= TmpBaseUsingNew.where(Function(f)  f.FloorNo=i  And f.ci_usingtype=801).tolist
										SumAnbar=TmpFlorBaseUsing20.Sum (Function(f) f.UsingArea )
										TmpFlorBaseUsing20= TmpBaseUsingNew.where(Function(f) f.ci_usingtype=401).tolist
										if TmpFlorBaseUsing20.count>0 Then 
											UnitCol=TmpFlorBaseUsing20.sum(Function(f) f.UnitCount)
										Else
											UnitCol=0
										End if	
											TmpFlorBaseUsing20= TmpBaseUsingNew.where(Function(f)  f.FloorNo=i  And f.ci_usingtype=806).tolist			
											TmpFlorBaseUsing_M= TmpBaseUsingNew.where(Function(f)  f.FloorNo=i ).tolist
											if TmpFlorBaseUsing_M.Sum (Function(f) f.UsingArea )- TmpFlorBaseUsing20.Sum (Function(f) f.UsingArea )-(UnitCol*4)<2 _
											And TmpFlorBaseUsing20.Sum (Function(f) f.UsingArea )<26 And SumAnbar<=(UnitCol*4) Then 
										Else
											TmpFloor=TmpFloor+1
											TmpFlorBaseUsing_GhM = TmpBaseUsingNew.where(Function(f)  f.FloorNo=i  And f.CI_UsingGroup=12).tolist 'sp 459756
											if TmpFlorBaseUsing_GhM.count>0 Then tejfloor +=1	 'sp 459756
										End if
						  			End if		
						Next
				       	
				        FireReqMessage.Floors=TmpFloor
				        FireReqMessage.PermitNo = 0
					    FireReqMessage.PermitDate = ""
				        Msgbox("B9","B9")
				        
			            Dim LstSazehType As String = ""
			            Dim LstUsing As String = ""
			           
			            TmpBaseUsingTyp=TmpBaseUsing 
			
			            For i = 0 To TmpBaseUsing.GroupBy(Function(f) f.CI_SazehType).Count - 1
			                LstSazehType += TmpBaseUsing.GroupBy(Function(f) f.CI_SazehType)(i)(0).CI_SazehType.ToString
			                If i < TmpBaseUsing.GroupBy(Function(f) f.CI_SazehType).Count - 1 Then LstSazehType += ","
			            Next
			            
			            'Maskoni= True 
		                Dim NotMaskoniAndTejari as Boolean
						Maskoni= True 
						For i = 0 To TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup).Count - 1
						if  TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup)(i)(0).CI_UsingGroup.ToString<>11 Then Maskoni=False
							
						If TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup).Select(Function(g) g.ToList())(i)(0).CI_UsingGroup.ToString() <> "11" AndAlso TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup).Select(Function(g) g.ToList())(i)(0).CI_UsingGroup.ToString() <> "12" Then
						    NotMaskoniAndTejari = True
						End If
			        '    For i = 0 To TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup).Count - 1
			        '    	if  TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup)(i)(0).CI_UsingGroup.ToString<>11 Then Maskoni=False
			                LstUsing += TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup)(i)(0).CI_UsingGroup.ToString
			                groupNumbers.Add(TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup)(i)(0).CI_UsingGroup)	'sp 459756   'useless
			                If i < TmpBaseUsing.GroupBy(Function(f) f.CI_UsingGroup).Count - 1 Then LstUsing += ","
			            Next
			          
			         ' ---------------------------------------------------
			           
			           
				        FireReqMessage.BuildingType = LstSazehType
				        FireReqMessage.UsageId = LstUsing

				        Dim Map As New Biz.SC.ClsSettings_Map
				        Dim codeMap As Biz.SC.SrvMap.MapService_SrvFindResult
				        
				        Dim tmStrFire As String
				 		tmStrFire = Info8.GetRequest.Info.NidRevisit.tostring
				 		Dim tmpClsNidLibFire As BIZ.SC.ClsNidLib
						tmpClsNidLibFire = Info8.GetFunctions.GetNidLib(Guid.Parse(tmStrFire))  
						
		Dim LP_nosazicode , LP_nosazicodetmp As New List(Of BIZ.SA.ClsNosaziCodeType)    
		LP_nosazicode=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.info.NidNosaziCode,0)
		
		LP_nosazicodetmp = LP_nosazicode.Where(Function(f) f.EumNosaziCodeObjType = 2).tolist
		Dim NosaziNickName_House = Info8.GetFunctions.GetNosaziNickName(LP_nosazicodetmp(0).NidNosaziCode)
		
		'LP_nosazicodetmp = LP_nosazicode.Where(Function(f) f.EumNosaziCodeObjType = 5).tolist
	'	Dim NosaziNickName_DastgaH = Info8.GetFunctions.GetNosaziNickName(LP_nosazicodetmp(0).NidNosaziCode)
		
'    If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
    '	Dim nosaziNNN = Info8.GetFunctions.GetNosaziNickName(tmpClsNidLibFire.RHouse.NidNosaziCode)
'    	codeMap = Info8.GetMap.GetMapLatAndLong(Map, Info8.GetFunctions.GetNosaziNickName(tmpClsNidLibFire.RHouse.NidNosaziCode), False)
'       Info8.AddError(BIZ.SA.EumErrorAction.Stop, "codeMap2", codeMap.CenterX.tostring	)
'       Exit Function
'   End If
						
				 		codeMap = Info8.GetMap.GetMapLatAndLong(Map, Info8.GetFunctions.GetNosaziNickName(tmpClsNidLibFire.RHouse.NidNosaziCode), False)
						Try
						
							Dim tmpMap as Decimal
							tmpMap=codeMap.CenterX.tostring	  	    

						Catch 
							Try
							codeMap = Info8.GetMap.GetMapLatAndLong(Map, Info8.GetFunctions.GetNosaziNickName(tmpClsNidLibFire.RDastgah.NidNosaziCode), False)
							Catch 
								Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "موقعیت بر روی نقشه جانمایی نشده است " & Info8.GetMap.GetMapLatAndLong(Map, Info8.GetFunctions.GetNosaziNickName(tmpClsNidLibFire.RHouse.NidNosaziCode) )) :  Exit Function
							End Try
						End Try
				      
				        FireReqMessage.Latitude = codeMap.CenterX.tostring
				        FireReqMessage.Longitude = codeMap.CenterY.tostring
				        
				         
				        Dim ResualtFire2 As New  Biz.Communication.SrvFireFighting.resualt2
						Dim ResualtFire As  Biz.Communication.SrvFireFighting.resualt
						
						
						If String.IsNullOrWhiteSpace(FireReqMessage.Mobile.tostring)  Then 
							Info8.AddError(BIZ.SA.EumErrorAction.Stop, "شماره مالک ", "شماره همراه مالک بدرستی پر نشده است -3") : Exit Function
						End if	
												
											
					  
					 '===============================  'sp 486982 ========================================
					   Dim CheckJandD as Boolean = False 	'اینکه ساختمان گروه جیم و دال میباشد یا خیر   'sp 486982
					   
					 '  if FireReqMessage.metrage>2000 or TmpFloor >5 Then CheckJandD = True	
					  CheckJandD = ExecSp("[dbo].[spRptGetBuildingGroupByNidProc]") ' تشخیص گروه بر اساس پروسیجری که اعلام شده از طرف شهرداری 
					 '===============================  'sp 486982 ========================================	
					    				

						ResualtFire2 = Fire.GetReauestState(Info8.GetRequest.Info.NidWorkItem.tostring ,Info8.User.UserGuid,Info8.User.UserName.ToString)
						
					
						
						'or ( Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="677FBEE4-B05C-4FA6-AB19-CE897739195A" ) And  LstMangerConFire.Count > 0)
					 
					'281387 	if  Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB"  Then ResualtFire2 =Fire.GetRequestStateBySubject(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode), 1)
						'msgbox("ResualtFire2.returnCode:",ResualtFire2.returnCode)
				'sp 304696 'sp 303549 sp 303530	'sp 303018 	'sp 289920   'sp 299201
				'	If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a1",ResualtFire2.returnCode ) 
						if  Info8.GetRequest.Info.NidworkItem ="3877581" or Info8.GetRequest.Info.NidworkItem ="3759776" or Info8.GetRequest.Info.NidworkItem ="3776986" _
							or Info8.GetRequest.Info.NidworkItem ="3893024" or Info8.GetRequest.Info.NidworkItem ="3894554" or Info8.GetRequest.Info.NidworkItem ="3899454" _
							 or Info8.GetRequest.Info.NidworkItem ="3880950" Then ResualtFire2.returnCode=2
						'sp 377755

 					
						
					'SP 393578 	if RequestSubjectFire<>1 Then 
					'if ResualtFire2 is Nothing Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی&"  ,"DD")
					'	if  Info8.GetRequest.Info.NidworkItem ="3682829" Then 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی&"  ,ResualtFire2.returnCode)
				'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "a: " & ResualtFire2.returnCode  ,FireReqMessage.Metrage) 
							if ResualtFire2.returnCode=4 Then
		'---------------------------------------------------------------------	
								if NotMaskoniAndTejari = False Then
									if Maskoni=True Then 
										if  CheckJandD = True Then
											if floormanfiYek < 0 Then 
													ResualtFire=Fire.RegisterRequest(FireReqMessage,Info8.User.UserGuid,Info8.User.UserName.ToString) 
										   ' if FireReqMessage.Metrage> 999.9 or TmpFloor>4 Then	'sp 459756
											ElseIf TmpFloor>4 Then
												 ResualtFire=Fire.RegisterRequest(FireReqMessage,Info8.User.UserGuid,Info8.User.UserName.ToString) 
											End if
										End if	 
									Else
										if floormanfiYek < 0 Then
											ResualtFire=Fire.RegisterRequest(FireReqMessage,Info8.User.UserGuid,Info8.User.UserName.ToString) 
										ElseIf TmpFloor = tejfloor Then			'تشخیص اینکه ساختمان تجاری هست . اگر مساوی نباشه میشه استفاده ترکیبی مسکونی تجاری - درصورتی که تجاری باشه باید بره آتش نشانی ولی مصرف ترکیبی شرایط داره که در بقیه دستور اومده 
										'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "نوجه ","ساختمان تجاری ") 	
											ResualtFire=Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)
										ElseIf SumGheirMaskooni>= 280 Then
											'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "نوجه ","ساختمان ترکیبی با مترژ تجاری بیشتر از 280 متر") 
											ResualtFire=Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)
										ElseIf TmpFloor-tejfloor>4 Then	 
										'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "نوجه ","ساختمان ترکیبی بیش از 4 طبقه مسکونی ") 
											ResualtFire=Fire.RegisterRequest(FireReqMessage,Info8.User.UserGuid,Info8.User.UserName.ToString)
										End if
											
									End if	 ' spp
									
								Else
								
									ResualtFire=Fire.RegisterRequest(FireReqMessage,Info8.User.UserGuid,Info8.User.UserName.ToString)
								
								End if	
		'---------------------------------------------------------------------						
			' spp					if Maskoni=True Then 
								
'sp 459756									if FireReqMessage.Metrage> 999.9 or TmpFloor>4 Then	
			' spp						if TmpFloor>4 Then	'sp 459756
			' spp							Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString) 
			' spp							if TmpListManger.CI_ResourceManagerConfirm <> 5 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "T1 پرونده به اتش نشانی ارسال شد لطفا منتظر پاسخ باشید")
			' spp						End if	
			' spp					Else
			' spp						Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString) 'mousavi
									'if TmpListManger.CI_ResourceManagerConfirm <> 5 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "T11 پرونده به اتش نشانی ارسال شد لطفا منتظر پاسخ باشید")
			' spp					End if		
										
							Else
							
						

								if  ResualtFire2.returnCode = 1 or ResualtFire2.returnCode = 5 Then 
'sp 459756							if ResualtFire2.returnCode = 1 And Maskoni=True And TmpFloor<5 And FireReqMessage.Metrage< 999.9 Then
									if ResualtFire2.returnCode = 1 And Maskoni=True And TmpFloor<5 Then	'sp 459756
									Else
									'sp 407893 
										if {"10229856","3919750","2830546","4756005","10976836"}.contains(Info8.GetRequest.Info.NidworkItem.tostring) Then 'sp521050{10976836}  sp 503073 ' 4756005 (sp 513721)
									
										Else 
											if TmpListManger.CI_ResourceManagerConfirm <> 5 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "**توجه**", "پرونده در حال پیگیری در اتش نشانی می باشید تا پاسخ اتش نشانی اجازه تایید ندارید " & ResualtFire2.returnCode.tostring) :  Exit Function 
										End if 	
									End if	
								End if	
								
								if Info8.getRequest.Info.RequestDate >"1397/03/08"  And (TmpArchiveGroup=1872 or TmpArchiveGroup=1871 or TmpArchiveGroup=1839 or TmpArchiveGroup=1881 or TmpArchiveGroup=1874) Then 
								Else
								 if  ResualtFire2.returnCode = 3 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "پرونده به دلیل" + ResualtFire2.failMessage +" در اتش نشانی تایید نشد اجازه تایید ندارید ") :  Exit Function
								End if 
						'228637
						'313755	End if
							
						'	 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "result" & ResualtFire2.metrage, FireReqMessage.metrage)
						'sp 
						
 '   If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
 '      Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ResualtFire12", ResualtFire2.metrage)
 '      Info8.AddError(BIZ.SA.EumErrorAction.Stop, "FireReqMessage", FireReqMessage.metrage)
 '      Info8.AddError(BIZ.SA.EumErrorAction.Stop, "NotMaskoniAndTejari", NotMaskoniAndTejari)
 '      Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Maskoni", Maskoni)
 '      Info8.AddError(BIZ.SA.EumErrorAction.Stop, "floormanfiYek", floormanfiYek)
 '      Info8.AddError(BIZ.SA.EumErrorAction.Stop, "TmpFloor", TmpFloor)
 '     ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "CI_Manager", TmpListManger.CI_ResourceManagerConfirm)
 '      Exit Function
 '  End If						
										
						if  ResualtFire2.metrage > FireReqMessage.metrage+0.5 or  ResualtFire2.metrage < FireReqMessage.metrage-0.5 Then 'ResualtFire2.floors < FireReqMessage.Floors or ' 313755 And TmpListManger.CI_ResourceManagerConfirm <>5 

					       if  NotMaskoniAndTejari = False Then
					        
						        if Maskoni=True Then
						        	if Info8.GetRequest.Info.NidworkItem= "3093649" And Info8.GetRequest.Info.NidworkItem= "10229856"  Then 
'sp 459756					       ElseIf FireReqMessage.Metrage> 999.9 or TmpFloor>4 Then
									ElseIf floormanfiYek<0 Then 
										Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)	
								   ElseIf  TmpFloor>4 Then
						        		Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)
						        		'SP 351595
						        	'	if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm <> 5 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه.", "@پرونده به اتش نشانی ارسال شد لطفا منتظر پاسخ باشید") : Exit Function
						        	ElseIf  Info8.GetRequest.Info.NidWorkItem.tostring="2307008" Then 
										Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)
										'SP 351595
									'	if Info8.GetManagerConfirm.Sh_ManagerConfirm.CI_ResourceManagerConfirm <> 5 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه*", "$$پرونده به اتش نشانی ارسال شد لطفا منتظر پاسخ باشید") : Exit Function
					
									End if		
						        '	Else
						        	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "با توجه به متراژ طبقه و نوع کاربری شرایط ارسال ندارد")
						        '	End if
						        Else
				'---------------------------------------------------------------------	
							'		if Maskoni=True Then 
								
								   ' if FireReqMessage.Metrage> 999.9 or TmpFloor>4 Then	'sp 459756
							'		if TmpFloor>4 Then	 ResualtFire=Fire.RegisterRequest(FireReqMessage,Info8.User.UserGuid,Info8.User.UserName.ToString) 
									 
							'	Else
							
									if Info8.GetRequest.Info.NidworkItem ="2830546" Then
									
									Else
										if floormanfiYek<0 Then
											 Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)
										ElseIf TmpFloor = tejfloor Then	
											ResualtFire=Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)
											if TmpListManger.CI_ResourceManagerConfirm <> 5 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then
												Info8.AddError(BIZ.SA.EumErrorAction.Stop, "نوجه ","پرونده به اتش نشانی ارسال شد لطفا منتظر پاسخ باشید %") 
											End if
										ElseIf SumGheirMaskooni>= 280 Then
											ResualtFire=Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)
											if TmpListManger.CI_ResourceManagerConfirm <> 5 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then
												Info8.AddError(BIZ.SA.EumErrorAction.Stop, "نوجه ","پرونده به اتش نشانی ارسال شد لطفا منتظر پاسخ باشید %%") 
											End if
										ElseIf TmpFloor-tejfloor>4 Then
											ResualtFire=Fire.RegisterRequest(FireReqMessage,Info8.User.UserGuid,Info8.User.UserName.ToString)
											if TmpListManger.CI_ResourceManagerConfirm <> 5 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then
												Info8.AddError(BIZ.SA.EumErrorAction.Stop, "نوجه ","پرونده به اتش نشانی ارسال شد لطفا منتظر پاسخ باشید %%%") 
											End if	
										End if	
											
									End if			' spp	
								 End if	 'maskoni	
														
						Else
						
								ResualtFire=Fire.RegisterRequest(FireReqMessage,Info8.User.UserGuid,Info8.User.UserName.ToString)
						        
						End if        
				'---------------------------------------------------------------------			        
						        
						       ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Fir" & ResualtFire2.metrage, FireReqMessage.metrage)
							      '  Info8.AddError(BIZ.SA.EumErrorAction.Warning, "befor", "befor")
							      'SP 351595
				' spp			       if Info8.GetRequest.Info.NidworkItem <>"3880950" And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then
							       	
							       	
				' spp			         Fire.RegisterRequest(FireReqMessage, Info8.User.UserGuid,Info8.User.UserName.ToString)
							       '	313755 if TmpListManger.CI_ResourceManagerConfirm <> 5 Then 
							       	' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه!", "%%پرونده به اتش نشانی ارسال شد لطفا منتظر پاسخ باشید | " + ResualtFire.returnCode.tostring)
				' spp			      End if 	 
				' spp		        End if 
						     '   Info8.AddError(BIZ.SA.EumErrorAction.Warning, "After", "After")
						    
				'DP	393578   End if  
				End if
				 End if 	
				End if	   
        Catch ex As Exception
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی&" & ex.Message.tostring , " عدم موفقیت در  ارسال به اتش نشانی ")
        End Try	
       		
        Msgbox("B11","B11")
		              ''''''''''''''''''''''''''''''''
		Dim ResualtFire22 As Biz.Communication.SrvFireFighting.resualt2
		Dim Fire2 As New Global.Biz.Communication.ClsFireFighting
 
'SP 393578		if RequestSubjectFire<> 1  Then 
		
		    ResualtFire22 = Fire2.GetReauestState(Info8.GetRequest.Info.NidWorkItem.tostring ,Info8.User.UserGuid,Info8.User.UserName.ToString)
		    '173167
			if  ResualtFire22.returnCode = 1 or   ResualtFire22.returnCode = 5 Then 
				if ResualtFire22.returnCode = 1 And Maskoni=True And TmpFloor<5 Then
				Else 
				'SP 351595
				'sp 382735
				'	if TmpListManger.CI_ResourceManagerConfirm <> 5 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه.**", "پرونده در حال پیگیری در اتش نشانی می باشید تا پاسخ اتش نشانی اجازه تایید ندارید ") :  Exit Function 
				End if	
			End if	
			'SP 351595
			if  ResualtFire22.returnCode = 3 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<>106 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "پرونده به دلیل" + ResualtFire22.failMessage +" در اتش نشانی تایید نشد اجازه تایید ندارید ") :  Exit Function
		
'SP 393578		End if	
		'''''''''''''''''''''''''''''''''''''''''''''
		
	End if	
	
  End if  	
 
      if String.isNullOrEmpty(StrComments.tostring) Then StrComments = ""
      Return StrComments.tostring
      
Catch e as exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی- FireFighting_Func", e.message)
End Try      

End function