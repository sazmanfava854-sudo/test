<DisplayName("اجراي فرمول")> Public Function Run as clsOut 

Dim NazerAliShahrsazi as Boolean=False 'sp 419604 
Dim tmStr as String=0
Try

Dim kk,tt as Integer



if ucase(Info8.User.UserGuid.ToString()) = ucase("6ADBDE3E-1FA3-42D0-AA28-177C4460BCBB")  Then 

	Dim str as String = ""	
	Dim NidProc = Info8.FindParameter("NidProc")
	
'	Dim SHRequest As New Biz.SH.ClsRequest(1)
'	SHRequest.LoadObj(guid.parse(NidProc))	

   Dim OperationResult As New BIZ.SH.ClsSealedOperation
   Dim OperationList As New List(Of DtoSH.SelSealedOperationListResult)	
	

    Try
		

        OperationList = OperationResult.GetSealedOperationList(Guid.parse(NidProc)) 
    
        Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار",OperationList.count) : Exit Function
  
        if OperationList.count>0 Then 
        		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار"," بدلیل صدور دستور پلمب امکان ابطال نمیباشد. ") : Exit Function
        End if
 
     Catch e as exception
     	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"WW",e.message & " - " & str) : Exit Function
     End Try   	    	
End if

'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"Test22",Info8.FindParameter("NidProc")) : Exit Function


'sp 492304	======   چک کردن اقدام باز سمت مالک نداشتن 
 Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
 LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetRequest.Info.NidProc.tostring)
 Dim i as Integer
  
  if LstObj.data.count>0 Then 
     For i = 0 To LstObj.data.count - 1
		Try
			if LstObj.data(i).NeedToConfirm  Then
				   
				   If CheckMalek(LstObj.data(i).NidCheckList) = False Then
				   	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, " بایگانی ", " اقدام تکمیل نشده سمت مالک دارید - sp 492304") :  Exit Function
				   	'	SaveCheck(LstObj.data(i).NidCheckList,0,2)
				   End If
					                    
		     End if 			        
	    Catch
		End Try        
	 Next
 End If		'end sp 492304	======   چک کردن اقدام باز سمت مالک نداشتن 


if 1 = 2 Then
		Dim BB As New List(Of Global.DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	    BB = Info8.GetFunctions.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
	   
	    BB = BB.where(Function(f) f.EumRequestStatus = 0).toList() 'check Jari bodan
	    if BB.any() Then
	    	if  {"edb5f611-3648-47fd-8808-900700d16983","8d20be44-78ab-4001-bb94-f064d22b9ce1"}.contains(Lcase(BB(0).NidWorkflowDeff.tostring)) Then
	            
	          	  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","Found") 
				  Exit Function
				
			Else
	
				  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","NotFound") 
				  Exit Function
			End if	  
		Else
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار",Info8.Getrequest.GetWorkFlowInstance.NidWorkflowDeff.tostring) 
			Exit Function 
		End if					  
End if
		
'sp 521470

'end sp 521470		


'sp 485506
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring) = ucase("88543dab-f235-4b44-84b4-2776173cf0b4") Then

	if ControlTask("Activity_1atjcx5") = 0 Then  'هر چیزی غیر از بسته شدن 
	'	if ControlCheckList("37e5959a-c385-40ed-99cf-51f2557b6b6c")=2
	'		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","برای گروه های ساختمانی غیر الف تا بعد از بازدید امکان ابطال نمیباشد - sp 485506 ") : Exit Function
	'	End if	
	Else
		Dim CanDelete as Boolean = False
		Dim StrUser as String=""
		
		Try	
		
			StrUser=Info8.GetFunctions.Sec_GetGroupList(Info8.User.UserGuid.tostring).tostring
		Catch e as exception
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار 1",e.message)
			Exit Function			
		End Try 
			
	    Dim GroupMyInfo as Integer=0
	   	Dim ww as Integer
	   
		Dim ErrorUser As New Global.Biz.SA.ClsErrorResult
		ErrorUser = Info8.GetFunctions().ErrorResult 
		if ErrorUser.BizErrors.Count >0 Then
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "خطا:" , ErrorUser.BizErrors(0).ErrorTitel  )
			Exit Function
		End if
		
	
		
		Dim xxx = Newtonsoft.Json.JsonConvert.DeserializeObject(Of Newtonsoft.Json.Linq.JObject)(StrUser)
		For ww = 0 To Convert.ToInt32(xxx.SelectToken("total").ToString) - 1
		
	     '	If ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring  ) = ucase("fd755990-b2f0-44da-b356-2a8ad71a7e2f") or _
	     '	   ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring  ) = ucase("722C8C51-F624-40B9-9623-C9C905C08D06") Then		
		
	     	If {ucase("fd755990-b2f0-44da-b356-2a8ad71a7e2f"),ucase("722C8C51-F624-40B9-9623-C9C905C08D06")}. _
	     		contains(ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring))
	     	   
	     		CanDelete = True
	     		Exit For
	     	End if
	    Next
	    if Not CanDelete Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","ابطال پرونده پارکینگ شهرسازی برای کارشناسان امور پارکینگ ستاد امکان پذیر میباشد - sp 485506") : Exit Function		
	End if
End if	'end sp 485506


'sp 475775
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring) = ucase("b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a") Then
	if Info8.GetRequest.Info.CI_Requester = 13 or Info8.GetRequest.Info.CI_Requester = 14 Then
			Dim TaskByNidProcResponse As New Global.Biz.SC.ClsNewWorkFlow.ClsGetTaskByNidProcResponse
		
			TaskByNidProcResponse=Info8.GetFunctions.WF_GetTaskByNidProc(Info8.GetRequest.Info.NidProc.tostring)
			Dim counter as Integer=0
			Dim ik as Integer=0
			For ik = 0 To TaskByNidProcResponse.data.Count - 1
			    If TaskByNidProcResponse.data(ik).EumTaskStatus = 0 Then 
			    	if TaskByNidProcResponse.data(ik).TaskTitel <> "مالک" Then counter+=1	    
			    End if 
			Next
			
			'if counter > 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار"," در حال حاظر مجاز به ابطال نیستید (وجود نود باز) - sp 475775 ") : 	Exit Function	
	End if			
End if 


if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring) = ucase("24b35887-28e0-45c5-8952-b8e796067cfc") Then

	Dim flag as Boolean = False
	Dim TmpListManger,TmpListMan As New List(of DtoSC.Sh_ManagerConfirm)
	TmpListMan = Info8.GetManagerConfirm.Sh_ManagerConfirm_List()
	TmpListManger = TmpListMan.where(Function(f) f.CI_ResourceManagerConfirm = 1 And (f.EumManagerConfirmLicence=1 or f.EumManagerConfirmLicence=2)).ToList
	TmpListManger = TmpListManger.OrderBy(Function(x) x.ConfirmationDate).ToList()  ' اختمالا در آینده برای اینکه ممکنه تو یک روز تایید و عدم تایید بشه مشکل ایجاد بشه اونجا بر اساس ساعت هم باید چک بشه 
	
	Dim List_NidProc As New List(Of Global.DtoSC.Commission_Vote)

    List_NidProc = Info8.GetCommission_List().GetCommission_Vote()


	If TmpListManger.count > 0 Then										'کلا این شرط برای این هست که درخواستی که لایحه جریمه نداشته باشه هیچ جوره پیش نمیاد بره کمیسیون برای همین بررسی شد 
		if TmpListManger.last.EumManagerConfirmLicence=1 Then			'و اینکه اگه جریمه داشت ولی ابطال نبود فرقی نمیکنه دیگه کمیسیون داشته باشه یا نه  این متن خطاها برای اینه که 
		If List_NidProc.count > 0 Then									'دقیق مشکل خطا بدونن چیه
		    Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار"," درخواست رای کمیسیون دارد مجاز به ابطال نیستید // ")
			Exit Function				
		Else 
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار"," درخواست تایید لایحه دارد مجاز به ابطال نیستید ** ") 
			Exit Function
		End if	
		End if
	End if	
	For kk = 0 To Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid).Count - 1 
			if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(kk).GUID.ToString()) = Ucase("4a58bd9f-df2a-4a15-b8a7-327d41542697").toString Then    '4a58bd9f-df2a-4a15-b8a7-327d41542697 
				flag = True	
				'Exit Function			
			End if
	Next
	if flag=False Then			
				Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار"," $$ شما مجاز به ابطال درخواست نیستید ") 
				Exit Function
	End if				
'	msg("flag1",flag.tostring)
End if
 
if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=66 Then
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","درخواست هيئت فني ، مجاز به ابطال نمی باشید ++") 
	Exit Function 	
End if
'sp 407680

if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("b2f0cc45-b0f2-4a1f-a1af-9b8120e4f58a") Then
	For kk = 0 To Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid).Count - 1 
		if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(kk).GUID.ToString())=ucase("57273cb4-83f4-4aaf-9954-a3bbfde832e5") Then
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار"," ++ مجاز به ابطال نمی باشید") 
			Exit Function 	
		End if 	
	Next
End if 		
'tmStr = Info8.FindParameter("Nidrevisit")
Dim tmStrTablet as Integer=0
Dim ii as Integer=0
Dim cc as String
'skyp
'IF Info8.getRequest.Info.IsSara10Request  Then
'Else
	cc=Info8.User.UserGuid.tostring 
	
	if ucase(cc) <>ucase("eef07f67-63d9-41d6-90d6-e6bf2d7c9a57") Then 
	Try
	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"a","a") 
		For ii = 0 To Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid).Count - 1
		 
			if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = "63367D12-E1BD-443B-BF29-9FBC6B27C26E" Then tmStr=1
			'299983
		
			if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = "05AED9EE-74D6-4300-A1B3-C7D37CFB15AF" Then tmStr=1
			if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = "08D20B53-D68A-4D22-A0E0-B43EEC51461E" Then tmStr=1 'SP412269
			if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = "3A718066-1C33-445E-9B03-E0277F6CC756" Then tmStr=1
			'299983
			if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = "055EE619-0E97-4454-961F-32805805DD09" Then tmStr=1
			if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = "3EA579D1-68D0-406B-BDD5-C5FBCFD1C1E8" Then tmStr=1
			if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = "77C2E62A-ACDC-4471-9B31-5FF6245F4592" Then tmStrTablet=1
		    if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = "8364897D-8836-4004-866D-514959163786" Then NazerAliShahrsazi=True
		    if Ucase(Info8.GetFunctions.GetUserGroups(Info8.User.UserGuid)(ii).GUID.ToString()) = Ucase("fa28aa51-9523-489e-8197-af33156fb8c6") Then NazerAliShahrsazi=True  'sp 502156
		Next
		Catch ex as Exception
		tmStr=0	
		 Info8.AddError(BIZ.SA.EumErrorAction.Stop,"newff",ex.tostring) 
		 End Try 
	Else
		tmStr=0
		'sp 399093
	'	tmStr=1
		tmStrTablet=1
	End if	
'End IF  

 Try
 

' 	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"b","b") 
 IF Guid.Empty=Info8.GetRequest.Info.NidRevisit Then
   
 Else
 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"c","c") 
if Info8.GetRequest.Info.NidRevisit.tostring<>"" And Info8.GetRequest.Info.NidRevisit.tostring<>"00000000-0000-0000-0000-000000000000" Then
	 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"cc","cc") 
'	if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="5A659F4C-8DC6-4283-A496-BD20C952A810" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="102D761E-C698-4C9B-A5A9-C6D1B57EBBCC" _
	'	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA" _
	'	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="F5B285CC-8792-498C-A6C0-0D1571EEB793" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="50E0B7EF-E10C-4727-BF96-4399B6A37196" _
	'	or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="E5F7F86A-0D02-4A01-A01C-19F13F6BeDB9" or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="D0127432-AA14-4A16-BA07-D6A6C7DF5C5D"  or Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="53E7F83A-59A2-4C0D-805A-7BE86E1DC56D" Then
'if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=107 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=108 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=83  _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=104 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=111 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=99  _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=98 	or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=109 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=100 _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=110 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=99  _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=81  _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=97 	or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=97  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=92  _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=70 _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=102 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=99  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=9   _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=30 	or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=102 _
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=97  _ 
'or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=31  _

		' 	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"ccc","ccc") 
		    Dim lstP As List(Of Biz.SA.ClsNosaziCodeType)
		   
		    lstP = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		   
		    lstP = lstP.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
		   
		    Dim GuidBase As Guid = Info8.GetFunctions.GetNidBase(lstP(0).NidNosaziCode, Biz.SA.ClsEnum.EumBaseInfoGroup.Revisit, Biz.SA.ClsEnum.EumRevisitGroup.Revisit, Info8.GetRequest.Info.NidRevisit)
	        	Info8.ParameterList.REMove("Revisit_House_NidBase")
	        	Info8.ParameterList.Add("Revisit_House_NidBase", GuidBase.ToString)
        		Dim tablet as Integer=-1
        	 
			 Try
					if UseBazdidT()=0 Then tablet=Info8.GetFunctions.GetRevisitTabletTask(Info8.GetRequest.Info.NidProc).EumTaskState
				Catch ex As Exception
					tablet=-1
		      End Try
		      
		     ' 	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"cccc","cccc") 	
	
			if  tablet=-1 And Info8.GetRequest.Info.NidWorkItem="1533776" Then 	tablet=3
				if tablet<>3 Then 
					If UseBazdidT()=0 Then
			    
				'	if Info8.GetRevisit_House.GetBase_HouseInfo.CI_ParvanehType<>3	Then  
						if tmStrTablet=0 Then
					'120247		if Set_UserGroupId(Info8.User.UserGroups) =1 Then	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبار سنجی", "تا زمانیکه بازدید تبلت به شهرسازی برگشت داده نشده است امکان ابطال ندارید")
						End IF
					End if
				End if
	
		End if
	End if	
'End IF	
	 Catch ex as Exception
	 Info8.AddError(BIZ.SA.EumErrorAction.Stop,"aaa",ex.tostring) 
	 End Try 
	  '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"d","d") 

	if tmStr=0 Then 
	
		if Fiche()=1 And NazerAliShahrsazi=False Then
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار : ","دارای فیش درامد می باشد مجاز به ابطال نمی باشید **") 
		ElseIf Comm100()=1 Then 
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","دارای رای کمیسیون ماده 100 می باشد مجاز به ابطال نمی باشید **") 
		End if
	End if 

Dim ErrorR As New Global.Biz.SA.ClsErrorResult		
 if Info8.getRequest.Info.IsSara10Request  Then 
 	
 	if ControlTask("Activity_056rjh9")=1 or ControlTask("Activity_0mfimf6")=1 or ControlTask("Activity_0r2poh1")=1 _
 	or ControlTask("Activity_0vcpof4")=1 or ControlTask("Activity_0ygetjc")=1 or ControlTask("Activity_13z3e1z")=1 _
 	or ControlTask("Activity_18th9bt")=1 or ControlTask("Activity_1e7bq44")=1 or ControlTask("Activity_1jicsxf")=1 _
 	or ControlTask("Activity_1jz9ix6")=1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","دارای بازدید فعال می باشد مجاز به ابطال نمی باشید --" ) : Exit Function
 	'''''''''''''''''''''''''''''''''
 	
 
 	if ControlTask("1")=1 or ControlTask("Activity_07mtpus")=1 or ControlTask("Activity_0fx4od8")=1 or ControlTask("")=1 _
 	or ControlTask("Activity_0rnk5qw")=1 or ControlTask("Activity_0x0ylrz")=1 or ControlTask("Activity_12ebsu9")=1 _
 	or ControlTask("Activity_1ipj02l")=1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","دارای بروکف فعال می باشد مجاز به ابطال نمی باشید //") : Exit Function

 End if 
 
 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار",ErrorR.BizErrors.count.tostring ) : Exit Function
 
	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
   	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Info8.GetRequest.Info.NidProc)
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
    Dim ss as Integer =0
    For ss=0 to lstOwner.count-1
    	Sms(lstOwner(ss).OwnerMobile,"ابطال درخواست"," شهروند گرامی: پرونده شما به شماره ارجاع " & Info8.GetRequest.Info.nidworkitem.tostring & " ابطال گردید - شهرداری مشهد") 
    Next
    Sms(Info8.GetRequest.Info.Cellphone,"ابطال درخواست"," شهروند گرامی: پرونده شما به شماره ارجاع " & Info8.GetRequest.Info.nidworkitem.tostring & " ابطال گردید - شهرداری مشهد") 
	
 	
'IF Info8.User.UserGuid.tostring="6f99c240-c92b-4fb3-99db-6760750167eb" Then 	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","ffff") 
 Catch ex as Exception
Info8.AddError(BIZ.SA.EumErrorAction.Stop,"exNewNN" &  ex.tostring ) 
 
End Try 
 End Function

Public Function Fiche() as Integer
Try

	Dim chek as Integer=0
    Dim AllFiche  As List(Of DtoSC.Income_Fiche)  
       
    AllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse (Info8.FindParameter("Nidproc")))
                 
    if AllFiche.any Then        
    	if AllFiche.Any Then  AllFiche = AllFiche.Where(Function(f)   f.EumFicheStatus=3 or f.EumFicheStatus=5).ToList() '7
    	if AllFiche.Any Then    		chek=1
    	'f.EumFicheStatus=2 or چاپ   194223
  		
    End if		
   ' Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار",  Info8.FindParameter("NidIncome")) 
   ' Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار",  Info8.GetIncome.GetIncome_Calculation(0).CI_IncomeCalculation) 
  
  '  if chek =0 Then 
  '  	Info8.ParameterList.REMove("NidIncome")
  '  	Info8.ParameterList.Add("NidIncome", AllFiche(c).NidIncome.tostring)
'					
'	        			AllCalculation = Info8.GetIncome().GetIncome_Calculation_InNidIncome(AllFiche(c).NidIncome)
            
   Return  chek                
              
 Catch  ex as Exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","فیش های درآ مدخطا " &  ex.tostring ) 
 
End Try 
End Function 

 Public Function Comm100() as Integer
Try

	Dim chek as Integer=0      
	Dim AllVoteList As New List(Of DtoSC.Commission_Vote )
    Dim tmps = Info8.FindParameter("NidProc")
	Dim List_NidProc As New List(Of DtoSC.SrvGetCommissionList_InNidProcResult)
	List_NidProc = Info8.GetCommission_List().CommissionList_InNidProc
	
	Dim i as Integer=0
	
	For i=0 to List_NidProc.count-1
		Info8.ParameterList.Remove("Commission_NidCommission")
		Info8.ParameterList.Add("Commission_NidCommission", List_NidProc(i).NidCommission.ToString)
	   Info8.GetCommission.GetAllVote(Guid.Parse(tmps))

		AllVoteList = Info8.GetCommission.Commission_Vote_All 'Info8.GetCommission.Commission_Trepass_All
		if AllVoteList.count>0 Then
			chek=1 
			Return  chek  
	   End if	 
	Next
   Return  chek                
              
 Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","ارائ کمیسیون") 
 
End Try 
End Function                  
 Private Function UseBazdidT () as Integer

Try
	Dim i as Integer=0
	Dim TlstR As List(Of DtoSC.Sh_Revisit)
	TlstR = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	
    If TlstR.Count > 0 Then  
    	TlstR = TlstR.Where(Function(f) f.NidRevisit = Info8.GetRequest.Info.NidRevisit).ToList
    Else
    		Return 0 
    End if	
    
    if TlstR.Count=0 Then 	Return 0 
    Dim UserList As List(Of Biz.SA.SrvSecurityWCF.UserGroup)
   	
    Dim a=TlstR(0).NidRevisitAgent

    Dim b = Info8.GetRevisitAgent.GetSh_RevisitAgent(a).NidUser
    
    UserList =  Info8.GetFunctions.GetUserGroups(b)
 
For i=0 to UserList.count-1 
	IF Ucase(UserList(i).GUID.tostring)="5F4030CE-99FB-4731-892B-0D0F525A910E"  Then Return 1
Next
Dim TabletB() as String = {"CBC70236-F6DC-4D16-AD9D-02527170C473","50F22591-BAB7-487D-B1C4-06A86ACBAD06",
						  "7F260B4B-EA24-4060-B128-06BA986703E9","CF39113D-7E70-4D6F-A41C-19668244BE69",
"2AFE139A-374E-4CEC-B779-1FCC51C06B58","FA8FBD28-A413-4D86-99E9-22B5F6B6BCF4",
"1DC46692-0F35-4129-A42E-25F072A994FC","D8BF2867-ECEA-4B88-8D54-307F97C4B734",
"02917896-8BC8-44AD-91DA-35A311619D84","25725B9D-48CF-4502-A70B-3CCDE913A767",
"001F1D2C-1597-4C7B-BF95-4D77061253FF","7385A97D-FB66-440A-BE54-4F4A3A959B01",
"2FDE2544-9CD2-4C45-AF17-57025A2307CC","AFAB977F-B75E-42F0-BB34-5EB3396CFB24",
"F22B360F-949F-42AC-BEB3-60F985CC5766","7248B372-00C7-4264-A63C-6620756CBDFC",
"2FEA51E2-424C-4A24-81D5-69C52A7CD10E","BE74782A-6C69-4E6A-8DF7-7793A339ACD4",
"7CE6F8B6-6B37-4F4F-BDD5-831E68E462EE","98ADB216-BC4F-4F93-876E-840547A0C9DA",
"371855B6-80BE-44C3-AFBD-843B224784FC","2EFCCA1D-D0C1-47C7-85E3-886A3C3A53D8",
"4E6E828F-0FC9-4E23-9F1F-8872930CF8A4","E0C6069E-1B6F-470E-AC4A-898010EA23D8",
"59B2F424-AFF2-4FFD-A842-93B6BE1EA9D4","8095EC07-F028-47E0-A3A9-9E964E5DEF4C",
"AA4ABA9B-2014-4463-BE98-A26CF158E11C","EC363FE1-BDD9-4FA7-934A-A8402F56A092",
"6F728723-4F37-44E6-8F9D-A9C5B3E2F544","50A7C193-3EE4-4575-87F5-AEF8F4C70899",
"E7DB882E-7394-4773-BEFB-BC54275CCE13","13B37055-8CA2-44D7-A770-BE85064BD121",
"892DA199-3509-49D7-BD59-C7CA791BFFDD","AFA166E6-595D-4623-9533-C95B8F31766E","6E3B6547-7F2A-4B30-BEA5-110A4412A915",
"D6A3779B-1B7B-41A5-90B6-CC6C3ECFFDF0","D8015082-F770-44BA-8B19-D93DCF680760","7A496558-E933-483E-B31A-DAE6E3D29DD5",
"487E1212-300B-414E-A7E9-E337079DC29A","98195966-5B41-4018-AD49-F1EF84DBCBCD","124BFE89-53D7-4F80-987C-F69CB20A0646"}

'For Each b As String In TabletB
	'	    If Ucase(b) = Ucase(Info8.GetRevisitAgent.GetSh_RevisitAgent(TlstR(0).NidRevisitAgent).NidUser.tostring) Then Return 1
	'	 Next  

	
	Return 0 
 Catch Ex As Exception
 			Return 0 
    '    Info8.AddError(BIZ.SA.EumErrorAction.warning, " گروه تبلت  ", Ex.Message)
    End Try
    ''''''''''''''''''
End Function	

Private Function GetIdWorkflow (ByVal NIdWorkF as String) as Double
Try 


'Msgbox("WorkFlowGuid" , Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)) 
	Select Case Ucase(NIdWorkF)
 
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
		'استعلام تجدید اجاره آپارتمان/صنف
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
		Case "86E86F40-F36C-42FE-991F-8DDAD743E6ED","61935FDF-D85C-4946-9BA2-DFB7FC1E1322" 
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
		Case "D6F25EBA-45A5-4851-ACED-7EED0105E977","B3407F6F-967D-47DE-A8EB-9A7AB98BDA75","6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9" 
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
		Case "18313B50-F76D-428A-BAE4-134A918B2600","8ECF01D4-A93C-44A7-B52B-4685634C54C6","D8A215F8-2E28-45A0-A826-5ED886F16CCF","68C87B74-097E-4EC0-83D1-7723C403B1F8","F32B14FF-E1E8-479B-8A9B-979BF74F7579","5A659F4C-8DC6-4283-A496-BD20C952A810","E5F5C5A4-F51E-4ED1-80E5-E6A11853C746","097C8C41-BA8C-4B60-8B2F-6BED0221F573" 
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
			'تقاضاي پلمپ - رفع پلمپ شهرداري - حکم ورود قضايي - برای سمپا 459572	
		Case "edb5f611-3648-47fd-8808-900700d16983" ,"ED9A2CC8-8471-4879-9F5A-55D785D1DF1E"	
			Return 113
	End Select

Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"GetIdWorkflow", "اجرای فرمول GetIdWorkflow")
End Try
End Function

Public Function ControlTask(ByVal NIdNod as String) as Integer

		Dim TaskByNidProcResponse As New Global.Biz.SC.ClsNewWorkFlow.ClsGetTaskByNidProcResponse
		Dim strClose as String =""
	
		TaskByNidProcResponse=Info8.GetFunctions.WF_GetTaskByNidProc(Info8.getRequest.Info.NidProc.tostring)
		Dim counter as Integer=0
		Dim Colescounter as Integer=0
		Dim Opencounter as Integer=0
		Dim ik as Integer=0
		For ik = 0 To TaskByNidProcResponse.data.Count - 1
		    If TaskByNidProcResponse.data(ik).NidNode = NIdNod Then Opencounter+=1	     
		Next
		
		For ik = 0 To TaskByNidProcResponse.data.Count - 1
		    If TaskByNidProcResponse.data(ik).NidNode = NIdNod Then
		    	if  {1,3,5,6}.contains(TaskByNidProcResponse.data(ik).EumTaskStatus) Then Colescounter+=1    
		    End if	
		     
		Next
		
		if Colescounter=Opencounter And Opencounter =0 Then counter=0  'وجود ندارد
		if Colescounter<Opencounter Then  counter=1 ' وجود دارد
		if Colescounter=Opencounter And Opencounter <>0  Then  counter=2 ' بسته شده 
		        
		        ''''''''''''''''''''''''''''''''''''''
		 	
		 Return counter
	
End Function

Public function ControlCheckList(ByVal NIdCheckList as String) as Integer
  Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
  LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetRequest.Info.NidProc.tostring)
  Dim i as Integer
  
  if LstObj.data.count>0 Then 
     For i = 0 To LstObj.data.count - 1
		Try
			   If ucase(LstObj.data(i).NidCheckList.tostring) = ucase(NIdCheckList) Then
			   		Return LstObj.data(i).CI_CheckListValue
			   End If
	    Catch
	    	Return 0
		End Try        
	 Next
 End If		'end sp 492304	======   چک کردن اقدام باز سمت مالک نداشتن  
End function

Private Function msg(ByVal a as String , ByVal b as String)
 if ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop,a,b)
 	Exit Function
 End if
End function


'sp 492304
Public Function CheckMalek( ByVal NidCheckList as String ) as Boolean 
 
  Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
  LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetRequest.Info.NidProc.tostring)
  Dim i as Integer
  
  if LstObj.data.count>0 Then 
     For i = 0 To LstObj.data.count - 1
		Try
			if ucase(LstObj.data(i).NidCheckList) <> ucase(NidCheckList)   Then
		 
			   		Dim OwnerConfirm as Integer = 0
			   		Try
				        OwnerConfirm= LstObj.data(i).OwnerConfirm
					Catch
						OwnerConfirm= 0
					End Try
			   
			   If String.IsNullOrEmpty( LstObj.data(i).CI_CheckListValue) Then
			   	
			   ElseIf LstObj.data(i).CI_CheckListValue=2 Then
			   		If OwnerConfirm =0 And  LstObj.data(i).NeedToConfirm  Then 	Return False
			   End If
				                    
		  End if 
		          
	    Catch
		End Try        
	 Next
 End If
 	
Return True	
End Function			'end sp 492304
Public Function SaveCheck(ByVal NidCheckList as String, ByVal CheckListValue as Integer,ByVal OwnerConfirm as Integer, Optional ByVal Com_M as String="سمپا 492304")

Try
		
		Dim SaveCheckList As New Global.Biz.SC.ClsNewWorkFlow.ClsSaveCheckListValueMessageContract
		
		SaveCheckList.CI_CheckListValue = CheckListValue.tostring
		SaveCheckList.Comments=Com_M '"تایید سیستمی"
		
		SaveCheckList.NidCheckList=NidCheckList.tostring
		SaveCheckList.NidProc=Info8.GetRequest.Info.NidProc.tostring 
		SaveCheckList.NidUser="EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"
		SaveCheckList.FullUserName ="تایید سیستمی"
	
	   ' SaveCheckList.OwnerConfirm =OwnerConfirm.tostring
	    SaveCheckList.OwnerComments="تایید سیستمی"
	  '  if ucase(NidCheckList)=ucase("799f97bf-7531-4a00-b1e8-7631a09cf7f3") or  ucase(NidCheckList)=ucase("0d9d7760-cc20-4f14-a9d9-1f81b4b7010e") Then SaveCheckList.OwnerComments="کاربر محترم لطفا مبلغ پرداختی اول را وارد نمایید"		
		Dim result as Boolean = Info8.GetFunctions.WF_SaveCheckListValue(SaveCheckList)
		
Catch e as exception
		 Info8.GetOnTickChecklistParameter().Result="false"
		 Info8.GetOnTickChecklistParameter().Description= "SaveCheck Func: " + e.message	   
		 Exit Function
End Try
		    	

		     		    
End Function	'end sp 492304



Public Function Sms(ByVal Number as String , ByVal SMSType as String,ByVal Text as String) 
	Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
	tmpSMS.BizCode =Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	tmpSMS.Number =Number
	tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
	tmpSMS.SMSType = SMSType '"مامور "
	tmpSMS.Text =Text '"شهروند گرامی : کارشناسی ملک شما توسط آقا/خانم" & NameUser & "در تاریخ " & lstR_T(0).RevisitDate & " انجام و نتیجه حداکثر تا یک روز کاری بعد از بازدید اعلام خواهد شد شماره همراه مامور بازدید جهت هماهنگی " & Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Phone & " شهرداری مشهد"
	tmpSMS.UserID = "EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"
	tmpSMS.UserName = "Esup"
	Dim smsErrorResult As New BIZ.SA.ClsErrorResult
	
	smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, " aa",  smsErrorResult.BizErrors.Count )
'	if smsErrorResult.BizErrors.Count>0 Then 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, " bb", smsErrorResult.BizErrors(0).ErrorTitel  )
End Function