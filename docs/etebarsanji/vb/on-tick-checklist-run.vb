<DisplayName("اجراي فرمول")> Public Function Run as clsOut  

'msg("123")
'Exit Function


'	Info8.GetOnTickChecklistParameter().Result="false"	
	'Info8.GetOnTickChecklistParameter().Description =Info8.GetOnTickChecklistParameter().NidUser
	'Exit Function
'if Info8.GetOnTickChecklistParameter().NidTask is Nothing Then

'	Info8.GetOnTickChecklistParameter().Result="false"	
'				Info8.GetOnTickChecklistParameter().Description =Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring'"g"
'Exit Function
'Else

'End if 
 AssingTo =""
 AssingToUserName=""
 AnaliysParvaneh_Date =""
 AnaliysParvaneh_time =""
 Agreement_time =""
 Agreement_Date =""
 BaseU=New List(Of Global.DtoSC.Base_Using)
 
'Info8.GetOnTickChecklistParameter().Result="false"  
'If String.IsNullOrEmpty( Info8.GetOnTickChecklistParameter.Comment) Then  
'	Info8.GetOnTickChecklistParameter().Description="D"
'Else            
                  
'Info8.GetOnTickChecklistParameter().Description=Info8.GetOnTickChecklistParameter.Comment
'End if
'Exit Function
	
Info8.GetOnTickChecklistParameter().Result="true"

'msg("T")


Try  
  
  Info8.ParameterList.REMove("NidProc")
  Info8.ParameterList.Add("NidProc", Info8.GetOnTickChecklistParameter().NidProc.tostring)   
Catch
End Try 



  Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
Try


  TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))
Catch 
	'etesal b Patrolman
'javaheri	Dim SHRequest As New Global.Biz.SH.ClsRequest(1)  '(1) Get district I dont know hows it work now!!!
'javaheri	SHRequest.LoadObj(guid.parse(Info8.GetOnTickChecklistParameter().nidproc.tostring))	
	
	'CodeNosazi 	
'	 Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType  
'javaheri	 TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(SHRequest.Request.NidNosaziCode_Sara))

'	Try
'	
'		Dim SHRequest As New Global.Biz.SH.ClsRequest(1)  '(1) Get district I dont know hows it work now!!!
'		SHRequest.LoadObj(guid.parse(Info8.GetOnTickChecklistParameter().NidProc))	
'	    TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(SHRequest.GetRequest.NidNosaziCode_Sara))
	    
'	Catch 	
'	End Try

End Try  


' sp 359885


 Try       

If TmpNosazi.EumNosaziCodeObjType = 4 or TmpNosazi.EumNosaziCodeObjType = 3 Then
	If TmpNosazi.EumNosaziCodeObjType = 4 Then asobject = 4
	if TmpNosazi.EumNosaziCodeObjType = 3 Then asobject = 3
	Try
		Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
	    Info8.ParameterList.Add("Revisit_Apartment_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    
	    
	   	Dim TmpErrorg As New Global.Biz.SA.clsBizError
		Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
		Dim ListParent_M As List(Of BIZ.SA.ClsNosaziCodeType)
	
	 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)
	 	ListParent_M=ListParent
	    ListParent = ListParent_M.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
	    Info8.ParameterList.Remove("Revisit_House_NidBase")
	    Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(ListParent(0).NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
		ListParent = ListParent_M.Where(Function(f) f.EumNosaziCodeObjType = 3).ToList
		Info8.ParameterList.Remove("Revisit_Building_NidBase")
	    Info8.ParameterList.Add("Revisit_Building_NidBase", Info8.GetFunctions.GetNidBase(ListParent(0).NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    
	    Base_Using_Buidling = Info8.GetRevisit_Building().GetBase_Using()
	    Base_Using_House=Info8.GetRevisit_House().GetBase_Using()
	    Base_Using_Apartment=Info8.GetRevisit_Apartment().GetBase_Using()
	Catch
	End Try 

ElseIf TmpNosazi.EumNosaziCodeObjType = 5 Then
	asobject = 5
	Try
		Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
	    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	    
	    
	   	Dim TmpErrorg As New Global.Biz.SA.clsBizError
		Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
	 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)
	 	
	 
	    ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
	    Info8.ParameterList.Remove("Revisit_House_NidBase")
	    Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(ListParent(0).NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
		
		Base_Using_HouseSharing=Info8.GetRevisit_HouseSharing().GetBase_Using()
	    Base_Using_House=Info8.GetRevisit_House().GetBase_Using()	
	Catch
	End Try 

ElseIf TmpNosazi.EumNosaziCodeObjType = 2 Then
	asobject = 2
    Info8.ParameterList.Remove("Revisit_House_NidBase")
    Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	Base_Using_House=Info8.GetRevisit_House().GetBase_Using()
End If

Catch
End Try

''***********************************
''**********************************
'if (Info8.GetOnTickChecklistParameter().TickType=1 or Info8.GetOnTickChecklistParameter().TickType=2) And (Info8.GetOnTickChecklistParameter.NidNode="Activity_0db5ew3" or Info8.GetOnTickChecklistParameter.NidNode="Activity_0lu3kns" _
'or Info8.GetOnTickChecklistParameter.NidNode="Activity_0reckeu" or Info8.GetOnTickChecklistParameter.NidNode="Activity_12gt6gw" _
'or Info8.GetOnTickChecklistParameter.NidNode="Activity_1dngzpz" or Info8.GetOnTickChecklistParameter.NidNode="Activity_1nci1wn" _
'or Info8.GetOnTickChecklistParameter.NidNode="Activity_1tge8on" or Info8.GetOnTickChecklistParameter.NidNode="Activity_1wrddxk" )Then
'	 Info8.GetOnTickChecklistParameter().Result="false"                       
'	 Info8.GetOnTickChecklistParameter().Description="مجاز به تغییر دیتا مالک نمی باشید" 
'End if 
'Dim eee As New Global.Biz.SA.ClsErrorResult
     
   
'Dim ListMangerConfimaaatest As New List(Of DtoSC.Sh_MokatebatConfirm)
' ListMangerConfimaaatest = Info8.GetMokatebatConfirm.GetSh_MokatebatConfirm_List()
''Dim   eee11 = Info8.ErrorResult.BizErrors.Count 
 ' Info8.GetOnTickChecklistParameter().Result="false"	
 '  Info8.GetOnTickChecklistParameter().Description = eee11

'Exit Function
if Info8.GetOnTickChecklistParameter().NidProc.tostring= "70bff2c0-274e-4c4e-9692-e48ef4a39541" Then 
Try 
   Dim ssss As Global.Biz.SC.ClsEngineer
   Dim kkk As New Global.Biz.SC.SrvEngineering.ClsFil_Info
   kkk = ssss.GetFilInfo(Info8.GetRequest.Info.NidProc, Info8.User)
   Info8.GetOnTickChecklistParameter().Result="false"	
   Info8.GetOnTickChecklistParameter().Description = kkk.Fil_Info.NidWorkItem
   
   Catch ex As Exception
      Info8.GetOnTickChecklistParameter().Result="false"	
	  Info8.GetOnTickChecklistParameter().Description =ex.message
   End Try    
End if 



'If ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)=ucase("2635fe18-fe4c-4a2d-9d05-993756751262") Then
' Call NewParvaneh()
'End if

if Info8.GetOnTickChecklistParameter().TickOwner =1  Then  ' تایید مالک 

If ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)=ucase("2635fe18-fe4c-4a2d-9d05-993756751262") Then
 Call NewParvaneh()
End if

	Call ConfirmOwner()
ElseIf Info8.GetOnTickChecklistParameter().TickOwner = 2 Then  '  عدم تایید مالک
	
	
If ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)=ucase("2635fe18-fe4c-4a2d-9d05-993756751262") Then
 Call NewParvaneh()
End if

	Call UnConfirmOwner()
ElseIf 	Info8.GetOnTickChecklistParameter().TickType =1 Then   ' تایید شهرداری

If ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)=ucase("2635fe18-fe4c-4a2d-9d05-993756751262") Then
	
 Call NewParvaneh()
End if
		
	Call Confirm ()	
	
ElseIf Info8.GetOnTickChecklistParameter().TickType =2  Then   ' عدم تایید شهرداری

   If ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)=ucase("2635fe18-fe4c-4a2d-9d05-993756751262") Then
 Call NewParvaneh()
End if

	Call UnConfirm ()
	
Else

End if



if ucase(Info8.Getrequest.Info.NidWorkFlowDeff.tostring) = ucase("e7a80e33-172e-4e3c-8ba5-88a5eb69f146") Then 

	Call NewParvaneh_ToseEh
	
End if	

End Function
 


'-----------\\\\\\\  Sajedi 1403-03  //////---------------- 
Public Function NewParvaneh() 

Info8.GetOnTickChecklistParameter.Result = "true"

if Info8.GetOnTickChecklistParameter().TickOwner =1  Then  ' تایید مالک 	

'----------------  پرداخت فاکتور نقشه برداری توسط مالک
'sp 485569	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("c5281b9f-fbe0-40de-a8f2-a263a2236569") Then
'sp 485569	Call CopyNodeChecklistToAnotherNode()
			
'sp 485569	End if	



'---------------- متقاضی پرداختی اول پروانه توسعه 
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("67b9c53e-ce96-4ad0-be37-d72375391d6c") Then
	
		AssingTo = ""
		AssingToUserName = ""
		Call User(5)

		if ControlTask("Activity_054nk0o")=0 or ControlTask("Activity_054nk0o")=2  Then CreatTask("Activity_054nk0o",AssingTo,AssingToUserName)
			
	End if	
	
'------- کنترل پرداخت فیش نوسازی و تشکیل نود 	
'sp 487040
'sp 487040	IF ucase(Info8.GetOnTickCheckListParameter().NidChecklistValue.tostring)=ucase("ab53ee36-f006-406e-b9d4-9d6ae2675e9f") Then
		
'sp 487040		AssingTo = ""
'sp 487040		AssingToUserName = ""
'sp 487040		Call User(3)
'sp 487040		CreatTask("Activity_0zajk2p",AssingTo,AssingToUserName)
'sp 487040		Dim LstChild As New List(Of Global.Biz.SA.ClsNosaziCodeType)
'sp 487040		LstChild = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
'sp 487040		LstChild = LstChild.Where(Function(f) f.EumNosaziCodeObjType = 6).ToList
'sp 487040		if LstChild.count>0 Then SaveCheck("b2f4be86-7af7-43c7-9352-3ce03b513e7a",2,0)
'sp 487040	End if

'------- تایید اعلام ضابطه و تشکیل چک لیست جرم گیری
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("4c323663-2238-465e-930d-d8fb5d579e61") Then
		SaveCheck("d7e5337e-ee9f-47b0-ae17-16da276d5d03",2,0)
	End if	
			

'-----------تایید جرم گذاری و تشکیل تود جرم گذاری
'sp 497580	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("d7e5337e-ee9f-47b0-ae17-16da276d5d03") Then
	
'sp 497580		Call User(9)
'sp 497580		CreatTask("Activity_10x9i62",AssingTo,AssingToUserName)
'sp 497580		Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است. ",2)
		   		
'sp 497580	End if	

ElseIf Info8.GetOnTickChecklistParameter().TickOwner = 2 Then  '  عدم تایید مالک

				
'sp 472382 '------- تایید اعلام ضابطه و تشکیل چک لیست جرم گیری
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("4c323663-2238-465e-930d-d8fb5d579e61") Then
		SaveCheck("d7e5337e-ee9f-47b0-ae17-16da276d5d03",2,0)
	End if					
'end sp 472382

	'sp 461077		چک لیست پرداخت فاکتور نقشه برداری 
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("1e1db1b4-12b5-4224-b466-81d7e67e4ac1") Then		
		SaveCheck("1e1db1b4-12b5-4224-b466-81d7e67e4ac13",2,0)
	End if	
	
	
				'\\\\\\\\\\\\\\\\\ 	درآمد ناظر 	//////////////////////


	'------------ عدم تایید جرمگیری و تشکیل نود ناظر
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("d7e5337e-ee9f-47b0-ae17-16da276d5d03") Then
				Call User(1)
				CreatTask("Activity_0uc9dny",AssingTo,AssingToUserName)
				Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است. ",2)
			End if
	
ElseIf 	Info8.GetOnTickChecklistParameter().TickType =1 Then   ' تایید شهرداری

Info8.GetOnTickChecklistParameter.Result = "true"

		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("23d6e636-9b56-4200-9ef9-d2237b2955f6") Then

		'	Call SendAmlakMazaya
			'Call Payamak("چک لیست","Test")
		End If

'sp 461077
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("c0b582dd-08c8-4e82-942b-88b47d6d18ec") Then
	SaveCheck("c5703e93-ada3-43a7-a8fe-e17acd9d3a53",2,0)
End if
	

			'\\\\\\\\\\\\\\\\\ 	ناظر شهزسازی 	//////////////////////	
	'-------کنترل اعلام ضابطه وکمیسیون ماده 5	
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("b6315a90-eb61-4cc3-96a0-4adc061aa435") Then

	Dim b as Integer = 0
	b = Agreement()
	
	Dim BaseUNew As New List(Of DtoSC.Base_Using)
	BaseUNew= BaseU.where( Function(f) f.FloorNo < 80).tolist
	
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 530 And f.CI_UsingType <> 853 And f.CI_UsingType <> 746 And f.CI_UsingType <> 751 And f.CI_UsingType <> 855 And f.CI_UsingType <> 1002 And f.CI_UsingType <> 1014 And f.CI_UsingType <> 1026).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f16 And f.CI_UsingType <> 2038 And f.CI_UsingType <> 810 And f.CI_UsingType <> 850 And f.CI_UsingType <> 851 And f.CI_UsingType <> 812 And f.CI_UsingType <> 703 And f.CI_UsingType <> 840).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <.CI_UsingType <> 20> 817 And f.CI_UsingType <> 844 And f.CI_UsingType <> 801 And f.CI_UsingType <> 2033 And f.CI_UsingType <> 811 And f.CI_UsingType <> 822 And f.CI_UsingType <> 845 And f.CI_UsingType <> 720).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 820 And f.CI_UsingType <> 838 And f.CI_UsingType <> 832 And f.CI_UsingType <> 831 And f.CI_UsingType <> 998 And f.CI_UsingType <> 846 And f.CI_UsingType <> 2019 And f.CI_UsingType <> 803).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 821 And f.CI_UsingType <> 981 And f.CI_UsingType <> 802 And f.CI_UsingType <> 821 And f.CI_UsingType <> 822 And f.CI_UsingType <> 805 And f.CI_UsingType <> 806 And f.CI_UsingType <> 832 And f.CI_UsingType <> 843).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 111 And f.CI_UsingType <> 833 And f.CI_UsingType <> 431 And f.CI_UsingType <> 2035 And f.CI_UsingType <> 834 And f.CI_UsingType <> 904 And f.CI_UsingType <> 906 And f.CI_UsingType <> 922 And f.CI_UsingType <> 923).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 967 And f.CI_UsingType <> 813 And f.CI_UsingType <> 814 And f.CI_UsingType <> 827 And f.CI_UsingType <> 828 And f.CI_UsingType <> 854 And f.CI_UsingType <> 994 And f.CI_UsingType <> 813 And f.CI_UsingType <> 814).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 827 And f.CI_UsingType <> 828 And f.CI_UsingType <> 854 And f.CI_UsingType <> 994 And f.CI_UsingType <> 2036 And f.CI_UsingType <> 847 And f.CI_UsingType <> 933 And f.CI_UsingType <> 753 And f.CI_UsingType <> 999).tolist 
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2024 And f.CI_UsingType <> 830 And f.CI_UsingType <> 1013 And f.CI_UsingType <> 829 And f.CI_UsingType <> 930).tolist
	
	  'sp384374             
	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2054 And f.CI_UsingType <> 438 And f.CI_UsingType <> 816 And f.CI_UsingType <> 751 And f.CI_UsingType <> 418 And f.CI_UsingType <> 806 And f.CI_UsingType <> 838 And f.CI_UsingType <> 827).tolist
	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 827 And f.CI_UsingType <> 923 ).tolist
	if BaseUNew.count=0 Then 
		Info8.GetOnTickChecklistParameter().Result="false"
		Info8.GetOnTickChecklistParameter().Description="ترتیب محاسبات رعایت نشده"
		Exit Function
	End if	

	Dim UsingArea as Double= BaseUNew.sum( Function (f) f.UsingArea)

	Dim FloorNoNew as Integer = BaseUNew.Max( Function (f) f.FloorNo)  +1 '- BaseUNew.Min( Function (f) f.FloorNo)
'	Info8.GetOnTickChecklistParameter().Result="false"
	'	Info8.GetOnTickChecklistParameter().Description=UsingArea
	'	Exit Function
	
'sp 486354 	 	if UsingArea > 1999 Then 	'sp 377181 if UsingArea>2999 or FloorNoNew>7 Then 

'sp 486354 	 		if ControlTask("Activity_1hbnwl9")=0 or ControlTask("Activity_1hbnwl9")=2  Then 
 		'sp 427737	
'sp 486354 	 			Call user(10)
	 		'CreatTask("Activity_0frpo8z","1e8df752-a4b8-42a9-8b39-a13562d0c761","کمیته انطباق شهری(شهرساز)") 
	 		'msg(AssingToUserName)
			'Exit Function 
'sp 486354 		 		CreatTask("Activity_1hbnwl9",AssingTo,AssingToUserName)
	 	'	Info8.GetOnTickChecklistParameter().Result="false"
		'	Info8.GetOnTickChecklistParameter().Description="D"
'sp 486354 		 		Call Payamak("نظام مهندسی - مهندس شهرساز"," مالک محترم جهت تعیین مهندس شهرساز ظرف مدت 24 ساعت به سازمان نظام مهندسی ساختمان مراجعه نمایید")
	 		
'sp 486354 		 	End if		
'sp 486354 		End if 			  	
		    
End if 

			'\\\\\\\\\\\\\\\\\ درآمد ناظر	//////////////////////

	'--------تشکیل نود تامین اجتماعی ------ چک لیست ارسال بیمه ساحتمانی
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("cf168011-6ccb-4f38-80f7-ae8b94c7d028") Then
  
	  	Dim districtA as String 
		Dim tmpDistrictA() as String
		Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
		if tmpNosaziNickName<>"" Then 
			tmpDistrictA = tmpNosaziNickName.Split("-")
			districtA=tmpDistrictA(0)
		End if		
		
	if TaeedM() = 0 Then 
		AssingTo =""
	 	AssingToUserName=""
	 	Call User(12)
	 	CreatTask("Activity_001cjwa",AssingTo,AssingToUserName)	'نود  تایمین اجتماعی
'	 	InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "TaminEjtemaii_New" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "گزارش فاکتور درامدی تامین اجتماعی", "/Sara8Reports/TaminEjtemaii_New", "1", districtA,4100)
	  Else
	    	Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description="تا زمانیکه تایید مکاتبات تایید نشده باشد اجازه تایید ندارید."
			Exit Function
	  End if
	End if

	'--------تشکیل نود املاک ----------مشمول سهم سرانه 
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("74a407ff-6310-4e05-acf8-59dbdd8ababd") Then
	
	if Info8.Getrequest.Info.NidWorkItem = 11302791 Then
		'	SendAmlakMazayaSabet
			msg("TEst")
			SendAmlakMazaya
			Exit Function
		
	Else
		if Info8.GetBarokaf.GetSh_BaroKaf.PathValue<>0  or Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed<>0  Then 
		

			Call CreatRequestAmlak()	'sp 477716
	
			'	msg(Info8.GetBarokaf.GetSh_BaroKaf.PathValue.tostring + " | " + Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed.tostring) : Exit Function
'sp 495818			Call User(7)
'sp 495818			CreatTask("Activity_1xrs4bw",AssingTo,AssingToUserName)
			Call Payamak("املاک"," برای اداره املاک منطقه ارسال شده است لطفا ظرف 24 ساعت آینده به اداره املاک مراجعه فرمایید.",3)
			
		 Else
		   	Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" میزان در مسیر و یا میزان در مسیر  رعایت شده ندارید و نود املاک تشکیل نشد."
		'		Exit Function
		 End if		
		 
	End if	 
		 
	'	 msg("T1:" & Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed.tostring) : Exit Function
	End if
	
	'sp 461077		 چک لیست انصراف از تغییر جرم گذاری
'sp 497580	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("8192c3e6-ddda-4185-b439-80810c0d846f") Then		
'sp 497580		Info8.GetFunctions.WF_CloseSimpleTaskWithoutRunFormula(Info8.GetRequest.Info.NidProc.tostring , "Activity_10x9i62")
'sp 497580	End if	
		
	
ElseIf Info8.GetOnTickChecklistParameter().TickType =2  Then   ' عدم تایید شهرداری

	Info8.GetOnTickChecklistParameter.Result="true"
	
if Info8.Getrequest.info.Nidworkitem = 11190931 Then 
	msg(Info8.GetBarokaf.GetSh_BaroKaf.PathValue & " _ " & Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed)' : Exit Function
	CreatRequestAmlak
	Exit Function
End if		
			
'sp 477716	- ارسال به املاک
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("74a407ff-6310-4e05-acf8-59dbdd8ababd") Then
	
'	msg(Info8.GetBarokaf.GetSh_BaroKaf.PathValue.tostring + " / " + Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed.tostring) : Exit Function
	
		if Info8.GetBarokaf.GetSh_BaroKaf.PathValue<>0 or Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed<>0  Then 
		
			Call CreatRequestAmlak()
	'	msg(Info8.GetBarokaf.GetSh_BaroKaf.PathValue.tostring + " || " + Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed.tostring) : Exit Function
		End if	
	End if	

				'\\\\\\\\\\\\\\\\\ نقشه برداری //////////////////////							
	
	'------- چک لیست احراز هویت
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("67f75965-b0b3-469a-94b6-9a4fa228c501") Then
	'sp 483590
		If Info8.GetRequest.Info.CI_Requester=13 Then	
			Call EbtallAndPyamak("احراز هویت","احراز هویت ")		
		End if
	'end sp 483590				
	End If
	
		
	'---------عدم پرداخت و ابطال بعد از 72 ساعت		-   پرداخت فاکتور نقشه برداری توسط مالک
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("1e1db1b4-12b5-4224-b466-81d7e67e4ac1") Then
	
		Dim CountFiche as Integer 
		Dim AllFiche as  list(of Dtosc.Income_Fiche)
		AllFiche=Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
		AllFiche=AllFiche.where(Function (f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) ).tolist
		CountFiche= AllFiche.count
		If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1e1db1b4-12b5-4224-b466-81d7e67e4ac1")
			if CountFiche>0 Then 
				Info8.GetOnTickChecklistParameter().Result="false" 
			 	Info8.GetOnTickChecklistParameter().Description=" به علت پرداخت فیش توسط مالک مجاز به ابطال درخواست نمی باشید" 							
		 	Exit Function
		End if 	
			
			if AddDateForHolidays(Info8.GetRequest.Info.RequestDate, 3)  > BIZ.SA.ClsCommon.CurrentShamsiDateString Then
				Info8.GetOnTickChecklistParameter().Result="false"
				Info8.GetOnTickChecklistParameter().Description="فرصت 72 ساعت مالک هنوز به اتمام نرسیده" 						
			End if 
			if Info8.GetOnTickChecklistParameter().Result= "true" Then Call EbtallAndPyamak("ابطال درخاست","پرداخت فاکتور نقشه برداری توسط مالک")
			
	End if
	End if

		'------------چک لیست تکمیل مدارک
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("23d6e636-9b56-4200-9ef9-d2237b2955f6") Then
			
			Call Payamak("چک لیست","Test")
		End If
		
		
		'---------انطباق محل (آدرس) با موقعت پلاک در سیستم شهرسازی
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("cf5c846a-5af6-43c7-aaf0-90fcb7d23c16") Then
			Call EbtallAndPyamak("چک لیست","انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی")
		End If
		
		
		'--------انطباق مالکیت و پلاک ثبتی
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("f555829e-2fec-42e3-86f5-2ec726de5549") Then
			Call Payamak("چک لیست","نطباق مالکیت و پلاک ثبتی")
		End If
		
		'--------اانطباق حدود و وقوع ملک با اسناد در محل 
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("f0f66760-ab09-4d7f-9d3e-b8f0c40182d2") Then
			Call Payamak("چک لیست"," انطباق حدود و وقوع ملک با اسناد در محل ")
		End if



	
			'\\\\\\\\\\\\\\\\\ 	ناظر شهرداری 	//////////////////////
	
				'-------- چک لیست نقص اطلاعات نقشه برداری ----------------------------
	'------- چک لیست احراز هویت
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("f8142a44-1056-4a05-9664-9a10265dea92") Then
		Call EbtallAndPyamak("احراز هویت","احراز هویت ")
	End If
			
	'---------عدم پرداخت و ابطال بعد از 72 ساعت		-   پرداخت فاکتور نقشه برداری توسط مالک
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("665ff873-1d4b-4adc-a1a7-fd43fee216da") Then
	
		Dim CountFiche as Integer 
		Dim AllFiche as  list(of Dtosc.Income_Fiche)
		AllFiche=Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
		AllFiche=AllFiche.where(Function (f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) ).tolist
		CountFiche= AllFiche.count
		If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1e1db1b4-12b5-4224-b466-81d7e67e4ac1")
			if CountFiche>0 Then 
				Info8.GetOnTickChecklistParameter().Result="false" 
			 	Info8.GetOnTickChecklistParameter().Description=" به علت پرداخت فیش توسط مالک مجاز به ابطال درخواست نمی باشید" 							
		 	Exit Function
		End if 	
			
			if AddDateForHolidays(Info8.GetRequest.Info.RequestDate, 3)  > BIZ.SA.ClsCommon.CurrentShamsiDateString Then
				Info8.GetOnTickChecklistParameter().Result="false"
				Info8.GetOnTickChecklistParameter().Description="فرصت 72 ساعت مالک هنوز به اتمام نرسیده" 						
			End if 
			if Info8.GetOnTickChecklistParameter().Result= "true" Then Call EbtallAndPyamak("ابطال درخاست","پرداخت فاکتور نقشه برداری توسط مالک")
			
	End if
	End if

		'------------چک لیست تکمیل مدارک
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("51e95d0d-4804-4709-9424-19bc89778a26") Then
			Call Payamak("چک لیست","Test")
		End If
		
		
		'---------انطباق محل (آدرس) با موقعت پلاک در سیستم شهرسازی
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("f5eb3d65-d583-4025-8d39-a441ffdefa90") Then
			Call EbtallAndPyamak("چک لیست","انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی")
		End If
		
		
		'--------انطباق مالکیت و پلاک ثبتی
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("13fbf86e-ca48-4014-86d4-c8f544e99ce2") Then
			Call Payamak("چک لیست","نطباق مالکیت و پلاک ثبتی")
		End If
		
		'--------اانطباق حدود و وقوع ملک با اسناد در محل 
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("07f76fdb-502a-4fae-8b8f-54bbf4490b8c") Then
			Call Payamak("چک لیست"," انطباق حدود و وقوع ملک با اسناد در محل ")
		End if
	

	'----- کنترل اطلاعات نقشه برداری 
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("d4ec982a-418e-49ca-a767-0ebb19627799") Then
		Call User(2)
		CreatTask("Activity_1qte3ts",AssingTo,AssingToUserName)  'کلید نود نقص اطلاعات نقشه برداری
		Call CopyNodeChecklistToAnotherNode() 'sp 473749
	'	if Info8.GetRequest.Info.CI_Requester = 14 Then
	' 			SaveCheck("f8142a44-1056-4a05-9664-9a10265dea92",1,0) 'احراز هویت 
	' 			SaveCheck("13fbf86e-ca48-4014-86d4-c8f544e99ce2",1,0) 'انطباق مالکیت و پلاک ثبتی		
	'	End if
	End if
	
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("243a5ca5-3abe-408b-a9df-09a648c6c33f") Then
		Call Payamak("چک لیست ","")   	'sp 461077
		SaveCheck("70028644-454a-4ce0-83ac-99d5db9bb588",2,0)
	End if	
'sp 461077	
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("b6315a90-eb61-4cc3-96a0-4adc061aa435") Then
		Call Payamak("چک لیست ","")   	
	End if
	
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("605dfb4a-bdf5-4bd4-b460-1c8bcb7c30d6") Then
		Call Payamak("چک لیست ","")   	
	End if			
	
	'------ کنترل گزارش آخرین سابقه مجاز ملک
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("c5703e93-ada3-43a7-a8fe-e17acd9d3a53") Then

		Call Payamak("چک لیست ","لطفا جهت اصلاح سابقه مجاز ،نسبت به ثبت درخواست سابقه مجاز اقدام فرمایید")   	
	End if

			'\\\\\\\\\\\\\\\\\ پایان ناظر شهرداری //////////////////////
			
			'\\\\\\\\\\\\\\\\\ ناظر درآمد شروع //////////////////////	
			
	 	'هیچوفت این مدلی یک درخواست عدم تایید رو عدم تایید نکن 
	 	
	 					'-----------   تکمیل مدارک
	 '	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("c0f26cbb-786d-4269-be77-6719b81fda7f") Then
	 	'	SaveCheck("c0f26cbb-786d-4269-be77-6719b81fda7f",2,0)
	 '	End if	
	 	 	
							'\\\\\\\\\\\\\\\\\ پایان ناظر درآمد  //////////////////////
							
'sp 468921			'\\\\\\\\\\\\\\\\\  کنترل نهایی ناظر شهرسازی //////////////////////
			
'	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bff4c7a6-e96b-48d0-bc48-e50970664df9") Then
'		Dim M_Str as String ="دارای فرم تعهدنامه تخریب تخلفات (تخریب 6 ماهه) به شماره .......... مورخ ............ می باشد ." & _
'"دارای فرم تعهدنامه املاک بدون سند و مشاعی به شماره .......... مورخ ............ می باشد ." & _
'"دارای فرم تعهدنامه ماده واحده به شماره .......... مورخ ............ می باشد ." & _
'"دارای فرم تعهدنامه تجمیع املاک به شماره .......... مورخ ............ می باشد ." & _
'"دارای فرم تعهدنامه استحکام بنا در درخواست توسعه بنا به شماره .......... مورخ ............ می باشد ." & _
'"دارای فرم تاییدیه توسعه بنا توسط مهندس ..................... با کد شهرداری ....................... می باشد ." & _
'"دارای فرم تعهد نامه اراضی و املاک مشمول سهم سرانه خدماتی (غیرقابل تفکیک) به شماره .......... مورخ ............ می باشد ." & _
'"دارای مفاصا حساب بیمه به شماره .......... مورخ ............ می باشد . )" & _
'"دارای تعهدنامه ارائه مفاصاحساب بيمه تامين اجتماعي مورخ ............ می باشد .)" '

'	SaveCheck("6926f63ebf88-1bf2-4980-9e44-574ca2ffd9df",2,0,M_Str)
'End if			
			
'end sp 468921			'\\\\\\\\\\\\\\\\\ پایان  کنترل نهایی ناظر شهرسازی //////////////////////
			
End if	
End function

Public Function NewParvaneh_ToseEh
Try
	if Info8.GetOnTickChecklistParameter().TickOwner =1  Then  ' تایید مالک 			/////////////////////////////////////////////////////////
	
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("846fd4df-5a1c-40bd-b099-cf89e4a491f2") Then 
			if NewControlCheck("36b5c24f-bd95-4a24-8065-8d03122a3d75") = 4 Then  'تایید مالک
				Call User(5)
				CreatTask("Activity_0lxgvus",AssingTo,AssingToUserName)
			End if
		End if
		
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7a97816a-140a-458c-8ef6-1f527b2f244a") Then 
		'	if NewControlCheck("36b5c24f-bd95-4a24-8065-8d03122a3d75") = 4 Then  'تایید مالک
		'		Call User(5)
		'		CreatTask("Activity_0lxgvus",AssingTo,AssingToUserName)
		'	End if
		End if		
	

	ElseIf Info8.GetOnTickChecklistParameter().TickOwner = 2 Then  '  عدم تایید مالک	/////////////////////////////////////////////////////////
	
	
		

	ElseIf 	Info8.GetOnTickChecklistParameter().TickType =1 Then   ' تایید شهرداری		/////////////////////////////////////////////////////////
	


	ElseIf Info8.GetOnTickChecklistParameter().TickType =2  Then   ' عدم تایید شهرداری	/////////////////////////////////////////////////////////
	
		'چک لیست های نقشه برداری - عدم تایید شهرداری
		
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("846fd4df-5a1c-40bd-b099-cf89e4a491f2") Then 
				Call payamak("چک لیست - نقشه برداری"," احراز هویت. ",1)
			End if
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3acafd68-4683-4aa9-b09d-d07765afc1cd") Then 
				Call payamak("چک لیست - نقشه برداری"," پرداخت فاکتور نقشه برداری توسط مالک. ",1)
			End if
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2fd2eecc-bfe6-4805-a98c-48b015f175e7") Then 
				Call payamak("چک لیست - نقشه برداری"," تکمیل مدارک. ",1)
			End if	
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0b89803a-24ed-4c68-9f89-9ee8139901dc") Then 
				Call payamak("چک لیست - نقشه برداری"," انطباق محل (آدرس) با موقعت پلاک در سیستم شهرسازی. ",1)
			End if
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b3ea03bb-f99a-4b93-a962-a61876c721c0") Then 
				Call payamak("چک لیست - نقشه برداری"," انطباق مالکیت و پلاک ثبتی . ",1)
			End if
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fe602708-a193-481b-a5ec-71700cf84279") Then 
				Call payamak("چک لیست - نقشه برداری"," انطباق حدود و وقوع ملک با اسناد در محل. ",1)
			End if		
		
		'چک لیست های بازدید - عدم تایید شهرداری	
		
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("45ae89cc-ce07-492f-9dff-d863d00d1446") Then 
				Call payamak("چک لیست - بازدید"," تکمیل مدارک. ",1)
			End if	
			IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6e4551a3-943d-4cfb-a94b-b73594bdd450") Then 
				Call payamak("چک لیست - بازدید"," پذیرش بازدید. ",1)
			End if	
			
		'چک لیست های ناظر شهرسازی - عدم تایید شهرداری		
		
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ae87e3ae-3284-4597-b0f3-d86ac5856070") Then 
				Call User(2)
				CreatTask("Activity_0e5z5gg",AssingTo,AssingToUserName)	' نود نقص اطلاعات نقشه برداری
		End if	
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6e4551a3-943d-4cfb-a94b-b73594bdd450") Then 
			Call payamak("چک لیست - ناظر"," کنترل تغییر جرم گذاری. ",1)
		End if	
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0634e084-9d49-4260-9484-1c332baf7f9b") Then 
			Call payamak("چک لیست - ناظر"," کنترل  اعلام ضابطه وکمیسیون ماده 5. ",1)
		End if																					
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("123e5773-f673-40d6-b3c4-a7b18a79377d") Then 
			Call payamak("چک لیست - ناظر","  کنترل گزارش آخرین سابقه مجاز ملک. ",1)
		End if																		
		IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d190c896-4ce1-4a8e-9053-bdb457b4a403") Then 
			Call payamak("چک لیست - ناظر","  کنترل  شمولیت صلحنامه . ",1)
		End if	
		
		
																			
	End if
	
Catch e as exception
	msg("Ex: " & e.message)
End Try	
End function

Public Function Payamak(ByVal Kind as String,ByVal Reason as String, Optional ByVal matnFlag as Integer = 1)

'	if ucase(Info8.Getrequest.info.NidWorkFlowDeff.tostring)  =  ucase("88543dab-f235-4b44-84b4-2776173cf0b4") Then
	
'	Else
		Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
    	Dim ss as Integer =0
    	if matnFlag = 1 Then
    		For ss=0 to lstOwner.count-1
    			Sms(lstOwner(ss).OwnerMobile,Kind," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem.tostring & "و کدنوسازی " & ReverseString(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring) &" بدلیل عدم تایید " & Reason & "در کارتابل اقدامات شما جهت اقدام / بررسی و تایید قرار گرفته است. چنانچه در انجام مجموع  اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد. شهرداری مشهد")  
    		Next
				Sms(Info8.GetRequest.Info.Cellphone, Kind ," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem.tostring & "و کدنوسازی " & ReverseString(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring) &" بدلیل عدم تایید " & Reason & "در کارتابل اقدامات شما جهت اقدام / بررسی و تایید قرار گرفته است. چنانچه در انجام مجموع  اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد. شهرداری مشهد")
		ElseIf matnFlag = 2 Then
			For ss=0 to lstOwner.count-1
				Sms(lstOwner(ss).OwnerMobile,Kind," مالک محترم پرونده شما با شماره درخواست " & Info8.GetRequest.Info.NidWorkItem.tostring & " و کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & Reason) 
			Next        	
				Sms(Info8.GetRequest.Info.Cellphone,Kind," مالک محترم پرونده شما با شماره درخواست " & Info8.GetRequest.Info.NidWorkItem.tostring & " و کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & Reason)
    	ElseIf matnFlag = 3 Then
			For ss=0 to lstOwner.count-1
				Sms(lstOwner(ss).OwnerMobile,Kind," مالک محترم پرونده شما با شماره درخواست " & Info8.GetRequest.Info.NidWorkItem.tostring & " و کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & Reason) 
			Next        	
				Sms(Info8.GetRequest.Info.Cellphone,Kind," مالک محترم پرونده شما با شماره درخواست " & Info8.GetRequest.Info.NidWorkItem.tostring & " و کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & Reason)
    	End if	
 '   End if			  	
' msg("Tel: ") : Exit Function
End Function

Public Function SendPayamak(ByVal Kind as String,ByVal Reason as String,Optional ByVal Who as String="")	

			Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
        	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
        	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
        	Dim ss as Integer =0
        	For ss=0 to lstOwner.count-1
        		Sms(lstOwner(ss).OwnerMobile,Kind," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem.tostring & (IIf( Who="" , "و کدنوسازی " & ReverseString(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring) , Who)) & " " & Reason & " . شهرداری مشهد")  
        	Next
				Sms(Info8.GetRequest.Info.Cellphone, Kind ," شهروند گرامی : درخواست شما به شماره " & (IIf( Who="" , "و کدنوسازی " & ReverseString(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring) , Who)) & " " & Reason & " . شهرداری مشهد") 
		  	
End Function

Public	Function EbtallAndPyamak(ByVal Kind as String, ByVal Reason as String)

			Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
        	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
        	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
        	Dim ss as Integer =0
        	For ss=0 to lstOwner.count-1
        		Sms(lstOwner(ss).OwnerMobile, kind," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem.tostring & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & "یدلیل عدم" & Reason & "ابطال گردید .شهرداری مشهد")  
        	Next
				Sms(Info8.GetRequest.Info.Cellphone, kind ," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem.tostring & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & "یدلیل عدم" & Reason & "ابطال گردید .شهرداری مشهد") 
		 			 	
		  		
			Info8.GetFunctions.SendRequestToGarbage(Guid.Parse(Info8.GetOnTickChecklistParameter.NidProc) , Guid.Parse (Info8.GetOnTickChecklistParameter.NidUser),"", 3, Reason)
			Dim WorkFlow As New Global.Biz.SC.ClsNewWorkFlow.ClsSetProcStatusMessageContract
	        WorkFlow.NidProc=Info8.GetOnTickChecklistParameter().NidProc.tostring
		    WorkFlow.NidUser="CD03B7F0-B61A-11EB-A116-951CC710A286"
		    WorkFlow.ProcStatus = 2
		    WorkFlow.UserName ="SaraSys"
		    Info8.GetFunctions.WF_SetProcStatus(WorkFlow)

End Function
'-----------\\\\\\\   End -  Sajedi 1403-03  //////---------------- 

 
Public Function CreatTask(ByVal NIdNod as String ,ByVal RevisitAgent as String,ByVal RevisitAgentName as String )
Try
		Dim NewSara As New Global.Biz.SC.ClsNewWorkFlow.ClsGetWorkFlowCommandMessageContract
		NewSara.NidProc = Info8.GetOnTickChecklistParameter().NidProc.tostring
		NewSara.AssingTo = RevisitAgent
		NewSara.AssingToUserName = RevisitAgentName
		NewSara.CreatedBy = "EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"
		NewSara.CreatedByName = "Esup"
		NewSara.EumAssingType = "0"
		NewSara.NidNode = NIdNod

	 	Dim Retursara as Boolean
	    Retursara = Info8.GetFunctions.WF_Formula_CreateSimpleTask(NewSara)
Catch 
	msg("CT: خطایی بوجود  آمده لطفا مجدد تلاش کنید")
End Try  
			
End Function

Public Function NewControlCheck( ByVal NidCheckList as String ) as Integer 
 '  0   پیدا نشد
 '1 تایید نشده 
 '2 تایید شده 
 '3  تایید نشده مالک 
 '4 تایید مالک
 '5 تایید شهرداری - عدم تایید مالک
 '6 تایید شهرداری - تایید مالک
 
' NidCheckList = "37e5959a-c385-40ed-99cf-51f2557b6b6c"
 Try
 			
	Dim Resultint as Integer = 0
    Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
    LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetOnTickChecklistParameter().nidproc.tostring)
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
							'	msg("CHECK: "  & LstObj.data(i).NeedToConfirm.tostring )	
								if LstObj.data(i).NeedToConfirm Then    ' NeedToConfirm return an boolean value -1=true , 0=false
						            Dim OwnerConfirm as String = "0"
						          '  msg("Owner: "  & LstObj.data(i).OwnerConfirm.tostring)
						            Try
						                OwnerConfirm= LstObj.data(i).OwnerConfirm.tostring
								 	Catch
										OwnerConfirm= "0"
								 	End Try
								 	
								    if OwnerConfirm = "1" Then
								       Resultint=4
								    Else
								   	   Resultint=3
								    End if   
								   
								End if   								
				         Else	
					         			         	
				         	  Resultint=2
				              if LstObj.data(i).NeedToConfirm Then
					            Dim OwnerConfirm as String = "0"
						            Try
						                OwnerConfirm= LstObj.data(i).OwnerConfirm.tostring
								 	Catch
										OwnerConfirm= "0"
								 	End Try
							    if OwnerConfirm <> "1" Then
							       Resultint=5
							    Else
							   	   Resultint=6
							    End if 
									
							End if	
						End if 	
					 End if    				          		         
		       Next
		 End if    		 				        	 
	Return Resultint	 
	
	Catch ex as exception
			Info8.GetOnTickChecklistParameter.Description = "CheckList - NewControlcheck: " + ex.message
            Info8.GetOnTickChecklistParameter.Result = "false"
	End Try   
		     
End Function

Public Function ControlCheck ( ByVal NidCheckList as String ) as Integer 
 '  0   پیدا نشد
 '1 تایید نشده 
 '2  تایید نشده مالک 
 '3 تایید شده 
 
 Try
	Dim Resultint as Integer =0
    Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
    LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetOnTickChecklistParameter().nidproc.tostring)
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
						            Dim OwnerConfirm as Integer = 0
						            Try
						                OwnerConfirm= LstObj.data(i).OwnerConfirm
								 	Catch
										OwnerConfirm= 0
								 	End Try
								   if OwnerConfirm =1 Then   Resultint=3
								End if   
				         Else
				         	  Resultint=3
				              if LstObj.data(i).NeedToConfirm  Then
					            Dim OwnerConfirm as Integer = 0
					            Try
					                OwnerConfirm= LstObj.data(i).OwnerConfirm
							 	Catch
									OwnerConfirm= 0
							 	End Try
							   if OwnerConfirm <>1 Then   Resultint=2
									
							End if	
						End if 	
					 End if    
				          
		         
		       Next
		 End if     

	       '     Info8.GetOnTickChecklistParameter.Description = Info8.GetOnTickChecklistParameter().nidproc.tostring
           ' Info8.GetOnTickChecklistParameter.Result = "false"
		    
	Return Resultint
	
Catch ex as exception

            Info8.GetOnTickChecklistParameter.Description = ex.message
            Info8.GetOnTickChecklistParameter.Result = "false"

End Try		    
		     
End Function

Public Function ControlTask(ByVal NIdNod as String,Optional ByVal Control as Integer =0 ) as Integer

		Dim TaskByNidProcResponse As New Global.Biz.SC.ClsNewWorkFlow.ClsGetTaskByNidProcResponse
		Dim strClose as String =""
	
		TaskByNidProcResponse=Info8.GetFunctions.WF_GetTaskByNidProc(Info8.GetOnTickChecklistParameter().NidProc.tostring)
		Dim counter as Integer=0
		Dim Colescounter as Integer=0
		Dim Opencounter as Integer=0
		Dim ik as Integer=0
		For ik = 0 To TaskByNidProcResponse.data.Count - 1
		    If TaskByNidProcResponse.data(ik).NidNode = NIdNod Then Opencounter+=1	     
		Next
		
		For ik = 0 To TaskByNidProcResponse.data.Count - 1
		    If TaskByNidProcResponse.data(ik).NidNode = NIdNod Then
		    	if {1,3,5}.contains(TaskByNidProcResponse.data(ik).EumTaskStatus) Then Colescounter+=1  
		    End if	
		     
		Next
		
		if Colescounter=Opencounter And Opencounter =0 Then counter=0  'وجود ندارد
		if Colescounter<Opencounter Then  counter=1 ' وجود دارد
		if Colescounter=Opencounter And Opencounter <>0  Then  counter=2 ' بسته شده 
		if Control=1 Then  if Colescounter>0  Then  counter=3 ' بسته شده  حداقل یکبار نود استعلام گرفته شده       
		        ''''''''''''''''''''''''''''''''''''''
		        		 	
		 Return counter
	
End Function

Function Archive(ByVal NidEntity as Integer) 
	Info8.Request=Nothing
    Info8.ParameterList.REMove("NidProc")
  	
    Info8.ParameterList.Add("NidProc", Info8.GetOnTickChecklistParameter().nidproc.tostring)
    
    Dim lstNosazi As New List(Of Global.Biz.SA.ClsNosaziCodeType)
    Dim lstNosaziNew As New List(Of Global.Biz.SA.ClsNosaziCodeType)
	lstNosazi = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
	lstNosaziNew = lstNosazi.Where(Function(f) f.EumNosaziCodeObjType = 4).ToList
	Dim M_ObjectType as Integer=0
	Dim Nidnosazimelak as guid
	
	if lstNosaziNew.count>0 Then 
		M_ObjectType=4
		Nidnosazimelak=lstNosaziNew(0).NidNosaziCode
	Else
		lstNosaziNew = lstNosazi.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
		
		if lstNosaziNew.count>0 Then 
			M_ObjectType=5
			Nidnosazimelak=lstNosaziNew(0).NidNosaziCode
		Else
			lstNosaziNew = lstNosazi.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
			M_ObjectType=2
		    Nidnosazimelak=lstNosaziNew(0).NidNosaziCode
		End if	
	End if	
	 
	Dim district as String 
	Dim tmpDistrict() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	if tmpNosaziNickName<>"" Then 
		tmpDistrict = tmpNosaziNickName.Split("-")
		district=tmpDistrict(0)
	End if	
	
	Dim ArcWrrap As List(Of Global.DtoSC.Archive_Wrapper)
	ArcWrrap = Info8.GetFunctions.GetArchiveBizCode(Nidnosazimelak)
	Dim TmpUrlFromBizCodeResult As New List(Of Global.Biz.SC.SrvArchive.SrvGetUrlFromBizCodeResult)
	Dim LstArchive As New List(Of Global.BIZ.SC.SrvArchive.ClsImageItem)
	if ArcWrrap.count >0 Then 
		
		ArcWrrap=ArcWrrap.where (Function (f) f.CI_ArchiveGroup =1 ).tolist ()
		if ArcWrrap.count >0 Then
			Dim tmpSerchArchive As New Global.Biz.SC.SrvArchive.ClsSearchDataFile
		    Dim tmpSerchArchiveOut As New  Global.Biz.SC.SrvArchive.ClsSearchDataFile
		    Dim TmpG As New List(Of Guid) 
			TmpG.Add(Guid.Parse("00c21306-2043-428f-9f9d-f211ff85fedc"))
			tmpSerchArchive.BizCode = ArcWrrap(0).BizCode.tostring
			tmpSerchArchive.DomainName = district
			tmpSerchArchive.AllGuids = TmpG
			tmpSerchArchive.UserID ="00c21306-2043-428f-9f9d-f211ff85fedc"
			tmpSerchArchive.UserName ="تایید سیستمی"
		
			tmpSerchArchiveOut = Info8.GetFunctions.GetArchiveDataFileByBizCode(tmpSerchArchive, district)		
		
			TmpUrlFromBizCodeResult = tmpSerchArchiveOut.Result.GetFile
				
	        TmpUrlFromBizCodeResult = TmpUrlFromBizCodeResult.Where(Function(f) (f.NidEntity = NidEntity )And f.isdeleted=0 And f.CreateDate >= Info8.GetRequest.Info.RequestDate).ToList
            if Not TmpUrlFromBizCodeResult.any() Then 
            	Info8.GetOnTickChecklistParameter().Result="false"	
				Info8.GetOnTickChecklistParameter().Description ="ارشيو ندارد"
            End if 
		 End if 
	End if	 
	End Function			        
	
Public Function SaveCheck(ByVal NidCheckList as String, ByVal CheckListValue as Integer,ByVal OwnerConfirm as Integer, Optional ByVal Com_M as String="تایید سیستمی")
Dim a as String=""
Try
		
		Dim SaveCheckList As New Global.Biz.SC.ClsNewWorkFlow.ClsSaveCheckListValueMessageContract

		SaveCheckList.CI_CheckListValue = CheckListValue.tostring
		SaveCheckList.Comments=Com_M '"تایید سیستمی"

		SaveCheckList.NidCheckList=NidCheckList.tostring
		SaveCheckList.NidProc=Info8.GetOnTickChecklistParameter().NidProc.tostring 
		SaveCheckList.NidUser="00c21306-2043-428f-9f9d-f211ff85fedc"
		SaveCheckList.FullUserName ="تایید سیستمی"

	    SaveCheckList.OwnerConfirm =OwnerConfirm
	    SaveCheckList.OwnerComments="تایید سیستمی"
	'    SaveCheckList.TickOwner = "4"

	    if {"799f97bf-7531-4a00-b1e8-7631a09cf7f3","0d9d7760-cc20-4f14-a9d9-1f81b4b7010e","d7406af8-84c1-4b8d-b15f-4ac2bd9a134d", _
	        "1d4cb98d-d5bb-45d6-9e4f-0a81d6f5f57c","e0b65449-a4ba-49db-80bf-ab42eebc2b6c"}.contains(ucase(NidCheckList)) Then
	    	SaveCheckList.OwnerComments="کاربر محترم لطفا مبلغ پرداختی اول را وارد نمایید"		
	    End if	

		Dim result as Boolean = Info8.GetFunctions.WF_SaveCheckListValue(SaveCheckList)

Catch e as exception
		 Info8.GetOnTickChecklistParameter().Result="false"
		 Info8.GetOnTickChecklistParameter().Description= "SaveCheck Func: " + e.message	   
		 Exit Function
End Try
		    	

		     		    
End Function

Public Function Check( ByVal NidCheckList as String,ByVal CheckNot as Integer) as String 
 
  Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
  LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetOnTickChecklistParameter().NidProc.tostring)
  Dim i as Integer
  if LstObj.data.count>0 Then 
     For i = 0 To LstObj.data.count - 1
		Try
		  if ucase(LstObj.data(i).NidCheckList)= ucase(NidCheckList)    Then
			   if CheckNot =1 Then
			   		Dim OwnerConfirm as Integer = 0
			   		Try
				        OwnerConfirm= LstObj.data(i).OwnerConfirm
					Catch
						OwnerConfirm= 0
					End Try
			   		if OwnerConfirm =0 or OwnerConfirm =2  Then Return "false"	
			   ElseIf String.IsNullOrEmpty( LstObj.data(i).CI_CheckListValue) Then
			   		Return "false"
			   ElseIf  LstObj.data(i).CI_CheckListValue = 0 or LstObj.data(i).CI_CheckListValue = 2  Then 
					Return "false"
			   
			   End If
				                    
		    End if  
		          
	    Catch
		End Try        
	 Next
 End If
Return "true"	
End Function

Function ReverseString(input As String) As String
    Dim parts As String() = input.Split("-"c)
    Array.Reverse(parts)
    Return String.Join("-", parts)
End Function
		

Public Function SmsOwner() 
	
'	if ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)=ucase("88543dab-f235-4b44-84b4-2776173cf0b4") Then
	
'	Else

	  if ucase(CheckConfirm(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)) =ucase("true") Then 
	  	
	  	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
		lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
		if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
		Dim ss as Integer =0
		
			 	
		For ss=0 to lstOwner.count-1
			if String.IsNullOrEmpty(lstOwner(ss).OwnerMobile) Then
			ElseIf lstOwner(ss).OwnerMobile.Length =11 Then
				 Sms(lstOwner(ss).OwnerMobile,"اقدامات مالک"," شهروند گرامی " + lstOwner(0).OwnerFirstName.ToString +" " + lstOwner(0).OwnerLastName.ToString + " پرونده شما با کد نوسازی " + ReverseString(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)) + " با شماره ارجاع " _
				+Info8.GetRequest.Info.NidWorkItem.tostring + "در کارتابل اقدامات شما جهت اقدام / بررسی و تایید  قرار گرفته است  چنانچه در انجام مجموع اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد - شهرداری مشهد. ")
			End if	
		Next
		
		 Sms(Info8.GetRequest.Info.Cellphone,"اقدامات مالک"," شهروند گرامی " + Info8.GetRequest.Info.RequesterName + " پرونده شما با کد نوسازی " + ReverseString(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)) + " با شماره ارجاع " _
			+Info8.GetRequest.Info.NidWorkItem.tostring + "در کارتابل اقدامات شما جهت اقدام / بررسی و تایید  قرار گرفته است  چنانچه در انجام مجموع اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد -  شهرداری مشهد. ")
	End if 	
'End if		
End Function

Public Function CheckConfirm( ByVal NidCheckList as String) as String 
 
 
  Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
  LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetOnTickChecklistParameter().nidproc.tostring)
 
  Dim i as Integer
  if LstObj.data.count>0 Then 
  
     For i = 0 To LstObj.data.count - 1
		Try
		  if ucase(LstObj.data(i).NidCheckList)= ucase(NidCheckList) Then
			   Return LstObj.data(i).NeedToConfirm.tostring
				                    
		    End if  
		          
	    Catch
		End Try        
	 Next
 End If
Return "False"
End Function


Public Function CheckMalek( ByVal NidCheckList as String ) as Boolean 
 
  Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
  LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetOnTickChecklistParameter().NidProc.tostring)
  Dim i as Integer
  
  if LstObj.data.count>0 Then 
     For i = 0 To LstObj.data.count - 1
		Try
		
			if ucase(LstObj.data(i).NidCheckList) <> ucase("0c3ac5ff-4809-4ffa-9cef-4555e7d4ef3e") And _ 
			   ucase(LstObj.data(i).NidCheckList) <> ucase("71b90f46-985c-40b7-a40e-d06fd4aeb70b") And _ 
			   ucase(LstObj.data(i).NidCheckList) <> ucase("5509a940-41be-4dee-ad39-b6330cc218a2") And _ 
			   ucase(LstObj.data(i).NidCheckList) <> ucase("41eb5f18-2f38-4076-8645-415070d1c55e") Then    'sp 492647  عدم گیر دادن به چک لیست توافقات مضاعف بر تعرفه در درخواست های استعلام 
			   
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
			End if          
	    Catch
		End Try        
	 Next
 End If
 	
Return True	
End Function


Public Function MapBarokaf() as String
 ''''''''''''

 Try
 	
 		Dim NidNosaziCode as String = Info8.GetRequest.Info.NidNosaziCode.tostring 'Info8.FindParameter("NidNosaziCode")
 	
 		Dim Nidl = Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Guid.Parse(NidNosaziCode), 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000"))
        Dim lstNosazi,lstNosaziDas As New List(Of Global.Biz.SA.ClsNosaziCodeType)
        lstNosazi = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Guid.Parse(NidNosaziCode), 0)
        lstNosaziDas = lstNosazi.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
        lstNosazi = lstNosazi.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
        
        Dim Nidb = Info8.GetFunctions.GetNidBase(lstNosazi(0).NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000"))
        Try
        	Info8.ParameterList.Remove("Parvandeh_House_NidBase")
        	Info8.ParameterList.Add("Parvandeh_House_NidBase", Nidb.ToString)
        Catch
        End Try	
        if lstNosaziDas.count>0 Then  Nidb = Info8.GetFunctions.GetNidBase(lstNosaziDas(0).NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000"))
        Try
        	Info8.ParameterList.Remove("Parvandeh_HouseSharing_NidBase")
        	Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", Nidb.ToString)
        Catch
        End Try	
        
        'Dim key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
       ' key_List=Info8.GetMap().GetMapFind(Info8.GetFunctions.GetNosaziNickName(lstNosazi(0).NidNosaziCode),5)
 
       Dim Tmp as String="0" 
       Tmp="0"  
           Dim LayerListStr as String
            ' خواندن اطلاعات نقشه
            Dim i As Integer
            Dim IndexKey as Integer 
           

            Dim gu As New Guid
            Dim gu2 
            
            if lstNosaziDas.count>0 Then
            	gu2=Info8.GetParvandeh_HouseSharing.GetBase_NosaziCode
            Else
           		gu2=Info8.GetParvandeh_House.GetBase_NosaziCode
            
			End if
            

            gu = gu2.NidNosaziCode
        
            Dim strnosazicode As String = ""
            strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
            
         Dim key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
         Dim key as Object 
         IndexKey=5
        LayerListStr="10239,5"
           
	        	IndexKey=5
	        	LayerListStr="10239,5"
	          	key_List=Info8.GetMap().GetMapFind(strnosazicode,"5")
	    	
			          
            	if Not key_List is Nothing And key_List.count > 0 Then 
            	    key = key_List(0).Guid
            	 Else
            		 
            		 key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
            	 if Not key_List is Nothing And key_List.count > 0 Then 
            		 	IndexKey=839
            		 	LayerListStr="10239,839"
            		     key = key_List(0).Guid
            		  Else
            		      Info8.AddError(BIZ.SA.EumErrorAction.Warning, " توجه ","ملک مورد نظر روی نقشه جانمایی نشده است . " )
            		  End if 
	               
              	End if 
         	
            Dim P_LapList As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
            Dim TmpFeatureTarhs As List(Of BIZ.SC.GeographyInfo.GeoCalculationResponse)
            P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(key.tostring,LayerListStr)
            
            P_LapList=P_LapList.where ( Function (f) f.LayerID=10239).tolist   
	 
			if P_LapList.count>0 Then 
		
				Dim ff as Integer =0
				For ff=0 to P_LapList.count-1
					if P_LapList(ff).OverLapArea > key_List(0).Area*0.2 Then
					
					 	Tmp=P_LapList(ff).OverLapArea
					Else
						Tmp="0"
					End if 	
				Next	
			Else
				Tmp="0"
			End if	 
	     

   Return Tmp
                
 Catch Ex as Exception
     Info8.AddError(BIZ.SA.EumErrorAction.Warning, " توجه*a* ","خواندن  لایه با خطا حریم بروکف" & Ex.message)
End Try   
''''''''''''''''''
 End Function 
 
Public Function User(ByVal Type as Integer )        
            
	Dim District as Integer 
	
	District=Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, Global.Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Global.Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000")).District
	Dim Region as Integer 
	Region=Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.Request.Info.NidNosaziCode, Global.Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Global.Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000")).Region
	if Type=2 Then
		if MapBarokaf()="0" Then type=4
	End if 	
	
' 1 ناظر
' 2 شرکت نقشه برداری
' 3  نوسازی اصناف
' 4 نقشه بردار منطقه
' 5 پرداخت اول 

'Dim Kanon_Karshenasan as String
if type = 13 Then
		AssingTo = "4e05f9da-87f0-4cce-b943-f7c942a99a7e"
		AssingToUserName="کانون کارشناسان"  'کانون کارشناسان​ رسمی
End if			
if type = 14
		AssingTo = "3a16a45d-9cea-40a2-9829-bf6ab91e75e1"
		AssingToUserName="مرکز کارشناسان" 'کانون-مرکز کارشناسان
End if

if type = 16
		AssingTo = "22835b9b-7469-4d1d-8f34-49dbe194ea9a"	'search Activity_1074fhi line 1741 
		AssingToUserName="مهندس ناظر معماری" 
End if
		
Dim NahiBime as Integer =0
if type =12 Then
	NahiBime= Fa_Bimeh() 
if  NahiBime= 0 Then AssingTo="" : AssingToUserName=""
	if NahiBime=1 Then
		AssingTo = "5ebac7ae-9c9e-4ccf-9f8f-11385ff143fc"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 1"
	ElseIf NahiBime=2 Then
		AssingTo = "6dc13174-a0f8-4093-87b8-127e26c66021"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 2"
	ElseIf NahiBime=3 Then
		AssingTo = "2c639384-5322-4a08-8b52-641c4ab70912"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 3"
	ElseIf NahiBime=4 Then
		AssingTo = "cf289df4-69be-4bff-ac65-28f6c8b49258"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 4"
	ElseIf NahiBime=5 Then
		AssingTo = "f67bc411-7478-405e-8a23-412a452f7fb3"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 5"
	ElseIf NahiBime=6 Then
		AssingTo = "6dc13174-a0f8-4093-87b8-127e26c66021"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 2"	
'sp 523059		AssingTo = "9cfdfa17-63a0-4631-828b-62650853dc35"
'sp 523059		AssingToUserName="سازمان تامین اجتماعی-شعبه 6"
	ElseIf NahiBime=7 Then
		AssingTo = "9cfdfa17-63a0-4631-828b-62650853dc35"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 6"
	ElseIf NahiBime=8 Then
		AssingTo = "6fcfc59e-fd0c-4039-8bbf-7f848ef9dea9"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 7"
	ElseIf NahiBime=9 Then
		AssingTo = "6fcfc59e-fd0c-4039-8bbf-7f848ef9dea9"
		AssingToUserName="سازمان تامین اجتماعی-شعبه 7"	
	End if 
End if 	


if type =9 Then
	AssingTo = "7721198A-FB6A-413B-A8C4-16175F14215A"
	AssingToUserName="گروه انجمن مهندسین شهرساز-جرم گذاری "
End if 	

if type =8 Then
	'AssingTo = "AA94EE71-A872-4EC0-A574-1664EAD5656F"
	'AssingToUserName="جامع نظام مهندسی-حدنگار"
	'sp 392543
	AssingTo = "d5ad3002-fa83-4042-bd8b-90d6cfed0c45"
	AssingToUserName="بازدید ثبت (حدنگار)"
End if 

IF District = 1 Then
		if type =1 Then
	        AssingTo = "0BA327DF-E7AD-4B0D-AD0F-A174E2C2812B"
	      	AssingToUserName="ناظر شهرسازی منطقه1"
      	ElseIf type =2 Then
	      	AssingTo ="ac3a67c1-79c2-47ed-9bdd-bbd287b1b243"			 'sp 482109  "8923d414-828d-4462-b3c3-2b85d9acb7cc" 'sp 406426 "ea722297-661a-4e06-b367-dd1f3d1e3ce3"
	      	AssingToUserName="نقشه برداری - شرکت پارس طلوع خاوران"	 'sp 482109""شهرسازان نام آشنا شرق خراسان - نقشه برداری"
	    ElseIf type =3 Then
	      	AssingTo = "30efeecf-c6b7-4516-ba3f-7ad1ee36c38d"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 1"  
	    ElseIf type =4 Then
	      	AssingTo = "6a482068-3900-49e8-a9c4-591188d315ce"
	      	AssingToUserName="نقشه برداری منطقه1"  
	    ElseIf type =5 Then
	      	AssingTo = "9075B33C-A98D-4A8B-8C13-EA644FBA52E3"
	      	AssingToUserName="پرداختی اول - منطقه 1"  
	     ElseIf type =6 Then
	      	AssingTo = "4fc02d90-603a-4326-9dcc-0861f97c32de"
	      	AssingToUserName="رئیس شهرسازی منطقه1"
	      ElseIf type =7 Then
	      	AssingTo = "c2eda76e-0941-43b9-9dd4-e2a68914e9da"
	      	AssingToUserName="املاك-منطقه1"  
	      ElseIf type =10 Then
	      	AssingTo = "1b7a4a5a-c9bd-4e05-ab7e-eafa35849279"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه1" 
	     ElseIf type =11 Then
	      	AssingTo = "cd4fb51f-6dda-4e17-b37a-1571af1181b9"
	      	AssingToUserName="مسئول ترافیک منطقه1 " 
	     ElseIf type =15 Then
	      	AssingTo = "920B3C04-1D46-4FEC-B032-D21DC014C85A"
	      	AssingToUserName="ناظر درآمد منطقه1 "     	 	     		    	 	  	    		  	  		  		
	     ElseIf type =16 Then  'sp 502980
	      	AssingTo = "970db144-ef2c-4e26-b396-3dea1ff1697a"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه1"	    	
	     ElseIf type =17 Then  
	      	AssingTo = "396A9B4D-CBA2-42C5-86C2-F33A4E7F7ECA"
	      	AssingToUserName="حسابداری-منطقه1"	      	  	   	  	 	  	   	   		 	  		
	    End if 		    
    End if 
 



	IF District = 2   Then
		if type =1 Then
			AssingTo = "50935DB4-4C47-4050-879D-625F15DB2E6C"
      		AssingToUserName="ناظر شهرسازی منطقه2"
		ElseIf type =2 Then
		'	if MapLayer13846 = True Then
		'		AssingTo = "8923d414-828d-4462-b3c3-2b85d9acb7cc"
	    '  		AssingToUserName="دفترشهرسازان نام آشنا-نقشه برداری"			
		'	Else				
				AssingTo = "B385422F-8FDD-4B1A-80A3-FB25027F8A65"
	      		AssingToUserName="دفتر توتال نقش اسیا"
	   '   	End if	
      	ElseIf type =3 Then
	      	AssingTo = "28566ee4-c41e-47c9-b12e-835aea641339"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 2"
	    ElseIf type =4 Then		    
		      	AssingTo = "dc60f982-75ce-464d-9eaf-3d0d3a87367d"
		      	AssingToUserName="ننقشه برداری منطقه2"  	
	    ElseIf type =5 Then
	      	AssingTo = "3AD46396-6B90-448B-A389-1DFD01B144B1"
	      	AssingToUserName="پرداختی اول - منطقه 2" 
	    ElseIf type =6 Then
	      	AssingTo = "877e9bf8-16d0-4865-ba90-218b04f90a7b"
	      	AssingToUserName=" رئیس شهرسازی منطقه2"   
	     ElseIf type =7 Then
	      	AssingTo = "51df0a9b-22bb-4ec3-8c10-88033ba0ae71"
	      	AssingToUserName="املاك-منطقه2"   
	     ElseIf type =10 Then
	      	AssingTo = "0e572720-5228-4ee2-8ad4-8ade5303ed92"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه2"  
	     ElseIf type =11 Then
	      	AssingTo = "af2ab746-631e-46c6-bbec-b5cb99bbf358"
	      	AssingToUserName="مسئول ترافیک منطقه2 " 
	     ElseIf type =15 Then
	      	AssingTo = "C06E49A1-7A32-4BDE-A6E1-79CF118E9676"
	      	AssingToUserName="ناظر درآمد منطقه2"  
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "f83cf298-b18b-4df5-9413-190da2333047"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه2"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "CBBE2E80-52D6-452A-B30B-135204409566"
	      	AssingToUserName="حسابداری-منطقه2"	      	  	   	  	 	  	   	   		 	  		
	    End if 	
    	
     End if 


     IF District = 3 Then
    	if type =1 Then
	    	AssingTo = "45008385-A42E-4F62-8E72-8015AACCFD50"
	      	AssingToUserName="ناظر شهرسازی منطقه3"
	     ElseIf type =2 And (Region =1 or Region =2 or Region =3 or Region =5 or Region =6 or Region =7 or Region =8 or Region =27 or Region =25 or Region =20 or Region =21 or Region =26)Then
	     	AssingTo = "ac01f67f-fae6-4cad-8b61-aeb2c3e345ab"
      		AssingToUserName=" کیان طرح نقش جاوید-نقشه برداری " 
      	 ElseIf type =2 And (Region =4 or Region =9 or Region =10 or Region =30 or Region =31 or Region =32 or Region =28 or Region =29 or Region =33 or Region =34 or Region =35)Then
	     	AssingTo = "b0a03cc1-4db9-446e-b2bc-44128c594c85"
      		AssingToUserName="نگار شهر ارجمند-نقشه برداری "
      	ElseIf type =3 Then
      		AssingTo = "29eea167-097f-47a0-8e7e-8462cf579b51"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 3"	
	    ElseIf type =4 Then
	      	AssingTo = "e5914d2f-adc6-4490-8708-684590b30322"
	      	AssingToUserName="نقشه برداری منطقه3"  
	    ElseIf type =5 Then
	      	AssingTo = "3B337176-94E2-4708-94EE-2B0BCD220A1A"
	      	AssingToUserName="پرداختی اول - منطقه 3"    	  	  	  	 	
	     ElseIf type =6 Then
	      	AssingTo = "24f78648-ad10-48a4-81f7-59b911dfcc3a"
	      	AssingToUserName="رئیس شهرسازی منطقه 3" 
	     ElseIf type =7 Then
	      	AssingTo = "fa5b7877-f58a-42f5-9fa9-421bf150e229"
	      	AssingToUserName="املاك-منطقه3"   
	     ElseIf type =10 Then
	      	AssingTo = "21b09b7d-8a76-48c2-88f2-199a215ed121"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه3" 
	     ElseIf type =11 Then
	      	AssingTo = "c396cb76-b204-48f7-9664-d78574e7142d"
	      	AssingToUserName="مسئول ترافیک منطقه3"  
	     ElseIf type =15 Then
	      	AssingTo = "EB623637-FA77-426A-88C7-E396DDD6EC59"
	      	AssingToUserName="ناظر درآمد منطقه3 "    	  	  	 	   	  	  	
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "35d36753-d350-4898-85b9-44a738238ee9"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه3"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "19A3473A-B5FE-4F55-A521-2438B70C6477"
	      	AssingToUserName="حسابداری-منطقه3"	      	  	   	  	 	  	   	   		 	  		
	    End if 			    		
    
   End if

    IF District = 4 Then
    	if type =1 Then
	    	AssingTo = "FE7CFB75-C0B3-4ECC-80D2-F6EDD1259431"
	      	AssingToUserName="ناظر شهرسازی منطقه4"
	    ElseIf type =2 Then
	    	AssingTo = "8FAA82DD-3F37-45A9-A536-F4BCCD9F6E2C"
      		AssingToUserName="دفتر برداشت نگار پیمایش شرق-نقشه برداری" 
      	ElseIf type =3 Then
      		AssingTo = "be622dbe-62c9-4cc3-89f4-6c8aab86025a"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 4"		
	    ElseIf type =4 Then
	      	AssingTo = "c0027b35-2ac9-45eb-8ade-0f2435a285e0"
	      	AssingToUserName="نقشه برداری منطقه4"  
	    ElseIf type =5 Then
	      	AssingTo = "16C4C57A-9DE7-4F52-A926-9BCBC17B8DC7"
	      	AssingToUserName="پرداختی اول - منطقه 4"
	    ElseIf type =6 Then
	      	AssingTo = "6cc8c6bc-992b-4917-94d6-11c596317d7c"
	      	AssingToUserName="رئیس شهرسازی منطقه4"    
	     ElseIf type =7 Then
	      	AssingTo = "7014e85d-cccf-4914-9f02-7d9ea9401be5"
	      	AssingToUserName="املاك-منطقه4"  
	     ElseIf type =10 Then
	      	AssingTo = "40e16b21-70d6-446a-b3a2-5f2a7482cb31"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه4"  
	      ElseIf type =11 Then
	      	AssingTo = "aef54771-a2e5-428d-b0eb-81e112427c48"
	      	AssingToUserName="مسئول ترافیک منطقه4" 
	     ElseIf type =15 Then
	      	AssingTo = "9B51A6F6-8B21-4253-BBF3-7AAC19C5C948"
	      	AssingToUserName="ناظر درآمد منطقه4 "      	 	 	    	   		     	   
 	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "50131458-9e7b-41d1-b3f8-01dd12d1be62"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه4"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "2D396254-E655-441F-9D4B-7E68FAB4AE45"
	      	AssingToUserName="حسابداری-منطقه4"	      	  	   	  	 	  	   	   		 	  		
	    End if 	 	
    
   End if


    IF District = 5  Then
    	if type =1 Then
	    	AssingTo = "A8D96288-15DD-4B20-B938-7998DD8D3701"
	      	AssingToUserName="ناظر شهرسازی منطقه5"
	    ElseIf type =2 Then
	      	AssingTo = "B315159F-1824-4DB7-957A-125AE4695923"
      		AssingToUserName="دفتر346-نقشه برداری"
      	ElseIf type =3 Then	
      		AssingTo = "14960070-E6F2-4833-8BBF-0A110BC86BF4" '"e6f2-4833-8bbf-0a110bc86bf4-14960070"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 5"
	    ElseIf type =4 Then
	      	AssingTo = "14cefeca-bc36-4efb-a75c-505a5d542d13"
	      	AssingToUserName="نقشه برداری منطقه5"    
	    ElseIf type =5 Then
	      	AssingTo = "2F2A515B-2527-4F30-A166-9783898AC6ED"
	      	AssingToUserName="پرداختی اول - منطقه 5"  
	    ElseIf type =6 Then
	      	AssingTo = "509951e4-e0b3-442b-8ca8-e9f166ccef8e"
	      	AssingToUserName="رییس شهرسازی منطقه5"  
	    ElseIf type =7 Then
	      	AssingTo = "26d495e2-b5af-44b9-8ecf-d54f618678fb"
	      	AssingToUserName="املاك-منطقه5" 
	    ElseIf type =10 Then
	      	AssingTo = "3de8cd81-bd8c-4a10-97b5-7af2063c140e"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه5" 
	    ElseIf type =11 Then
	      	AssingTo = "9073426c-3a82-49a7-9ddd-7fe348d19b41"
	      	AssingToUserName="مسئول ترافیک منطقه5"  
	     ElseIf type =15 Then
	      	AssingTo = "F2406FAB-5AB2-4276-A3A3-E314E7C34CE5"
	      	AssingToUserName="ناظر درآمد منطقه5"     	  	  	 	      	      	   	   	   		  	   	   	   				
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "0124d005-6f9b-4521-975f-259e6b4c76b9"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه5"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "5260EB60-48F9-426F-A87D-4D7093DCC32F"
	      	AssingToUserName="حسابداری-منطقه5"	      	  	   	  	 	  	   	   		 	  		
	    End if 	
    
     End if


    IF District = 6 Then
    	if type =1 Then
	    	AssingTo = "2DFD530B-00EF-4F34-8384-CF5B92058048"
	      	AssingToUserName="ناظر شهرسازی منطقه6"
	    ElseIf type =2 Then
	      	AssingTo = "6CD4C355-4D12-4CEB-8CA4-4690BF31253D"
      		AssingToUserName="دفتر مسیر نقش آپادانا-نقشه برداری"
      	ElseIf type =3 Then	
      		AssingTo = "20b67eca-2ec6-4bd4-a040-752820aa0ddb"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 6"	
	    ElseIf type =4 Then
	      	AssingTo = "644b6d18-b71a-40cf-99b7-27447856dcbb"
	      	AssingToUserName="نقشه برداری منطقه6" 
	    ElseIf type =5 Then
	      	AssingTo = "6868E52E-AAC4-48DC-85E0-F5E8EEEC66ED"
	      	AssingToUserName="پرداختی اول - منطقه 6" 
	     ElseIf type =6 Then
	      	AssingTo = "1c3d6d00-7166-426f-b929-65175a9d2573"
	      	AssingToUserName="رئیس شهرسازی منطقه6"  
	     ElseIf type =7 Then
	      	AssingTo = "d209b83c-49fc-4a6d-b1a6-9d98e08d9ffa"
	      	AssingToUserName="املاك-منطقه6" 	 
	     ElseIf type =10 Then
	      	AssingTo = "b000d0b2-3974-4246-941f-1db1c461aad5"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه6" 
	     ElseIf type =11 Then
	      	AssingTo = "d23a1436-4ae4-4ea4-ab80-c721e35eff67"
	      	AssingToUserName="مسئول ترافیک منطقه6"  
	     ElseIf type =15 Then
	      	AssingTo = "D8419ABB-E04C-4542-BAAA-9C16D927FF72"
	      	AssingToUserName="ناظر درآمد منطقه6 "  		 	 	    	   	    	        				
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "8eb532b7-e725-4209-a4bc-a5e1c4d0cbda"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه6"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "03F3B22E-8F1A-43E7-B154-5AFDC542A5B3"
	      	AssingToUserName="حسابداری-منطقه6"	      	  	   	  	 	  	   	   		 	  		
	    End if 	
    
    
   End if


    IF District = 7 Then
    	if type =1 Then
	    	AssingTo = "B31BED91-243A-43FD-B66A-3812A7AC1721"
	      	AssingToUserName="ناظر شهرسازی منطقه7"
	    ElseIf type =2 Then
        	AssingTo = "45E29C8E-9F62-4933-8A77-CB012FF21D4C"
      		AssingToUserName="شهرنگارسامان نقش پارسه-نقشه برداری"
      	ElseIf type =3 Then	
      		AssingTo = "849fefeb-2ef4-43e7-85a0-8b5d44bc4935"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 7"	
	    ElseIf type =4 Then
	      	AssingTo = "0fcebaea-eb99-41ff-8043-aebd9af04e83"
	      	AssingToUserName="نقشه برداری منطقه7"
	   ElseIf type =5 Then
	      	AssingTo = "4C4B8DEE-7085-46A2-BEDA-2EDEA8BEEB59"
	      	AssingToUserName="پرداختی اول - منطقه 7"   
	   ElseIf type =6 Then
	      	AssingTo = "5f4764fb-0937-401b-946e-5a275212fa06"
	      	AssingToUserName="رئیس شهرسازی منطقه7" 
	   ElseIf type =7 Then
	      	AssingTo = "d5abb162-7c34-473d-8b16-9f959deb222b"
	      	AssingToUserName="املاك-منطقه7"    	  
	   ElseIf type =10 Then
	      	AssingTo = "42f9ad02-bd68-45f0-be10-aa0e0511029c"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه7" 
	  ElseIf type =11 Then
	      	AssingTo = "dff715e2-c41a-4d72-95d3-7276082ffcd2"
	      	AssingToUserName="مسئول ترافیک منطقه7" 
	     ElseIf type =15 Then
	      	AssingTo = "FECA8CB2-C31E-4BE4-9489-0F0878784EFB"
	      	AssingToUserName="ناظر درآمد منطقه7 "      	   	   	  	  	   	  	  	
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "e12a1bca-b461-4cb5-8d7d-5e8c7ea4efb2"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه7"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "4F31FA3B-140F-4861-8758-409FD07F4060"
	      	AssingToUserName="حسابداری-منطقه7"	      	  	   	  	 	  	   	   		 	  		
	    End if 		
      	
     End if


   IF District = 8 Then
    	if type =1 Then
	    	AssingTo = "FFABD92F-047F-4BE5-B552-073FD10C32E8"
	      	AssingToUserName="ناظر شهرسازی منطقه8"
	    ElseIf type =2 Then	
	      AssingTo = "C0B05F70-5A08-41F5-BFD3-5783FA4E2160"
      	  AssingToUserName="دفتر نقش آفرینان داوین-نقشه برداری"	
      	ElseIf type =3 Then	
      		AssingTo = "115f3b4f-a668-499c-b72b-07af47778b1c"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 8"	
	    ElseIf type =4 Then
	      	AssingTo = "375c2bc1-2346-42aa-b8b7-a64840b8e327"
	      	AssingToUserName="نقشه برداری منطقه8"  
	    ElseIf type =5 Then
	      	AssingTo = "BAA667AA-261F-46DD-97E4-A7CE261967BB"
	      	AssingToUserName="پرداختی اول - منطقه 8"  
	    ElseIf type =6 Then
	      	AssingTo = "510c16e9-9c0d-429c-892f-49acf102276c"
	      	AssingToUserName="رئیس شهرسازی منطقه8"  
	    ElseIf type =7 Then
	      	AssingTo = "de2154bf-f920-47ff-8578-38d510daf9ab"
	      	AssingToUserName="املاك-منطقه8"  
	     ElseIf type =10 Then
	      	AssingTo = "d40b643f-7ee1-496e-85be-e846be6ac718"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه8" 
	     ElseIf type =11 Then
	      	AssingTo = "c134142a-41b1-4ac3-a3dd-9d4ada74236a"
	      	AssingToUserName="مسئول ترافیک منطقه8" 	
	     ElseIf type =15 Then
	      	AssingTo = "15B6D716-8F6A-42A9-A26F-3153CD4E2222"
	      	AssingToUserName="ناظر درآمد منطقه8 "      	     	    	     	    	  	    	     	    	  	  
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "01037a42-4e0e-41ed-9d66-ae4f473c8f2a"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه8"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "A68F84DD-C751-40D5-89D2-B7CFCE5DFFAB"
	      	AssingToUserName="حسابداری-منطقه8"	      	  	   	  	 	  	   	   		 	  		
	    End if 	   
     End if

    IF District = 9 Then
    	if type =1 Then 
	    	AssingTo = "D16F1E72-14CA-4BAA-ABB6-5BAEB81F8E3B"
	      	AssingToUserName="ناظر شهرسازی منطقه9"
	     ElseIf type =2 Then
	      	AssingTo = "EA722297-661A-4E06-B367-DD1F3D1E3CE3"
      		AssingToUserName="دفتر آتی سازان"
      	ElseIf type =3 Then	
      		AssingTo = "ebb929d4-8345-4481-9fce-f0431d9509d0"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 9"	
	    ElseIf type =4 Then
	      	AssingTo = "4c9d6ec7-ae05-45d0-ab21-78e76a639256"
	      	AssingToUserName="نقشه برداری منطقه9"  
	    ElseIf type =5 Then
	      	AssingTo = "D0DD6AAC-AF7F-4F39-96E6-BC3AB70D555A"
	      	AssingToUserName="پرداختی اول - منطقه 9"
	    ElseIf type =6 Then
	      	AssingTo = "35e8fc43-418a-431f-9e8e-15cf116287f6"
	      	AssingToUserName="رئیس شهرسازی منطقه9"
	     ElseIf type =7 Then
	      	AssingTo = "f58b455d-411d-4ee8-8fa4-a0d469916a64"
	      	AssingToUserName="املاك-منطقه9" 
	    ElseIf type =10 Then
	      	AssingTo = "9203a73b-9525-4d18-9b41-bd0fa8f65fb0"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه9"
	    ElseIf type =11 Then
	      	AssingTo = "8b1ba4a0-e667-4fbc-b2d8-02a720994e7d"
	      	AssingToUserName="مسئول ترافیک منطقه9" 	 
	     ElseIf type =15 Then
	      	AssingTo = "3B62E988-6932-47A9-8C0E-24B476F6991A"
	      	AssingToUserName="ناظر درآمد منطقه9	 "       	        	       	      	    	
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "9a5e9a85-8c1c-4553-bba0-4d9d33b5b559"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه9"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "96CABD8B-EC53-4F43-A308-5D2B3FC11A0A"
	      	AssingToUserName="حسابداری-منطقه9"	      	  	   	  	 	  	   	   		 	  		
	    End if	
      	
     End if



    IF District = 10 Then
    	if type =1 Then 
	    	AssingTo = "D5A22042-E3E3-48DD-9729-0663A13FE836"
	      	AssingToUserName="ناظر شهرسازی منطقه10"
	     ElseIf type =2 Then
	      	AssingTo = "A3DD1FD6-0D77-409B-BD49-77CBD841CA70"
      		AssingToUserName="دفتر طرح مسیر پارت"
      	ElseIf type =3 Then	
      		AssingTo = "62583296-EDF8-4C44-9B6A-FE1D8DEA1E18"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 10"	  
	    ElseIf type =4 Then
	      	AssingTo = "ebb38a07-cd2a-4f71-bb54-26eb4efaa09e"
	      	AssingToUserName="نقشه برداری منطقه10"   
	    ElseIf type =5 Then
	      	AssingTo = "FE6BB968-FD23-43D0-8F0B-66B6711EEF38"
	      	AssingToUserName="پرداختی اول - منطقه 10" 
	     ElseIf type =6 Then
	      	AssingTo = "f2b549d3-2bf8-4ba1-ac3d-98cdfff2ffe4"
	      	AssingToUserName="رئیس شهرسازی منطقه10"   
	     ElseIf type =7 Then
	      	AssingTo = "f5b54f6f-fb8c-485c-8717-e06a41447770"
	      	AssingToUserName="املاك-منطقه10" 
	    ElseIf type =10 Then
	      	AssingTo = "bbdc4800-14c2-48a7-bcbb-d2d62159ea48"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه10"    
	    ElseIf type =11 Then
	      	AssingTo = "bdb7e852-4a70-4c2f-8dd1-6787c1059c76"
	      	AssingToUserName="مسئول ترافیک منطقه10" 
	     ElseIf type =15 Then
	      	AssingTo = "25768045-AF16-44D4-BFFA-A0F464774E7A"
	      	AssingToUserName="ناظر درآمد منطقه10 "  	      	   		      	  	  	    	  	      	   	    		
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "06195ee5-307c-4259-8d51-03a1aa49fc8e"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه10"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "E35A988C-E8BF-402A-9F5C-D5133029F219"
	      	AssingToUserName="حسابداری-منطقه10"	      	  	   	  	 	  	   	   		 	  		
	    End if		
      	
     End if

	
    IF District = 11 Then
    	if type =1 Then
	    	AssingTo = "80DBF034-4632-468D-B503-A87A3D8AD4C8"
	      	AssingToUserName="ناظر شهرسازی منطقه11"
	    ElseIf type =2 Then
	      	AssingTo = "7422715a-7cc9-48fd-a6ee-a032c2fc524c"
      		AssingToUserName="دفتر نقش ارا پیمایش شرق"
      	ElseIf type =3 Then	
      		AssingTo = "3ada68fa-7a99-4ab6-a403-9057a7786404"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 11"	
	    ElseIf type =4 Then
	      	AssingTo = "51f93d4a-83c0-4386-884c-8cceda5ae728"
	      	AssingToUserName="نقشه برداری منطقه11" 
	    ElseIf type =5 Then
	      	AssingTo = "F5EE3CA2-A12A-4A5F-9551-8749916ACBE1"
	      	AssingToUserName="پرداختی اول - منطقه 11"
	     ElseIf type =6 Then
	      	AssingTo = "1fbe4962-1aa5-4564-b805-9611d2cec45e"
	      	AssingToUserName="رییس شهرسازی منطقه 11" 
	     ElseIf type =7 Then
	      	AssingTo = "55223402-765d-4008-8002-c01e37c48bad"
	      	AssingToUserName="املاك-منطقه11"   
	     ElseIf type =10 Then
	      	AssingTo = "51b07cbd-6174-4592-b2af-b40b3eedb558"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه11" 
	     ElseIf type =11 Then
	      	AssingTo = "b2ef2d9b-5ab2-4134-b352-0901ed2c4299"
	      	AssingToUserName="مسئول ترافیک منطقه11" 
	     ElseIf type =15 Then
	      	AssingTo = "00393BED-5B74-48E4-B7FA-B21E7C39B55B"
	      	AssingToUserName="ناظر درآمد منطقه11 "  
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "5a444fd2-887b-4eb6-9bab-54a194d09faa"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه11 "	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "AC27B681-9C2D-479D-8CB8-BB097A22A7D2"
	      	AssingToUserName="حسابداری-منطقه11"	      	  	   	  	 	  	   	   		 	  		
	    End if	
     End if


    IF District = 12 Then
    	if type =1 Then
	    	AssingTo = "1573B65F-F3FA-4C41-9A2F-EB46DCB34D7C"
	      	AssingToUserName="ناظر شهرسازی منطقه12"
	    ElseIf type =2 Then
	    	if {16,17,18,19,20}.contains(Region) Then	'sp 507430
		      	AssingTo = "8923d414-828d-4462-b3c3-2b85d9acb7cc"
	      		AssingToUserName="دفترشهرسازان نام آشنا-نقشه برداری"	    	
	    	Else
		      	AssingTo = "4e110b2d-e0eb-448f-a417-2a005ae96ec0"
	      		AssingToUserName="دفتر باتاب آژند طوس-نقشه برداری"
	      	End if	      	
	 '   ElseIf type =2 Then
	 '     	AssingTo = "4e110b2d-e0eb-448f-a417-2a005ae96ec0"
     ' 		AssingToUserName="دفتر باتاب آژند طوس-نقشه برداری"
      	ElseIf type =3 Then	
      		AssingTo = "f5741054-0108-4c03-872b-867c46e18d91"
	      	AssingToUserName="ناظر نوسازی و اصناف منطقه 12"	
	    ElseIf type =4 Then
	      	AssingTo = "d0146965-8f21-4ea2-a4fb-c6cdac3f0382"
	      	AssingToUserName="نقشه برداری منطقه12"  
	     ElseIf type =5 Then
	      	AssingTo = "581D9482-A6DB-4C3A-819A-080C6B1A5582"
	      	AssingToUserName="پرداختی اول - منطقه 12" 
	     ElseIf type =6 Then
	      	AssingTo = "2070504a-4849-4163-83c0-6cdbb910bea0"
	      	AssingToUserName="رئیس شهرسازی منطقه12"     
	      ElseIf type =7 Then
	      	AssingTo = "de58350a-c663-4296-8b80-4246a6ee08a8"
	      	AssingToUserName="املاك-منطقه12"
	      ElseIf type =10 Then
	      	AssingTo = "f8065bd5-223d-4f22-acb4-7b175374a18e"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه12" 
	     ElseIf type =11 Then
	      	AssingTo = "83ec86b3-3457-4397-b1bb-99eb08789b9f"
	      	AssingToUserName="مسئول ترافیک منطقه12"  
	     ElseIf type =15 Then
	      	AssingTo = "A181DB82-921A-4F0C-AAC3-91E8889438B7"
	      	AssingToUserName="ناظر درآمد منطقه12"  		      	     	 	    	     	  	  			
	     ElseIf type =16 Then 'sp 502980
	      	AssingTo = "97dd8d25-fec2-42d8-b42e-e81c91a355ac"
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه12"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "56B8FD55-B366-4679-B54C-3E3C5C5EA0FA"
	      	AssingToUserName="حسابداری-منطقه12"	      	  	   	  	 	  	   	   		 	  		
	    End if
    End if
    
IF District = 80 Then
    	if type =1 Then
	    	AssingTo = "F419126B-9B48-4F1A-BBC3-32982BBFEC09"
	      	AssingToUserName="ناظر شهرسازی منطقه ثامن"   
	     ElseIf type =2 Then
	      	AssingTo = 	"AC01F67F-FAE6-4CAD-8B61-AEB2C3E345AB"	' 485857	"AC3A67C1-79C2-47ED-9BDD-BBD287B1B243"
      		AssingToUserName= "دفترکیان طرح نقش جاوید-نقشه برداری"	'485857	"دفتر پارس طلوع خاوران-نقشه برداری"
      	ElseIf type =3 Then	
      		AssingTo = "5877DEB5-2ED6-4578-A4C6-60361A91C790"
	      	AssingToUserName = "ناظر نوسازی و اصناف منطقه ثامن "
	     ElseIf type =4 Then  
	    	AssingTo = "96CC516A-70AF-4D07-AF5A-8EFD0C28E978"
	      	AssingToUserName="نقشه برداری منطقه ثامن" 
	     ElseIf type =5 Then  
	    	AssingTo = "39F2E70E-DC79-46FB-A5A6-6BB44CFA26A8"
	      	AssingToUserName="پرداختی اول - منطقه ثامن"	      	
	     ElseIf type =6 Then  
	    	AssingTo = "647C296A-8109-45DA-870A-4BC7E2EC31C6"
	      	AssingToUserName="ناظر عالی شهرسازی-تاییدگروه الف وب-منطقه ثامن" 
	     ElseIf type =7 Then  
	    	AssingTo = "F419126B-9B48-4F1A-BBC3-32982BBFEC09"
	      	AssingToUserName="ناظر شهرسازی منطقه ثامن " 
	     ElseIf type =8 Then  
	    	AssingTo = "D5AD3002-FA83-4042-BD8B-90D6CFED0C45"
	      	AssingToUserName="بازدید ثبت(حدنگار)" 	      	
	     ElseIf type =11 Then  
	    	AssingTo = "AFE14000-BFFF-4706-9693-3CB16A9C1676"
	      	AssingToUserName="مسئول ترافیک منطقه ثامن"     		      		      		      		      		      		
      	ElseIf type =9 Then
	      	AssingTo = "7857a335-e3b9-41ae-ae08-e4af9361cd09"
	      	AssingToUserName="پلیس ساختمان"   	
	    ElseIf type =10 Then
	      	AssingTo = "42BE848F-D73C-472E-9761-FCD8A38D5F7B"
	      	AssingToUserName="کمیته انطباق شهری(شهرساز)-منطقه ثامن"  
	    ElseIf type =11 Then
	      	AssingTo = "5829F586-7C15-43E0-8D46-848FC2576533"
	      	AssingToUserName=" املاک-منطقه ثامن" 	  	
	'	ElseIf type =12 Then
	 '     	AssingTo = "F67BC411-7478-405E-8A23-412A452F7FB3"
	 '     	AssingToUserName="سازمان تامین اجتماعی-شعبه 5"
	     ElseIf type =15 Then
	      	AssingTo = "3FA4A5E4-A64D-4225-A452-F003DC20F7F7"
	      	AssingToUserName="ناظر درآمد منطقه ثامن"      	  			
	    ElseIf type =16 Then 'sp 502980
	      	AssingTo = "a5b1b08e-ff34-429b-9e0e-c09264cdef48 "
	      	AssingToUserName="کارشناس کمیسیون ماده5-منطقه ثامن"	      	  	   	  	 	  	   	   		 	  		
	     ElseIf type =17 Then  
	      	AssingTo = "E29AE23A-E076-4050-8564-6CDF2E8B7CB6"
	      	AssingToUserName="حسابداری-منطقه ثامن"	      	  	   	  	 	  	   	   		 	  		
	    End if
   End if	  
End Function


Public Sub  Logesup(ByVal N1 as String,ByVal N2 as String )

Dim tmpFileStream As New System.IO.FileStream( "C:\logC\" & N1 & ".txt", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite)
Dim tmpBinaryWriter As New System.IO.BinaryWriter(tmpFileStream)
tmpBinaryWriter.Write (N2)
tmpFileStream.Close()


   
End Sub

Public Function Tablet () as Integer
	Dim ResultTablet as Integer=-1
	if Info8.GetRequest.Info.NidNosaziCode.tostring ="8D91AC3D-7DE2-4265-B681-8E2939E3DEB1" or Info8.GetRequest.Info.NidNosaziCode.tostring ="A28C7738-0B5B-48E6-80E4-960EFAEC5A8D" _
	or Info8.GetRequest.Info.NidNosaziCode.tostring ="AB47BE15-2EF3-43BB-A446-C46A5C8B6C42" or Info8.GetRequest.Info.NidNosaziCode.tostring ="7C237F82-96FF-4B55-B776-AF0B9E9E9982" _
	or Info8.GetRequest.Info.NidNosaziCode.tostring ="9C642710-A9CA-44D0-986E-16DA244DE66B" or Info8.GetRequest.Info.NidNosaziCode.tostring ="9149FBAF-386F-413D-9AB7-E9BB4C158D30" _
	or Info8.GetRequest.Info.NidNosaziCode.tostring ="FDE4A19E-6323-4105-9298-4FE98AD35287" or Info8.GetRequest.Info.NidNosaziCode.tostring ="2228FE6B-AED6-4CD6-B83E-070755A16CDD" _
	or Info8.GetRequest.Info.NidNosaziCode.tostring ="62E0B0B3-FD76-4132-91DF-9D89B08C4208" Then Return 3      
	

	Try
		ResultTablet=Info8.GetFunctions.GetRevisitTabletTask(Info8.GetRequest.Info.NidProc).EumTaskState
	Catch ex As Exception
			ResultTablet=-1
	 End Try
	 
	 Return ResultTablet      
		    
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
            
           
           '  If Not  Info8.GetFunctions.GetHolidays_ByDate(TmpD) Then 
              i = i + 1
        Loop
        

         
        Return TmpD
    End Function
    
Private Function Analys()  as Boolean
    Try
         Dim tmpAllAnaliys As New List(Of DtoSC.AnalysisBuilding)
         Dim FlagL As Integer
 
        Info8.ParameterList.Add("AnalysisBuilding_Nidbase",Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 1, 0,Info8.GetRequest.Info.NidRevisit).tostring)
		
			
        tmpAllAnaliys = Info8.GetAnalysisBuilding.AnalysisBuildingList
        
        
        
        AnaliysParvaneh_time = ""
        
        If tmpAllAnaliys.Any Then
           AnaliysParvaneh_Date = tmpAllAnaliys.Max(Function(F) F.PenaltyDate)
           tmpAllAnaliys = tmpAllAnaliys.Where(Function(F) F.PenaltyDate = AnaliysParvaneh_Date).ToList
           If tmpAllAnaliys.Any Then AnaliysParvaneh_time = tmpAllAnaliys.Max(Function(F) F.PenaltyTime)                 
           Return True
       Else
           Return False    
       End If

        
    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Analys", ex.message)
    End Try
End Function    

Private Function FinePenalty()  as Boolean
    Try
         Dim tmpAllFinePenalty As New List(Of DtoSC.Commission_FinePenalty)
         Dim FlagL As Integer
 
        'Info8.ParameterList.Add("AnalysisBuilding_Nidbase",Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 1, 0,Info8.GetRequest.Info.NidRevisit).tostring)
		
			
        tmpAllFinePenalty = Info8.GetFine.GetCommission_FinePenalty
        
        
        If tmpAllFinePenalty.Any Then
           Return True
       Else
           Return False    
       End If

        
    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "FinePenalty", ex.message)
    End Try
End Function    

Private Function Agreement() as Integer
	
    Try    

        Dim tmpAgreement As New List(Of DtoSC.Sh_Agreement)
         
        Dim TmpParent As New List(Of Global.Biz.SA.ClsNosaziCodeType)
         
		TmpParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
		if TmpParent.count > 0 Then
			TmpParent=TmpParent.where(Function (f) f.EumNosaziCodeObjType=2).tolist
			Info8.ParameterList.Add("Agreement_List_NidBase",Info8.GetFunctions.GetNidBase(TmpParent(0).NidNosaziCode, 1,5,Info8.GetRequest.Info.NidRevisit).tostring)
		End if
		
		
        tmpAgreement = Info8.GetAgreement_List.GetSh_Agreement_List(Info8.GetRequest.Info.Nidproc, Info8.Security_RequestGuid, Info8.Security_EncryptCode)

    
        If tmpAgreement.Any Then
           Agreement_Date = tmpAgreement.Max(Function(F) F.AgreementDate)
           tmpAgreement = tmpAgreement.Where(Function(F) F.AgreementDate = Agreement_Date).ToList
           Agreement_time = tmpAgreement.Max(Function(F) F.AgreementTime) 
           tmpAgreement=tmpAgreement.Where(Function(F) F.AgreementTime = Agreement_time).ToList
           Info8.GetAgreement_List.LoadObj(tmpAgreement(0).NidBase, Info8.GetRequest.Info.NidProc, Info8.Security_RequestGuid, Info8.Security_EncryptCode)

		   WhatisNidBase = TmpParent(0).NidNosaziCode.tostring
           BaseU=Info8.GetAgreement_List.GetBase_Using()
 
           Return 1
        Else
           Return 2	              
        End If

       'Return -2  
    Catch Ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Agreement", ex.message)
    End Try
End Function    

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
		
End Function

Public Function PriceSms(ByVal NameSp as String ) 
	Dim p_Connection As String = "Data Source=172.16.8.136;Initial Catalog=Sara8M03;Persist Security Info=True;User ID=esup92;Password=zxc@789-sf"
	Dim intRowsAffected As String 
	
	Dim con As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(p_Connection)
	con.Open()

	Dim cmd As New System.Data.SqlClient.SqlCommand(NameSp, con)
	cmd.CommandType = CommandType.StoredProcedure
	cmd.Parameters.AddWithValue("@NidProc", Info8.GetRequest.Info.NidProc)
	intRowsAffected = cmd.ExecuteNonQuery()
	Dim Ret As Double = 0
 	Ret=cmd.ExecuteScalar()
	
	con.Close()
	con = Nothing

	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
    Dim ss as Integer =0
   
    For ss=0 to lstOwner.count-1
    	   Sms(lstOwner(ss).OwnerMobile,"فاکتور بازدید"," مالک محترم مبلغ فاکتور خدمات بازدید   کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " با احتساب 10% مالیات بر  ارزش افزوده: " + Ret.tostring + " ریال می باشد " _
				+ " که توسط کارشناس بازدید دریافت می شود. ضمنا فاکتور حاضر بر مبنای سابقه مجاز ملک/آپارتمان برآورد گردیده که در صورت تغییر گروه ساختمانی فاکتور تغییر خواهد یافت.شهرداری مشهد ")
    Next
       Sms(Info8.GetRequest.Info.Cellphone,"فاکتور بازدید"," مالک محترم مبلغ فاکتور خدمات بازدید   کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " با احتساب 10% مالیات بر  ارزش افزوده: " + Ret.tostring + " ریال می باشد " _
				+ " که توسط کارشناس بازدید دریافت می شود. ضمنا فاکتور حاضر بر مبنای سابقه مجاز ملک/آپارتمان برآورد گردیده که در صورت تغییر گروه ساختمانی فاکتور تغییر خواهد یافت.شهرداری مشهد ")
'sp 10143614
   ' Sms(Info8.GetRequest.Info.Cellphone,"فاکتور بازدید"," مالک محترم مبلغ فاکتور خدمات بازدید   کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " به شرح ذیل میباشد " _
'		+"بهای خدمات بازدید + 9% مالیات ارزش افزوده: " + Ret.tostring + " ریال می باشد .شهرداری مشهد ")

	
End Function	        

Public Function ConfirmOwner()   'تایید مالک

Call DontCloseCheckList(Info8.GetOnTickChecklistParameter().NidChecklistValue)	' sp 492647End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("08a457bd-97af-4e28-b200-e80928a0b4ed") Then     'sp 492585
	Try	
	
			Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
			lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
			Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
			lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
	       	Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
	       	lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
			Dim UserG as String
			Dim NameUser as String
			UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
			NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
			CreatTask("Activity_06wuin7",UserG,NameUser)
	Catch e as exception
		msg("Ex: " & e.message)
	End Try
End if   'sp 492585

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d281ff96-275b-4f64-bec2-92fa2556171a") Then

 	Call User (1) 
 	CreatTask("Activity_16rf1pk",AssingTo,AssingToUserName)  'nazer
End if		 	

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e058ccf7-c728-417c-90f5-a90dfce2bec6") Then
	Dim G as guid
	Dim PayOfParent as Integer 	
	Dim TmpParent,Parent,ParentApr As New list(of BIZ.SA.ClsNosaziCodeType)
	
	Parent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
							
	TmpParent = Parent.Where(Function(f) f.EumNosaziCodeObjType =5).tolist
	if TmpParent.any Then 
		PayOfParent = Info8.GetDutyCalc.GetPayOffYear(TmpParent(0).NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
		if BIZ.SA.ClsCommon.CurrentShamsiYear > PayOfParent Then '  DontPay = True	
		
			msg(" تسویه نوسازی نشده است @@ ") 				
		End if
	Else
		ParentApr=Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)	
		TmpParent = ParentApr.Where(Function(f) f.EumNosaziCodeObjType=4).tolist			
		if TmpParent.any Then  
			For Each X as BIZ.SA.ClsNosaziCodeType in TmpParent
				'PayOfParent = Info8.GetDutyCalc.GetPayOffYear(TmpParent(0).NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
				PayOfParent = Info8.GetDutyCalc.GetPayOffYear(X.NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
				if BIZ.SA.ClsCommon.CurrentShamsiYear > PayOfParent Then 'DontPay = True : G = X.NidNosaziCode : Exit For
					
					msg("  تسویه نوسازی نشده است  %% " & Info8.GetFunctions.GetNosaziNickName(X.NidNosaziCode).tostring)						
				End if	
			Next	
		Else
			TmpParent = Parent.Where(Function(f) f.EumNosaziCodeObjType=2).tolist
			if TmpParent.any Then 				
				PayOfParent = Info8.GetDutyCalc.GetPayOffYear(TmpParent(0).NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
				if BIZ.SA.ClsCommon.CurrentShamsiYear > PayOfParent Then '  DontPay = True
					
					msg(" تسویه نوسازی نشده است  $$ ")				
				End if
			End if	
		End if	
	End if	
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8c36de6f-9b7c-4a4f-bfac-87131878c063") Then
		AssingTo =""
	 	AssingToUserName=""
	 	CreatTask("Activity_1h40goa",AssingTo,AssingToUserName)	'نود  تایمین اجتماعی
End if



'====================   تایید چک لیست مالک ====================

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7589e0f8-a7bb-4ca0-8512-eee357852733") Then	'sp 504929
	Dim G as guid
	Dim PayOfParent as Integer 	
	Dim TmpParent,Parent,ParentApr As New list(of BIZ.SA.ClsNosaziCodeType)
	
	Parent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
							
	TmpParent = Parent.Where(Function(f) f.EumNosaziCodeObjType =5).tolist
	if TmpParent.any Then 
		PayOfParent = Info8.GetDutyCalc.GetPayOffYear(TmpParent(0).NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
		if BIZ.SA.ClsCommon.CurrentShamsiYear > PayOfParent Then '  DontPay = True	
			Info8.GetOnTickChecklistParameter().Result="false"	
			Info8.GetOnTickChecklistParameter().Description ="  تسویه نوسازی انجام نشده است - دستگاه @@ " 				
		End if
	Else
		ParentApr=Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)	
		TmpParent = ParentApr.Where(Function(f) f.EumNosaziCodeObjType=4).tolist			
		if TmpParent.any Then  
			For Each X as BIZ.SA.ClsNosaziCodeType in TmpParent
				'PayOfParent = Info8.GetDutyCalc.GetPayOffYear(TmpParent(0).NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
				PayOfParent = Info8.GetDutyCalc.GetPayOffYear(X.NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
				if BIZ.SA.ClsCommon.CurrentShamsiYear > PayOfParent Then 'DontPay = True : G = X.NidNosaziCode : Exit For
					Info8.GetOnTickChecklistParameter().Result="false"	
					Info8.GetOnTickChecklistParameter().Description ="  تسویه نوسازی آپارتمان انجام نشده است @ " & Info8.GetFunctions.GetNosaziNickName(X.NidNosaziCode).tostring							
				End if	
			Next	
		Else
			TmpParent = Parent.Where(Function(f) f.EumNosaziCodeObjType=2).tolist
			if TmpParent.any Then 				
				PayOfParent = Info8.GetDutyCalc.GetPayOffYear(TmpParent(0).NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
				if BIZ.SA.ClsCommon.CurrentShamsiYear > PayOfParent Then '  DontPay = True
					Info8.GetOnTickChecklistParameter().Result="false"	
					Info8.GetOnTickChecklistParameter().Description ="  تسویه نوسازی ملک انجام نشده است "			
				End if
			End if	
		End if	
	End if		
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fab0db1c-efa4-4044-9b0c-29d1323e69b6") Then	'sp 504929
	Dim G as guid
	Dim PayOfParent as Integer 	
	Dim TmpParent,Parent,ParentApr As New list(of BIZ.SA.ClsNosaziCodeType)
	
	Parent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
							
	TmpParent = Parent.Where(Function(f) f.EumNosaziCodeObjType =6).tolist
	if TmpParent.any Then 
		PayOfParent = Info8.GetDutyCalc.GetPayOffYear(TmpParent(0).NidNosaziCode, BIZ.SA.ClsEnum.EumDutyType.Renovation)
		if BIZ.SA.ClsCommon.CurrentShamsiYear > PayOfParent Then '  DontPay = True	
			Info8.GetOnTickChecklistParameter().Result="false"	
			Info8.GetOnTickChecklistParameter().Description ="  تسویه صنفی انجام نشده است @@ " 				
		End if
	Else
			Info8.GetOnTickChecklistParameter().Result="false"	
			Info8.GetOnTickChecklistParameter().Description =" واحد صنفی وجود ندارد" 		
	End if			
End if




IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5a48480b-582b-4a0a-a017-8785d648732d") Then	'sp 504929
	user(1)
	CreatTask("Activity_0kod3q5",AssingTo,AssingToUserName)	'ناظر
End if

'sp 487040
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("00d2452a-1045-474d-9047-cf8313083626") Then
	Dim LstChildd As New List(Of Global.Biz.SA.ClsNosaziCodeType)
	LstChildd = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
	LstChildd = LstChildd.Where(Function(f) f.EumNosaziCodeObjType = 6).ToList
	if LstChildd.count>0 Then
		if NewControlCheck("80570d3a-c536-4d8c-aca1-d819afd01d5c")=4 And NewControlCheck("4c323663-2238-465e-930d-d8fb5d579e61")=4 Then 'اگر صنفی داشته باشه
			Call User(1)  
			CreatTask("Activity_0uc9dny",AssingTo,AssingToUserName)
		End if	
	Else
		if NewControlCheck("4c323663-2238-465e-930d-d8fb5d579e61")=4 Then 	
			Call User(1)  
			CreatTask("Activity_0uc9dny",AssingTo,AssingToUserName)		
		End if
	End if 
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("80570d3a-c536-4d8c-aca1-d819afd01d5c") Then
	if NewControlCheck("00d2452a-1045-474d-9047-cf8313083626")=4 And NewControlCheck("4c323663-2238-465e-930d-d8fb5d579e61")=4 Then 
		Call User(1)  
		CreatTask("Activity_0uc9dny",AssingTo,AssingToUserName)
	End if 
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4c323663-2238-465e-930d-d8fb5d579e61") Then
	Dim LstChildd As New List(Of Global.Biz.SA.ClsNosaziCodeType)
	LstChildd = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
	LstChildd = LstChildd.Where(Function(f) f.EumNosaziCodeObjType = 6).ToList
	if LstChildd.count>0 Then
		if NewControlCheck("00d2452a-1045-474d-9047-cf8313083626")=4 And NewControlCheck("80570d3a-c536-4d8c-aca1-d819afd01d5c")=4 Then  'اگر صنفی داشته باشه
			Call User(1)  
			CreatTask("Activity_0uc9dny",AssingTo,AssingToUserName)
		End if	
	Else
		if NewControlCheck("00d2452a-1045-474d-9047-cf8313083626")=4 Then 	
			Call User(1)  
			CreatTask("Activity_0uc9dny",AssingTo,AssingToUserName)		
		End if
	End if 
End if
'end sp 487040

'sp 486407	============
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3365fa93-cd00-44f1-9c14-5a695c11eac9") Then 
      	         
	 Dim BlackList As New List(Of DtoSA.Sh_BlackList)
	 BlackList=Info8.GetFunctions.CheckInBlackList(Info8.GetRequest.Info.NidNosaziCode)
	 
	 if BlackList.any Then 
		SendPayamak("شهرداری مشهد "," مالک محترم پرونده شما به شماره " & Info8.GetRequest.Info.NidWorkItem.tostring & " کدنوسازی " & ReverseString(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring) & " در لیست کنترلی قرار دارد لطفا نسبت به رفع مشکل به شهرداری منطقه مراجعه فرمایید ")
	 End if
End if 'sp 486407


'sp 462171	============
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4ba9db18-b41e-4e83-97de-a1891e533317") Then 

'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", ControlCheck("76b715db-927c-4283-adf1-9c079efdca34").tostring)

	if ControlCheck("76b715db-927c-4283-adf1-9c079efdca34") = 3 And ControlCheck("cd2e0d48-c0e4-41b9-b1bb-4fa388f905c7")= 3 Then
	'if ControlTask("Activity_0scwa4a")<> 1 Then
		 AssingTo =""
		 AssingToUserName=""	
		 Call User(1)
		CreatTask("Activity_0scwa4a",AssingTo,AssingToUserName)
'	End if
	End if
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("76b715db-927c-4283-adf1-9c079efdca34") Then 

	if ControlCheck("4ba9db18-b41e-4e83-97de-a1891e533317") = 3 And ControlCheck("cd2e0d48-c0e4-41b9-b1bb-4fa388f905c7")= 3 Then
	'if ControlTask("Activity_0scwa4a")<> 1 Then
		 AssingTo =""
		 AssingToUserName=""	
		 Call User(1)
		CreatTask("Activity_0scwa4a",AssingTo,AssingToUserName)
'	End if
	End if
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("cd2e0d48-c0e4-41b9-b1bb-4fa388f905c7") Then 

	if ControlCheck("76b715db-927c-4283-adf1-9c079efdca34") = 3 And ControlCheck("4ba9db18-b41e-4e83-97de-a1891e533317")= 3 Then
	'if ControlTask("Activity_0scwa4a")<> 1 Then
		 AssingTo =""
		 AssingToUserName=""	
		 Call User(1)
		CreatTask("Activity_0scwa4a",AssingTo,AssingToUserName)
'	End if
	End if
End if
'End sp 462171	============

'  sp  483615  ============

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7c443cc5-cb27-4c19-833d-defe46e54b23") Then 'اختلاف ناظر و مجری  (تایید مالک)
'msg(ControlTask("Activity_1atjcx5").tostring)
'Exit Function

	
	Dim BB As New List(Of Global.DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	BB = Info8.GetFunctions.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
	BB = BB.where(Function(f) f.EumRequestStatus = 2).toList()
	BB.orderByDescending(Function (f) f.RequestDate)
	
	Try
	if BB.any() Then
		
		For Each x as DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult in BB
	
	        if  Ucase(x.NidWorkflowDeff.tostring) = Ucase("88543DAB-F235-4B44-84B4-2776173CF0B4") Then
	        
			  Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
			  LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(BB(0).NidProc)
			  Dim i as Integer
			  if LstObj.data.count>0 Then 
			     For i = 0 To LstObj.data.count - 1
				'	Try
					  if ucase(LstObj.data(i).NidCheckList)= ucase("36411494-d782-4901-830c-e16f4494e073")  Then					  	
					  	if LstObj.data(i).CI_CheckListValue.tostring = "2" Then msg("درخواست قبلی طبق فرآیند وجود ندارد - sp 503426") : Exit Function
		  	          End if
		  	     ''   Catch ex as exception  
		  	     '   	msg("خطا در دریافت دریافت چک لیست قبلی")
		  	    '    End Try  
		  	     Next
		  	   End if       	
	        End if	        
	    Next 
	End if 
	Catch e as exception
	
	 End Try

'if 1 = 2 Then  'sp 503426
	if NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c") = 4 Then  'اگر ساختمان گروه الف نبود 


        Dim randomValue As Integer
        Dim rng As New Random()
        randomValue = rng.Next(1, 3)

	    If randomValue = 1 Then
	        'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
				AssingTo =""
				AssingToUserName=""
				Call User(13)   'کانون کارشناسان​ رسمی
				CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
				SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
	    Else
	        'CreatTask("Activity_0mpzy4j","22835b9b-7469-4d1d-8f34-49dbe194ea9a","مرکز کارشناسان قوه قضاییه")
				AssingTo =""
				AssingToUserName=""
				Call User(14)  'کانون-مرکز کارشناسان
				CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
				SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
	    End If 
	    
	 Else
	 
	 	if ControlTask("Activity_1atjcx5")=2 Then
	        Dim randomValue As Integer
	        Dim rng As New Random()
	        randomValue = rng.Next(1, 3)
	
		    If randomValue = 1 Then
		        'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
					AssingTo =""
					AssingToUserName=""
					Call User(13)   'کانون کارشناسان​ رسمی
					CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
					SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
		    Else
		        'CreatTask("Activity_0mpzy4j","22835b9b-7469-4d1d-8f34-49dbe194ea9a","مرکز کارشناسان قوه قضاییه")
					AssingTo =""
					AssingToUserName=""
					Call User(14)  'کانون-مرکز کارشناسان
					CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
					SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
		    End If 
		Else
			msg("ابتدا مرحله بازدید تکمیل گردد")
			Exit Function    
		End if    	 	
	 End if 
'End if '1=2	  	    	    
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6a95e162-2d02-47dc-ae23-19ab16324c11") Then 

	if GetBuildingGroup() Then 'true: ساختمان گروه الف و بازدید چک نشود ارسال مرحله 


        Dim randomValue As Integer
        Dim rng As New Random()
        randomValue = rng.Next(1, 3)

	    If randomValue = 1 Then
	        'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
				AssingTo =""
				AssingToUserName=""
				Call User(13)   'کانون کارشناسان​ رسمی
				CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
				SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
	    Else
	        'CreatTask("Activity_0mpzy4j","22835b9b-7469-4d1d-8f34-49dbe194ea9a","مرکز کارشناسان قوه قضاییه")
				AssingTo =""
				AssingToUserName=""
				Call User(14)  'کانون-مرکز کارشناسان
				CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
				SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
	    End If 
	    
	 Else
	 
	 	if ControlTask("Activity_1d3nq81")=2 Then
	        Dim randomValue As Integer
	        Dim rng As New Random()
	        randomValue = rng.Next(1, 3)
	
		    If randomValue = 1 Then
		        'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
					AssingTo =""
					AssingToUserName=""
					Call User(13)   'کانون کارشناسان​ رسمی
					CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
				SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
		    Else
		        'CreatTask("Activity_0mpzy4j","22835b9b-7469-4d1d-8f34-49dbe194ea9a","مرکز کارشناسان قوه قضاییه")
					AssingTo =""
					AssingToUserName=""
					Call User(14)  'کانون-مرکز کارشناسان
					CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
					SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
		    End If 
		Else
		'	msg("ابتدا مرحله بازدید تکمیل گردد")
		'	Exit Function    
		End if    	 	
	 End if   
	    
End if
'End  sp  483615  ============

'=====================  پروانه پارکینگ -  تایید چک لیست مالک  	=============== sp 475882 ===================
'ساختمان گروه الف (تا دوسقف یا 600 متر مربع ) می باشد؟
if 1=2 Then
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d8aa0716-3410-48a1-92d1-3d88bbdfe737") Then 

			'به آخرین کارشناس بازدید فعال ارسال گردد.
			
			Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
			lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
			Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
			lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
	       	Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
	       	lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
			Dim UserG as String
			Dim NameUser as String
			UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
			NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
			CreatTask("Activity_1d3nq81",UserG,NameUser)
End if			
End if 

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("dbbc7443-f653-44d4-a4a8-243a98c31313") Then 
	ExecSp("[dbo].[SP_CopyOwnerShahkar]","")		
End if 


'دریافت فاکتور خدمات فنی و مهندسی (گروه ساختمانی ب-ج-د)
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("dbbc7443-f653-44d4-a4a8-243a98c31313") Then 
	ExecSp("[dbo].[SP_CopyOwnerShahkar]","")		
End if 

' ساختمان گروه الف (تا دوسقف یا 600 متر مربع )
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("475eb350-1c76-4ce9-8591-b5deecd5390c") Then  
	
        Dim randomValue As Integer
        Dim rng As New Random()
        randomValue = rng.Next(1, 3)
        If randomValue = 1 Then	
			AssingTo =""
			AssingToUserName=""
		 	Call User(16)   'مهندس ناظر معماری        
        	CreatTask("Activity_1074fhi",AssingTo,AssingToUserName)  ' ارسال به نظام مهندسی 
        Else
        	randomValue = rng.Next(1, 3)
	        If randomValue = 1 Then
	            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
				AssingTo =""
				AssingToUserName=""
				Call User(13)   'کانون کارشناسان​ رسمی
				CreatTask("Activity_0tqwaqe",AssingTo,AssingToUserName)
				SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
	        Else
	            'CreatTask("Activity_0mpzy4j","22835b9b-7469-4d1d-8f34-49dbe194ea9a","مرکز کارشناسان قوه قضاییه")
				AssingTo =""
				AssingToUserName=""
				Call User(14)  'کانون-مرکز کارشناسان
				CreatTask("Activity_0tqwaqe",AssingTo,AssingToUserName)
					SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
	        End If        	        	
        End if	      	
End if

' چک لیست مهندس ناظر دارم
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6d7ab24e-9480-4efa-b7cf-b10b316f3b39") Then

Try
	Dim Test as New BIZ.SC.SrvEngineering.EngineerList
'	Test = Info8.GetFunctions.GetAllEngWithNidProc(guid.Parse("0005f012-5241-4fc2-8170-642b25042dfd"))
	Test = Info8.GetFunctions.GetAllEngWithNidProc(Info8.GetRequest.Info.nidproc)

	Dim listEnginer as New List(Of BIZ.SC.SrvEngineering.Eng_Info)
	Dim RefInfoLst as New List(Of BIZ.SC.SrvEngineering.Ref_Info)
	listEnginer = Test.EngInfoList
	RefInfoLst = Test.RefInfoList.where(Function (f) f.CI_Ability=1).tolist
	Dim i as Integer = 0
	Dim j as Integer = 0
	AssingTo =""
	AssingToUserName=""
  Dim dd as String 
  'الویت اول
 	For i=0 to RefInfoLst.count-1 
 	 
		For j=0 to listEnginer.count-1 
		       
			if RefInfoLst(i).NIdEng = listEnginer(j).NIdEng  Then
				if listEnginer(j).ADPstudyFieldRel = 100 or listEnginer(j).ADPstudyFieldRel = 110 or listEnginer(j).ADPstudyFieldRel = 120 _
				or listEnginer(j).ADPstudyFieldRel = 130 or listEnginer(j).ADPstudyFieldRel = 191 Then
			       
					AssingTo = listEnginer(j).NIdEng.tostring
					AssingToUserName = listEnginer(j).EngName.tostring + listEnginer(j).EngFamily.tostring 					
				 
					Exit For
				End if				
			
			End if	
	  	
		Next
			if Not String.IsNullOrEmpty(AssingTo) Then Exit For
	Next

	'الویت دوم 
if String.IsNullOrEmpty(AssingTo) Then 
	
 	For i=0 to RefInfoLst.count-1 
 	 
		For j=0 to listEnginer.count-1 
		       
			if RefInfoLst(i).NIdEng = listEnginer(j).NIdEng  Then
				if listEnginer(j).ADPstudyFieldRel = 300 or listEnginer(j).ADPstudyFieldRel = 310 or listEnginer(j).ADPstudyFieldRel = 320 _
					or listEnginer(j).ADPstudyFieldRel = 330 or listEnginer(j).ADPstudyFieldRel = 340 or listEnginer(j).ADPstudyFieldRel = 350 or listEnginer(j).ADPstudyFieldRel = 360 _
					or listEnginer(j).ADPstudyFieldRel = 370 or listEnginer(j).ADPstudyFieldRel = 380 or listEnginer(j).ADPstudyFieldRel = 391 _
					or listEnginer(j).ADPstudyFieldRel = 392 or listEnginer(j).ADPstudyFieldRel = 393 or listEnginer(j).ADPstudyFieldRel = 394 _
					or listEnginer(j).ADPstudyFieldRel = 395 Then	
			       
					AssingTo = listEnginer(j).NIdEng.tostring
					AssingToUserName = listEnginer(j).EngName.tostring + listEnginer(j).EngFamily.tostring 					
			   
					Exit For
				End if				
			
			End if	
	  	
		Next
			if Not String.IsNullOrEmpty(AssingTo) Then Exit For
	Next
End if 	
	
	'msg(AssingTo + " " + AssingToUserName)
	'Exit Function
	
	    CreatTask("Activity_1074fhi",AssingTo,AssingToUserName)  ' ارسال به نظام مهندسی 
	    
 Catch e as exception
       Info8.GetOnTickChecklistParameter.Result = "false"
       Info8.GetOnTickChecklistParameter.Description = e.message
	   Exit Function
 End Try 	    
End if


'=====================  پروانه پارکینگ - اتمام تایید چک لیست مالک  ================= sp 475882 ====================


'bana b sp 481116 sampa sp 480578 hazf shod	
'sp 480578
'	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1d6d8544-78ef-460e-bf8c-3343b67451e6") Then
'		Dim CountFiche as Integer 
'		Dim AllFiche as  list(of Dtosc.Income_Fiche)
'		AllFiche=Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
'		AllFiche=AllFiche.where(Function (f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) ).tolist '3 Taeed, 5 TaeedBank
'		CountFiche= AllFiche.count
'			if CountFiche<=0 Then 
'				Info8.GetOnTickChecklistParameter().Result="false" 
'			 	Info8.GetOnTickChecklistParameter().Description=" فیش تایید شده پرداختی درآمد ندارین" 							
'		 	Exit Function
'		End if 	
'	End if
'end sp 480578	

'=====================  پروانه کمیسیون ماده 5 -  تایید چک لیست مالک  ==================================
'sp 476127
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9efff845-7bb1-4588-a021-01bd62194fe1") Then
		AssingTo =""
		AssingToUserName=""
	 	Call User(1)
	 	CreatTask("Activity_0ul19d1",AssingTo,AssingToUserName)
End if 
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("762bc141-9e63-4bf1-b593-0e9039d642cc") Then
		if Check("2ded9566-1f3c-45fd-bc8d-1ecbea6658dd",1) = True		'sp 477680
			AssingTo =""
			AssingToUserName=""
		 	Call User(1)
		 	CreatTask("Activity_0ul19d1",AssingTo,AssingToUserName)
		 End if	
End if 
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2ded9566-1f3c-45fd-bc8d-1ecbea6658dd") Then
'msg(Check("762bc141-9e63-4bf1-b593-0e9039d642cc",1).tostring)
	if Check("762bc141-9e63-4bf1-b593-0e9039d642cc",1) = True	'sp 477680
			AssingTo =""
			AssingToUserName=""
		 	Call User(1)
		 	CreatTask("Activity_0ul19d1",AssingTo,AssingToUserName)
	 End if	
End if 
'=====================  پروانه کمیسیون ماده 5 - اتمام تایید چک لیست مالک  ==================================
	
'sp 448285    WTF ?!!!
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("88543dab-f235-4b44-84b4-2776173cf0b4") Then
	ExecSp("[SP_BazdidSara10]","")
End if 

'sp 476664
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bb4dadfa-e1da-4f28-9eb2-120373793f9d") Then
  		Call User (1) 
		CreatTask("Activity_06q8adj",AssingTo,AssingToUserName)	
End if 

'sp 446220
'sp 488436	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ea3ba403-3e38-4d9c-9480-2d00971a75b8") Then
'sp 488436		SaveCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c",2,0)
'sp 488436	End if 
'sp 488436	
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("37e5959a-c385-40ed-99cf-51f2557b6b6c") Then 'متراژ زیر بنا ملک تا 600 مترمربع و ارتفاع آن حداکثر 2 طبقه از روی زمین 
	SaveCheck("ea3ba403-3e38-4d9c-9480-2d00971a75b8",2,0)
	
End if 		

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ea3ba403-3e38-4d9c-9480-2d00971a75b8") Then  ' مهندس ناظر دارم

		'	Info8.GetOnTickChecklistParameter().Result="false"                       
		'  	Info8.GetOnTickChecklistParameter().Description=" الف: " + NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c").tostring

'msg(NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c").tostring)



	if NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c") = 3 Then  'اگر ساختمان گروه الف نبود -  تایید نشده مالک 
		SaveCheck("e7d55d2a-2986-404a-86e6-3f7eb0c8c03b",2,0)	
	Else	
	' گروه الف	
		Call User(11)
		CreatTask("Activity_0c0luv4",AssingTo,AssingToUserName) 	'مسئول ترافیک منطقه			
	End if

	if 1 = 2 Then  'sp 488436
		if NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c") = 3 Then  'اگر ساختمان گروه الف نبود -  تایید نشده مالک 
		
			'به بازدید کننده قبلیش ارسال میکرد 
			
			
				
				Dim NidProcForGetRevisitAgent as String
			
				
				Dim BB As New List(Of Global.DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
			    BB = Info8.GetFunctions.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
			    BB = BB.where(Function(f) f.EumRequestStatus = 2).toList()
				BB.orderByDescending(Function (f) f.RequestDate)
			
				Try
				    if BB.any() Then
		
		        
		        		For Each x as DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult in BB
		        
				            if  Ucase(x.NidWorkflowDeff.tostring) = Ucase("88543DAB-F235-4B44-84B4-2776173CF0B4") Then
				            
				 				Dim DateNow11 As Date = Date.Now
				  				Dim EndDate as String = BIZ.SA.ClsCommon.ConvertDateTimeToPersionString(Date.Now.AddDays(-30))		'(DateTime.Parse(x.RequestDate).AddDays(30))
				  				
				  				if EndDate <= x.RequestDate.tostring Then 
				            
					            	NidProcForGetRevisitAgent = x.NidProc.tostring
					            	
					            End if	
					            Exit For
				            End if 
				         Next
				    End if
				Catch e as exception
					msg("تاریخ بازدید " + e.message)
					Exit Function
				End Try    		
					
				if Not String.IsNullOrEmpty(NidProcForGetRevisitAgent)	Then
					Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
					lstR_T = Info8.GetRequest.GetRevisit_List(Guid.parse(NidProcForGetRevisitAgent))
					Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
					lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
					Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
					lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
					Dim UserG as String
					Dim NameUser as String
				'	if ucase(lstR_T(0).NidRevisitAgent.tostring)=ucase("7862F32C-A3E8-4C0A-95F5-4161A0671B35") Then
				'		Info8.GetOnTickChecklistParameter().Result="false"                       
				'	  	Info8.GetOnTickChecklistParameter().Description=" @@ - لطفا ابتدا زمان بازدید را انتخاب و اعلام مامور نمایید و سپس تایید نمایید " 
				'	Else		
						UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
						NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
						CreatTask("Activity_1atjcx5",UserG,NameUser)
				'	End if
				Else			
					SaveCheck("e7d55d2a-2986-404a-86e6-3f7eb0c8c03b",2,0)
				End if	
		Else	
		' گروه الف	
			Call User(11)
			CreatTask("Activity_0c0luv4",AssingTo,AssingToUserName) 	'مسئول ترافیک منطقه			
		End if	
	End if 'end 1=2 '  'sp 488436	
	
End if 'end sp 488436


IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e7d55d2a-2986-404a-86e6-3f7eb0c8c03b") Then

		Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
		lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
		Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
		lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
		Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
		lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
		Dim UserG as String
		Dim NameUser as String
		if ucase(lstR_T(0).NidRevisitAgent.tostring)=ucase("7862F32C-A3E8-4C0A-95F5-4161A0671B35") Then
			Info8.GetOnTickChecklistParameter().Result="false"                       
		  	Info8.GetOnTickChecklistParameter().Description=" لطفا ابتدا زمان بازدید را انتخاب و اعلام مامور نمایید و سپس تایید نمایید" 
		Else 
			UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
			NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
			'448608	CreatTask("Activity_0c0luv4",UserG,NameUser) 
			CreatTask("Activity_1atjcx5",UserG,NameUser)
		End if
'	End if	
End if

'sp 455029	
'	Call ExecSp("[SP_ShahrsaziParking]",NameUser) 'For Send Sms
'end sp 455029
 	
'End if		        	    

 'sp 409883
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c73725f2-e025-43fe-bc14-23549cf77454") Then
	
	AssingTo=""
	AssingToUserName=""
	
	ExecSp("[dbo].[SP_ParKing]","")
	
	CreatTask("Activity_06k22h0",AssingTo,AssingToUserName) 

	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
    Dim ss as Integer =0
   
	Dim TmpCountnew as Double
	TmpCountnew=Convert.ToDouble(TmpCount)*2000000
    For ss=0 to lstOwner.count-1
      Sms(lstOwner(ss).OwnerMobile,"هیئت پارکینگ","شهروند گرامی کارشناسی ملک شما با کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & _
      " توسط آقا/خانم " &  AssingToUserName.tostring & " به شماره همراه " & TmpMobile.tostring & " انجام می گردد مبلغ خدمات جهت بازدید برای هر نفر کارشناس  " & TmpCountnew.tostring  & " ریال می باشد .شهرداری مشهد ")
    Next
    
    Sms(Info8.GetRequest.Info.Cellphone,"هیئت پارکینگ","شهروند گرامی کارشناسی ملک شما با کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & _
      "توسط آقا/خانم " &  AssingToUserName.tostring & " به شماره همراه " & TmpMobile.tostring & " انجام می گردد مبلغ خدمات جهت بازدید برای هر نفر کارشناس  " & TmpCountnew.tostring  & "  ریال می باشد .شهرداری مشهد ")

'Naghibi:9:36:58 AM

	 Dim lstOwnernew As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
     Dim TellNew  as String
     TellNew=Info8.GetRequest.Info.Cellphone
     lstOwnernew= lstOwner.where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone And (f.OwnerMobile.ToString.Length = 11 Or f.LawyerMobile.ToString.Length = 11) ).tolist
     if lstOwnernew.count>0 Then TellNew=lstOwnernew(0).OwnerMobile 
     If TellNew.Length <>11 Then TellNew=lstOwnernew(0).LawyerMobile 
   'Naghibi:9:36:58 AM
		    'SP 414703 Sms(Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Phone,"مامور بازدید"," مامور بازدید محترم : پرونده با شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " با کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " مربوط به متقاضی " & Info8.GetRequest.Info.RequesterName.tostring  & " به شماره همراه " & Info8.GetRequest.Info.Cellphone & " و مالک/وکیل به شماره همراه " & TellNew & " جهت بازدید به تاریخ " & lstR_T(0).RevisitDate & " به کارتابل شما ارسال گردید - شهرداری مشهد")   
	 Sms(TmpMobile.tostring,"کارشناس پارکینگ "," مامور بازدید محترم : پرونده با شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " با کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " با شماره همراه  مالک/وکیل " & TellNew & " به کارتابل شما ارسال گردید - شهرداری مشهد")   

'درخواست بازدید ملک با کد نوسازی.... و شماره درخواست... به شما ارجاع گردید.نام مالک .... و شماره تماس.... جهت هماهنگی
End if 
 
 ' sp 403219 
 '-------------------------------------------
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("471200b5-564b-42e5-96ae-f687bdf91e51") Then
'sp 441273
'	if FinePenalty() And Analys() Then
		SaveCheck("e2672658-5b5e-4e6a-a444-bf9a5907d779",2,0)
'	Else
'	'sp 435119
'		Info8.GetOnTickChecklistParameter().Result="false" 
'		Info8.GetOnTickChecklistParameter().Description=  " لطفا اطلاعات تحلیل و جریمه را بررسی فرمایید " 
'	End if	
End if 	

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6fc3ddff-d479-41b4-99d0-367b0fe8e723") Then
'sp 441273	
'	if FinePenalty() And Analys() Then
		SaveCheck("49848249-3551-41aa-89fb-cf4a1a73d33c",2,0)
'	Else
'	'sp 435119
'		Info8.GetOnTickChecklistParameter().Result="false" 
'		Info8.GetOnTickChecklistParameter().Description=  " لطفا اطلاعات تحلیل و جریمه را بررسی فرمایید " 
'	End if	
End if 	

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4d21352a-886d-4682-9c45-21a603bc1dc2") Then
'sp 441273	
'	if FinePenalty() And Analys() Then
'	Else
'	'sp 435119
'		Info8.GetOnTickChecklistParameter().Result="false" 
'		Info8.GetOnTickChecklistParameter().Description=  " لطفا اطلاعات تحلیل و جریمه را بررسی فرمایید " 
'	End if	
End if 	
'-----------------------------------------------
'sp 395370
'sp 497580 IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4ece4a7f-9213-4a7a-ba0a-dd0a5b2067e7") Then
'sp 497580 	Call User(9)
'sp 497580 	CreatTask("Activity_1t2l38e",AssingTo,AssingToUserName) 
'sp 497580 	Call payamak("جرم گذاری"," به انجمن صنفی شهرسازی واقع در بلوار ارشاد پلاک 116 ارسال گردید. جهت تعیین مهندس شهرساز ظرف مدت 24 ساعت محل انجمن مراجعه نمایید ") 'sp464212
'sp 497580 End IF
'sp 497580 IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("47b66048-23a5-4ecf-9465-2a8da3e3de6e") Then
'sp 497580 	Call User(9)
'sp 497580 	CreatTask("Activity_18r6tzn",AssingTo,AssingToUserName) 
'sp 497580 	Call payamak("جرم گذاری"," به انجمن صنفی شهرسازی واقع در بلوار ارشاد پلاک 116 ارسال گردید. جهت تعیین مهندس شهرساز ظرف مدت 24 ساعت محل انجمن مراجعه نمایید ") 'sp464212

'sp 497580 End IF

'sp 496015 IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fbc4601a-e629-40cb-9647-6555562ac077") Then
'sp 496015	'sp 398539 Call User(4)
'sp 496015	Call User(2)
'sp 496015	CreatTask("Activity_1es02uy",AssingTo,AssingToUserName)
	
	 'sp 407575
'sp 496015	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
'sp 496015    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
'sp 496015    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
'sp 496015    Dim ss as Integer =0
   
'sp 496015    For ss=0 to lstOwner.count-1
'sp 496015      Sms(lstOwner(ss).OwnerMobile,"رگ اول بنا"," مالك محترم : پرونده شماره " + Info8.GetRequest.Info.nidworkitem.tostring() +" جهت تحويل رگ اول بنا(بروكف)به شركت نقشه برداري" _
'sp 496015       + AssingToUserName + " ارسال گرديد. درصورت عدم پيگيري ازشركت نقشه برداري طي 72 ساعت آينده پرونده با عنوان انصراف ازتحويل رگ اول بنا (بدون اعلام نظر) ارسال مي شود. شهرداری مشهد ")
'sp 496015    Next
     
'sp 496015    Sms(Info8.GetRequest.Info.Cellphone,"رگ اول بنا"," مالك محترم :پرونده شماره " + Info8.GetRequest.Info.nidworkitem.tostring() +" جهت تحويل رگ اول بنا(بروكف)به شركت نقشه برداري" _
'sp 496015       + AssingToUserName + " ارسال گرديد. درصورت عدم پيگيري ازشركت نقشه برداري طي 72 ساعت آينده پرونده با عنوان انصراف ازتحويل رگ اول بنا (بدون اعلام نظر) ارسال مي شود. شهرداری مشهد ")

'sp 496015 End IF 
'sp 496015 IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("114d638e-e8b3-4603-9c58-8d3c585ef2b3") Then
'sp 496015	Call User(2)
'sp 496015	CreatTask("Activity_1es02uy",AssingTo,AssingToUserName) 
'sp 496015 End IF 
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b98c714f-8689-4a83-b835-19e8ad1a9040") Then

	Call HadNegarWebService()
	Call User(8)
	CreatTask("Activity_09omebd",AssingTo,AssingToUserName) 		'حد نگار
End IF 

if (ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3dd38365-0f7b-40a0-8b4d-230f91e8094d") _
or  ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("39504dea-f6f8-40c2-805d-25c0da57a558")) And String.IsNullOrEmpty(Info8.GetRequest.Info.Comment)=True   Then 
	Info8.GetOnTickChecklistParameter().Result="false" 
	Info8.GetOnTickChecklistParameter().Description=  " لطفا توضیحات درخواست متقاضی ثبت گردد " 
End if 

 '''''''''کنترل تقدم تاخر محاسبات
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3dd38365-0f7b-40a0-8b4d-230f91e8094d") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("39504dea-f6f8-40c2-805d-25c0da57a558") Then 
	 Try
		Dim MaxIncomedate , TahlilDate as String 
		Dim MaxIncomeTime , TahlilTime as String 
		Dim Control=  Analys()
		
	Dim CountA as Integer = 0
	CountA=	Agreement()
		
		if CountA =-2 Then
			Info8.GetOnTickChecklistParameter().Result="false" 
			Info8.GetOnTickChecklistParameter().Description=  " تایید اقدام در شرایط تکمیل اطلاعات فرم چیدمان امکان پذیر است " 
		End if 
		Dim ListIncome As New List(Of DtoSC.Income)
        ListIncome = Info8.GetIncome.GetIncomeList(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc.tostring))
        If ListIncome.Count > 0 Then 
        	MaxIncomedate = IIf(IsDBNull(ListIncome.Max(Function(f) f.CreateDate)), "1300/00/00", ListIncome.Max(Function(f) f.CreateDate))
           	ListIncome=ListIncome.where(Function(f) f.CreateDate=MaxIncomedate).tolist
            MaxIncomeTime = IIf(IsDBNull(ListIncome.Max(Function(f) f.CreateTime)), "00:00:00", ListIncome.Max(Function(f) f.CreateTime))
        End if     
        If AnaliysParvaneh_Date > MaxIncomedate Then Info8.GetOnTickChecklistParameter().Result="false" : Info8.GetOnTickChecklistParameter().Description=  " با توجه به تغییر در تحلیل محاسبات نسبت به محاسبات  درامد اقدام نمایید" 
        If AnaliysParvaneh_Date = MaxIncomedate And AnaliysParvaneh_time>MaxIncomeTime Then Info8.GetOnTickChecklistParameter().Result="false" : Info8.GetOnTickChecklistParameter().Description=  " با توجه به تغییر در تحلیل محاسبات نسبت به محاسبات  درامد اقدام نمایید" 
        
        If Agreement_Date > MaxIncomedate Then Info8.GetOnTickChecklistParameter().Result="false" : Info8.GetOnTickChecklistParameter().Description=  " با توجه به تغییر در چیدمان نسبت به محاسبات  درامد اقدام نمایید" 
        If Agreement_Date = MaxIncomedate And Agreement_time>MaxIncomeTime Then Info8.GetOnTickChecklistParameter().Result="false" : Info8.GetOnTickChecklistParameter().Description=  " با توجه به تغییر در چیدمان نسبت به محاسبات درامد اقدام نمایید" 
        
        If Agreement_Date > AnaliysParvaneh_Date Then Info8.GetOnTickChecklistParameter().Result="false" : Info8.GetOnTickChecklistParameter().Description=  " با توجه به تغییر در چیدمان نسبت به محاسبات تحلیل اقدام نمایید" 
        If Agreement_Date = AnaliysParvaneh_Date And Agreement_time>AnaliysParvaneh_time Then Info8.GetOnTickChecklistParameter().Result="false" : Info8.GetOnTickChecklistParameter().Description=  " با توجه به تغییر در چیدمان نسبت به محاسبات تحلیل اقدام نمایید" 
      Catch
      		Info8.GetOnTickChecklistParameter().Result="false" 
			Info8.GetOnTickChecklistParameter().Description=  " تایید اقدام در شرایط تکمیل اطلاعاتو رعایت تقدم و تاخر دیتا امکان پذیر است " 

      End Try
      	
	End if
	
	
''----------------------------------------------------------------------------------------	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f892bb7d-5901-4130-aaff-7a7b9abc2a8a") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("91a99c6c-4ceb-4519-9c83-f60f9c5f9f8f") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f3500694-2d52-4312-b42e-e96e9d23e61b") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("61f13d00-9ab1-47e6-b7b9-47abcec995ac") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c5d09119-8e46-4b57-b425-73070cc9f252") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("47d786eb-ddc9-492c-853e-8decf41b0246") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("534a18ba-f262-4601-8c7f-a44eb0d6c8df") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("91a99c6c-4ceb-4519-9c83-f60f9c5f9f8f") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b8cef52b-1385-440c-bcb2-a4c66cf03ac8") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1b1c03b9-3f8c-475b-8663-f63334fb5941") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("66caf2db-d017-428d-bd74-b9169a48a1ea") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("aba2cfeb-d00e-46bd-968e-b4e284b90681") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2ede6cd8-404f-4cbc-bdb2-423343ccb8bc") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6be7122d-e39a-4b10-b15d-119fabc006d5") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2331a630-bfc2-4c75-83e5-e0ad083adfd8") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("a1322d6a-1cb4-4f77-b5a3-3fbbe4677ff7") _		
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("28d21878-dd31-467e-8a9b-2476e450d4a3") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("00b19551-993e-4483-b6c1-369941060023") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f46fde1d-b87a-413e-bca5-ff234d0a0a35") _
		or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d8aa0716-3410-48a1-92d1-3d88bbdfe737") _
		Then
		
		
			
		
			Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
		    lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
		  
		    Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
		   
       	    lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
       	    Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
       	    lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
		    Dim UserG as String
		    Dim NameUser as String
		    
		    if ucase(lstR_T(0).NidRevisitAgent.tostring)=ucase("7862F32C-A3E8-4C0A-95F5-4161A0671B35") Then
		    	
		    	 if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f892bb7d-5901-4130-aaff-7a7b9abc2a8a") _
					or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("91a99c6c-4ceb-4519-9c83-f60f9c5f9f8f") _
					or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f3500694-2d52-4312-b42e-e96e9d23e61b") _
					or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("61f13d00-9ab1-47e6-b7b9-47abcec995ac") _
					or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c5d09119-8e46-4b57-b425-73070cc9f252") _
					or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1b1c03b9-3f8c-475b-8663-f63334fb5941") _
					or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("66caf2db-d017-428d-bd74-b9169a48a1ea") _
					or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f46fde1d-b87a-413e-bca5-ff234d0a0a35") _
					or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d8aa0716-3410-48a1-92d1-3d88bbdfe737") Then    'sp 473490
		    	 		Info8.GetOnTickChecklistParameter().Result="false"                       
	  			 		Info8.GetOnTickChecklistParameter().Description=" لطفا ابتدا زمان بازدید را انتخاب و اعلام مامور نمایید و سپس تایید نمایید" 
	  			 	End if 	
		    Else 
		 
		    	UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
		   
		    	NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
		 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f892bb7d-5901-4130-aaff-7a7b9abc2a8a") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("91a99c6c-4ceb-4519-9c83-f60f9c5f9f8f") Then  CreatTask("Activity_18th9bt",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f3500694-2d52-4312-b42e-e96e9d23e61b") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b8cef52b-1385-440c-bcb2-a4c66cf03ac8") Then  CreatTask("Activity_0mfimf6",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("61f13d00-9ab1-47e6-b7b9-47abcec995ac") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("47d786eb-ddc9-492c-853e-8decf41b0246") Then  CreatTask("Activity_1jz9ix6",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c5d09119-8e46-4b57-b425-73070cc9f252") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("534a18ba-f262-4601-8c7f-a44eb0d6c8df") Then  CreatTask("Activity_1e7bq44",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1b1c03b9-3f8c-475b-8663-f63334fb5941") Then  CreatTask("Activity_0ygetjc",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("66caf2db-d017-428d-bd74-b9169a48a1ea") Then  CreatTask("Activity_056rjh9",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("aba2cfeb-d00e-46bd-968e-b4e284b90681") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2ede6cd8-404f-4cbc-bdb2-423343ccb8bc")Then  CreatTask("Activity_0vcpof4",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("28d21878-dd31-467e-8a9b-2476e450d4a3") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("00b19551-993e-4483-b6c1-369941060023")Then  CreatTask("Activity_1jicsxf",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("a1322d6a-1cb4-4f77-b5a3-3fbbe4677ff7") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2331a630-bfc2-4c75-83e5-e0ad083adfd8")Then  CreatTask("Activity_13z3e1z",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6be7122d-e39a-4b10-b15d-119fabc006d5") Then  CreatTask("Activity_0302lcv",UserG,NameUser) 
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f46fde1d-b87a-413e-bca5-ff234d0a0a35") Then  CreatTask("Activity_1vk0n5v",UserG,NameUser) 
		       ' if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e7d55d2a-2986-404a-86e6-3f7eb0c8c03b") Then  CreatTask("Activity_1atjcx5",UserG,NameUser)  'sp 473490
		        if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d8aa0716-3410-48a1-92d1-3d88bbdfe737") Then  CreatTask("Activity_1d3nq81",UserG,NameUser)
		       
		       if Info8.GetRequest.Info.CI_Requester = 6 Then
		       Else 
			        Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
	        		lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
	        		Dim   Cellphone as String ="0"
	        		Cellphone= Info8.GetRequest.Info.Cellphone 
	        		 if Cellphone.Length<>11 Then Cellphone="0"
	        		if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Cellphone).tolist
	        		Dim ss as Integer =0
	        		For ss=0 to lstOwner.count-1
	        			if lstOwner(ss).OwnerMobile.Length=11 Then Sms(lstOwner(ss).OwnerMobile,"مامور اعلام"," شهروند گرامی : کارشناسی ملک شما با کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " توسط آقا/خانم " & NameUser & " در تاریخ " & lstR_T(0).RevisitDate & " انجام و نتیجه حداکثر تا یک روز کاری بعد از بازدید اعلام خواهد شد شماره همراه مامور بازدید جهت هماهنگی " & Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Phone.tostring & " حضورمالک /مالکین به همراه اصل مدارک هویتی و مالکیتی در زمان مراجعه کارشناس بازدید در محل ملک الزامی می باشد  - شهرداری مشهد") 
	        		Next
	        	 
	        	
	        	 
			   		 if Cellphone.Length=11 Then Sms(Cellphone,"مامور اعلام"," شهروند گرامی : کارشناسی ملک شما با کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " توسط آقا/خانم " & NameUser & " در تاریخ " & lstR_T(0).RevisitDate & " انجام و نتیجه حداکثر تا یک روز کاری بعد از بازدید اعلام خواهد شد شماره همراه مامور بازدید جهت هماهنگی " & Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Phone.tostring & " حضورمالک /مالکین به همراه اصل مدارک هویتی و مالکیتی در زمان مراجعه کارشناس بازدید در محل ملک الزامی می باشد - شهرداری مشهد") 
			   	    
			   	    Dim lstOwnernew As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
		        	Dim TellNew  as String
		        	TellNew=Info8.GetRequest.Info.Cellphone
		        	
		        	lstOwnernew= lstOwner.where(Function(f) f.OwnerMobile<> Cellphone).tolist
		        	if lstOwnernew.count>0 Then TellNew=lstOwnernew(0).OwnerMobile 
				   	If TellNew.Length <> 11 Then TellNew=lstOwnernew(0).LawyerMobile()    
			   	   '414703 Sms(Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Phone,"مامور بازدید"," مامور بازدید محترم : پرونده با شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " با کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " مربوط به متقاضی " & Info8.GetRequest.Info.RequesterName.tostring  & " به شماره همراه " & Info8.GetRequest.Info.Cellphone & " و مالک/ وکیل به شماره همراه " & TellNew & " جهت بازدید به تاریخ " & lstR_T(0).RevisitDate & " به کارتابل شما ارسال گردید - شهرداری مشهد")   
			   	   'sp 431126 Sms(Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Phone,"مامور بازدید"," مامور بازدید محترم : پرونده با شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " با کد نوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & "با شماره همراه  مالک/ وکیل  " & TellNew & " جهت بازدید به تاریخ " & lstR_T(0).RevisitDate & " به کارتابل شما ارسال گردید - شهرداری مشهد")   
			       		       
			       
			     	Dim TCode() as String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring.Split("-")
			    	 Dim StrC as String= TCode(0) + "-" +TCode(1) + "-"+TCode(2) + "-"+TCode(3) + "-"+TCode(4) + "-"+TCode(5) + "-"+TCode(6) 
			     	Dim StrTypeR as String = "(نماینده فنی)"
			     	if Info8.GetRequest.Info.CI_Requester=13 Then  StrTypeR  = "(شهروند سپاری)"
			     			     			     	
			     	Sms(Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Phone,"کارشناس بازدید"," کارشناس بازدید محترم : پرونده با شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " و کد نوسازی " & StrC.tostring & " با شماره همراه  مالک/وکیل " & TellNew & " جهت بازدید به تاریخ " & lstR_T(0).RevisitDate & " به کارتابل شما ارسال گردید  " & StrTypeR & " - شهرداری مشهد")    
	
			        PriceSms("[dbo].[SP_BazdidSara10]") 
		   		End if   ' end of CI_Requester = 6
		    End if 
		    
		    
	End if

' بانک	
		
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6e2a2764-f1d1-441d-8baf-507314a25c9a") Then 
	
		if ControlTask("Activity_0mfimf6")=1 or ControlTask("Activity_0x0ylrz")=1 or ControlTask("Activity_1tqlk2x")=1 Then 
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه کارشناسان شهرداری بمانيد"
  			 
  		Else 
  			if Not CheckMalek("6e2a2764-f1d1-441d-8baf-507314a25c9a") Then
  				Info8.GetOnTickChecklistParameter().Result="false"                       
  				Info8.GetOnTickChecklistParameter().Description="اقدام انجام نشده دارید" 
  			Else
  				
  				SaveCheck("2629701c-1e97-4004-abef-19db6104a64d",2,0) 
  				SaveCheck("41eb5f18-2f38-4076-8645-415070d1c55e",0,1) 	'sp 492647
  			End if	
    	End if 
    '	ادارات
    ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("78e4bea2-29a5-4df0-8fee-42cf2e1b0ae7") Then 
		
		if ControlTask("Activity_0fx4od8")=1 or ControlTask("Activity_1jz9ix6")=1 or ControlTask("Activity_1xqicbz")=1 Then 
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه کارشناسان شهرداری بمانيد" 
  		Else 
  			if Not CheckMalek("78e4bea2-29a5-4df0-8fee-42cf2e1b0ae7") Then
  				Info8.GetOnTickChecklistParameter().Result="false"                       
  				Info8.GetOnTickChecklistParameter().Description="اقدام انجام نشده دارید" 
  			Else
  				
  				SaveCheck("7bc19dca-7ce6-40be-88ce-7ab562076ea4",2,0)
  				SaveCheck("78e4bea2-29a5-4df0-8fee-42cf2e1b0ae7",0,1)	'sp 492647
  			End if		
    	End if 
  	'	اجاره
    ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("19b19edb-d9cf-452e-a6b1-d5c4f1a8fe0a") Then 
    
    'sp 482936  بر اساس هامش که گفته شده همانند محضر هست
	Call FunBlackListNew(0)	
	
	 Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	 TmpListReq_N= Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
	 if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 And f.RequestDate>"1397/10/01" ).tolist ''ابطال نباشد 	 
	 Dim jj as Integer
	 'sp 367183
     For jj =0 to TmpListReq_N.count-1 
		if Taghsid(TmpListReq_N(jj).nidproc,1)=2 Then
			if TaeedTaghsit()=1 Then 
				Info8.GetOnTickChecklistParameter().Result="false"                       
	  			Info8.GetOnTickChecklistParameter().Description= "این ملک دارای چک سررسید نشده میباشد."
	  			Exit Function
	  		End if	
	    End if	
    Next    'end 482936    
    
		if ControlTask("Activity_12ebsu9")=1 or ControlTask("Activity_143vz32")=1 or ControlTask("Activity_1e7bq44")=1 Then 
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه کارشناسان شهرداری بمانيد" 
  		Else
  			if Not CheckMalek("19b19edb-d9cf-452e-a6b1-d5c4f1a8fe0a") Then
  				Info8.GetOnTickChecklistParameter().Result="false"                       
  				Info8.GetOnTickChecklistParameter().Description="اقدام انجام نشده دارید" 
  			Else 
  			
  				SaveCheck("d38f62f4-2a58-4080-b9e6-9f64d239568e",2,0) 
  				SaveCheck("19b19edb-d9cf-452e-a6b1-d5c4f1a8fe0a",0,1) 		'sp 492647
  			End if			
    	End if 
  	'	محضر
    ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ec7410d1-eaad-41c0-b8d4-328cf119d229") Then 
	

'sp 524255
		Dim CountFiche as Integer 
		Dim AllFiche as  list(of Dtosc.Income_Fiche)
		AllFiche=Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
		AllFiche=AllFiche.where(Function (f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) ).tolist '3 Taeed, 5 TaeedBank
		CountFiche= AllFiche.count
			if CountFiche<=0 Then 
				Info8.GetOnTickChecklistParameter().Result="false" 
			 	Info8.GetOnTickChecklistParameter().Description=" فیش تایید شده درآمدی ندارین - sp 524255" 							
		 		Exit Function
		End if 	
'end sp 524255	

	
	Call FunBlackListNew(0)	
	
	 Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	 TmpListReq_N= Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
	 if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 And f.RequestDate>"1397/10/01" ).tolist ''ابطال نباشد 	 
	 Dim jj as Integer
	 'sp 367183
     For jj =0 to TmpListReq_N.count-1 
		if Taghsid(TmpListReq_N(jj).nidproc,1)=2 Then
			if TaeedTaghsit()=1 Then 
				Info8.GetOnTickChecklistParameter().Result="false"                       
	  			Info8.GetOnTickChecklistParameter().Description= "این ملک دارای چک سررسید نشده میباشد."
	  			Exit Function
	  		End if	
	    End if	
    Next   
	 
		 
		if ControlTask("Activity_18th9bt")=1 or ControlTask("Activity_1alwiso")=1 or ControlTask("Activity_1ipj02l")=1 Then 
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه کارشناسان شهرداری بمانيد" 
  		Else
  			
  			if Not CheckMalek("ec7410d1-eaad-41c0-b8d4-328cf119d229") Then
  				Info8.GetOnTickChecklistParameter().Result="false"                       
  				Info8.GetOnTickChecklistParameter().Description=Info8.GetOnTickChecklistParameter().Description + "اقدام انجام نشده دارید" 
  			Else 
  				
  				SaveCheck("1d6d8544-78ef-460e-bf8c-3343b67451e6",2,0)
  				SaveCheck("ec7410d1-eaad-41c0-b8d4-328cf119d229",0,1)	'sp 492647
  				
  			End if 		
    	End if 	
	End if

		
	
'''''''''''''''''بدهی نوسازی 	
Dim BedehiNosazi As New Global.Biz.SB.ClsDutyCalculate(1)

'کنترل بدهی نوسازی برای درخواست استعلا
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1d6d8544-78ef-460e-bf8c-3343b67451e6") _
or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2629701c-1e97-4004-abef-19db6104a64d") _
or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7bc19dca-7ce6-40be-88ce-7ab562076ea4") _
or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d38f62f4-2a58-4080-b9e6-9f64d239568e") Then

	if Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0,0,Guid.Parse ("00000000-0000-0000-0000-000000000000")).EumNosaziCodeObjType =3 Then
	
		Dim TmpParent As New List(Of Global.Biz.SA.ClsNosaziCodeType)
		TmpParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
		TmpParent=TmpParent.where(Function (f) f.EumNosaziCodeObjType=2).tolist	
		
		If BedehiNosazi.HasDept(TmpParent(0).NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode) Then
			Info8.GetOnTickChecklistParameter().Result="false"                       
			Info8.GetOnTickChecklistParameter().Description= " **داراي بدهي نوسازي مي باشيد لطفا پس ار پرداخت تایید نمایید" 
		End if	
	Else 
		If BedehiNosazi.HasDept(Info8.GetRequest.Info.NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode) Then
			Info8.GetOnTickChecklistParameter().Result="false"                       
			Info8.GetOnTickChecklistParameter().Description= "@داراي بدهي نوسازي مي باشيد لطفا پس ار پرداخت تایید نمایید" 
		End if	
	End if				
	
	
End if
'
'			
 '  	End if
	''==================================================================
	'کنترل بدهی نوسازی برای درخواست پروانه , payankar
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e1d8db61-5f60-416c-b572-087f2eb13c8e") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0684a439-2bae-4da3-bdab-bec3153268be") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2dcb5136-7b64-4721-8d23-9dffb9a1da9e") Then 
		Dim TmpParentApar As New List(Of Global.Biz.SA.ClsNosaziCodeType)
		Dim TmpParent As New List(Of Global.Biz.SA.ClsNosaziCodeType)
		TmpParent=Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
		TmpParentApar=TmpParent.where(Function (f) f.EumNosaziCodeObjType=4 or f.EumNosaziCodeObjType=5 ).tolist
		
		if TmpParentApar.count>0 Then
			Dim Si as Integer=0
			For Si=0 to TmpParentApar.count -1 
		
				If BedehiNosazi.HasDept(TmpParentApar(Si).NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode) Then
					Info8.GetOnTickChecklistParameter().Result="false"                       
					Info8.GetOnTickChecklistParameter().Description= "دارای بدهی نوسازی بر روی یک یا چند تا از اپارتمان ها می باشد" 
				End if	 
			Next
		Else
			TmpParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
			TmpParent=TmpParent.where(Function (f) f.EumNosaziCodeObjType=2).tolist
			
			If BedehiNosazi.HasDept(TmpParent(0).NidNosaziCode, Global.Biz.SA.ClsEnum.EumDutyType.Renovation, 1395, Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"), "Esup", False, Info8.GetFunctions.Security_RequestGuid, Info8.GetFunctions.Security_EncryptCode) Then
				Info8.GetOnTickChecklistParameter().Result="false"                       
				Info8.GetOnTickChecklistParameter().Description= "#داراي بدهي نوسازي مي باشيد لطفا پس ار پرداخت تایید نمایید" 
			End if			
		End if 
	
	End if 
	'کنترل بدهی صنفی  برای درخواست پروانه
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ead40660-7add-4438-9bba-3d4c7147d600") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("93404a0d-d789-486b-a61a-41a9f6e80eff") Then 
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
		            	  	Info8.GetOnTickChecklistParameter().Result="false"                       
							Info8.GetOnTickChecklistParameter().Description="داراي بدهي صنفی مي باشيد لطفا پس ار پرداخت تایید نمایید" 
		            	End if
		             End if	  
	              End if	  	 
			    Next
			Next
		End if		
	End if			
	
''***********************************
' متقاضی پرداختی اول پروانه توسعه 


'sp 515495  - sp 515419
if BIZ.SA.ClsCommon.CurrentShamsiYear = "1404" Then
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0d9d7760-cc20-4f14-a9d9-1f81b4b7010e") Then
			AssingTo=""
			AssingToUserName=""
			Call User(5)
			if ControlTask("Activity_19x10kg")=0 or ControlTask("Activity_19x10kg")=2  Then CreatTask("Activity_19x10kg",AssingTo,AssingToUserName)  
	End if
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e0b65449-a4ba-49db-80bf-ab42eebc2b6c") Then
			AssingTo=""
			AssingToUserName=""
			Call User(5)
			if ControlTask("Activity_0joeeta")=0 or ControlTask("Activity_0joeeta")=2  Then CreatTask("Activity_0joeeta",AssingTo,AssingToUserName)  	 	
	End if
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1d4cb98d-d5bb-45d6-9e4f-0a81d6f5f57c") Then
			AssingTo=""
			AssingToUserName=""
			Call User(5)
			if ControlTask("Activity_1s65rhl")=0 or ControlTask("Activity_1s65rhl")=2  Then CreatTask("Activity_1s65rhl",AssingTo,AssingToUserName)  
	End if
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d7406af8-84c1-4b8d-b15f-4ac2bd9a134d") Then
			AssingTo=""
			AssingToUserName=""
			Call User(5)
			if ControlTask("Activity_0ep8zws")=0 or ControlTask("Activity_0ep8zws")=2  Then CreatTask("Activity_0ep8zws",AssingTo,AssingToUserName)  	
	End if
End if   'BIZ.SA.ClsCommon.CurrentShamsiYear = 1404   -  sp 515495  - sp 515419


' متقاضی پرداختی اول پروانه احداث 

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("799f97bf-7531-4a00-b1e8-7631a09cf7f3") Then 
'sp 440272
'	if Info8.GetOnTickChecklistParameter.Comment.Length <10 Then 
'sp 422312
'sp 440272	
	'	Info8.GetOnTickChecklistParameter().Result="false"                       
'sp 422312	
	'	Info8.GetOnTickChecklistParameter().Description="در حال حاضر امکان استفاده از پرداختی اول وجود ندارد" 
'	Else 
			AssingTo=""
			AssingToUserName=""
			Call User(5)
			if ControlTask("Activity_1m37txg")=0 or ControlTask("Activity_1m37txg")=2  Then CreatTask("Activity_1m37txg",AssingTo,AssingToUserName)
'	End if 			  
End if
' نود بروکف شروع عملیات ساختمانی
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("114d638e-e8b3-4603-9c58-8d3c585ef2b3") Then 
'sp 422312	
	Info8.GetOnTickChecklistParameter().Result="false"                       
'sp 422312	
	Info8.GetOnTickChecklistParameter().Description="در حال حاضر امکان استفاده از پرداختی اول وجود ندارد" 

'sp 422312			AssingTo=""
'sp 422312			AssingToUserName=""
'sp 422312			Call User(2)
'sp 422312			if ControlTask("Activity_1es02uy")=0 or ControlTask("Activity_1es02uy")=2  Then CreatTask("Activity_1es02uy",AssingTo,AssingToUserName)
		  
End if
''***********************************
' تایید پیش نویس پایانکار ایجاد نود شهرسازی
'sp 414131
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9e013712-d86a-49ae-99d2-15d11da63ad3") Then
   Dim TaskNew As Global.Biz.SC.ClsNewWorkFlow.ClsGetTaskByNidProcResponse
   TaskNew = Info8.GetFunctions.WF_GetTaskByNidProc(Info8.GetRequest.Info.NidProc.tostring)
   Dim LstTas As New List(Of Global.Biz.SC.ClsNewWorkFlow.data1)
   LstTas = TaskNew.data
   LstTas = LstTas.Where(Function(f) f.EumTaskStatus = 0 And f.NidNode <> "Activity_0zhqjkf" And f.NidNode <> "Activity_0ggtuxu").ToList ()
   if LstTas.Count>0 Then 
  		Info8.GetOnTickChecklistParameter().Result="false" 
   		Info8.GetOnTickChecklistParameter().Description=" اجازه تایید ندارید پرونده منتظر پاسخگویی کارشناسان شهرداری می باشد" 							
   		Exit Function
   End if 		
   SaveCheck("1dd4db82-030d-4cb5-9b5d-e1f148eea040",2,0)
End if      

		'تایید مالک 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b83c895e-79ea-4960-a71e-d335b14bb629") Then 		'آخرین وضعیت مجاز ساختمان
	SaveCheck("b98c714f-8689-4a83-b835-19e8ad1a9040",2,0)		'درخواست بازدید کارشناس اداره ثبت (حدنگار)
	if MapBarokaf()="0" Then 
   	Else
   		User(2) 
		CreatTask("Activity_1brcze0"  ,AssingTo,  AssingToUserName)	'نقشه برداری
		ExecSp("[dbo].[SP_BarokafSara10]","") 
   	End if 
End if 
 'Info8.GetOnTickChecklistParameter().Result="false"                       
  			''Info8.GetOnTickChecklistParameter().Description=ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)
  			'Exit Function 	
'' صورت مجلس تفکیک اپارتمان
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1a77761d-5480-4c0f-ab7d-e4648637388d") Then ' ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) = Ucase("25AE9AB1-EF73-403A-B07A-15FCC515782B") or 
   
    Dim NidPayankar As Guid
	Dim UnitCount as Integer=0
	Dim tmpNidRNew As Guid
	Dim M_BaseUsing_Payankar as New  List(of DtoSc.Base_Using)
	NidPayankar = Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, BIZ.SA.ClsEnum.EumBaseInfoGroup.MojazPayankar, BIZ.SA.ClsEnum.EumRevisitGroup.None,tmpNidRNew)
	Info8.ParameterList.Remove("MojazPayankar_Building_NidBase")
	Info8.ParameterList.Add("MojazPayankar_Building_NidBase", NidPayankar.ToString)
	M_BaseUsing_Payankar=Info8.GetMojazPayankar_Building.GetBase_Using
 
	              
		
	If M_BaseUsing_Payankar.Count =0 Then
	Else
		M_BaseUsing_Payankar=M_BaseUsing_Payankar.where(Function(f) f.CI_UsingType=402 or f.CI_UsingType=401 or f.CI_UsingType=411 or f.CI_UsingType=412).tolist
		if M_BaseUsing_Payankar.count>0 Then UnitCount=M_BaseUsing_Payankar.sum(Function(f) f.UnitCount)
		
		
		if UnitCount>0 Then 
			Dim LstChild As New List(Of Global.Biz.SA.ClsNosaziCodeType)
			Dim LstParent As New List(Of Global.Biz.SA.ClsNosaziCodeType)
			Dim Parent as New Global.Biz.SA.ClsNosaziCodeType
		'	Dim Child  as New Global.Biz.SA.ClsNosaziCodeType
			Dim CodeNew as New Global.Biz.SA.ClsNosaziCodeType
        	LstChild = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
        	LstParent= LstChild.Where(Function(f) f.EumNosaziCodeObjType = 3).ToList
        	LstChild = LstChild.Where(Function(f) f.EumNosaziCodeObjType = 4).ToList
        	
        	Parent=LstParent(0)
        
        	
        	if LstChild.count<UnitCount Then
        		
        		Dim ApartemanNo as Integer=0
        		Dim ii as Integer
        
        		For ii =1 to UnitCount-LstChild.count
        			ApartemanNo=ApartemanNo+1
        			Dim TmpCodeNew As New Global.Biz.SA.ClsNosaziCodeType
        			Dim Child  as New Global.Biz.SA.ClsNosaziCodeType
	        	'	if LstChild.count >0 Then 
		        '		Child=LstChild(0)
		        '	Else
		        '		Child=LstParent(0)	
		        '    End if 
        			Dim jj as Integer
        			Do While APNumber(LstChild,ApartemanNo)
        				ApartemanNo=ApartemanNo+1
        			Loop
        			
        			Child.District=Parent.District
				    Child.Region=Parent.Region
				    Child.Block=Parent.Block
				    Child.House=Parent.House
				    Child.Building=Parent.Building
				   ' Child.Apartment
               
				    Child.CI_City=Parent.CI_City
				    Child.EumNosaziCodeObjType=4
		            Child.Apartment =ApartemanNo
		             child.Shop = 0 
		            	
		
        		
        			'Dim rrrr As New Global.Biz.SA.ClsErrorResult
        		'	Try
        		
           			TmpCodeNew=Info8.GetFunctions.CreateNewNosaziCode(Parent,Child,True)
           			'Catch ex As Exception
           			 Dim ErrorR As Global.Biz.SA.ClsErrorResult
        ErrorR = Info8.GetFunctions.ErrorResult()
        
           		'		Info8.GetOnTickChecklistParameter().Result="false"                       
		'Info8.GetOnTickChecklistParameter().Description=ErrorR.BizErrors.count  & ": " & ApartemanNo & " : "  & 	ErrorR.BizErrors(0).ErrorTitel.tostring
	'	Exit Function
       ' End Try
        		'	rrrr=Info8.ErrorResult 	
        	'	if rrrr.BizErrors.Count>0 Then 
        	'			Info8.GetOnTickChecklistParameter().Result="false"                       
	'	Info8.GetOnTickChecklistParameter().Description="D"
	'	Exit Function
	'	End if 
        		Next
        		
        	End if 
        
        End if	
        
	End if
	SaveCheck("25ae9ab1-ef73-403a-b07a-15fcc515782b",2,0)	
					  
End if
End Function   


Public Function UnConfirmOwner()  ' عدم تایید مالک

Call DontCloseCheckList(Info8.GetOnTickChecklistParameter().NidChecklistValue)	' sp 520182


IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bb4dadfa-e1da-4f28-9eb2-120373793f9d") Then
		AssingTo =""
	 	AssingToUserName=""
	 	Call user(1)
	 	CreatTask("Activity_06q8adj",AssingTo,AssingToUserName)	'نود  ناظر 
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8c36de6f-9b7c-4a4f-bfac-87131878c063") Then
		AssingTo =""
	 	AssingToUserName=""
	 	Call user(15)
	 	CreatTask("Activity_140scbs",AssingTo,AssingToUserName)	'نود  درآمد
End if



IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e7d55d2a-2986-404a-86e6-3f7eb0c8c03b") Then

'		Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
'		lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
'		Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
'		lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
'		Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
'		lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
'		Dim UserG as String
'		Dim NameUser as String
'		if ucase(lstR_T(0).NidRevisitAgent.tostring)=ucase("7862F32C-A3E8-4C0A-95F5-4161A0671B35") Then
'			Info8.GetOnTickChecklistParameter().Result="false"                       
'		  	Info8.GetOnTickChecklistParameter().Description=" لطفا ابتدا زمان بازدید را انتخاب و اعلام مامور نمایید و سپس تایید نمایید" 
'		Else 
'			UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
'			NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
			'448608	CreatTask("Activity_0c0luv4",UserG,NameUser) '
'			CreatTask("Activity_1atjcx5",UserG,NameUser)
'		End if
'	End if	
End if


'  sp  483615  ============   عدم تایید مالک

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7c443cc5-cb27-4c19-833d-defe46e54b23") Then 'اختلاف ناظر و مجری
'	if GetBuildingGroup() Then 'true: ساختمان گروه الف و بازدید چک نشود ارسال مرحله 

if 1 = 2 Then ' sp 503426
	if NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c") = 4 Then  'اگر ساختمان گروه الف نبود 
	 
	 '	if ControlTask("Activity_1atjcx5")=2 Then
			 AssingTo =""
			 AssingToUserName=""	
			 Call User(11)
			CreatTask("Activity_0c0luv4",AssingTo,AssingToUserName)	    
	'	Else
		'	msg("ابتدا مرحله بازدید تکمیل گردد")
		'	Exit Function    
	'	End if			
	 Else
				
		AssingTo =""
		AssingToUserName=""	
		Call User(11)
		CreatTask("Activity_0c0luv4",AssingTo,AssingToUserName)
	    	 	
	 End if  
End if '1=2	 	    	    
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6a95e162-2d02-47dc-ae23-19ab16324c11") Then 

	if GetBuildingGroup() Then 'true: ساختمان گروه الف و بازدید چک نشود ارسال مرحله 
		 AssingTo =""
		 AssingToUserName=""	
		 Call User(11)
		CreatTask("Activity_1197lv3",AssingTo,AssingToUserName)	    
	 Else
	 
	 	if ControlTask("Activity_1d3nq81")=2 Then
	 	
			 AssingTo =""
			 AssingToUserName=""	
			 Call User(11)
			CreatTask("Activity_1197lv3",AssingTo,AssingToUserName)
		Else
			msg("ابتدا مرحله بازدید تکمیل گردد")
			Exit Function    
		End if    	 	
	 End if   
	    
End if
'End  sp  483615  ============   عدم تایید مالک

'=====================  پروانه پارکینگ هیئت -  عدم تایید چک لیست مالک  	=============== sp 475882 ===================
'ساختمان گروه الف (تا دوسقف یا 600 متر مربع ) می باشد؟
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("475eb350-1c76-4ce9-8591-b5deecd5390c") Then 
	SaveCheck("d8aa0716-3410-48a1-92d1-3d88bbdfe737",2,0)	' انتخاب کارشناس بازدید منتخب
End if 

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d8aa0716-3410-48a1-92d1-3d88bbdfe737") Then 

        Dim randomValue As Integer
        Dim rng As New Random()
        randomValue = rng.Next(1, 3)
        If randomValue = 1 Then
	            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
				AssingTo =""
				AssingToUserName=""
				Call User(13)   
				CreatTask("Activity_0tqwaqe",AssingTo,AssingToUserName)
				SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
        Else
	            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
				AssingTo =""
				AssingToUserName=""
				Call User(14)  
				CreatTask("Activity_0tqwaqe",AssingTo,AssingToUserName)
					SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
        End If	
End if

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6d7ab24e-9480-4efa-b7cf-b10b316f3b39") Then
	Dim RevistIsDone as New List(Of DtoSC.Sh_Revisit)
	RevistIsDone = Info8.Getrequest.GetRevisit_List(Info8.GetRequest.info.NidProc)
	RevistIsDone = RevistIsDone.where(Function (f) f.NidRevisit = Info8.GetRequest.Info.NidRevisit And f.IsDoIt = 1).tolist
	
	If RevistIsDone.any Then
        Dim randomValue As Integer
        Dim rng As New Random()
        randomValue = rng.Next(1, 3)
        If randomValue = 1 Then
	            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
				AssingTo =""
				AssingToUserName=""
				Call User(13)   
				CreatTask("Activity_0tqwaqe",AssingTo,AssingToUserName)
				SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
        Else
	            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
				AssingTo =""
				AssingToUserName=""
				Call User(14)  
				CreatTask("Activity_0tqwaqe",AssingTo,AssingToUserName)
					SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
        End If	
     Else 
		 Info8.GetOnTickChecklistParameter().Result="false"                       
	  	 Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بازديد بمانيد"      
     End if   
End if


'=====================  پروانه پارکینگ هیئت - اتمام عدم تایید چک لیست مالک  ================= sp 475882 ====================

'=====================  پروانه کمیسیون ماده 5 - عدم تایید چک لیست مالک  ==================================
'sp 476127
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9efff845-7bb1-4588-a021-01bd62194fe1") Then
	 '	Call User(1)
	 '	AssingTo =""
	 '	AssingToUserName=""
	 '	CreatTask("Activity_0ul19d1",AssingTo,AssingToUserName)
End if 
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("762bc141-9e63-4bf1-b593-0e9039d642cc") Then
	 '	Call User(1)
	 '	AssingTo =""
	 '	AssingToUserName=""
	 '	CreatTask("Activity_0ul19d1",AssingTo,AssingToUserName)
End if 
'=====================  پروانه کمیسیون ماده 5 - عدم تایید چک لیست مالک  اتمام ==================================
'sp 450965 
If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("21ca42cd-0f4a-4d25-86d0-3b3f8bcbd512") Then
	SaveCheck("ef8284a0-17a5-4ec1-9ac9-8f97e59aa433",2,0)
End IF 

'sp 465277 
'sp 446220
 'sp 469583
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ea3ba403-3e38-4d9c-9480-2d00971a75b8") Then

'if ucase(Info8.GetOnTickChecklistParameter().NidProc.tostring) = ucase("7EE331CD-734C-4FBE-ABD7-4782F47CA228") Then

'	Info8.GetOnTickChecklistParameter().Result="false"	
'	Info8.GetOnTickChecklistParameter().Description = "N : " + NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c").tostring
'	Exit Function

'End if

'msg(NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c").tostring)

	if NewControlCheck("37e5959a-c385-40ed-99cf-51f2557b6b6c") = 3 Then	'اگر ساختمان گروه الف نبود (ملاک چک لیست است) با تایید یا عدم تایید چک لیست ناظر ارسال به تعیین زمان بازدید
		SaveCheck("e7d55d2a-2986-404a-86e6-3f7eb0c8c03b",2,0)	 
	Else	
	    Dim randomValue As Integer
	    Dim rng As New Random()
	    randomValue = rng.Next(1, 3)
	    If randomValue = 1 Then
		            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
					AssingTo =""
					AssingToUserName=""
					Call User(13)   
					CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
				'	SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
	    Else
		            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
					AssingTo =""
					AssingToUserName=""
					Call User(14)   
					CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
				'	SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
	    End If	
    
    '	Call payamak("درخواست پارکینگ شهرسازی","شهروندگرامی تعرفه بررسی پارکینگ توسط کارشناس رسمی دادگستری مطابق با تعرفه های کانون و مرکز کارشناسان رسمی خراسان رضوی می باشد")

  End if 
 'end sp 465277       
'sp 469583	
	'end sp 469583
End if 


'عدم تایید مالک
'sp 488436
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("37e5959a-c385-40ed-99cf-51f2557b6b6c") Then
	SaveCheck("ea3ba403-3e38-4d9c-9480-2d00971a75b8",2,0)
End if 
'IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ea3ba403-3e38-4d9c-9480-2d00971a75b8") Then
	  ' فرقی نمیکند که گروه الف باشد یا خیر باید بازدید تشکیل شود
'End if


'sp 496015 IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fbc4601a-e629-40cb-9647-6555562ac077") Then
'sp 496015 IF 	 'sp 407575
'sp 496015 IF 	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
'sp 496015 IF     lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
'sp 496015 IF     if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
'sp 496015 IF     Dim ss as Integer =0
   
 'sp 496015 IF    For ss=0 to lstOwner.count-1
 'sp 496015 IF      Sms(lstOwner(ss).OwnerMobile,"رگ اول بنا"," مالك محترم : با توجه به اعلام عدم نياز به ارسال پرونده به شماره " + Info8.GetRequest.Info.nidworkitem.tostring() +" جهت تحويل رگ اول بنا به اطلاع مي رساند رعايت بروكف برابر مستندات جواز شهرداری الزامي است چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد. شهرداری مشهد ")
'sp 496015 IF     Next
'sp 496015 IF      
 'sp 496015 IF    Sms(Info8.GetRequest.Info.Cellphone,"رگ اول بنا"," مالك محترم : با توجه به اعلام عدم نياز به ارسال پرونده به شماره " + Info8.GetRequest.Info.nidworkitem.tostring() +" جهت تحويل رگ اول بنا به اطلاع مي رساند رعايت بروكف برابر مستندات جواز شهرداری الزامي است چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد. شهرداری مشهد ")

'sp 496015IF  End IF 
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("00b19551-993e-4483-b6c1-369941060023") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1b1c03b9-3f8c-475b-8663-f63334fb5941") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2331a630-bfc2-4c75-83e5-e0ad083adfd8") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("28d21878-dd31-467e-8a9b-2476e450d4a3") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2ede6cd8-404f-4cbc-bdb2-423343ccb8bc") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("47d786eb-ddc9-492c-853e-8decf41b0246") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("534a18ba-f262-4601-8c7f-a44eb0d6c8df") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("61f13d00-9ab1-47e6-b7b9-47abcec995ac") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("66caf2db-d017-428d-bd74-b9169a48a1ea") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6be7122d-e39a-4b10-b15d-119fabc006d5") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("91a99c6c-4ceb-4519-9c83-f60f9c5f9f8f") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("a1322d6a-1cb4-4f77-b5a3-3fbbe4677ff7") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("aba2cfeb-d00e-46bd-968e-b4e284b90681") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b8cef52b-1385-440c-bcb2-a4c66cf03ac8") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c5d09119-8e46-4b57-b425-73070cc9f252") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f3500694-2d52-4312-b42e-e96e9d23e61b") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f892bb7d-5901-4130-aaff-7a7b9abc2a8a") Then
   		Info8.GetOnTickChecklistParameter().Result="false" 
		Info8.GetOnTickChecklistParameter().Description=  " مجاز به عدم تایید نمی باشید  " 
End if   



'sp 402508
'----------------
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ea7d2063-7693-489b-9f47-154a1c9d2856") Then
	SaveCheck("33e5c7f6-8f2c-4de0-940d-3a8f53d84247",2,0)
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0d383608-f58e-4482-be91-e3fe73966975") Then
	SaveCheck("556c177f-60e8-4bd7-9490-da76eda801c2",2,0)
End if
'----------------
' sp 407966
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("13adc984-555d-422c-a5cb-727e1e6f92c2") Then
	SaveCheck("253ba293-db0e-4134-bd4b-709f86a26aec",2,0)
End if

'sp 398614      عدم تایید مالک 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b98c714f-8689-4a83-b835-19e8ad1a9040") Then
	SaveCheck ("4b291443-633b-462e-8259-f0df594478ef",2,0)	'عدم نیاز به صورتمجلس تفکیکی
End if 
'sp 396836 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f50aa94c-10b6-4aa9-a591-32bbce3e687a") Then 
	SaveCheck ("c1355e26-5e4b-4ab9-b024-828d5bd39308",2,0) 
End if 

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("316d0d01-c867-428d-b2fb-37593fd97bdb") Then
	Info8.GetOnTickChecklistParameter().Result="false" 
	Info8.GetOnTickChecklistParameter().Description=  " گواهی صادر شده است ، فقط مجاز به تایید میباشید " 
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b98c714f-8689-4a83-b835-19e8ad1a9040") Then	'درخواست بازدید کارشناس اداره ثبت (حدنگار)
	if ControlTask("Activity_1brcze0")<> 1	Then 	'نقشه برداری
		if Info8.GetRequest.Info.CI_Requester=14 Then 
			SaveCheck("2ede6cd8-404f-4cbc-bdb2-423343ccb8bc",2,0) 	'تعیین زمان بازدید نماینده فنی
		Else	
			SaveCheck("aba2cfeb-d00e-46bd-968e-b4e284b90681",2,0) 		'تعیین زمان بازدید شهروند
		End if 	
	End if 	
End IF 
	    	
if (ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3dd38365-0f7b-40a0-8b4d-230f91e8094d") _
or  ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("39504dea-f6f8-40c2-805d-25c0da57a558")) And String.IsNullOrEmpty(Info8.GetRequest.Info.Comment)=True Then 
	Info8.GetOnTickChecklistParameter().Result="false" 
	Info8.GetOnTickChecklistParameter().Description=  " لطفا توضیحات درخواست متقاضی ثبت گردد " 
End if  
	    
	    AssingTo=""
		AssingToUserName=""
 		Call User(1)
	' ایجاد ناظر در صورت عدم تایید 
	
	' بانک	
		
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6e2a2764-f1d1-441d-8baf-507314a25c9a") Then 
	
		if ControlTask("Activity_0x0ylrz")=0  And  (ControlTask("Activity_0mfimf6")=0 or ControlTask("Activity_0mfimf6")=2) Then 
			if ControlTask("Activity_1tqlk2x")=0  or ControlTask("Activity_1tqlk2x")=2 Then 	
				CreatTask("Activity_1tqlk2x",AssingTo,AssingToUserName)   '393146 or ControlTask("Activity_1tqlk2x")=2 
				Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
			End if
		ElseIf ControlTask("Activity_0x0ylrz")=2  And  ControlTask("Activity_0mfimf6")=2 Then 
			if ControlTask("Activity_1tqlk2x")=0  or ControlTask("Activity_1tqlk2x")=2  Then 	
				CreatTask("Activity_1tqlk2x",AssingTo,AssingToUserName)  '393146 or ControlTask("Activity_1tqlk2x")=2
				Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
			End if
		ElseIf ControlTask("Activity_0x0ylrz")=2  And  ControlTask("Activity_0mfimf6")=0 Then 

		  if  Check ("f3500694-2d52-4312-b42e-e96e9d23e61b",1) Then
		  	if ControlTask("Activity_1tqlk2x")=0  or ControlTask("Activity_1tqlk2x")=2  Then  
		  		CreatTask("Activity_1tqlk2x",AssingTo,AssingToUserName)  '393146 or ControlTask("Activity_1tqlk2x")=2 
		  		Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
		  	End if
		  Else
		     if  Check ("b8cef52b-1385-440c-bcb2-a4c66cf03ac8",1) Then 
		  		if ControlTask("Activity_1tqlk2x")=0 or ControlTask("Activity_1tqlk2x")=2  Then 	
		  			CreatTask("Activity_1tqlk2x",AssingTo,AssingToUserName)   '393146 or ControlTask("Activity_1tqlk2x")=2
		  			Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
		  		End if
		  	 Else
			  	 Info8.GetOnTickChecklistParameter().Result="false"                       
	  			 Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بازديد بمانيد" 	
		  	 End if 	
		  
		  End if
		  
	    ElseIf ControlTask("Activity_0mfimf6") =1 Then
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بازديد بمانيد" 
		ElseIf ControlTask("Activity_0x0ylrz") =1 Then
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بروکف بمانيد" 
    	End if 
    '	ادارات
    ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("78e4bea2-29a5-4df0-8fee-42cf2e1b0ae7") Then 
		
		if ControlTask("Activity_0fx4od8")=0  And  (ControlTask("Activity_1jz9ix6")=0 or ControlTask("Activity_1jz9ix6")=2) Then 
			if ControlTask("Activity_1xqicbz")=0  or ControlTask("Activity_1xqicbz")=2 Then 	
				CreatTask("Activity_1xqicbz",AssingTo,AssingToUserName)	 '393146 or ControlTask("Activity_1xqicbz")=2
				Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
			End if
		ElseIf ControlTask("Activity_0fx4od8")=2  And  ControlTask("Activity_1jz9ix6")=2 Then 
			if ControlTask("Activity_1xqicbz")=0 or ControlTask("Activity_1xqicbz")=2 Then 	
				CreatTask("Activity_1xqicbz",AssingTo,AssingToUserName)	 '393146 or ControlTask("Activity_1xqicbz")=2
				Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
			End if
		ElseIf ControlTask("Activity_0fx4od8")=2  And  ControlTask("Activity_1jz9ix6")=0 Then 
		  
		  if  Check ("61f13d00-9ab1-47e6-b7b9-47abcec995ac",1) Then
		  	if ControlTask("Activity_1xqicbz")=0 or ControlTask("Activity_1xqicbz")=2  Then 	
		  		CreatTask("Activity_1xqicbz",AssingTo,AssingToUserName)	 '393146or  or ControlTask("Activity_1xqicbz")=2 
		  		Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
		  	End if
		  Else
		     if  Check ("47d786eb-ddc9-492c-853e-8decf41b0246",1) Then 
		  		if ControlTask("Activity_1xqicbz")=0 or ControlTask("Activity_1xqicbz")=2 Then 	
		  			CreatTask("Activity_1xqicbz",AssingTo,AssingToUserName)	 '393146or ControlTask("Activity_1xqicbz")=2
		  			Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
		  		End if
		  	 Else
			  	 Info8.GetOnTickChecklistParameter().Result="false"                       
	  			 Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بازديد بمانيد" 	
		  	 End if 	
		  
		  End if
		  
		  
	
	    ElseIf ControlTask("Activity_1jz9ix6") =1 Then
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بازديد بمانيد" 
  		ElseIf ControlTask("Activity_0fx4od8") =1 Then
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بروکف بمانيد" 	
  		End if	
  	'	اجاره
    ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("19b19edb-d9cf-452e-a6b1-d5c4f1a8fe0a") Then 
		if  ControlTask("Activity_1e7bq44")=2 Then
			if ControlTask("Activity_143vz32")=1 Then
				Info8.GetOnTickChecklistParameter().Result="false"                       
  				Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه ناظر شهرسازی بمانيد"  
			Else
				if ControlTask("Activity_143vz32")=0 or ControlTask("Activity_143vz32")=2  Then 	
					CreatTask("Activity_143vz32",AssingTo,AssingToUserName)	 '393146 or ControlTask("Activity_143vz32")=2
					Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
				End if
			End if	
	    Else
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بازديد بمانيد" 
  		End if	
  	'	محضر
    ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ec7410d1-eaad-41c0-b8d4-328cf119d229") Then 
		if  ControlTask("Activity_18th9bt")=2 Then
			if ControlTask("Activity_1alwiso")=1 Then
				Info8.GetOnTickChecklistParameter().Result="false"                       
  				Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه ناظر شهرسازی بمانيد"  
			Else
				if ControlTask("Activity_1alwiso")=0 or ControlTask("Activity_1alwiso")=2 Then 	
					CreatTask("Activity_1alwiso",AssingTo,AssingToUserName) '393146 or ControlTask("Activity_1alwiso")=2
					Call payamak("ناظر شهرسازی"," جهت بررسی پرونده و کنترل محاسبات ضابطه و تحلیل به کارتابل ناظر شهرسازی ارسال شده است.",2)
				End if	
			End if
	    Else
			Info8.GetOnTickChecklistParameter().Result="false"                       
  			Info8.GetOnTickChecklistParameter().Description="منتظر نتيجه بازديد بمانيد" 
  		End if				
	End if
	
	'==================================================================		
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f426b446-1053-427f-9a99-e60ddaf851fd") Then Call User(1) : CreatTask("Activity_07dulz0",AssingTo,AssingToUserName)  
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("72935896-57f5-481c-b399-eadab6c73ccc") Then Call User(1) : CreatTask("Activity_1ldlnq4",AssingTo,AssingToUserName) 
'عدم تایید پیش نویس پایانکار ایجاد نود شهرسازی
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9e013712-d86a-49ae-99d2-15d11da63ad3") Then Call User(1) : CreatTask("Activity_1llq4cn",AssingTo,AssingToUserName) 
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f4211155-b93d-401a-97b5-332e75c13e17") Then Call User(1) : CreatTask("Activity_0goatsn",AssingTo,AssingToUserName)  
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b83c895e-79ea-4960-a71e-d335b14bb629") Then 'آخرین وضعیت مجاز ساختمان
		Call Payamak("جهت اطلاع","شهروند محترم نیاز به ثبت درخواست تایید سابقه مجاز توسط شما می باشد")
		   	    
	End if
 if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2a660b0d-420f-4cfa-bdaa-0a0e16d6f7dd") Then 	SaveCheck ("54296476-cc80-462d-8444-0761b08570bc",2,0)	   	 
 End Function
 

Public Function Confirm  () ' تایی

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("be222d92-5121-4e23-a3a8-2cccac232734") Then  'sp 517394
	CreatTask("Activity_1sboo8n","F1DC9C86-F40D-4D44-9806-1FA977536A8B","نظام مهندسی-ارجاع کار")
End if


IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e3da1059-2108-4516-b819-dbe0adbe941b") Then  'sp 494439
	SendPayamak("چک لیست","زمان مراجعه برای رسیدگی " & Info8.GetOnTickChecklistParameter().Comment)
End if


IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("74a407ff-6310-4e05-acf8-59dbdd8ababd") Then  'sp 494439
	if ControlCheck("38804314-12bc-4bbe-8b54-abd9dfe1f562") <> 2  Then msg(" عدم تایید چک لیست فرم اطلاعات اولیه صلحنامه در آرشیو درج شده است ") : Exit Function
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4ecab320-faa1-4aa3-bcee-86dabb0fad34") Then  'sp 494439
	if ControlCheck("40c75261-c8ff-42e1-a315-2e5cf059a20d") <> 2  Then msg(" عدم تایید چک لیست فرم اطلاعات اولیه صلحنامه در آرشیو درج شده است ") : Exit Function
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c4d616d8-5473-43ce-84e8-52b369f50826") Then  'sp 494439
	if ControlCheck("0b818295-bab3-4ae2-98c4-b1060de8d0cb") <> 2  Then msg(" عدم تایید چک لیست فرم اطلاعات اولیه صلحنامه در آرشیو درج شده است ") : Exit Function
End if




IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("cc058097-003f-4bae-9bbb-f3d4379c313d") Then  'sp 512373
	if String.IsNullOrEmpty(Info8.GetOnTickChecklistParameter().Comment) Then msg("درج توضیحات الزامی میباشد") : Exit Function
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("49649914-00b3-476e-aed6-2e1dc854f281") Then  'sp 512373
	if String.IsNullOrEmpty(Info8.GetOnTickChecklistParameter().Comment) Then msg("درج توضیحات الزامی میباشد") : Exit Function
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fe68d68a-8f0c-4ea9-9bb6-d079bf601b25") Then  'sp 512373
	if String.IsNullOrEmpty(Info8.GetOnTickChecklistParameter().Comment) Then msg("درج توضیحات الزامی میباشد") : Exit Function
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4446fa18-fa43-4cd5-8b3e-6a806ce9664e") Then  'sp 517394
'	CreatTask("Activity_1o9ntdx",,)
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4446fa18-fa43-4cd5-8b3e-6a806ce9664e") Then  'sp 517394
'	CreatTask("Activity_1o9ntdx",,)
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8839bb13-0563-405c-98c0-a82f82753b9b") Then
	'اعتبارسنجی شود که حتما فیش با وضعیت تایید بانک داشته باشد
	
		Dim CountFiche as Integer 
		Dim AllFiche as  list(of Dtosc.Income_Fiche)
		AllFiche=Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
		AllFiche=AllFiche.where(Function (f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5)).tolist
		CountFiche= AllFiche.count
		if CountFiche<=0 Then 
			Info8.GetOnTickChecklistParameter().Result="false" 
		 	Info8.GetOnTickChecklistParameter().Description=" به علت وجود فیش درآمدی پرداخت نشده امکان تایید نمیباشد" 							
	 		Exit Function
		End if
End if

	'--------تشکیل نود املاک ----------مشمول سهم سرانه  -----------  پروانه دیوار کشی   ' sp 511929
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)=ucase("c4d616d8-5473-43ce-84e8-52b369f50826") Then
'	
		if Info8.GetBarokaf.GetSh_BaroKaf.PathValue<>0  or Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed<>0  Then 
			Call CreatRequestAmlak()	'sp 477716 511929
			Call Payamak("املاک"," برای اداره املاک منطقه ارسال شده است لطفا ظرف 24 ساعت آینده به اداره املاک مراجعه فرمایید.",3)			
		 Else
		   	Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" میزان در مسیر و یا میزان در مسیر  رعایت شده ندارید و نود املاک تشکیل نشد."
		'		Exit Function
		 End if		
	'	 msg("T1:" & Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed.tostring) : Exit Function
	End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9bec1b93-322e-4bea-b571-83c729ad13a9") Then
	'در صورت  تایید چک لیست  فیش و فاکتور برای مالک نمایش داده شود
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9348263a-da4f-4bc7-a2e9-7c9834018283") Then
	'بعد از تایید فیش درامدی امکان تغییر این چک لیست نباشد
	'sp 515899
		Try
			Dim incomeList,incomeList_Taeed As New List(Of DtoSC.Income_Fiche)
			Dim incomeFiches As New List(Of DtoSC.Income_Fiche)
			incomeFiches = Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))			
			incomeList_Taeed = incomeFiches.Where(Function(x) {3,5}.contains(x.EumFicheStatus)).ToList
			if Not incomeList_Taeed.any Then
				msg("بدلیل عدم وجود فیش تایید شده امکان تغییر چک لیست نمیباشد - sp 515899")
				Exit Function	
			Else
	
			End if							
		Catch e as exception 
			msg("E: " & e.message)
		End Try	
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("96fbb8df-9ef5-48a7-b956-528218c82509") Then
		AssingTo =""
	 	AssingToUserName=""
	 	user(7)
	 	CreatTask("Activity_0d8wcvh",AssingTo,AssingToUserName)		'تشکیل نود املاک
End if

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ce38594b-236d-4564-aa96-f0d21d814947") Then
		AssingTo =""
	 	AssingToUserName=""
	 	Call user(7)
		CreatTask("Activity_1a6cuql",AssingTo,AssingToUserName)	'نود  املاک
End if  
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("65b4c6e2-811a-4082-8610-71f1da14c61b") Then
		AssingTo =""
	 	AssingToUserName=""
	 	Call user(7)
		CreatTask("Activity_0eps2zw",AssingTo,AssingToUserName)	'نود   تامین اجتماعی
End if 


'sp  490704
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b6e6dade-7d18-4023-b3bc-e0b571b660d5") Then

	Call CreatRequestAmlak()

End if   'end sp 490704

'Test Radif FILL_INFO  
'Fj1403
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2cbb5d29-45c3-4b47-9c6e-1f8195e8ed4e") Then
	if Info8.GetRequest.info.nidworkitem = 10378073 or Info8.GetRequest.info.nidworkitem = 10267913 Then

'if 1=1 Then	

   		Global.Biz.SC.ClsCommon.GIsPlanFromParvandeh = "false"
   		Global.Biz.SC.ClsCommon.EngineeringConnect = "false"
      	Dim tmpMapControl As New Global.Biz.SC.ClsMapControl(Info8._District)
      	
		tmpMapControl.LoadObj(Info8.GetCrowdsourcing_RevisitAgent().NidProc.tostring, Info8.Security_RequestGuid, Info8.Security_EncryptCode)
		 Dim tttt As New Global.Biz.SA.ClsErrorResult
        tttt = tmpMapControl.ErrorResult()
		Dim TmpNosazi As New Global.Biz.SA.ClsNosaziCodeType
		Dim NidMojazParvaneh as Guid
		Dim tmpNidR As Guid=Guid.Parse("00000000-0000-0000-0000-000000000000")
		Dim LstParent As List(Of Global.Biz.SA.ClsNosaziCodeType)
	    LstParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
        if LstParent.any Then  LstParent = LstParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
        
		NidMojazParvaneh = Info8.GetFunctions.GetNidBase(LstParent(0).NidNosaziCode,2, 0,tmpNidR)
		
		Info8.MojazParvaneh_House=Nothing
		Info8.ParameterList.REMove("MojazParvaneh_House_NidBase")
		Info8.ParameterList.Add("MojazParvaneh_House_NidBase", NidMojazParvaneh.ToString)
		Info8.ParameterList.Remove("MojazParvaneh_House_LoadFunc")
		Info8.ParameterList.Add("MojazParvaneh_House_LoadFunc", "")
	'		Info8.AddError(BIZ.SA.EumErrorAction.warning, "Test", "Alo 1 2 ")

      '  'ذخیره کردن
       
        tmpMapControl.Save(Info8.Security_RequestGuid, Info8.Security_EncryptCode)
        
        Dim rrrr As New Global.Biz.SA.ClsErrorResult
        rrrr = tmpMapControl.ErrorResult()
       
      
       tmpMapControl.Base_Using = Info8.GetMojazParvaneh_House.GetBase_Using
       tmpMapControl.Base_Front = Info8.GetMojazParvaneh_House.GetBase_Front     
       tmpMapControl.Save(Info8.Security_RequestGuid, Info8.Security_EncryptCode)       

		Try 
		   Dim ssss As Global.Biz.SC.ClsEngineer
		   Dim kkk As New Global.Biz.SC.SrvEngineering.ClsFil_Info
        Dim UserInfo as New BIZ.SA.ClsUser 
        
        UserInfo.UserGuid=Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57")
        UserInfo.UserName="esup"		   



      '  msg(Info8.GetRequest.Info.NidProc.tostring())
      '  Exit Function
		   kkk = ssss.GetFilInfo(Info8.GetRequest.Info.NidProc,UserInfo)

		 '  Info8.GetOnTickChecklistParameter().Result="false"	
		 '  Info8.GetOnTickChecklistParameter().Description = kkk.Fil_Info.NidWorkItem
		   
		   Catch ex As Exception

		     ' Info8.GetOnTickChecklistParameter().Result="false"	
			'			Info8.GetOnTickChecklistParameter().Description =ex.message
				 msg(ex.message.tostring())
                 Exit Function
		   End Try    
'End if '1=2	
	  
	End if
	
End if


'sp 483693	--------------------------
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("67f75965-b0b3-469a-94b6-9a4fa228c501") Then	'احرازهویت
'sp 499849	if Info8.GetRequest.Info.CI_Requester=13 Then
		if check("f555829e-2fec-42e3-86f5-2ec726de5549",0) = True		'با تایید چک لیست پایینی 
			Info8.GetRequest.Info.IsConfirmAuthentication =1
			Info8.GetRequest.Save()  
		End if
'sp 499849	End if	
End if
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f555829e-2fec-42e3-86f5-2ec726de5549") Then	'انطباق مالکیت و پلاک ثبتی
'sp 499849	if Info8.GetRequest.Info.CI_Requester=13 Then
		if check("67f75965-b0b3-469a-94b6-9a4fa228c501",0) = True	' با تایید چک لیست بالایی
			Info8.GetRequest.Info.IsConfirmAuthentication =1  
			Info8.GetRequest.Save()
		End if
'sp 499849	End if	
End if
'end sp 483693 --------------------------

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6d7ab24e-9480-4efa-b7cf-b10b316f3b39") Then

Try
	Dim Test as New BIZ.SC.SrvEngineering.EngineerList
'	Test = Info8.GetFunctions.GetAllEngWithNidProc(guid.Parse("67591412-CA4B-4AB5-8B70-04DB3B6010CC"))
	Test = Info8.GetFunctions.GetAllEngWithNidProc(Info8.GetRequest.Info.nidproc)

	Dim listEnginer as New List(Of BIZ.SC.SrvEngineering.Eng_Info)
	Dim RefInfoLst as New List(Of BIZ.SC.SrvEngineering.Ref_Info)
	listEnginer = Test.EngInfoList
	RefInfoLst = Test.RefInfoList.where(Function (f) f.CI_Ability=1).tolist

	Dim i as Integer = 0
	Dim j as Integer = 0
	AssingTo =""
	AssingToUserName=""

  Dim dd as String 
  'الویت اول
 	For i=0 to RefInfoLst.count-1 
 	 
		For j=0 to listEnginer.count-1 
		       
			if RefInfoLst(i).NIdEng = listEnginer(j).NIdEng  Then
				if listEnginer(j).ADPstudyFieldRel = 100 or listEnginer(j).ADPstudyFieldRel = 110 or listEnginer(j).ADPstudyFieldRel = 120 _
				or listEnginer(j).ADPstudyFieldRel = 130 or listEnginer(j).ADPstudyFieldRel = 191 Then
			       
					AssingTo = listEnginer(j).NIdEng.tostring
					AssingToUserName = listEnginer(j).EngName.tostring + listEnginer(j).EngFamily.tostring 					
				 
					Exit For
				End if				
			
			End if	
	  	
		Next
			if Not String.IsNullOrEmpty(AssingTo) Then Exit For
	Next

	'الویت دوم 
if String.IsNullOrEmpty(AssingTo) Then 
	
 	For i=0 to RefInfoLst.count-1 
 	 
		For j=0 to listEnginer.count-1 
		       
			if RefInfoLst(i).NIdEng = listEnginer(j).NIdEng  Then
				if listEnginer(j).ADPstudyFieldRel = 300 or listEnginer(j).ADPstudyFieldRel = 310 or listEnginer(j).ADPstudyFieldRel = 320 _
					or listEnginer(j).ADPstudyFieldRel = 330 or listEnginer(j).ADPstudyFieldRel = 340 or listEnginer(j).ADPstudyFieldRel = 350 or listEnginer(j).ADPstudyFieldRel = 360 _
					or listEnginer(j).ADPstudyFieldRel = 370 or listEnginer(j).ADPstudyFieldRel = 380 or listEnginer(j).ADPstudyFieldRel = 391 _
					or listEnginer(j).ADPstudyFieldRel = 392 or listEnginer(j).ADPstudyFieldRel = 393 or listEnginer(j).ADPstudyFieldRel = 394 _
					or listEnginer(j).ADPstudyFieldRel = 395 Then	
			       
					AssingTo = listEnginer(j).NIdEng.tostring
					AssingToUserName = listEnginer(j).EngName.tostring + listEnginer(j).EngFamily.tostring 					
			   
					Exit For
				End if				
			
			End if	
	  	
		Next
			if Not String.IsNullOrEmpty(AssingTo) Then Exit For
	Next
End if 	

	if  String.IsNullOrEmpty(AssingTo)=False
	    CreatTask("Activity_1074fhi",AssingTo,AssingToUserName)  ' ارسال به نظام مهندسی 
	End if    
 Catch e as exception
           Msg("Confirm چک لیست " & e.message)
	   Exit Function
 End Try 	    
End if
'=================================  sp 481471 ===========  آتش نشانی ================  شهرداری تایید شروع ====================

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6f2ffc8a-2a5c-42e7-b9fe-d25056bffcfe") Then  	
	Call FunFire()
End if
'=================================  sp 481471 ===========  آتش نشانی ================  شهرداری تایید اتمام ====================

 '-------------------------------SP481228-------------------------------

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2cbb5d29-45c3-4b47-9c6e-1f8195e8ed4e") Then
    
 	if Check_ComNama <> 0 Then 
		CreatTask("Activity_0tvyeo9","4acf8b89-113d-4d23-a70f-f555c6eb9c2e","کمیته نما")
	End if		
	
End if


'=================================  sp 475882 ===========  پارکینگ ================  شهرداری تایید شروع ====================

'=================================  sp 475882 ===========  پارکینگ =============  شهرداری تایید اتمام ====================	


	Dim districtA as String 
	Dim tmpDistrictA() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	if tmpNosaziNickName<>"" Then 
		tmpDistrictA = tmpNosaziNickName.Split("-")
		districtA=tmpDistrictA(0)
	End if
	
'bana b sp 481116 sampa sp 480578 hazf shod	
'sp 480578 - محضز خانه
'	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bbc76503-68fa-43af-b3ed-8459654c0da1") Then
'	 	AssingTo =""
'	 	AssingToUserName=""		
'	 	Call User(15)
'	 	CreatTask("Activity_1j4cnoj",AssingTo,AssingToUserName)		
'	End if
'	
'	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6fa2c711-9739-4108-9e72-9af3fe6641bc") Then
'	 	AssingTo =""
'	 	AssingToUserName=""	
'	 	Call User(15)
'	 	CreatTask("Activity_1j4cnoj",AssingTo,AssingToUserName)		
'	End if
'	
'	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6eaded7a-79bf-43c0-8ca7-b5cd1ef141f9") Then
'	 	Info8.GetFunctions.WF_CloseSimpleTaskWithoutRunFormula(Info8.GetRequest.Info.NidProc.tostring , "Activity_1j4cnoj")	
'	End if
'end sp 480578			
	
	
'sp460965----Sajedi

'	در درخواست عدم خلاف
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("84affc06-a794-4534-bb8f-60a63471153a") Then
'			   	Info8.GetOnTickChecklistParameter().Result="false"
'			Info8.GetOnTickChecklistParameter().Description=" test."
	 	AssingTo =""
	 	AssingToUserName=""
	 	Call User(13)
	 	CreatTask("Activity_07r2jww",AssingTo,AssingToUserName)	'نود  کارشناسان رسمی
	 	Call SendPayamak("","به کانون کارشناسان رسمی جهت تایید استحکام بنا ارسال گردید.جهت پیگیری به نشانی :میدان استقلال - ضلع شمال شرقی - (فردوسی23) - پلاک 377 تلفن 05136153.")
'	 	InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "TaiidieEstehkamBana" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "گزارش استحکام بنا", "/Sara8Reports/TaiidieEstehkamBana", "1", districtA,4000)
 	End if 	
	 	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("322599dd-548d-4e70-afb0-23b644021049") Then	
		if ControlTask("Activity_07r2jww") = 1 Then
	 	    Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description="مرحله کارشناسان رسمی به اتمام نرسیده است ."	
		End if
    End if
    'در درخواست گواهی مفاصا حساب
   if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7c76771c-3494-4965-bc88-d56ce04cccbd") Then
	 	AssingTo =""
	 	AssingToUserName=""
	 	Call User(13)
	 	CreatTask("Activity_1hyn11b",AssingTo,AssingToUserName)	'نود  کارشناسان رسمی
	 	Call SendPayamak("","به کانون کارشناسان رسمی جهت تایید استحکام بنا ارسال گردید.جهت پیگیری به نشانی :میدان استقلال - ضلع شمال شرقی - (فردوسی23) - پلاک 377 تلفن 05136153.")
		InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "TaiidieEstehkamBana" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "گزارش استحکام بنا", "/Sara8Reports/TaiidieEstehkamBana", "1", districtA,4000)
 	End if 	
	 	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("cbd2bd56-c826-4193-bd49-40147cca4941") Then	
		if ControlTask("Activity_1hyn11b") = 1 Then
 	    Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description="مرحله کارشناسان رسمی به اتمام نرسیده است ."	
		End if
    End if
	'در درخواست پایانکار بهره برداری
   if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("20310ab0-4d33-4313-9d1c-ad2cba1ed88c") Then
	 	AssingTo =""
	 	AssingToUserName=""
	 	Call User(13)
	 	CreatTask("Activity_1f32562",AssingTo,AssingToUserName)	'نود  کارشناسان رسمی
	 	Call SendPayamak("","به کانون کارشناسان رسمی جهت تایید استحکام بنا ارسال گردید.جهت پیگیری به نشانی :میدان استقلال - ضلع شمال شرقی - (فردوسی23) - پلاک 377 تلفن 05136153.")
		InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "TaiidieEstehkamBana" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "گزارش استحکام بنا", "/Sara8Reports/TaiidieEstehkamBana", "1", districtA,4000)
 	End if 	
	 	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5c1ed5d0-8837-41ec-8f5a-ffa202318ada") Then	
		if ControlTask("Activity_1f32562") = 1 Then
	 	    Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description="مرحله کارشناسان رسمی به اتمام نرسیده است ."	
		End if
    End if    		
'End 'sp460965

'sp 467993
   if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7bf1a273-a5a5-4fb0-8bd6-4426bd99d0e5") Then
	 	AssingTo =""
	 	AssingToUserName=""
	 	Call User(12)
	 	CreatTask("Activity_1066zf1",AssingTo,AssingToUserName)	'نود  تایمین اجتماعی
	 	Call payamak("","محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
	End if 	
'end sp 467993
'-.-.-.-.-.-.-.-.-.-.--.-.-.-.-.-.-.--.-.-.-.-.-.-.-.--.-.-.-.--.-.-.-.-.
'450772

	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("839f8728-756f-4db1-acfe-a5a9c25cfc3d") Then SaveCheck ("f3ea25fc-c4bc-4995-b812-e3086c18891b", 1,0)

	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9d02e09a-6c67-48b8-b6c0-1dbccb8dbe09") Then SaveCheck ("9a93c7ba-3686-4c51-bcf2-f43c587a01f1", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1565f4e3-9513-4a12-8005-b522d58f1c70") Then SaveCheck ("cab9b25c-284a-413d-bdd8-e402fc30a7f6", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b4bcc0da-fc48-431b-b2f5-e8c52ea851a5") Then SaveCheck ("167c1bd2-803d-4d2f-85c7-45a36ecab4d2", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3c08e559-3041-4dea-af26-5f6be50779d1") Then SaveCheck ("5d7ada12-32ec-48a4-b72d-360f87f057b3", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8312a9bc-3904-49dc-9e8a-922825e33300") Then SaveCheck ("07f2d2d7-a46a-47c0-8388-94607a0f6700", 1,0)

'-.-.-.-.-.-.-.-.-.-.--.-.-.-.-.-.-.--.-.-.-.-.-.-.-.--.-.-.-.--.-.-.-.-.
 
if (Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)= "bfd0cfc6-d40a-46be-a733-f034e0fe4adc" or _
(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)= "c3cef645-6ede-4d6c-9ceb-c1369a33ae32" or _
(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)= "d628ccdd-6b74-4998-beda-6b4eb2948a0c" or _
(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)= "d62c1b5c-b34f-478c-b0f5-cae273920bc9" or _
(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)= "d7a55d10-25d8-4e15-8a02-a664d89f8666" or _
(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)= "cf168011-6ccb-4f38-80f7-ae8b94c7d028" or _
(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)= "7bf1a273-a5a5-4fb0-8bd6-4426bd99d0e5" Then   'sp sajedi 467993
  
	
	
	
'	Info8.GetOnTickChecklistParameter().Result="false"	
'	Info8.GetOnTickChecklistParameter().Description =TaeedM()
'	Exit Function
  if TaeedM() = 0 Then '
 	InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "TaminEjtemaii_New" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "گزارش فاکتور درامدی تامین اجتماعی", "/Sara8Reports/TaminEjtemaii_New", "1", districtA,4100)
  Else
    	Info8.GetOnTickChecklistParameter().Result="false"
		Info8.GetOnTickChecklistParameter().Description="تا زمانیکه تایید مکاتبات تایید نشده باشد اجازه تایید ندارید."
		Exit Function
  End if		
   
End if



'sp 451382
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("69b3e37f-e9f3-47bf-a037-fe55dd3a9393") Then
	BIZ.SC.ClsCommon.GIsCheckNationalCode = "false"
	Info8.GetRequest.Info.IsConfirmAuthentication =1  
	Info8.GetRequest.Save() 
End if
'sp 379897

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e3da1059-2108-4516-b819-dbe0adbe941b") Then
	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
    Dim ss as Integer =0
    For ss=0 to lstOwner.count-1
	    Sms(lstOwner(ss).OwnerMobile,"مالک"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & " متن پیامک کارشناس برای زمان مراجعه به شرح ذیل می باشد " & Info8.GetOnTickChecklistParameter.Comment.tostring & " چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد.شهرداری مشهد") 
    Next
	Sms(Info8.GetRequest.Info.Cellphone,"مالک"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & " متن پیامک کارشناس برای زمان مراجعه به شرح ذیل می باشد " & Info8.GetOnTickChecklistParameter.Comment.tostring & " چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد.شهرداری مشهد") 

End if
'sp 445217
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4ce4d183-5bf6-4ea9-b7cb-79afed4f8d40") Then
	Dim w = Info8.GetFunctions.WF_CloseSimpleTaskWithoutRunFormula(Info8.GetRequest.Info.NidProc.tostring , "Activity_1ix59rv")
ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4f6faa15-4c81-4cb2-8706-a85f794404d7") Then
	Dim w = Info8.GetFunctions.WF_CloseSimpleTaskWithoutRunFormula(Info8.GetRequest.Info.NidProc.tostring , "Activity_0zn42hd")
ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c9505d2d-f70b-4a77-8162-4049bc384723") Then
	Dim w = Info8.GetFunctions.WF_CloseSimpleTaskWithoutRunFormula(Info8.GetRequest.Info.NidProc.tostring , "Activity_0cdwiv3")
ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e35209a0-b546-4841-9906-52b6d1da6b86") Then
	Dim w = Info8.GetFunctions.WF_CloseSimpleTaskWithoutRunFormula(Info8.GetRequest.Info.NidProc.tostring , "Activity_1xgmet1")
ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5f49cc81-26a0-4b9b-8f1d-e7207771a29d") Then
	Dim w = Info8.GetFunctions.WF_CloseSimpleTaskWithoutRunFormula(Info8.GetRequest.Info.NidProc.tostring , "Activity_0i1bdi4")
End if 


'sp 439964 450449
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bfd0cfc6-d40a-46be-a733-f034e0fe4adc")
	User(12)
	CreatTask("Activity_1xgmet1",AssingTo,AssingToUserName )'"480d7bd3-fcfd-4e25-98e6-d77ce809d38f","سازمان تامین اجتماعی")
	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
    Dim ss as Integer =0
    For ss=0 to lstOwner.count-1
    
	    Sms(lstOwner(ss).OwnerMobile,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
	Next
	Sms(Info8.GetRequest.Info.Cellphone,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
   
ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c3cef645-6ede-4d6c-9ceb-c1369a33ae32") Then
	User(12)
	CreatTask("Activity_0i1bdi4",AssingTo,AssingToUserName )'"480d7bd3-fcfd-4e25-98e6-d77ce809d38f","سازمان تامین اجتماعی")
	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist

	Dim ss as Integer =0
    For ss=0 to lstOwner.count-1
    
	    Sms(lstOwner(ss).OwnerMobile,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
	Next
	Sms(Info8.GetRequest.Info.Cellphone,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
   

ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d628ccdd-6b74-4998-beda-6b4eb2948a0c") Then
            
'msg("TEST TEST")
'Exit Function
   
	Call User(12)
'   
	CreatTask("Activity_0cdwiv3",AssingTo,AssingToUserName )'"480d7bd3-fcfd-4e25-98e6-d77ce809d38f","سازمان تامین اجتماعی")
	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist

	Dim ss as Integer =0
    For ss=0 to lstOwner.count-1
    
	    Sms(lstOwner(ss).OwnerMobile,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
	Next
	Sms(Info8.GetRequest.Info.Cellphone,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
   


ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d62c1b5c-b34f-478c-b0f5-cae273920bc9") Then
	User(12)
	CreatTask("Activity_0zn42hd",AssingTo,AssingToUserName )'"480d7bd3-fcfd-4e25-98e6-d77ce809d38f","سازمان تامین اجتماعی")
	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist

	Dim ss as Integer =0
    For ss=0 to lstOwner.count-1
    	if String.IsNullOrEmpty(lstOwner(ss).OwnerMobile) Then
		ElseIf lstOwner(ss).OwnerMobile.Length =11 Then
	    	Sms(lstOwner(ss).OwnerMobile,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
	    End if 	
	Next
	Sms(Info8.GetRequest.Info.Cellphone,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
   


ElseIf ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d7a55d10-25d8-4e15-8a02-a664d89f8666") Then
	User(12)
	if AssingTo="" Then 
		Info8.GetOnTickChecklistParameter().Result="false"
		Info8.GetOnTickChecklistParameter().Description="ملک با لایه محدوده شعب تامین اجتماعی هم پوشانی ندارد "
		Exit Function
	End if 	
	CreatTask("Activity_1ix59rv",AssingTo,AssingToUserName )',"480d7bd3-fcfd-4e25-98e6-d77ce809d38f","سازمان تامین اجتماعی")				

	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist

	Dim ss as Integer =0
    For ss=0 to lstOwner.count-1
    	if String.IsNullOrEmpty(lstOwner(ss).OwnerMobile) Then
		ElseIf lstOwner(ss).OwnerMobile.Length =11 Then
	    	Sms(lstOwner(ss).OwnerMobile,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
	    End if	
	Next
	Sms(Info8.GetRequest.Info.Cellphone,"مالک"," شهروند گرامی :اکتور محاسبات شما طی نامه شماره " & Info8.GetRequest.Info.NidWorkItem & " برای سازمان تامین اجماعی شعبه  " & AssingToUserName & " ارسال گردید. سازمان تامین اجتماعی حداکثر 24 ساعت نسبت به محاسبه حق بیمه کارگران ساختمانی اقدام می نماید. پیگیری لازم از شعبه مربوطه صورت پذیرد. چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد")
   


End if 

'sp 434430
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("90fe1013-a816-437e-a1d4-d7eefd8896de") Then
	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
    Dim ss as Integer =0
    For ss=0 to lstOwner.count-1
	    Sms(lstOwner(ss).OwnerMobile,"مالک"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " به واحد امور مالی منطقه ارجاع شده است لطفا جهت پیگیری به منطقه / واحد امور مالی مراجعه فرمایید . چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد") 
    Next
	Sms(Info8.GetRequest.Info.Cellphone,"مالک"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " به واحد امور مالی منطقه ارجاع شده است لطفا جهت پیگیری به منطقه / واحد امور مالی مراجعه فرمایید . چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد") 

End if 
'sp 379897

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("94f3d28b-899e-49c0-8b9f-c391db4650e1") Then
	
	Dim CountFiche as Integer 
	Dim AllFiche as  list(of Dtosc.Income_Fiche)
	AllFiche=Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
	AllFiche=AllFiche.where(Function (f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) ).tolist
	CountFiche= AllFiche.count
	
	if CountFiche>0 Then
	Else	
		Info8.GetOnTickChecklistParameter().Result="false"
		Info8.GetOnTickChecklistParameter().Description="فیش تایید شده ندارد"
		Exit Function
	End if	
End if 
' sp 423328 , sp 424805
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("74aa741d-4b2b-47b9-a0ce-8f4fe00aefbe") _
or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2dc21df9-3469-4bd1-b378-2d666361313e") _ 
or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("cdd5f603-1259-4165-a06f-109432a8b5eb") _ 
or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("01a74d51-5d80-41a6-9ed9-7fd0c961fe8e") _ 
or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8e65be69-aebc-4eaf-993f-60e75aa580e8") _ 
or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f0c22c29-f230-46b3-bc94-256d8017608f") Then 

	Call CreatRequestAmlak()
End if 
'sp 418759
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("74aa5d85-b148-41a1-bd34-5fb2cef1b978")Then
	Call User(6) 
	CreatTask("Activity_0m8ll1t",AssingTo,AssingToUserName)
End if 
' sp 406686	
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bca81292-50e5-441b-a2b0-43855067682d")Then
	Call User(7) 
	CreatTask("Activity_0ih9z1h",AssingTo,AssingToUserName)
	Call payamak("املاک"," برای اداره املاک منطقه ارسال شده است لطفا ظرف 24 ساعت آینده به اداره املاک مراجعه فرمایید. ",3)
	Call CreatRequestAmlak()	'sp 475821
End if 	 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("add87936-32c0-4967-84ee-7f9ff1d01497") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d7fe474f-1dd1-48df-9d47-2fd710419208") Then
	   	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
		lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
		if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
		Dim ss as Integer =0
		
	    For ss=0 to lstOwner.count-1
		  Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰"," پرونده شما به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " برای تکمیل مستندات به منطقه عودت گردید.  چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد - شهرداری مشهد" )
		Next
		Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰"," پرونده شما به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " برای تکمیل مستندات به منطقه عودت گردید.  چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد - شهرداری مشهد" )
	
End if 		 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3dd38365-0f7b-40a0-8b4d-230f91e8094d") Then

	'Dim a = Agreement()	
	Dim d as Integer = 0
	d = Agreement()	
' msg(Numm)   
' Exit Function  	
	Dim BaseUNew As New List(Of Global.DtoSC.Base_Using)
	BaseUNew= BaseU.where( Function(f) f.FloorNo<80).tolist

	
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 530 And f.CI_UsingType <> 853 And f.CI_UsingType <> 746 And f.CI_UsingType <> 751 And f.CI_UsingType <> 855 And f.CI_UsingType <> 1002 And f.CI_UsingType <> 1014 And f.CI_UsingType <> 1026).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f16 And f.CI_UsingType <> 2038 And f.CI_UsingType <> 810 And f.CI_UsingType <> 850 And f.CI_UsingType <> 851 And f.CI_UsingType <> 812 And f.CI_UsingType <> 703 And f.CI_UsingType <> 840).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <.CI_UsingType <> 20> 817 And f.CI_UsingType <> 844 And f.CI_UsingType <> 801 And f.CI_UsingType <> 2033 And f.CI_UsingType <> 811 And f.CI_UsingType <> 822 And f.CI_UsingType <> 845 And f.CI_UsingType <> 720).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 820 And f.CI_UsingType <> 838 And f.CI_UsingType <> 832 And f.CI_UsingType <> 831 And f.CI_UsingType <> 998 And f.CI_UsingType <> 846 And f.CI_UsingType <> 2019 And f.CI_UsingType <> 803).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 821 And f.CI_UsingType <> 981 And f.CI_UsingType <> 802 And f.CI_UsingType <> 821 And f.CI_UsingType <> 822 And f.CI_UsingType <> 805 And f.CI_UsingType <> 806 And f.CI_UsingType <> 832 And f.CI_UsingType <> 843).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 111 And f.CI_UsingType <> 833 And f.CI_UsingType <> 431 And f.CI_UsingType <> 2035 And f.CI_UsingType <> 834 And f.CI_UsingType <> 904 And f.CI_UsingType <> 906 And f.CI_UsingType <> 922 And f.CI_UsingType <> 923).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 967 And f.CI_UsingType <> 813 And f.CI_UsingType <> 814 And f.CI_UsingType <> 827 And f.CI_UsingType <> 828 And f.CI_UsingType <> 854 And f.CI_UsingType <> 994 And f.CI_UsingType <> 813 And f.CI_UsingType <> 814).tolist
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 827 And f.CI_UsingType <> 828 And f.CI_UsingType <> 854 And f.CI_UsingType <> 994 And f.CI_UsingType <> 2036 And f.CI_UsingType <> 847 And f.CI_UsingType <> 933 And f.CI_UsingType <> 753 And f.CI_UsingType <> 999).tolist 
'sp384374	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2024 And f.CI_UsingType <> 830 And f.CI_UsingType <> 1013 And f.CI_UsingType <> 829 And f.CI_UsingType <> 930).tolist
	
	  'sp384374             
	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2054 And f.CI_UsingType <> 438 And f.CI_UsingType <> 816 And f.CI_UsingType <> 751 And f.CI_UsingType <> 418 And f.CI_UsingType <> 806 And f.CI_UsingType <> 838 And f.CI_UsingType <> 827).tolist
	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 827 And f.CI_UsingType <> 923 ).tolist
	if BaseUNew.count=0 Then 
		Info8.GetOnTickChecklistParameter().Result="false"
		Info8.GetOnTickChecklistParameter().Description=" ترتیب محاسبات رعایت نشده - توافق چیدمان ندارد "
		Exit Function
	End if	

	Dim UsingArea as Double= BaseUNew.sum( Function (f) f.UsingArea)
	
	Dim FloorNoNew as Integer = BaseUNew.Max( Function (f) f.FloorNo)  +1 '- BaseUNew.Min( Function (f) f.FloorNo)
'	Info8.GetOnTickChecklistParameter().Result="false"
	'	Info8.GetOnTickChecklistParameter().Description=UsingArea
	'	Exit Function

'sp 486354 	 	if UsingArea>1999 Then 	'sp 377181 if UsingArea>2999 or FloorNoNew>7 Then 
 
'sp 486354 	 		if ControlTask("Activity_0frpo8z")=0 or ControlTask("Activity_0frpo8z")=2  Then 
 		'sp 427737	
'sp 486354 	 			Call user(10)
	 		'CreatTask("Activity_0frpo8z","1e8df752-a4b8-42a9-8b39-a13562d0c761","کمیته انطباق شهری(شهرساز)")  
'sp 486354 		 		CreatTask("Activity_0frpo8z",AssingTo,AssingToUserName)
	 	'	Info8.GetOnTickChecklistParameter().Result="false"
		'	Info8.GetOnTickChecklistParameter().Description="D"
'sp 486354 		 		Call Payamak("نظام مهندسی - مهندس شهرساز"," مالک محترم جهت تعیین مهندس شهرساز ظرف مدت 24 ساعت به سازمان نظام مهندسی ساختمان مراجعه نمایید")
'sp 486354 		 	End if		
'sp 486354 		End if 		  	
		    
End if 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("39504dea-f6f8-40c2-805d-25c0da57a558") Then 
'sp 369274
'sp 377181 new 
		
	'Dim a = Agreement()
	Dim c as Integer = 0
	c = Agreement()
		
	'	msg(WhatisNidBase)
	'	Exit Function  	
		
	Dim BaseUNew As New List(Of Global.DtoSC.Base_Using)	
	BaseUNew= BaseU.where( Function(f) f.FloorNo<80).tolist
	Dim FloorNoNew as Integer = BaseUNew.Max( Function (f) f.FloorNo) - BaseUNew.Min( Function (f) f.FloorNo) +1	'


	
	 'sp384374             
	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2054 And f.CI_UsingType <> 438 And f.CI_UsingType <> 816 And f.CI_UsingType <> 751 And f.CI_UsingType <> 418 And f.CI_UsingType <> 806 And f.CI_UsingType <> 838 And f.CI_UsingType <> 827).tolist
	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 827 And f.CI_UsingType <> 923 ).tolist

'sp384374 
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 530 And f.CI_UsingType <> 853 And f.CI_UsingType <> 746 And f.CI_UsingType <> 751 And f.CI_UsingType <> 855 And f.CI_UsingType <> 1002 And f.CI_UsingType <> 1014 And f.CI_UsingType <> 1026).tolist
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2016 And f.CI_UsingType <> 2038 And f.CI_UsingType <> 810 And f.CI_UsingType <> 850 And f.CI_UsingType <> 851 And f.CI_UsingType <> 812 And f.CI_UsingType <> 703 And f.CI_UsingType <> 840).tolist
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 817 And f.CI_UsingType <> 844 And f.CI_UsingType <> 801 And f.CI_UsingType <> 2033 And f.CI_UsingType <> 811 And f.CI_UsingType <> 822 And f.CI_UsingType <> 845 And f.CI_UsingType <> 720).tolist
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 820 And f.CI_UsingType <> 838 And f.CI_UsingType <> 832 And f.CI_UsingType <> 831 And f.CI_UsingType <> 998 And f.CI_UsingType <> 846 And f.CI_UsingType <> 2019 And f.CI_UsingType <> 803).tolist
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 821 And f.CI_UsingType <> 981 And f.CI_UsingType <> 802 And f.CI_UsingType <> 821 And f.CI_UsingType <> 822 And f.CI_UsingType <> 805 And f.CI_UsingType <> 806 And f.CI_UsingType <> 832 And f.CI_UsingType <> 843).tolist
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 111 And f.CI_UsingType <> 833 And f.CI_UsingType <> 431 And f.CI_UsingType <> 2035 And f.CI_UsingType <> 834 And f.CI_UsingType <> 904 And f.CI_UsingType <> 906 And f.CI_UsingType <> 922 And f.CI_UsingType <> 923).tolist
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 967 And f.CI_UsingType <> 813 And f.CI_UsingType <> 814 And f.CI_UsingType <> 827 And f.CI_UsingType <> 828 And f.CI_UsingType <> 854 And f.CI_UsingType <> 994 And f.CI_UsingType <> 813 And f.CI_UsingType <> 814).tolist
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 827 And f.CI_UsingType <> 828 And f.CI_UsingType <> 854 And f.CI_UsingType <> 994 And f.CI_UsingType <> 2036 And f.CI_UsingType <> 847 And f.CI_UsingType <> 933 And f.CI_UsingType <> 753 And f.CI_UsingType <> 999).tolist 
'sp384374 	BaseUNew= BaseUNew.where( Function(f) f.CI_UsingType <> 2024 And f.CI_UsingType <> 830 And f.CI_UsingType <> 1013 And f.CI_UsingType <> 829).tolist
	
	if BaseUNew.count=0 Then 
		Info8.GetOnTickChecklistParameter().Result="false"
		Info8.GetOnTickChecklistParameter().Description="ترتیب محاسبات رعایت نشده"
		Exit Function
	End if	
	
	Dim UsingArea as Double= BaseUNew.sum( Function (f) f.UsingArea)
	
					
	
	Dim Nidb = Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000"))
        Try
        	Info8.ParameterList.Remove("Parvandeh_Building_NidBase")
        	Info8.ParameterList.Add("Parvandeh_Building_NidBase", Nidb.ToString)
        Catch
        End Try	
        
	
    Dim BaseUParvandeh As New List(Of Global.DtoSC.Base_Using) 
    BaseUParvandeh=  Info8.GetParvandeh_Building.GetBase_Using 
    
        		
    BaseUParvandeh=BaseUParvandeh.where( Function(f) f.FloorNo<80).tolist 
    
   
	
    Dim FloorNoParvandeh as Integer=0
    
    IF BaseUParvandeh.COUNT>0 Then FloorNoParvandeh= BaseUParvandeh.Max( Function (f) f.FloorNo) - BaseUParvandeh.Min( Function (f) f.FloorNo) +1'
	
'sp 442228 
'sp 377181 if UsingArea>2999 or FloorNoNew>7 Then 
'sp 486354 		 if UsingArea>1999 And FloorNoNew-FloorNoParvandeh> 1 Then 
'sp 486354 		if ControlTask("Activity_1b94luq")=0 or ControlTask("Activity_1b94luq")=2  Then 
	 		'sp 427737
'sp 486354 		 		 Call user(10)
'sp 486354 		 		 CreatTask("Activity_1b94luq",AssingTo,AssingToUserName)
	 		'CreatTask("Activity_1b94luq","8d20be44-78ab-4001-bb94-f064d22b9ce1","نظام مهندسی - مهندس شهرساز")  
	 	
'sp 486354 			 	Call Payamak("نظام مهندسی - مهندس شهرساز"," مالک محترم جهت تعیین مهندس شهرساز ظرف مدت 24 ساعت به سازمان نظام مهندسی ساختمان مراجعه نمایید")
'sp 486354 			 End if	
'sp 486354 		End if 			  	
End if 


	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("508c03fe-5a0d-4957-98b9-da003885df40") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c83c3710-154f-427d-a2dd-1c694a473081") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f122af0a-e4cf-4e9e-b170-60c08c437067") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f2ed3b31-fa22-4f2f-a080-2138729f2da4") Then
	
		Dim LstUser() as String
		LstUser={"ed17d70d-ca7b-4699-a4ec-3be0222e2090"}
		
	'	if LstUserGroupId( LstUser) <> 1 Then 
	'		Info8.GetOnTickChecklistParameter().Result="false"
	'		Info8.GetOnTickChecklistParameter().Description=" فقط گروه مشاور طرح تفصیلی جنوب غرب دسترسی تایید دارد " 						
	'	End if 

	End if 

					
 	        ''''''''احراز هویت مالک
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ca2bba59-a69b-4285-9472-318200615290") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("29265586-4b87-4f6c-bcd2-ba97f28af632") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("29c26a91-4909-49ee-a024-158c020250d4") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("31941212-8c34-4e09-bdd1-717ac643e82f") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("123bd225-1646-4efd-b43a-084591e29e63") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4619c940-ab88-4bd9-bba2-9d386a8b4a83") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5d4447fe-5b42-41b8-9b09-dbc91c9af114") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("004dd219-a352-4694-a334-5cb22c117774") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("49fd7080-6439-47f4-8cc4-2a5763197b78") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3247024c-70b2-4268-aef4-4b326b63d4d5") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fcbaf72c-c120-438a-8339-a8e35607e4e4") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2f6ff027-3a65-4e14-97e8-a50af0921360")	_
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2f29a6d4-da6d-40b6-a69e-3fd57adaaff0") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("effc6adf-527a-4f6a-8fc6-835ef3c56a85") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8069c7d2-a597-4778-a2d3-8628b2b593fa") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e79e2300-b10a-4900-b53f-96cdb5d031d5") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("dd812862-87a8-4a76-af80-317a10d1d985") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f9c770ea-c977-4878-b5c4-02a3f063748d") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9e84b9a7-e405-42ad-8689-411b15f8c425") _
	Then
	
		BIZ.SC.ClsCommon.GIsCheckNationalCode = "false"
		Info8.GetRequest.Info.IsConfirmAuthentication =1  
		Info8.GetRequest.Save() 
	End if 	


' استعلام افراز
		
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9e84b9a7-e405-42ad-8689-411b15f8c425") Then SaveCheck ("173d3fa8-74a9-4381-be04-c45d4e6a3bf7", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("00c00e4e-9023-4977-8c3b-3eea03362912") Then SaveCheck ("c49828f0-f462-46a8-a7ca-5e9d80800984", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("04940b3c-2725-4a4c-8af2-0dadb778e7b3") Then SaveCheck ("920bbf64-2822-426a-8943-d98113351806", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8312a9bc-3904-49dc-9e8a-922825e33300") Then SaveCheck ("07f2d2d7-a46a-47c0-8388-94607a0f6700", 1,0)

		
'مفاصا

	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1eafa87b-de91-4b49-ac25-82b8812ca46c") Then SaveCheck ("9974282b-0c5f-4c7a-a0c0-d77045366b6e", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("47047f4f-fa07-4703-ad6a-ce69eae87eda") Then SaveCheck ("abb359b5-968a-4a93-a2e1-644d18baba2c", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9d02e09a-6c67-48b8-b6c0-1dbccb8dbe09") Then SaveCheck ("9a93c7ba-3686-4c51-bcf2-f43c587a01f1", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e79e2300-b10a-4900-b53f-96cdb5d031d5") Then SaveCheck ("dd812862-87a8-4a76-af80-317a10d1d985", 1,0)


	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4619c940-ab88-4bd9-bba2-9d386a8b4a83") Then SaveCheck ("123bd225-1646-4efd-b43a-084591e29e63", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("31941212-8c34-4e09-bdd1-717ac643e82f") Then SaveCheck ("29c26a91-4909-49ee-a024-158c020250d4", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("49fd7080-6439-47f4-8cc4-2a5763197b78") Then SaveCheck ("004dd219-a352-4694-a334-5cb22c117774", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fcbaf72c-c120-438a-8339-a8e35607e4e4") Then SaveCheck ("5d4447fe-5b42-41b8-9b09-dbc91c9af114", 1,0)

'پایانکار

	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b520f0be-0872-491b-aa5f-ac7c0aa2c945") Then SaveCheck ("5149b668-fd65-4ab9-9c5a-1afd389b4ed4", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bfa754b5-13d9-4f4b-a703-e317894eba97") Then SaveCheck ("2f29a6d4-da6d-40b6-a69e-3fd57adaaff0", 1,0): SaveCheck ("b83c895e-79ea-4960-a71e-d335b14bb629", 2,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ce5f32fa-13a6-4d09-b9ea-b3baa5680084") Then SaveCheck ("cc4c0b5c-8041-4c88-bcc1-488a4941106a", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ea93af6c-645a-4609-b7b0-a7fcd88bd000") Then SaveCheck ("5d7ada12-32ec-48a4-b72d-360f87f057b3", 1,0)
	' عدم خلاف
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8069c7d2-a597-4778-a2d3-8628b2b593fa") Then SaveCheck ("9f87638f-8692-4671-a263-0ece2541be45", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5418dd3a-7bf8-44d8-9fba-0388dd5bf7b3") Then SaveCheck ("0ff5a997-f3ef-4a5f-87d2-dff63b422e26", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("156f6cf8-a2ad-4e3e-86b2-adaf20e8cda1") Then SaveCheck ("55b39130-41f9-4d80-904e-62cd42fc5df3", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b4bcc0da-fc48-431b-b2f5-e8c52ea851a5") Then SaveCheck ("167c1bd2-803d-4d2f-85c7-45a36ecab4d2", 1,0)
'نقشه برداری پروانه

	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("293bc52c-7f3d-4091-be76-408ae66b3b49") Then 
	'	if Check("839f8728-756f-4db1-acfe-a5a9c25cfc3d",0)="false" Then 
	'		Info8.GetOnTickChecklistParameter().Result="false"
	'		Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق حدود و وقوع ملک با اسناد در محل '  اجازه تایید ندارید" 		
	'	Else
		If Check("858bf483-6bf7-47fe-b5e0-f1e39c8c619c",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق مالکیت و پلاک ثبتی '  اجازه تایید ندارید" 
		ElseIf Check("8797e441-16ba-4622-8b76-e7cf180ea893",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی '  اجازه تایید ندارید" 		
		ElseIf Check("fae4ba0f-e639-4d88-95e3-a3133e3c674d",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' پرداخت فاکتور نقشه برداری توسط مالک '  اجازه تایید ندارید" 						
		End if 	
		 
	End if
	
   	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4d21352a-886d-4682-9c45-21a603bc1dc2") Then 
		if Check("4d21352a-886d-4682-9c45-21a603bc1dc2",1)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'توسط مالک '  اجازه تایید ندارید" 		
		End if	
	End if

	'نقشه برداری  محضر 
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3b0d1908-e51d-4430-b40c-78ad0af77fa9") Then 
		if Check("c688b096-a0a1-4be4-8e96-28e017c707bf",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' پرداخت فاکتور نقشه برداری توسط مالک '  اجازه تایید ندارید" 		
		ElseIf Check("ad174b00-5c7e-4b77-8fe7-cee44e1a1d0b",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی '  اجازه تایید ندارید" 
		ElseIf Check("7f494f57-64db-4295-8d2e-45f15e6f7493",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق مالکیت و پلاک ثبتی '  اجازه تایید ندارید" 		
	'	ElseIf Check("1cbe4464-aa96-4def-bcc9-1a6caaa37ed2",0)="false" Then 
	'		Info8.GetOnTickChecklistParameter().Result="false"
	'		Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق حدود و وقوع ملک با اسناد در محل '  اجازه تایید ندارید" 						
		End if 	
		 
	End if
	
	

		'نقشه برداری تجدید اجاره 
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0d0c0a15-43a2-475d-9c03-f884246a481e") Then 
		if Check("300718ee-70af-41f8-ab2e-2a9e426e3e01",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' پرداخت فاکتور نقشه برداری توسط مالک '  اجازه تایید ندارید" 		
		ElseIf Check("f3e1ccb4-aae0-414e-b4ca-d49258825048",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی '  اجازه تایید ندارید" 
		ElseIf Check("c4c029bf-15e4-40f1-9f01-a8caca7b99de",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق مالکیت و پلاک ثبتی '  اجازه تایید ندارید" 		
	'	ElseIf Check("24b4dfda-ed6b-418e-88a5-57cbc1f671c8",0)="false" Then 
	'		Info8.GetOnTickChecklistParameter().Result="false"
	'		Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' انطباق حدود و وقوع ملک با اسناد در محل '  اجازه تایید ندارید" 						
		End if 	
		 
	End if
	

'----------------------------	
	'بازدید   پروانه
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4660b4ae-93d6-495b-bc8c-89bd044afad5") Then 
	
		if Check("2b593f15-91ff-4019-b146-a6f6c63d35d4",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' نقص مدارک '  اجازه تایید ندارید" 		
		ElseIf Check("418200fb-1345-42c5-a42f-fd27d2452ee5",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' کنترل گزارش آخرین سابقه مجاز ملک '  اجازه تایید ندارید" 
		ElseIf Check("c0f7c342-943c-4071-bb47-eb1e934f161d",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' کنترل اطلاعات بر وکف '  اجازه تایید ندارید" 		
		ElseIf Check("41d4dc0a-de6d-4582-8dac-4992e3b093d4",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' کنترل فرم فضای سبز '  اجازه تایید ندارید" 
		ElseIf Check("ecdfca29-104b-4d86-a0fb-8510c52cc48c",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' کنترل شمولیت کمیسیون فنی '  اجازه تایید ندارید" 						
		ElseIf Check("548968b4-69f2-41c3-ae08-463f8c77f56c",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' کنترل شمولیت کمیسیون ماده 5 '  اجازه تایید ندارید" 						
		ElseIf Check("a46b0cdf-16f9-4531-9101-d4115b5f899b",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' کنترل شمولیت حرائم '  اجازه تایید ندارید" 						
    	ElseIf Check("9c438791-cd65-4dea-9c2d-313e132e2d7f",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' کنترل اعلام ضابطه پایه توسط ناظر '  اجازه تایید ندارید" 						
	   ElseIf Check("7c67b034-7988-416b-95a3-3b4c4fff52e1",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی ' کنترل شمولیت  تایید مهندس شهرساز برای ساختمان های گروه (ج-د) (در طرح تفصیلی جدید) '  اجازه تایید ندارید" 						
		ElseIf Check("648cdc88-e86f-4b0e-9736-6cb28d03867b",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'شمولیت صلح تبصره 4 '  اجازه تایید ندارید" 		
		ElseIf Check("45fbb532-46d0-4756-93ad-9a67160a9c71",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'شمولیت  صلح ماده 101 '  اجازه تایید ندارید" 		
		ElseIf Check("a4874fa9-7795-4fdf-ac4d-8eb39e76e5cf",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'ارسال به املاک جهت میزان در مسیر '  اجازه تایید ندارید" 		
		ElseIf Check("2096a7db-b908-4047-aafa-0500cd989390",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'کنترل صلنامه توسط ناظر '  اجازه تایید ندارید" 		
		ElseIf Check("69f9d5eb-8847-48f2-8b54-e975e127c938",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'قابل طرح در کمیته فنی در صورت نیاز '  اجازه تایید ندارید" 		
		ElseIf Check("8a859ccc-084f-43d1-be15-b081bbfd24d1",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'ثبت درخواست هیئت پارکینگ '  اجازه تایید ندارید" 		
		ElseIf Check("69495a8a-ca23-4702-b324-52fb59415bdd",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'کنترل فراخوانی نتایج هیئت پارکینگ '  اجازه تایید ندارید" 		
		
		End if 	
		 
	End if
	'تایید سابقه مجاز پروانه
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("09e4d173-e656-4dba-bd19-df13e2f6279e") Then 
		if ControlTask("Activity_14wxvho")<>1 Then CreatTask("Activity_14wxvho","EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57","Esup") 
	End if
	
	'تایید سابقه مجاز پایانکار
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7493ddbc-3a7a-4446-9831-8f3fbd192b71") Then 
		if ControlTask("Activity_0l172de")<>1 Then CreatTask("Activity_0l172de","EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57","Esup") 
	End if
	'  ارسال به املاک پروانه احداث
		if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("98535FE3-D2CC-48D4-94B7-17927E71E932") Then
		
           	
'		    Dim tmpF as New Global.BIZ.SC.ClsFunction 
' 
'			Dim RequestTyp As New List(Of Global.Biz.SC.SrvEstate.CI_RequestType)
'			Dim CiRequetTyp As New Global.Biz.SC.SrvEstate.CI_RequestType
'	        CiRequetTyp.ID = 71
'	        CiRequetTyp.Title =  "ملک در مسير"
'	        CiRequetTyp.IsActive = True
'	        CiRequetTyp.GUID = Guid.Parse("47987540-8ACF-4681-BC8A-7EED93788371")
'	
'	        RequestTyp.Add(CiRequetTyp)
'            Dim Info As New Biz.SC.SrvEstate.ClsRequest_Info()
'            '	'		Dim tmpNickName() as String
''	'		Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
'''	'	'	tmpNickName = tmpNosaziNickName.Split("-")	
'	        Info.District = 1'tmpNickName(0)
	'        Info.Region =2 'tmpNickName(1)
	''        Info.Block = 3 'tmpNickName(2)
	'        Info.House = 4'tmpNickName(3)
	'        Info.Building = 1'tmpNickName(4)
	'        Info.Apartment = 0'tmpNickName(5)
    '    	Info.Shop = 0'tmpNickName(6)
    '    	Info.UserInfo_GUID = Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57")
    '    	Info.UserName = "Esup"	       
    '        Info.CI_Region = 1	
     '        Info.CI_RequestType = RequestTyp 
    '		 Info.CI_RequestType=71
    '        Info._IsNew = True
    '        Info.IsBindingToSara = False
    '        Info.IsBuildingRequest = False
    '        Info.IsDuplicate = False
    '        Info.IsNotExistCodeFromSara = False
     '       Info.IsOtherKartable = False
     '       Info.IsRequestSys = True 
      '      Info.NidWorkflowDeff =  Guid.Parse("47987540-8ACF-4681-BC8A-7EED93788371")   
      '      Info.RequesterName = "ملک در مسير"
      '      tmpF.SaveRequestEstate(info)
			
	End if
		'sp 382377 

	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c9cca969-7c60-40a0-9dea-6ea4dcb059ee") Then
		
	'sp 385731	if ControlTask("Activity_06rgkdq")=0 Then
	'sp 385731		Call User(7) 
	'sp 385731		CreatTask("Activity_06rgkdq",AssingTo,AssingToUserName)
	'sp 385731		Call CreatRequestAmlaksara()
	'sp 385731	End if 
		'sp 387066'sp391021
	
	'---sajedi 1403-05 --- sp462315		
	'	if ControlTask("Activity_06rgkdq")=0 or ControlTask("Activity_06rgkdq")=2  Then
	'		Call User(7) 
	'		CreatTask("Activity_06rgkdq",AssingTo,AssingToUserName)
	'	End if 	
	'----sajedi
'	if Info8.GetBarokaf.GetSh_BaroKaf.PathValue<>0  or Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed<>0  Then  
	 Call CreatRequestAmlak()
	
'		messagecontract("میزان در مسیر ندارد")	
 		
			
	End if 
		'  ارسال به املاک پروانه توسعه
		'sp 382377 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5de5447a-f505-42d0-9256-89206eee1f8f") Then
	'sp 385731	if ControlTask("Activity_0rneuq9")=0 Then 
	'sp 385731		Call User(7) 
	'sp 385731		CreatTask("Activity_0rneuq9",AssingTo,AssingToUserName)
	'sp 385731		
	'sp 385731		Call CreatRequestAmlak()
	'sp 385731	End if
	'sp 387066
	'sp391021
	
	'---sajedi 1403-05 --- sp462315		
	'	if ControlTask("Activity_0rneuq9")=0 or ControlTask("Activity_0rneuq9")=2 Then
	'		Call User(7) 
	'		CreatTask("Activity_0rneuq9",AssingTo,AssingToUserName)
	'	End if 	
	'---sajedi 1403-05	
	'if Info8.GetBarokaf.GetSh_BaroKaf.PathValue<>0  or Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed<>0  Then  
	 Call CreatRequestAmlak()
	 
	End if 
	'ارسال به املاک 
	'sp 418046 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ec849ab8-0c23-4156-9125-dfd3a1f96df5") or _
	ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7bc001d9-dddb-4329-82c0-bd6e2eb5bc6d") or _
	ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6b77d75d-2c3c-42cf-8748-b6fda75437d1") Then
		Call CreatRequestAmlak()
	End if 
	'  ارسال به هیت پارکینگ پروانه توسعه
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b64a827e-c34f-48ec-a21b-d1747d8c0260") Then
		if ControlTask("Activity_1audz0x")<>1 Then CreatTask("Activity_1audz0x","EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57","Esup") 
	End if 
	'  ارسال به هیت پارکینگ پروانه احداث
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("94174184-2e48-40c8-9c2a-53b4071f6432") Then
		if ControlTask("Activity_00gk1m1")<>1 Then CreatTask("Activity_00gk1m1","EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57","Esup") 
	End if 
	' اچیدمان پروانه توسعه
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6721c323-3a7b-46d8-9e68-2db80b1fe28b") Then
		if Check("39504dea-f6f8-40c2-805d-25c0da57a558",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'کنترل اعلام ضابطه پایه و مجاز و محاسبات تحلیل توسط مالک '  اجازه تایید ندارید" 		
		End if 	
	End if 
	
		
	'  اچیدمان پروانه احداث
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7666b59f-54a4-465e-879f-2c6672f01332") Then
		if Check("3dd38365-0f7b-40a0-8b4d-230f91e8094d",0)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'کنترل اعلام ضابطه پایه و مجاز و محاسبات تحلیل توسط مالک '  اجازه تایید ندارید" 		
		End if 	
	End if

'----sajedi 1403-05 ---	sp465026
	' دادگستری پروانه توسعه
'	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("53146900-577e-427b-8293-66108a85821d") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6d087e35-e099-45e4-a902-1879fa5cff5c") Then
'		if ControlTask("Activity_124rh50")<>1 Then Call User(6) : CreatTask("Activity_124rh50",AssingTo,AssingToUserName)  
'	End if 
	'  دادگستری پروانه احداث
'	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("826785de-5bb8-4ac3-8317-a124fdbb7c4c") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("364b2a79-16b4-4839-9190-e786beab39fc") Then
'		if ControlTask("Activity_0bv12j6")<>1 Then Call User(6) : CreatTask("Activity_0bv12j6",AssingTo,AssingToUserName)  
'	End if  
'---------sajedi
 	
'sp 360311 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3247024c-70b2-4268-aef4-4b326b63d4d5") Then SaveCheck ("2f6ff027-3a65-4e14-97e8-a50af0921360", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("dc2ce77a-85f9-44fe-941f-1d53a0a6fab6") Then SaveCheck ("e18f4b0b-79b4-42b6-9d53-6ebe79d31143", 1,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("45d4741d-af70-4d40-828e-0b16ff04b8b0") Then SaveCheck ("cd79b990-ba7a-437b-9b2e-b6ab381ad516", 1,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1565f4e3-9513-4a12-8005-b522d58f1c70") Then SaveCheck ("cab9b25c-284a-413d-bdd8-e402fc30a7f6", 1,0) 
	
'
'sp 376634
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("17ebabbf-cc5a-491a-a2d4-ad69e7c91cf9") Then SaveCheck ("1dd4db82-030d-4cb5-9b5d-e1f148eea040",2,0) 
'بلامانع بودن صدور باعث فعال شدن صدور در اقدامات می باشد 
'تایید مجود و مجاز 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("38224427-48FF-4F10-A836-899D2484BCAB") Then SaveCheck ("b30aee10-8b36-4429-b182-2f4cae78131c",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("933fc7ee-9853-4b47-977d-dc6ca25991a0") Then SaveCheck ("b30aee10-8b36-4429-b182-2f4cae78131c",2,0) 
	 '  کنترل الویت چک لیست برای پایانکار بهره برداری
	 if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ec849ab8-0c23-4156-9125-dfd3a1f96df5") OR _
	    ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("140abb01-ab4a-4d73-b9e8-48741fada617") Then 
	 	if Check("2d27c0b6-a926-42fe-a4ec-1f526e1b3c41",0)="false" Then
	 		Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'کنترل اطلاعات بر وکف '  اجازه تایید ندارید" 		
		End IF
	 	IF  Check("479400cf-1065-44ab-91db-7062bbb9cf6a",0)="false"  Then 
	 		Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'کنترل کمیسیون ماده 5 - کمیسیون فنی '  اجازه تایید ندارید" 		
		End IF
	 End IF  
	 if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("140abb01-ab4a-4d73-b9e8-48741fada617") Then 
	 	IF  Check("e3539f3d-c3cb-4ad9-925d-ab9cf38e5ee3",0)="false"  Then 
	 		Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'کنترل  حرائم '  اجازه تایید ندارید" 		
		End IF
	 End IF
	 'sp 411428  Call User(1)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e7c8044e-ad32-47fa-8a16-46b2dc4cf159") Then
		if ControlTask("Activity_1swyraw")<>1 Then Call User(6) : CreatTask("Activity_1swyraw",AssingTo,AssingToUserName)  
	End if   
	
	 
	 if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f4211155-b93d-401a-97b5-332e75c13e17") Then
		if ControlTask("Activity_1jyc96v")<>1 Then Call User(1) : CreatTask("Activity_1jyc96v",AssingTo,AssingToUserName)  
	End if   
	
if ucase(Info8.GetOnTickChecklistParameter().NidNode) = ucase("Activity_0vcpof4") Then 
	' If Check("6fc3ddff-d479-41b4-99d0-367b0fe8e723",0)="false" Then 
	  if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6fc3ddff-d479-41b4-99d0-367b0fe8e723") Then
		If Check("6fc3ddff-d479-41b4-99d0-367b0fe8e723",1)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'تایید بازدید توسط مالک '  اجازه تایید ندارید" 		
		End if 	
	End if 	
End if 	

if ucase(Info8.GetOnTickChecklistParameter().NidNode) = ucase("Activity_13z3e1z") Then 
	  if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("471200b5-564b-42e5-96ae-f687bdf91e51") Then
		If Check("471200b5-564b-42e5-96ae-f687bdf91e51",1)="false" Then 
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description=" به علت عدم پاخگویی و یا جواب منفی 'تایید بازدید توسط مالک '  اجازه تایید ندارید" 		
		End if 	
	End if 	
End if 	
'sp 430172 
'sp 434816
   ' If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f0afb2e6-ea03-423f-a94e-4261ceaa198a") Then 
'		if ControlTask("Activity_1brcze0")=0 or ControlTask("Activity_1brcze0")=2  Then User(2) : CreatTask("Activity_1brcze0",AssingTo,AssingToUserName) 
'	End if 

	
	
	End Function

	'---------------\\\   عدم تایید   ////----------------
Public Function UnConfirm  () ' تایید	عدم
 'sp 509207
IF {ucase("fc659687-564a-4681-a17c-4bf247d762b4"),ucase("379d0f82-1af3-41e4-84cd-5f18f645406f")}.contains( _
	ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)) Then  
		
	Dim M_CommisionPenalty as New List(Of DtoSC.Commission_FinePenalty)
	Dim cc as DtoSC.Commission_FinePenalty
	cc = New DtoSC.Commission_FinePenalty
	cc.Area = 0
	cc.TrespassDateInMunicipality = BIZ.SA.ClsCommon.CurrentShamsiDateString
	cc.CI_CommissionFinePenalty = 0
	cc.CI_CommissionUrbanIdea = 0
	cc.CI_UsingGroup = 0
	cc.CI_UsingType = 0
	cc.EconomicPrice = 0	
	cc.FramePrice=0
	cc.FloorNo = 0
	cc.MinPrice = 0
	cc.IncomeArea = 0
	cc.MaxPrice = 0	
	cc.HappenDate = BIZ.SA.ClsCommon.CurrentShamsiDateString
	cc.Comments = Info8.GetOnTickChecklistParameter().Comment
	  
	M_CommisionPenalty.Add(cc)	 	     		 
	Info8.GetCommissionFineParameter_Calc.Commission_FinePenalty = M_CommisionPenalty
 'end sp 509207
End if


If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1d6d8544-78ef-460e-bf8c-3343b67451e6") Then 
'sp 524255
		Dim CountFiche1 as Integer 
		Dim AllFiche1 as  list(of Dtosc.Income_Fiche)
		AllFiche1=Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
		AllFiche1=AllFiche1.where(Function (f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) ).tolist '3 Taeed, 5 TaeedBank
		CountFiche1= AllFiche1.count
			if CountFiche1<=0 Then 
				Info8.GetOnTickChecklistParameter().Result="false" 
			 	Info8.GetOnTickChecklistParameter().Description=" فیش تایید شده درآمدی ندارین 1 - sp 524255" 							
		 		Exit Function
		End if 	
'end sp 524255	
End if


IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("cc058097-003f-4bae-9bbb-f3d4379c313d") Then  'sp 513980
	Call EbtallAndPyamak("چک لیست","قابلیت طرح در هیات فنی را دارد")
End if


IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bdb2cb64-1b4d-4b4d-9b42-3a2f338b1c19") Then  'sp 512373
	if String.IsNullOrEmpty(Info8.GetOnTickChecklistParameter().Comment) Then msg("درج توضیحات الزامی میباشد") : Exit Function
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("49649914-00b3-476e-aed6-2e1dc854f281") Then  'sp 512373
	if String.IsNullOrEmpty(Info8.GetOnTickChecklistParameter().Comment) Then msg("درج توضیحات الزامی میباشد") : Exit Function
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fe68d68a-8f0c-4ea9-9bb6-d079bf601b25") Then  'sp 512373
	if String.IsNullOrEmpty(Info8.GetOnTickChecklistParameter().Comment) Then msg("درج توضیحات الزامی میباشد") : Exit Function
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bdb2cb64-1b4d-4b4d-9b42-3a2f338b1c19") Then
	EbtallAndPyamak("ابطال","عدم تایید قابلیت طرح در هیات فنی دارد ")
End if	

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("46b34436-33bf-4269-b125-cb736b9354d7") Then
	EbtallAndPyamak("ابطال","عدم انطباق محل (آدرس) با موقعت پلاک در سیستم شهرسازی ")
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("96fbb8df-9ef5-48a7-b956-528218c82509") Then
		AssingTo =""
	 	AssingToUserName=""
	 	user(7)
	 	CreatTask("Activity_0hawxst",AssingTo,AssingToUserName)		'تشکیل نود نود سازمان زمین و مسکن/اداره کل املاک 
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2eb9aea2-9e96-4e3d-86b6-96542f76491e") Then
	Call payamak("نقشه برداری"," انطباق حدود و وقوع ملک با اسناد در محل ",1)
	Exit Function
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f09a82cd-b65f-4646-a2d2-4894d7fa9b15") Then
 	Call User(2) 
 	CreatTask("Activity_05uhwi2",AssingTo,AssingToUserName)   ' نقص نقشه برداری 
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("36411494-d782-4901-830c-e16f4494e073") Then
	Dim User,UserG as String
Try
	Dim TaskByNidProcResponse As New Global.Biz.SC.ClsNewWorkFlow.ClsGetTaskByNidProcResponse
	Dim strClose as String =""

	TaskByNidProcResponse=Info8.GetFunctions.WF_GetTaskByNidProc(Info8.GetOnTickChecklistParameter().NidProc.tostring)
	Dim counter as Integer=0
	Dim Colescounter as Integer=0
	Dim Opencounter as Integer=0
	Dim ik as Integer=0
	For ik = 0 To TaskByNidProcResponse.data.Count - 1
	    If TaskByNidProcResponse.data(ik).NidNode.tostring = "Activity_0mpzy4j" Then 
	    	If TaskByNidProcResponse.data(ik).AssingTo.tostring = "3a16a45d-9cea-40a2-9829-bf6ab91e75e1" Then
	    		User = TaskByNidProcResponse.data(ik).AssingTo.tostring
	    		UserG = TaskByNidProcResponse.data(ik).AssingToUserName.tostring
	    		Exit For
	    	Else
	    		User = TaskByNidProcResponse.data(ik).AssingTo.tostring
	    		UserG = TaskByNidProcResponse.data(ik).AssingToUserName.tostring
	    		Exit For	
	    	End if
	    End if	
	Next
Catch e as exception
	msg("E: " & e.message)		
End Try
	CreatTask("Activity_0mpzy4j",User,UserG)
	
'13 "کانون کارشناسان"  'کانون کارشناسان​ رسمی  ' Activity_03pck4b
			
'14 "مرکز کارشناسان" 'کانون-مرکز کارشناسان  ''Activity_1nbtat1
	
End if			

										'=======================
										' عدم تایید چک لیست 
										'=======================

'IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2ded9566-1f3c-45fd-bc8d-1ecbea6658dd") Then

	'	SaveCheck("762bc141-9e63-4bf1-b593-0e9039d642cc",2,0)  	'sp 500095
'End if			

if {"3f527e17-8291-4d2c-9599-d75303486361","6091756e-4eb3-43ca-91ef-ee76097ea1da","569d3499-1677-4e1c-a41c-bf926d0113b2","39283594-0eb5-4192-a74f-144af08f297e"} _
	.contains(Lcase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring)) Then 
	EbtallAndPyamak("عدم تایید ","عدم تایید")
End if	

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("25a1960f-8d4b-4fcf-872c-7676e58f5da3") Then
			Try
		 		Dim DateNow as Date = Date.Now
		        Dim UserInfo as New BIZ.SA.ClsUser 
		        
		        UserInfo.UserGuid=Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57")
			    UserInfo.UserName="esup"		      
		        
		        Dim RevisiAgentT As New DtoSC.Sh_Revisit
		        
		        RevisiAgentT.NidRevisitAgent = Guid.Parse(AssingTo)
		        RevisiAgentT.NidProc = Info8.GetRequest.Info.NidProc
		        RevisiAgentT.NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
		        RevisiAgentT.NidUser = Guid.Parse(AssingTo)
		        RevisiAgentT.UserName = AssingToUserName
		        RevisiAgentT.RevisitDate = Biz.SA.ClsCommon.ConvertDateTimeToPersionString(DateNow.AddDays(1))
		        RevisiAgentT.RevisitTimeFrom = "8:00"
				RevisiAgentT.RevisitTimeTo = "9:00"
				
		        Info8.GetFunctions.AddRevisitAgentForRequest(RevisiAgentT, UserInfo) 	 

		    Catch e as exception
				msg("bazdid: " & e.message)			       		       		    
		    End Try   												
			
			Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
			lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
			Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
			lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
	       	Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
	       	lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
			Dim UserG as String
			Dim NameUser as String
			UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
			NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
			CreatTask("Activity_0n1pilj",UserG,NameUser)	'نود بازدید دوباره سرقفلی 

End if

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("eaccd3e9-bcea-4e84-b4b5-92122138f2c5") Then
	SaveCheck("bc36db58-5cc6-4904-b9a2-406c3d1dc54d",2,0) 	
End if
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("eaccd3e9-bcea-4e84-b4b5-92122138f2c5") Then
	SaveCheck("bc36db58-5cc6-4904-b9a2-406c3d1dc54d",2,0) 	
End if

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("eaccd3e9-bcea-4e84-b4b5-92122138f2c5") Then
	SaveCheck("bc36db58-5cc6-4904-b9a2-406c3d1dc54d",2,0) 	
End if
		

'sp 463452
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f92e2f10-11db-4fe9-9f96-fccbe117ad40") Then 'کنترل درج فرم تعهد نامه ابلاغ الکترونیک در آرشیو شهرسازید
		Savecheck("f92e2f10-11db-4fe9-9f96-fccbe117ad40",0,2)
	End if
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f43968a5-3ae8-407a-a6b9-f2fa80c3a5d0") Then 'کنترل درج فرم تعهد نامه ابلاغ الکترونیک در آرشیو شهرسازید
		Savecheck("f43968a5-3ae8-407a-a6b9-f2fa80c3a5d0",0,2)
	End if
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("52f12877-0b34-4b9e-9276-d2358cb8a602") Then 'کنترل درج فرم تعهد نامه ابلاغ الکترونیک در آرشیو شهرسازید
		Savecheck("52f12877-0b34-4b9e-9276-d2358cb8a602",0,2)
	End if

'=====================  پروانه پارکینگ -  عدم تایید چک لیست شهرداری  	=============== sp 475882 ===================

	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("49c9d0c0-f4e2-4803-87d9-f53cb2dd8e80") Then 'تکمیل مدارک مامور بازدید
		Savecheck("49c9d0c0-f4e2-4803-87d9-f53cb2dd8e80",0,2)
	End if
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("807ac1ab-934e-490f-9d59-321c093ccb45") Then 'تکمیل مدارک کارشناس معماری 
		Savecheck("807ac1ab-934e-490f-9d59-321c093ccb45",0,2)	' 
	End if
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("199e26d2-3028-4f75-9495-554284b9a79c") Then 'تکمیل بودن فرم بازدید  
			'به آخرین کارشناس بازدید فعال ارسال گردد.
			
			Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
			lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
			Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
			lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
	       	Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
	       	lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
			Dim UserG as String
			Dim NameUser as String
			UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
			NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
			CreatTask("Activity_1d3nq81",UserG,NameUser)
	End if		
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ae58b3ae-106d-41e8-9de7-3ea2f964e039") Then 'پذیرش بازدید
		Info8.GetFunctions.WF_CloseSimpleTaskWithoutRunFormula(Info8.Request.Info.Nidproc.tostring, "Activity_1d3nq81").task  ' بسته شدن نود بازدید
		Savecheck("d8aa0716-3410-48a1-92d1-3d88bbdfe737",0,2)		
	End if		
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("27e2c426-a804-447c-b024-990ea66ee650") Then 'بررسی پلان پارکینگ وانطباق آن با وضع موجود توسط کارشناس بازدید(گروه ساختمانی ب ج د)
		
			'به آخرین کارشناس بازدید فعال ارسال گردد.
			
			Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
			lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
			Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
			lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
       	Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
       	lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
			Dim UserG as String
			Dim NameUser as String
			UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
			NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	' Bazdid
			CreatTask("Activity_1d3nq81",UserG,NameUser)
	End if	

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("aca7ce0e-5f18-4d48-9b99-6f9f10803a08") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8205c44d-a8fa-44ad-9817-34fbe2156a0f") or _
   ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b01dbd71-29b8-4223-9dd4-0b4be168a204") Then	 
	
	Dim Test as New BIZ.SC.SrvEngineering.EngineerList
	Test = Info8.GetFunctions.GetAllEngWithNidProc(Info8.GetRequest.Info.NidProc)	'(guid.Parse("0005f012-5241-4fc2-8170-642b25042dfd"))

	Dim listEnginer as New List(Of BIZ.SC.SrvEngineering.Eng_Info)
	Dim RefInfoLst as New List(Of BIZ.SC.SrvEngineering.Ref_Info)
	listEnginer = Test.EngInfoList
	RefInfoLst = Test.RefInfoList.where(Function (f) f.CI_Ability=1).tolist
	Dim i as Integer = 0
	Dim j as Integer = 0
	AssingTo =""
	AssingToUserName=""
'	msg("Engineer: Ablity: " + RefInfoList(1).CI_Ability.tostring + " | " + listEnginer(1).EngName.tostring + " " + listEnginer(1).EngFamily.tostring + " | CI_StudyField: " + listEnginer(1).CI_Ability.tostring)
  Dim dd as String 
  'الویت اول
 	For i=0 to RefInfoLst.count-1 
 	 
		For j=0 to listEnginer.count-1 
		       
			if RefInfoLst(i).NIdEng = listEnginer(j).NIdEng  Then
				if listEnginer(j).ADPstudyFieldRel = 100 or listEnginer(j).ADPstudyFieldRel = 110 or listEnginer(j).ADPstudyFieldRel = 120 _
				or listEnginer(j).ADPstudyFieldRel = 130 or listEnginer(j).ADPstudyFieldRel = 191 Then
			       
					AssingTo = listEnginer(j).NIdEng.tostring
					AssingToUserName = listEnginer(j).EngName.tostring + listEnginer(j).EngFamily.tostring 					
				 
					Exit For
				End if				
			
			End if	
	  	
		Next
			if Not String.IsNullOrEmpty(AssingTo) Then Exit For
	Next

	'الویت دوم 
if String.IsNullOrEmpty(AssingTo) Then 
	
 	For i=0 to RefInfoLst.count-1 
 	 
		For j=0 to listEnginer.count-1 
		       
			if RefInfoLst(i).NIdEng = listEnginer(j).NIdEng  Then
				if listEnginer(j).ADPstudyFieldRel = 300 or listEnginer(j).ADPstudyFieldRel = 310 or listEnginer(j).ADPstudyFieldRel = 320 _
					or listEnginer(j).ADPstudyFieldRel = 330 or listEnginer(j).ADPstudyFieldRel = 340 or listEnginer(j).ADPstudyFieldRel = 350 or listEnginer(j).ADPstudyFieldRel = 360 _
					or listEnginer(j).ADPstudyFieldRel = 370 or listEnginer(j).ADPstudyFieldRel = 380 or listEnginer(j).ADPstudyFieldRel = 391 _
					or listEnginer(j).ADPstudyFieldRel = 392 or listEnginer(j).ADPstudyFieldRel = 393 or listEnginer(j).ADPstudyFieldRel = 394 _
					or listEnginer(j).ADPstudyFieldRel = 395 Then	
			       
					AssingTo = listEnginer(j).NIdEng.tostring
					AssingToUserName = listEnginer(j).EngName.tostring + listEnginer(j).EngFamily.tostring 					
			   
					Exit For
				End if				
			
			End if	
	  	
		Next
			if Not String.IsNullOrEmpty(AssingTo) Then Exit For
	Next
End if 	


        Dim randomValue As Integer
        Dim rng As New Random()
        randomValue = rng.Next(1, 3)
        If randomValue = 1 Then	
        	CreatTask("Activity_1074fhi",AssingTo,AssingToUserName)  ' ارسال به نظام مهندسی 
        Else
        	randomValue = rng.Next(1, 3)
	        If randomValue = 1 Then
		            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
					AssingTo =""
					AssingToUserName=""
					Call User(13)   
					CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
				SendPayamak("کانون کارشناس رسمی","آدرس کانون کارشناسان رسمی : مشهد ، میدان استقلال، ضلع شمال شرقی (فردوسی ۲۳) ، پلاک ۳۷۷ تماس: 05136153 ")
	        Else
		            'Creattask("Activity_0mpzy4j","4e05f9da-87f0-4cce-b943-f7c942a99a7e","کانون کارشناسان رسمی دادگستری")
					AssingTo =""
					AssingToUserName=""
					Call User(14)   
					CreatTask("Activity_0mpzy4j",AssingTo,AssingToUserName)
					SendPayamak("مرکز کارشناس رسمی","آدرس مرکز کارشناسان رسمی : مشهد، حاشیه میدان جانباز، جنب بانک سپه، پلاک 105، تماس: 05137608480  الی 9 ")
	        End If	       	        	
        End if		
End if


'=====================  پروانه پارکینگ -  اتمام عدم تایید چک لیست شهرداری  	=============== sp 475882 ===================

	
'=== 'sp 462171 ==================  تایید سابقه بدون گواهی - عدم تایید چک لیست شهرداری  ==================================
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6f80a050-b319-450b-b5d2-9a10e2b8f114") Then 'تکمیل مدارک
	'	Savecheck("6f80a050-b319-450b-b5d2-9a10e2b8f114",0,2)
	End if
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7985a2f1-d307-4a2b-97ac-a1d8c661b4c6") Then 'تکمیل فرم بازدید ساختمان بر اساس سوابق مجازه
	'	Savecheck("7985a2f1-d307-4a2b-97ac-a1d8c661b4c6",2,0)
	End if
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7c613c9d-faa4-41a1-a073-20be3da0be45") Then 'تکمیل فرم کنترل نقشه بر اساس سوابق مجازه
	'	Savecheck("7c613c9d-faa4-41a1-a073-20be3da0be45",2,0)
	End if	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8fa0a534-1b26-4338-af2d-1e94862ddcf3") Then 'کنترل مدارک و اسناد آپلود شده
		Call EbtallAndPyamak(" تایید سابقه بدون گواهی "," تایید کنترل مدارک و اسناد آپلود شده ")
	End if
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5123f5e8-9cde-4cd2-9653-9c448a928da9") Then 'بررسی و تایید فرم بازدید ساختمان
	'	Savecheck("5123f5e8-9cde-4cd2-9653-9c448a928da9",2,0)
	End if
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e0309fdd-ed74-4b99-bea1-4981aac6a1ad") Then 'بررسی و تایید   فرم کنترل نقشه
	'	Savecheck("e0309fdd-ed74-4b99-bea1-4981aac6a1ad",2,0)
	End if
					
    if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("206c8b68-d3d6-4989-9513-167e890bff25") Then
	 	Call User(1)
	 	AssingTo =""
	 	AssingToUserName=""
	 	CreatTask("Activity_0scwa4a",AssingTo,AssingToUserName)
	End if

'=====================  پایان تایید سابقه بدون گواهی - عدم تایید چک لیست شهرداری  ==================================

'=====================  پروانه کمیسیون ماده 5 - عدم تایید چک لیست شهرداری  ==================================

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("36612605-66c8-444a-8542-7009bff20b5b") Then
	 	'Call User(7)
	 '	AssingTo =""
	' 	AssingToUserName=""
	 	CreatTask("Activity_0hawxst","6010D9B2-5F0B-437E-89C9-C1703310D91B","کارشناسان استحصال")
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("312112dc-400d-4818-a949-4a0630e536d4") Then
'	 	Call User(7)
'	 	AssingTo =""
'	 	AssingToUserName=""
	 	CreatTask("Activity_0t217u5","6010D9B2-5F0B-437E-89C9-C1703310D91B","کارشناسان استحصال")
End if

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c78e0fa2-b90d-476f-82e7-9e83d467cde0") Then
'	 	Call User(7)
'	 	AssingTo =""
'	 	AssingToUserName=""
	 	CreatTask("Activity_0t217u5","6010D9B2-5F0B-437E-89C9-C1703310D91B","کارشناسان استحصال")
End if

'sp 476127
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c78e0fa2-b90d-476f-82e7-9e83d467cde0") Then  'تکمیل مدارک	 	
		AssingTo =""
		AssingToUserName=""
		Call User(6)
	 	CreatTask("Activity_0t217u5",AssingTo,AssingToUserName)	'رییس شهرساز منطقه
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("312112dc-400d-4818-a949-4a0630e536d4") Then	'تنظیم لایحه
		AssingTo =""
		AssingToUserName=""
		Call User(6)
	 	CreatTask("Activity_0t217u5",AssingTo,AssingToUserName)	'رییس شهرساز منطقه
End if

'=========================== اتمام پروانه کمیسیون ماده 5 - عدم تایید چک لیست شهرداری ===========================


'sp 454792
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1c1b69c0-30dc-4db1-90df-0e31e553b84a") Then
		
  		Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
		Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
		Dim ListParent_M As List(Of BIZ.SA.ClsNosaziCodeType)
		
	Try	
 		TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))	
	 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)
	 	ListParent_M=ListParent
	Catch
	End Try

				Dim M_BaseUsing,TmpM_BaseUsing as New  List(of DtoSc.Base_Using)								
				Try  
					Info8.ParameterList.Remove("Revisit_Building_NidBase")
	   				Info8.ParameterList.Add("Revisit_Building_NidBase", Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
			
				 	M_BaseUsing = Info8.GetRevisit_Building.GetBase_Using
				 	
				Catch
				
					Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
				    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)							
					M_BaseUsing = Info8.GetRevisit_HouseSharing.GetBase_Using
					
				End Try 

				
					
				TmpM_BaseUsing=M_BaseUsing.where(Function(f) f.CI_UsingGroup=13 ).tolist
		    	Dim M_UsingArea as Double 
		    	Dim M_Nama as Integer=0
		    	
		    	M_UsingArea=TmpM_BaseUsing.sum(Function (f) f.UsingArea)
		 	
		    	if M_UsingArea >0 Then M_Nama=1
		    	if M_Nama=0 Then 
		    		TmpM_BaseUsing=M_BaseUsing.where(Function(f) f.CI_UsingGroup=12 ).tolist
		    		M_UsingArea=TmpM_BaseUsing.sum(Function (f) f.UsingArea)
		    		if M_UsingArea >2000  Then M_Nama=1
		    	End if
		    		
		    	if M_Nama=0 And M_BaseUsing.count>0 Then 
		    		M_UsingArea=M_BaseUsing.Max(Function (f) f.BuildingNo)
		   	
		    		if M_UsingArea >1 Then M_Nama=1
		    	End if
		    	'sp 371209 
		      	if M_Nama=0 And M_BaseUsing.count>0 Then 
		    			'sp 414875  
    				Dim LstFloor as New  List(of DtoSc.Base_Using)
    				LstFloor = M_BaseUsing.where(Function (f) f.FloorNo<100).tolist() 
    				
		    		M_UsingArea=LstFloor.Max(Function (f) f.FloorNo) -  M_BaseUsing.Min(Function (f) f.FloorNo) +1
		    		if M_UsingArea >10 Then M_Nama=1
		    	End if 
			    	
		    	
		    	if M_Nama=0 And M_BaseUsing.count>0 Then 
		    		M_UsingArea=M_BaseUsing.sum(Function (f) f.UsingArea)
		    		if M_UsingArea >5000  Then M_Nama=1
		    	End if 

			    	
		    	
		    	if M_Nama=0 Then 
		    	
		    		if Info8.GetRequest.Info.ActiveNidZabeteh=Guid.Empty Then
			    	ElseIf Info8.GetRequest.Info.ActiveNidZabeteh.tostring="" Then 
		
					Else
					 
						Dim tmpNidZ As Guid
				  		Dim TmpZabete As New DtoSC.Zabeteh
				  		Dim MapNama as Integer=0
			Try	 
				  		Dim M_ZabetehDetails As  List(Of DtoSC.Zabeteh_Details)
				  		M_ZabetehDetails = New List(Of DtoSC.Zabeteh_Details)
				   	    tmpNidZ = Info8.GetRequest.Info.ActiveNidZabeteh				   	    
			      		Info8.Zabeteh=Nothing
				  		Info8.ParameterList.REMove("NidZabeteh")
				  		Info8.ParameterList.Add("NidZabeteh", tmpNidZ.ToString)
					    Info8.ParameterList.REMove("NidNosaziCode")					    
				        Info8.ParameterList.Add("NidNosaziCode",Ucase(ListParent(1).NidNosaziCode.tostring))	'Info8.GetRevisit_House.GetBase_NosaziCode. ' باید نوسازی ملک باشه که ردیف داشته باشه در دیتا بیس ضابطه
				        			        
				  	 	TmpZabete =Info8.GetZabeteh(True).GetZabeteh
				  		M_ZabetehDetails = Info8.GetZabeteh(True).GetZabeteh_Details
				  		if M_ZabetehDetails.count>0  Then 
							M_ZabetehDetails=M_ZabetehDetails.where(Function(f) f.CI_Zabeteh=246 And f.ValueZabeteh=1).tolist
				  			if M_ZabetehDetails.count>0  Then M_Nama=1
				 		End if	 	
			Catch ex as exception
					
				Info8.GetOnTickChecklistParameter().Result="false"
				Info8.GetOnTickChecklistParameter().Description="فرمول چک لیست - تایید شهرداری"+ex.message
				Exit Function		   	
		    End Try	
						
					End if
					
		   	End if   
		   	
		   	
		   	if ControlMap = 0 Then 	M_Nama=1	'sp 454792
	   		   					   						
			if M_Nama=1 Then
				CreatTask("Activity_0otene2","4acf8b89-113d-4d23-a70f-f555c6eb9c2e","کمیته نما")							
			End if
	
		

	End if 		
'end 454792

'sp 472011
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9ba57e8f-c46a-4459-aabc-ebf0cc28efc0") Then
	Savecheck("c654bd8c-e640-40f6-aa2b-537407db4fa3",2,0)
End if	
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("eaccd3e9-bcea-4e84-b4b5-92122138f2c5") Then
	Savecheck("9c8dca82-7dff-47b8-a89f-409babb0034d",2,0)
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d0d21315-6639-48c7-b315-8f55d0a8eac1") Then
	Savecheck("bc380733-8f65-4937-9be5-608870f8b9e4",2,0)
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c93372f4-2da2-4b04-a9d1-ca84785dddf9") Then
	Savecheck("4ad1c91a-b212-47f8-89fb-5adecd8bbcc7",2,0)
	SaveCheck("9967d1d1-e506-4575-aee9-f757262e2aae",2,0) 	'sp 486714
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9f3b3c58-3c24-461a-a6fe-696f365f7968") Then
	Savecheck("395cf52e-c6cc-467d-be51-5ea79474ff88",2,0)
	SaveCheck("ad6d6d96-809d-431d-8128-4da52d6ee4c5",2,0) 	'sp 486714
End if
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1784b605-4de0-4f5e-9533-bbcdc33c2980") Then
	Savecheck("cd963521-d125-498f-ad72-4471379abc27",2,0)
	SaveCheck("b672e200-0a04-468d-ad97-42ca73cad169",2,0) 	'sp 486714
End if
'end sp 472011

'sp 468921		
			
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("189a5d67-4c63-4b6c-878f-9be579dfa37b") Then
		Dim M_Str as String ="دارای فرم تعهدنامه تخریب تخلفات (تخریب 6 ماهه) به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه املاک بدون سند و مشاعی به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه ماده واحده به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه تجمیع املاک به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه استحکام بنا در درخواست توسعه بنا به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تاییدیه توسعه بنا توسط مهندس ..................... با کد شهرداری ....................... می باشد ." & _
"دارای فرم تعهد نامه اراضی و املاک مشمول سهم سرانه خدماتی (غیرقابل تفکیک) به شماره .......... مورخ ............ می باشد ." & _
"دارای مفاصا حساب بیمه به شماره .......... مورخ ............ می باشد . )" & _
"دارای تعهدنامه ارائه مفاصاحساب بيمه تامين اجتماعي مورخ ............ می باشد .)" 

	SaveCheck("6926cfcc-2a9c-4d3c-a508-f1350235dcbc",2,0,M_Str)
End if		
			
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c996f731-b0d9-40ac-ac6d-6a70aac49cb7") Then
		Dim M_Str as String ="دارای فرم تعهدنامه تخریب تخلفات (تخریب 6 ماهه) به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه املاک بدون سند و مشاعی به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه ماده واحده به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه تجمیع املاک به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه استحکام بنا در درخواست توسعه بنا به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تاییدیه توسعه بنا توسط مهندس ..................... با کد شهرداری ....................... می باشد ." & _
"دارای فرم تعهد نامه اراضی و املاک مشمول سهم سرانه خدماتی (غیرقابل تفکیک) به شماره .......... مورخ ............ می باشد ." & _
"دارای مفاصا حساب بیمه به شماره .......... مورخ ............ می باشد . )" & _
"دارای تعهدنامه ارائه مفاصاحساب بيمه تامين اجتماعي مورخ ............ می باشد .)" 

	SaveCheck("02e121c1-a6f4-4c3d-a8f8-c9c5fc0a758b",2,0,M_Str)
End if	
	IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bff4c7a6-e96b-48d0-bc48-e50970664df9") Then
		Dim M_Str as String ="دارای فرم تعهدنامه تخریب تخلفات (تخریب 6 ماهه) به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه املاک بدون سند و مشاعی به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه ماده واحده به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه تجمیع املاک به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه استحکام بنا در درخواست توسعه بنا به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تاییدیه توسعه بنا توسط مهندس ..................... با کد شهرداری ....................... می باشد ." & _
"دارای فرم تعهد نامه اراضی و املاک مشمول سهم سرانه خدماتی (غیرقابل تفکیک) به شماره .......... مورخ ............ می باشد ." & _
"دارای مفاصا حساب بیمه به شماره .......... مورخ ............ می باشد . )" & _
"دارای تعهدنامه ارائه مفاصاحساب بيمه تامين اجتماعي مورخ ............ می باشد .)" 

	SaveCheck("f63ebf88-1bf2-4980-9e44-574ca2ffd9df",2,0,M_Str)
End if	
		
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("eaccd3e9-bcea-4e84-b4b5-92122138f2c5") Then
		Dim M_Str as String ="دارای فرم تعهدنامه تخریب تخلفات (تخریب 6 ماهه) به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه املاک بدون سند و مشاعی به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه ماده واحده به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه تجمیع املاک به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تعهدنامه استحکام بنا در درخواست توسعه بنا به شماره .......... مورخ ............ می باشد ." & _
"دارای فرم تاییدیه توسعه بنا توسط مهندس ..................... با کد شهرداری ....................... می باشد ." & _
"دارای فرم تعهد نامه اراضی و املاک مشمول سهم سرانه خدماتی (غیرقابل تفکیک) به شماره .......... مورخ ............ می باشد ." & _
"دارای مفاصا حساب بیمه به شماره .......... مورخ ............ می باشد . )" & _
"دارای تعهدنامه ارائه مفاصاحساب بيمه تامين اجتماعي مورخ ............ می باشد .)" 

	SaveCheck("f63ebf88-1bf2-4980-9e44-574ca2ffd9df",2,0,M_Str)
End if			
			
'end sp 468921			
'end sp 468921		
			
'end sp 468921	
'----------------  Sajedi -----------

'sp419555
'	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d8f2c14c-d99a-4e74-b822-3502a88a23c7") Then  Call Payamak("","لطفا به اداره املاک مراجعه نمایید")
'end sp419555

'---------End Sajedi Code ------------



'-.-.-.-.-.-.-.-.-.-.--.-.-.-.-.-.-.--.-.-.-.-.-.-.-.--.-.-.-.--.-.-.-.-.
'450772

	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("839f8728-756f-4db1-acfe-a5a9c25cfc3d") Then SaveCheck ("f3ea25fc-c4bc-4995-b812-e3086c18891b", 2,0)

	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9d02e09a-6c67-48b8-b6c0-1dbccb8dbe09") Then SaveCheck ("9a93c7ba-3686-4c51-bcf2-f43c587a01f1", 2,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1565f4e3-9513-4a12-8005-b522d58f1c70") Then SaveCheck ("cab9b25c-284a-413d-bdd8-e402fc30a7f6", 2,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b4bcc0da-fc48-431b-b2f5-e8c52ea851a5") Then SaveCheck ("167c1bd2-803d-4d2f-85c7-45a36ecab4d2", 2,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3c08e559-3041-4dea-af26-5f6be50779d1") Then SaveCheck ("5d7ada12-32ec-48a4-b72d-360f87f057b3", 2,0)
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8312a9bc-3904-49dc-9e8a-922825e33300") Then SaveCheck ("07f2d2d7-a46a-47c0-8388-94607a0f6700", 2,0)
'-.-.-.-.-.-.-.-.-.-.--.-.-.-.-.-.-.--.-.-.-.-.-.-.-.--.-.-.-.--.-.-.-.-.
 

 
 'sp 450393
If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6d99929e-ccfa-4c17-8e2b-57235fd780a7") Then
	Try
		 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
		 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
		 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
		 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
	     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
	     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
		 Dim UserG as String
		 Dim NameUser as String
		 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
		 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
		 CreatTask("Activity_0wn4b4a",UserG,NameUser)
	Catch e as exception
		msg(" خطا در خوانش مامور بازدید لطفا اعلام مامور بازدید را بررسی نمایید ")	'sp 514629
		Exit Function			
	End Try   
End IF 
 
 'sp 449225
If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("eb330aca-b569-4e29-9b81-f453c4c23a5d") Then
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	 CreatTask("Activity_1atjcx5",UserG,NameUser)
	 
	 'sp 496859
	 Dim UserGMobile as String
	 UserGMobile = Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Phone.ToString
	 Sms(UserGMobile,"مامور بازدید"," درخواست پارکینگ شهرسازی به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " به شما عودت گردید، مطلوب است سریعا نسبت به بررسی و پاسخگویی اقدام نمایید.")

   
End IF 
 
'sp 446227
If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ab24f8b0-fde3-48e2-a1d6-8df604c7f182") Then
	Call User(1)
    CreatTask("Activity_1yvcege",AssingTo,AssingToUserName)
End IF 


'SP 413240 
If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e5c5b4a9-9145-4d03-9124-acaf8affcde6") Then
	Call Payamak("چک لیست"," شهروند محترم لطفا جهت پیگیری مجددبرای نهایی شدن صلحنامه به سازمان زمین و مسکن مراجعه فرمایید")
End IF 



'sp 379897

If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1a661a9d-8572-447a-abba-34ed88cbe68a") Then 
		Call EbtallAndPyamak("ابطال درخواست"," به دلیل عدم تایید احراز هویت در سیستم شهرسازی  ابطال گردید")
End if 
If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c63198fe-3fc8-4aca-9b70-f56d9f826f5e") Then
		Call EbtallAndPyamak("ابطال درخواست"," به دلیل عدم تایید احراز هویت در سیستم شهرسازی  ابطال گردید")
End if 

'sp 434816
  If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f0afb2e6-ea03-423f-a94e-4261ceaa198a") Then 
		if ControlTask("Activity_0mknp3i")=0 or ControlTask("Activity_0mknp3i")=2  Then User(2) : CreatTask("Activity_0mknp3i",AssingTo,AssingToUserName) 
	End if 
	
'sp 419416
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8d177a4c-db48-4fc4-bf8a-d4acd8789282") Then
	Call User(6)
    CreatTask("Activity_1swyraw",AssingTo,AssingToUserName)
	
End if 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("df471dab-0797-40ed-b516-9aa257a77d99") Then
	Call User(6)
    CreatTask("Activity_0m8ll1t",AssingTo,AssingToUserName)
	
End if 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7e461045-6617-4b04-b978-96840ef71595") Then
	Call User(6)
    CreatTask("Activity_0e9jvh2",AssingTo,AssingToUserName)
	
End if 

'sp 416583
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3a0c6169-877b-4f06-8a76-ed0538b0211d") Then
	Call User(2)
    CreatTask("Activity_0iqwq73",AssingTo,AssingToUserName)
	
End if 
'sp 409884
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("23e25c9d-5d0e-46ed-b302-df0ea7a29ec4") Then
 	Call Payamak(" درخواست"," esup.mashhad.ir/documents/notice/nama.pptx ") 

 End if

' Naghibi
 if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5b2d0dbc-6044-4b5e-83e0-0abe854983b1") Then
 	SaveCheck("2a660b0d-420f-4cfa-bdaa-0a0e16d6f7dd",2,0)
 End if
 if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fa125fb2-7a0b-429f-b39b-1f113155366a") or _
    ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b2103a92-3cd0-4a5d-9d50-1e6f0839379c") Then
    
    	if Info8.GetRequest.Info.Ci_requester = 6 Then 
		    Call User(4)
		    CreatTask("Activity_0zqrj7e",AssingTo,AssingToUserName)    	'sp 524973
    	Else
		    Call User(2)
		    CreatTask("Activity_0zqrj7e",AssingTo,AssingToUserName)
		End if    
 End if   

 IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0d383608-f58e-4482-be91-e3fe73966975") or _
    ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ea7d2063-7693-489b-9f47-154a1c9d2856")  Then
 	
 	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
	Dim ss as Integer =0
	
	 Dim List_NidProc As New List(Of DtoSC.SrvGetCommissionList_InNidProcResult)
	List_NidProc = Info8.GetCommission_List().CommissionList_InNidProc
	

    Info8.ParameterList.Remove("Commission_NidCommission")
    if List_NidProc.count=0 Then Exit Function
	Info8.ParameterList.Add("Commission_NidCommission", List_NidProc(0).NidCommission.ToString)

	Dim Mylist As New List(Of Global.DtoSC.Commission_Vote)
    Dim TmpMylist As New List(Of Global.DtoSC.Commission_Vote)
    Mylist = Info8.GetCommission.GetCommission_Vote
    if Mylist.count>0 Then 
		    TmpMylist = Mylist.Where(Function(f) f.CI_CommissionVoteTypeDetail = 400).ToList
		    If TmpMylist.Count Then
		    	For ss=0 to lstOwner.count-1
				  Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰"," برای پرونده شما به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " رای به قرار بازدید صادر شده است. زمان بازدید در صورت لزوم از طریق شهرداری منطقه به اطلاع شما خواهد رسید چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد - شهرداری مشهد" )
				Next
				Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰"," برای پرونده شما به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " رای به قرار بازدید صادر شده است. زمان بازدید در صورت لزوم از طریق شهرداری منطقه به اطلاع شما خواهد رسید چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد - شهرداری مشهد" )
		
		    Else
		       TmpMylist = Mylist.Where(Function(f) f.CI_CommissionVoteType = 5).ToList
		       If TmpMylist.Count Then
		       		For ss=0 to lstOwner.count-1
					  Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰"," برای پرونده شما به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " قرار صادر گردیده است. برای پیگیری پرونده به منطقه مراجعه فرمایید چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد - شهرداری مشهد" )
					  
					Next
					 Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰"," برای پرونده شما به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " قرار صادر گردیده است. برای پیگیری پرونده به منطقه مراجعه فرمایید چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد - شهرداری مشهد" )
					 
		       Else
		       		For ss=0 to lstOwner.count-1
					  Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰"," رای کمیسیون ماده صد، برای پرونده به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " شما صادر و برای ابلاغ به اداره پست تحویل گردیده است - شهرداری مشهد" )
					  Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰","ادرس رویت رای : " &  " http://repservice.mashhad.ir:9000/Viewer/RptViewer.aspx?ReportPath=/CommissionReports/ComReport/RptLastVote&ReportParameter=NidProc;" & Info8.GetRequest.Info.Nidproc.tostring & ";")
					Next
					 Sms(lstOwner(ss).OwnerMobile,"کمیسیون ۱۰۰"," رای کمیسیون ماده صد، برای پرونده به شماره ارجاع " & Info8.GetRequest.Info.NidWorkItem.tostring & " شما صادر و برای ابلاغ به اداره پست تحویل گردیده است - شهرداری مشهد" )
					 Sms(Info8.GetRequest.Info.Cellphone,"کمیسیون ۱۰۰","ادرس رویت رای : " &  " http://repservice.mashhad.ir:9000/Viewer/RptViewer.aspx?ReportPath=/CommissionReports/ComReport/RptLastVote&ReportParameter=NidProc;" & Info8.GetRequest.Info.Nidproc.tostring & ";")
						
		       End If
		    End If
	End if 		 	
	
End if 


' sp 407966
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0e3082fa-0d76-46da-8983-e5f9ed5b2fa5") Then
	SaveCheck("13adc984-555d-422c-a5cb-727e1e6f92c2",2,0)
End if

'sp 404541
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2d27c0b6-a926-42fe-a4ec-1f526e1b3c41") Then
	Call User(2)
	CreatTask("Activity_0mknp3i",AssingTo,AssingToUserName) 
End if 

'sp 409008
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4ffedd28-f72a-4f1e-9f12-e9367f2288d7") Then
	Call User(2)
	CreatTask("Activity_0iqwq73",AssingTo,AssingToUserName) 
End if 

'sp 402889
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1bd35375-e4e4-4229-bd2e-2231f0d835f3") Then
	SaveCheck("a83e88a7-500a-4939-850e-fdf50d7f7c96",2,0)
End if 

'sp 401661
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("47c9548a-a1c9-4d59-bbbe-1df75729c58b") Then
	Dim M_Str as String ="-	دارای صورتجلسه هیات فنی به شماره ....... مورخ ......می باشد. (درصورت شمول)" & _
"-	دارای تعهد فضای سبز مورخ ... می باشد." & _
"-	دارای تائیدیه پرداخت بیمه تامين اجتماعي به شماره .....مورخ ..... می باشد. (در صورت ارائه تائیدیه)" & _
"-	صدور هرگونه پایانکار منوط به ارائه مفاصا حساب از سازمان تامین اجتماعی می باشد. (در صورت عدم ارائه تائیدیه تامین اجتماعی)" & _
"-  دارای صلحنامه به شماره .... مورخ .... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه رعایت میزان در مسیر مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه املاک بدون سند و مشاعی مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه تخریب تخلفات مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه  پاسخگویی به شکایت های احتمالی در خصوص پروانه صادره مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه ماده واحده مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه تجمیع املاک مورخ ... می باشد. (درصورت شمول)" & _
"-دارای تعهدنامه استحکام بنا مورخ ... می باشد. (درصورت شمول)" & _
"-دارای تاییدیه توسعه بنا مورخ ... می باشد. (درصورت شمول)"
	SaveCheck("02e121c1-a6f4-4c3d-a8f8-c9c5fc0a758b",2,0,M_Str)
End if		 
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("27a5254c-9aa3-4f7c-bcfe-d8336e1e53a4") Then
		Dim M_Str as String ="-	دارای صورتجلسه هیات فنی به شماره ....... مورخ ......می باشد. (درصورت شمول)" & _
"-	دارای تعهد فضای سبز مورخ ... می باشد." & _
"-	دارای تائیدیه پرداخت بیمه تامين اجتماعي به شماره .....مورخ ..... می باشد. (در صورت ارائه تائیدیه)" & _
"-	صدور هرگونه پایانکار منوط به ارائه مفاصا حساب از سازمان تامین اجتماعی می باشد. (در صورت عدم ارائه تائیدیه تامین اجتماعی)" & _
"-  دارای صلحنامه به شماره .... مورخ .... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه رعایت میزان در مسیر مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه املاک بدون سند و مشاعی مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه تخریب تخلفات مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه  پاسخگویی به شکایت های احتمالی در خصوص پروانه صادره مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه ماده واحده مورخ ... می باشد. (درصورت شمول)" & _
"- دارای تعهدنامه تجمیع املاک مورخ ... می باشد. (درصورت شمول)" & _
"-دارای تعهدنامه استحکام بنا مورخ ... می باشد. (درصورت شمول)" & _
"-دارای تاییدیه توسعه بنا مورخ ... می باشد. (درصورت شمول)"
	SaveCheck("6926cfcc-2a9c-4d3c-a508-f1350235dcbc",2,0,M_Str)
End if		 

'sp 403047
IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4da7e92d-9336-4e23-b0db-0a6c4f2acf84") _
   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c73bbdcf-baef-4f8c-a1d8-2e1a23ecb29e") Then
	Call User(8)
	CreatTask("Activity_09omebd",AssingTo,AssingToUserName) 
End IF 



'sp 401836
'IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("06e2a034-d678-4fd2-a1c7-9b26b3d6e1b2") Then
'	if Info8.GetRequest.Info.CI_Requester=14 Then 
'		SaveCheck("2ede6cd8-404f-4cbc-bdb2-423343ccb8bc",2,0) 
'	Else	
'		SaveCheck("aba2cfeb-d00e-46bd-968e-b4e284b90681",2,0) 
'	End if 	

'End IF 

IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b4d62eb1-9202-4bd8-8f64-78c009af1ede") Then
	if Info8.GetRequest.Info.CI_Requester=14 Then 
		SaveCheck("00b19551-993e-4483-b6c1-369941060023",2,0) 
	Else	
		SaveCheck("28d21878-dd31-467e-8a9b-2476e450d4a3",2,0) 
	End if 	
End IF 



IF ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("24895d1f-0df9-4bf7-b286-6039507ce9d5") Then
	if Info8.GetRequest.Info.CI_Requester=14 Then 
		SaveCheck("a1322d6a-1cb4-4f77-b5a3-3fbbe4677ff7",2,0) 
	Else	
		SaveCheck("2331a630-bfc2-4c75-83e5-e0ad083adfd8",2,0) 
	End if 	
End IF 

'sp 396836 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5fbe1ea1-fb44-4240-a24d-fed7b817b135") Then 
	SaveCheck ("f50aa94c-10b6-4aa9-a591-32bbce3e687a",2,0) 
End if 

if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0a6af71d-d227-427d-8578-2acd08b3122f") Then 
	if ControlTask("Activity_09omebd",1)=3 Then  Call User(8) :	 CreatTask("Activity_0m55nua",AssingTo,AssingToUserName)
End if 
'sp 389755 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0f201adb-fdb8-4337-8d28-146480c5644a") Then 
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	 CreatTask("Activity_1agutdl",UserG,NameUser)
End if
'sp 403596 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0d636d64-6f65-44fa-a215-de2f973bd952") Then 

	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList    
	 Dim UserG as String
	 Dim NameUser as String
'MessageContract(lstR_T(0).NidRevisit.tostring)
'Exit Function 	 
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString	 
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring		
	 CreatTask("Activity_1o37kvo",UserG,NameUser)
End if

'sp 409159 adamkhalaf 450393
'if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6d99929e-ccfa-4c17-8e2b-57235fd780a7") Then 
'	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
'	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
'	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
'	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
 '    Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
 '    lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
'	 Dim UserG as String
'	 Dim NameUser as String
'	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
'	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
'	 CreatTask("Activity_13z3e1z",UserG,NameUser)
'End if
'sp 409159 mafasa
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ebdc0141-45a7-46a7-9946-37558ecbaf4f") Then 
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	'sp 457742 CreatTask("Activity_1jicsxf",UserG,NameUser)
	CreatTask("Activity_13ide08",UserG,NameUser)
	
End if


if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0ee2ff95-e1b8-4e7f-8db3-2e99253988ca") Then 
	 Call User(2) 
	 CreatTask("Activity_0x0ylrz",AssingTo,AssingToUserName)
	
End if	  		
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b0be1372-4f99-40c9-b102-2b6e605e8ab7") Then 
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	 CreatTask("Activity_0a6f3g7",UserG,NameUser)
End if
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("17b46717-e31e-48dd-a8e3-a50ad976fe33") Then 
	 Call User(2) 
	 CreatTask("Activity_12ebsu9",AssingTo,AssingToUserName)
	
End if	 		 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3fe9704b-68ec-44c8-8d04-9f1378fd9cda") Then 
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	 CreatTask("Activity_1uqzw2d",UserG,NameUser)
End if
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0aa1475e-9dcc-490b-b02f-56f636482fa5") Then 
	 Call User(2) 
	 CreatTask("Activity_1ipj02l",AssingTo,AssingToUserName)
	
End if	 		 	
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2bfad4a0-0df2-42d4-b210-6e97034d1096") Then 
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	 CreatTask("Activity_1nre64p",UserG,NameUser)
End if
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5a465423-c2e6-48a3-92f2-df6c9190a06a") Then 
	 Call User(2) 
	 CreatTask("Activity_0fx4od8",AssingTo,AssingToUserName)
	
End if	 	
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("648954a2-85da-4daa-a300-719d7087baf6") Then 
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	 CreatTask("Activity_0302lcv",UserG,NameUser)
End if

	'  ------------------------------------------  استعلام 
		'----------------------------------------------  پروانه
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("a2c54aeb-50f3-419e-8176-9e75ff58b280") Then 
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	 CreatTask("Activity_056rjh9",UserG,NameUser)
End if	 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("49a0df00-ee1a-45d1-908a-15bb95c38654") Then 

	 Call User(2) 
	 CreatTask("1",AssingTo,AssingToUserName)
	
 
End if
 
'----------------
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("a7182a80-1abe-4d06-8ce5-099e898da96c") Then 
	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
	 Dim UserG as String
	 Dim NameUser as String
	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
	 CreatTask("Activity_0e3jn7l",UserG,NameUser)
End if	 
if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("850fedee-7551-4cc1-86fc-59261c8c0041") Then 
	 Call User(2) 
	 CreatTask("Activity_19k5kki",AssingTo,AssingToUserName)	'sp459193			تشکیل نود نقص نقشه برداری 
	
End if	 
'------------- شروع عملیات
'if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6f475248-4e5a-4a5b-968f-9f423b14d138") Then 
'	 Dim lstR_T As New List(Of Global.DtoSC.Sh_Revisit)
'	 lstR_T = Info8.GetRequest.GetRevisit_List(Info8.GetRequest.Info.NidProc)
''	 Dim MReviD = lstR_T.Max(Function(f) f.DateAddRevisit)
'	 lstR_T = lstR_T.Where(Function(f) f.DateAddRevisit = MReviD).ToList
''     Dim MReviT = lstR_T.Max(Function(f) f.TimeAddRevisit)
'     lstR_T = lstR_T.Where(Function(f) f.TimeAddRevisit = MReviT).ToList
'	 Dim UserG as String
'	 Dim NameUser as String
'	 UserG=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).NidUser.ToString
'	 NameUser=Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).Name.tostring + " " +  Info8.GetRevisitAgent.GetSh_RevisitAgent(lstR_T(0).NidRevisitAgent).LastName.tostring	
'	 CreatTask("Activity_0ygetjc",UserG,NameUser)
'End if	 
'if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("096e049b-f757-4e1d-9cf3-779be0d2a63d") Then 
'	 Call User(2) 
'	 CreatTask("Activity_0rnk5qw",AssingTo,AssingToUserName)
'	
'End if	
'sp 380747 
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("de6cc3fd-cd07-415f-a0b4-19e3be421e60")
		Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
       	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
       	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
       	Dim ss as Integer =0
        For ss=0 to lstOwner.count-1
        	Sms(lstOwner(ss).OwnerMobile,"پیامک مالک"," شهروند گرامی : درخواست شما - استعلام از دفتر خانه -  به شماره " & Info8.GetRequest.Info.NidWorkItem & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " ، لطفا جهت رفع اشکال و ثبت درخواست تایید سابقه مجاز  به  یکی دفاتر پیشخوان شهرداری مشهد  مراجعه فرمائید چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد") 
        Next
		Sms(Info8.GetRequest.Info.Cellphone,"پیامک مالک"," شهروند گرامی : درخواست شما - استعلام از دفتر خانه -  به شماره " & Info8.GetRequest.Info.NidWorkItem & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " ، لطفا جهت رفع اشکال و ثبت درخواست تایید سابقه مجاز  به  یکی دفاتر پیشخوان شهرداری مشهد  مراجعه فرمائید چنانچه در انجام اقدامات مرتبط با متقاضی بیش از یک ماه کاری تاخیر صورت پذیرد شهرداری مجاز به تعلیق و بایگانی درخواست می باشد .شهرداری مشهد") 
		  	
	End if

	  ' ابطال درخواست 
	Dim CountFiche as Integer 
	Dim AllFiche as  list(of Dtosc.Income_Fiche)
	AllFiche=Info8.GetIncome_Fiche.GetAll_Fiche(Info8.GetRequest.Info.NidProc)
	AllFiche=AllFiche.where(Function (f) (f.EumFicheStatus = 3 Or f.EumFicheStatus = 5) ).tolist
	CountFiche= AllFiche.count
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8797e441-16ba-4622-8b76-e7cf180ea893") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("a57675b7-f659-4b60-8d25-20aa53edff1e") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f3e1ccb4-aae0-414e-b4ca-d49258825048") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ff7c671c-36d8-4bed-b958-e90a1e0ef487") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ad174b00-5c7e-4b77-8fe7-cee44e1a1d0b") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6ce504dc-dce8-4b9a-b3c9-370a966900b1") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5953d918-e40e-4488-ad09-b46cba27bf06") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("dc2ce77a-85f9-44fe-941f-1d53a0a6fab6") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e18f4b0b-79b4-42b6-9d53-6ebe79d31143") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("6f15c1f7-ae0f-482a-9b6c-7727aa6d143a") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9b12b2f2-5484-4ebd-a870-01e131644dcc") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("71d5cfcc-e24f-45e5-bccf-747c705ee561") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5856e22b-f48e-435d-b163-e3dfec5b4fd6") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("cc4c0b5c-8041-4c88-bcc1-488a4941106a") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ce5f32fa-13a6-4d09-b9ea-b3baa5680084") _ 
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5418dd3a-7bf8-44d8-9fba-0388dd5bf7b3") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("47047f4f-fa07-4703-ad6a-ce69eae87eda") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("abb359b5-968a-4a93-a2e1-644d18baba2c") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fa87a40a-4e0f-47bf-8c50-972611198d31") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("00c00e4e-9023-4977-8c3b-3eea03362912") _
	    Then 
	   
			if CountFiche>0 Then 
				Info8.GetOnTickChecklistParameter().Result="false" 
			 	Info8.GetOnTickChecklistParameter().Description=" به علت پرداخت فیش توسط مالک مجاز به ابطال درخواست نمی باشید" 							
			 	Exit Function
			End if 	
			Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
        	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
        	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
        	Dim ss as Integer =0
        	For ss=0 to lstOwner.count-1
        		Sms(lstOwner(ss).OwnerMobile,"ابطال درخواست"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " به دلیل عدم انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی  ابطال گردید .شهرداری مشهد") 
        	Next
		  	Sms(Info8.GetRequest.Info.Cellphone,"ابطال درخواست"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " به دلیل عدم انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی  ابطال گردید .شهرداری مشهد") 
		  	
		    
		Info8.GetFunctions.SendRequestToGarbage(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc) , Guid.Parse (Info8.GetOnTickChecklistParameter.NidUser), "", 3, "عدم انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی")	 
		Dim WorkFlow As New Global.Biz.SC.ClsNewWorkFlow.ClsSetProcStatusMessageContract
        WorkFlow.NidProc=Info8.GetOnTickChecklistParameter().NidProc.tostring
	    WorkFlow.NidUser="CD03B7F0-B61A-11EB-A116-951CC710A286"
	    WorkFlow.ProcStatus = 2
	    WorkFlow.UserName ="SaraSys"
	    Info8.GetFunctions.WF_SetProcStatus(WorkFlow)
	End if 
	' sp 432030 or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5d7ada12-32ec-48a4-b72d-360f87f057b3") _ 
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("858bf483-6bf7-47fe-b5e0-f1e39c8c619c") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("054c4117-4ecc-4546-bcd8-b8199ed32b3b") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c4c029bf-15e4-40f1-9f01-a8caca7b99de") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7630d13d-52da-41de-a180-86be9a551024") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1eafa87b-de91-4b49-ac25-82b8812ca46c") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7f494f57-64db-4295-8d2e-45f15e6f7493") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("7b526248-c781-497f-891d-653dc4d2b8b7") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5f68090f-2d5d-42a4-ac32-4f2233f8ead8") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("45d4741d-af70-4d40-828e-0b16ff04b8b0") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("cd79b990-ba7a-437b-9b2e-b6ab381ad516") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("369b9139-2441-46aa-9f35-2f06a2ce1a0e") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("11fcf0c5-38c6-4e06-8166-e0ff87abbfc3") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("07dedd9e-7ae4-4f4c-8f94-7ac8fb502778") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("842e5852-b974-4cbf-87ab-0e076317a4d6") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("a427547d-d16e-4cd5-a29d-99e03a20976f") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0d5805a9-7e69-487f-8824-bdc3433e4e43") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("19816c01-56d4-4511-a11c-ae7ca4cf3a18") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("b520f0be-0872-491b-aa5f-ac7c0aa2c945") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ea93af6c-645a-4609-b7b0-a7fcd88bd000") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("156f6cf8-a2ad-4e3e-86b2-adaf20e8cda1") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1eafa87b-de91-4b49-ac25-82b8812ca46c") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9974282b-0c5f-4c7a-a0c0-d77045366b6e") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ca4861f2-cfc6-4874-ac36-2a2b112a77df") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("04940b3c-2725-4a4c-8af2-0dadb778e7b3") _
	Then
	 	if CountFiche>0 Then 
			Info8.GetOnTickChecklistParameter().Result="false" 
		 	Info8.GetOnTickChecklistParameter().Description=" به علت پرداخت فیش توسط مالک مجاز به ابطال درخواست نمی باشید" 							
		 	Exit Function
		End if 	
	 	
	 	Call EbtallAndPyamak("ابطال درخواست"," به دلیل عدم انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی  ابطال گردید ") 
	End if     
'sp 369100    If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("24b4dfda-ed6b-418e-88a5-57cbc1f671c8") _
'sp 369100    or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f43ee678-a3c6-4def-8cd2-7453b9aecd93") _
'sp 369100    or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1cbe4464-aa96-4def-bcc9-1a6caaa37ed2") _
'sp 369100    or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3e00cfb8-a8a8-4f77-a32b-a0f3e645436f") _
'sp 369100    or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9b18157e-2737-4631-a6b7-9a9f7d95c574") _
'sp 369100    or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d146c8d3-ee2a-4e74-8bea-bf5c39025a2d") _
'sp 369100    or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e386bd04-d79f-467f-94fc-fb840fee05cc") _
'sp 369100    or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("dc3ce724-8d43-444a-9408-6b95e8dc9a76") _
'sp 369100    or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2e264804-99fe-4afa-85d3-2737a636bd09") Then 
'sp 369100    	if CountFiche>0 Then 
'sp 369100			Info8.GetOnTickChecklistParameter().Result="false" 
'sp 369100		 	Info8.GetOnTickChecklistParameter().Description=" به علت پرداخت فیش توسط مالک مجاز به ابطال درخواست نمی باشید" 							
'sp 369100		 	Exit Function
'sp 369100		End if 	
    'sp 361794
    ' or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("cab9b25c-284a-413d-bdd8-e402fc30a7f6")
    'or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("1565f4e3-9513-4a12-8005-b522d58f1c70") _
    'ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("839f8728-756f-4db1-acfe-a5a9c25cfc3d") _
    'ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f3ea25fc-c4bc-4995-b812-e3086c18891b") _
   'sp 369100 		Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
   'sp 369100     	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Info8.GetRequest.Info.NidProc)
    'sp 369100    	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
   'sp 369100     	Dim ss as Integer =0
   'sp 369100     	For ss=0 to lstOwner.count-1
    'sp 369100    		Sms(lstOwner(ss).OwnerMobile,"ابطال درخواست"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & " به دلیل عدم انطباق حدود و وقوع ملک با اسناد در محل  ابطال گردید .شهرداری مشهد") 
    'sp 369100    	Next
	'sp 369100	  	Sms(Info8.GetRequest.Info.Cellphone,"ابطال درخواست"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & " به دلیل عدم انطباق حدود و وقوع ملک با اسناد در محل ابطال گردید .شهرداری مشهد") 
		  		
    		
  'sp 369100  	Info8.GetFunctions.SendRequestToGarbage(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc) , Guid.Parse("CD03B7F0-B61A-11EB-A116-951CC710A286"), "SaraSys", 3, "عدم انطباق حدود و وقوع ملک با اسناد در محل")
  'sp 369100  	Dim WorkFlow As New Global.Biz.SC.ClsNewWorkFlow.ClsSetProcStatusMessageContract
  'sp 369100      WorkFlow.NidProc=Info8.GetOnTickChecklistParameter().NidProc.tostring
'sp 369100	    WorkFlow.NidUser="CD03B7F0-B61A-11EB-A116-951CC710A286"
'sp 369100	    WorkFlow.ProcStatus = 2
'sp 369100	    WorkFlow.UserName ="SaraSys"
'sp 369100	    Info8.GetFunctions.WF_SetProcStatus(WorkFlow)
'sp 369100	End if   
'sp 415924 


if Info8.GetRequest.Info.CI_Requester=13 Then
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("ca2bba59-a69b-4285-9472-318200615290") Then SaveCheck ("2a420b4d-824b-48eb-be31-c4b6cfcba144",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("123bd225-1646-4efd-b43a-084591e29e63") Then SaveCheck ("ffaf3562-cda4-4dd9-af39-2b2497dd602f",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("effc6adf-527a-4f6a-8fc6-835ef3c56a85") Then SaveCheck ("9eb286f1-92fa-4058-844b-a357a4757b02",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("dd812862-87a8-4a76-af80-317a10d1d985") Then SaveCheck ("aedda915-74e1-4005-bb0d-ba19284a2313",2,0)   
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("29c26a91-4909-49ee-a024-158c020250d4") Then SaveCheck ("3b5593e9-1500-442b-8d0a-1eb9828b7ce6",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("004dd219-a352-4694-a334-5cb22c117774") Then SaveCheck ("a93d7692-2bbd-4c50-8943-c3d6af003717",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("5d4447fe-5b42-41b8-9b09-dbc91c9af114") Then SaveCheck ("f516e5ff-9260-4378-b993-dd67b37fdfa5",2,0) 	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2f6ff027-3a65-4e14-97e8-a50af0921360") Then SaveCheck ("5281dc4b-3126-4c57-8f8c-a2aa9b8e4b39",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2f29a6d4-da6d-40b6-a69e-3fd57adaaff0") Then SaveCheck ("ba11d7e3-8c8c-4b90-8e40-4d22f353f910",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("173d3fa8-74a9-4381-be04-c45d4e6a3bf7") Then SaveCheck ("4ff9c5ff-ae03-4914-8240-326ffd2d3b9f",2,0) 
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("26d26060-429a-4855-b6c4-1df4b02dd1e6") Then SaveCheck ("ba11d7e3-8c8c-4b90-8e40-4d22f353f910",2,0) 
End if 	
	
If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("29265586-4b87-4f6c-bcd2-ba97f28af632") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("4619c940-ab88-4bd9-bba2-9d386a8b4a83") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("49fd7080-6439-47f4-8cc4-2a5763197b78") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("31941212-8c34-4e09-bdd1-717ac643e82f") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("3247024c-70b2-4268-aef4-4b326b63d4d5") _	
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fcbaf72c-c120-438a-8339-a8e35607e4e4") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e84773fe-b1f0-43a0-b363-3698f6bc926a") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("bfa754b5-13d9-4f4b-a703-e317894eba97") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("8069c7d2-a597-4778-a2d3-8628b2b593fa") _
	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("e79e2300-b10a-4900-b53f-96cdb5d031d5") _
 	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f9c770ea-c977-4878-b5c4-02a3f063748d") _
 	or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9e84b9a7-e405-42ad-8689-411b15f8c425") _
 	 Then
		
	   if CountFiche>0 Then 
			Info8.GetOnTickChecklistParameter().Result="false" 
		 	Info8.GetOnTickChecklistParameter().Description=" به علت پرداخت فیش توسط مالک مجاز به ابطال درخواست نمی باشید" 							
		 	Exit Function
		End if 	
		Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
        lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
       	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
       	Dim ss as Integer =0
        For ss=0 to lstOwner.count-1
       		Sms(lstOwner(ss).OwnerMobile,"ابطال درخواست"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " به دلیل عدم احراز هویت مالک  ابطال گردید .شهرداری مشهد") 
       	Next
	  	Sms(Info8.GetRequest.Info.Cellphone,"ابطال درخواست"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & "و کدنوسازی " & Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring & " به دلیل عدم احراز هویت مالک  ابطال گردید .شهرداری مشهد") 
		  		
		
		Info8.GetFunctions.SendRequestToGarbage(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc) , Guid.Parse (Info8.GetOnTickChecklistParameter.NidUser) , "", 3, "احراز هویت مالک")
    
    	Dim WorkFlow As New Global.Biz.SC.ClsNewWorkFlow.ClsSetProcStatusMessageContract
        WorkFlow.NidProc=Info8.GetOnTickChecklistParameter().NidProc.tostring
	    WorkFlow.NidUser="CD03B7F0-B61A-11EB-A116-951CC710A286"
	    WorkFlow.ProcStatus = 2
	    WorkFlow.UserName ="SaraSys"
	    Info8.GetFunctions.WF_SetProcStatus(WorkFlow)
	End if
	

' عدم تایید فاکتور
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("fae4ba0f-e639-4d88-95e3-a3133e3c674d") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("17d51622-a4f3-49f6-97e1-f6f36a76c717") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("26bb2813-0a2f-433a-b1a0-19360481426f") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("300718ee-70af-41f8-ab2e-2a9e426e3e01") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9583d2ed-6b34-4103-b040-c0cde2378551") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("c688b096-a0a1-4be4-8e96-28e017c707bf") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f170f6c7-9c90-4a6a-8073-dc80e968c252") _
	   or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("2d9dc4f1-345a-456d-a97d-af1a50036814") Then 
	
		if CountFiche>0 Then 
			Info8.GetOnTickChecklistParameter().Result="false" 
		 	Info8.GetOnTickChecklistParameter().Description=" به علت پرداخت فیش توسط مالک مجاز به ابطال درخواست نمی باشید" 							
		 	Exit Function
		End if 	
		
		if AddDateForHolidays(Info8.GetRequest.Info.RequestDate, 3)  > BIZ.SA.ClsCommon.CurrentShamsiDateString Then
			Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description="فرصت 72 ساعت مالک هنوز به اتمام نرسیده" 						
		End if 
		if Info8.GetOnTickChecklistParameter().Result= "true" Then 
			Call EbtallAndPyamak("ابطال درخواست"," به دلیل عدم پرداخت فاکتور نقشه برداری توسط مالک ، ابطال گردید .شهرداری مشهد")
		End if	
			
	End if 
	
	

	'سابقه مجاز 
'sp 363012	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("80359f1d-adea-473e-8575-8cc85c92ff46") or ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("81880309-22c1-45b8-b82a-a71bba41f61b") Then
'sp 363012		
'sp 363012		if CountFiche>0 Then 
'sp 363012			Info8.GetOnTickChecklistParameter().Result="false" 
'sp 363012		 	Info8.GetOnTickChecklistParameter().Description=" به علت پرداخت فیش توسط مالک مجاز به ابطال درخواست نمی باشید" 							
'sp 363012		 	Exit Function
'sp 363012		End if 	
'sp 363012		
'sp 363012		Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
'sp 363012       	lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Info8.GetRequest.Info.NidProc)
'sp 363012       	if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
'sp 363012       	Dim ss as Integer =0
'sp 363012       	For ss=0 to lstOwner.count-1
'sp 363012        	Sms(lstOwner(ss).OwnerMobile,"ابطال درخواست"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & " به دلیل عدم  مدارک و اسناد آپلود شده گردید .شهرداری مشهد") 
'sp 363012       	Next
'sp 363012	  	Sms(Info8.GetRequest.Info.Cellphone,"ابطال درخواست"," شهروند گرامی : درخواست شما به شماره " & Info8.GetRequest.Info.NidWorkItem & " به دلیل عدم  مدارک و اسناد آپلود شده  ابطال گردید .شهرداری مشهد") 
'sp 363012		  		
'sp 363012		
'sp 363012		Info8.GetFunctions.SendRequestToGarbage(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc) , Guid.Parse("CD03B7F0-B61A-11EB-A116-951CC710A286"), "SaraSys", 3, "کنترل مدارک و اسناد آپلود شده")
 'sp 363012   
 'sp 363012   	Dim WorkFlow As New Global.Biz.SC.ClsNewWorkFlow.ClsSetProcStatusMessageContract
'sp 363012        WorkFlow.NidProc=Info8.GetOnTickChecklistParameter().NidProc.tostring
'sp 363012	    WorkFlow.NidUser="CD03B7F0-B61A-11EB-A116-951CC710A286"
'sp 363012	    WorkFlow.ProcStatus = 2
'sp 363012	    WorkFlow.UserName ="SaraSys"
'sp 363012	    Info8.GetFunctions.WF_SetProcStatus(WorkFlow)
'sp 363012	End if 


	
		'حد نگار  پایانکار بهره برداری
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("566df86c-c5e2-4676-ae69-09e55469ee8a") Then 
		if  ControlTask("Activity_09omebd")=0 or ControlTask("Activity_09omebd")=2  Then User(8) : CreatTask("Activity_09omebd",AssingTo,AssingToUserName) 
	End if
	
	'--------------------
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("0fa38ad7-2b4d-4aed-aa6d-a9803ef8f1bc") Then 
		if ControlTask("Activity_1ovdth7")=1 Then User(1) : CreatTask("Activity_1ovdth7",AssingTo,AssingToUserName) 
	End if 
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("83202524-7324-43e9-8828-cf68ec38d35d") Then 
		if ControlTask("Activity_0l172de")=1 Then User(1) : CreatTask("Activity_0l172de",AssingTo,AssingToUserName) 
	End if 
	'فیش پروانه احداث
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("9ba57e8f-c46a-4459-aabc-ebf0cc28efc0") Then
		
		SaveCheck("973af4ac-8902-46d3-b915-4b0742199e88",2,0) 
		SaveCheck("f3a59888-194d-478d-ae10-76c1d88f227b",2,0) 	'sp 486714
	End if
	'فیش پروانه توسعه
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("d0d21315-6639-48c7-b315-8f55d0a8eac1") Then
	
		SaveCheck("4eeda033-0ac8-4bba-bdea-3f8850279fc6",2,0) 
		SaveCheck("c5e47f6f-acc3-4bcb-aa3e-4a2fb755272d",2,0) 	'sp 486714
	End if
'sp 378247  ایجاد ناظر با عدم چک لیست 
	If ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("933fc7ee-9853-4b47-977d-dc6ca25991a0") Then
	
		if ControlTask("Activity_0d3lpag")=0 or  ControlTask("Activity_1m37txg")=2  Then User(1) : CreatTask("Activity_0d3lpag",AssingTo,AssingToUserName) 
	End if 
	
	if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f2b5444e-ba63-4035-ba94-72e02317a338") Then
	End if 
		' --------------


 
   if ucase(Info8.GetOnTickChecklistParameter().NidChecklistValue.tostring) =ucase("f4211155-b93d-401a-97b5-332e75c13e17") Then
		if ControlTask("Activity_0goatsn")<>1 Then Call User(1) : CreatTask("Activity_0goatsn",AssingTo,AssingToUserName)  
	End if  

	
		
	Call SmsOwner()	
End Function

Private Function  FunBlackListNew(ByVal CheckExport as Integer )

'CheckExport =0  صدور توقف
 'CheckExport=0  توقف صدور مشکل ندارد
 	
Dim LstNosaziSelf  As New List(Of Global.Biz.SA.ClsNosaziCodeType)
Dim BlackList_temp As New List(Of DtoSA.Sh_BlackList)
Dim LstNosazicode  As New List(Of Global.Biz.SA.ClsNosaziCodeType)
Dim LstNosazi_apartem As New List(Of Global.Biz.SA.ClsNosaziCodeType)
Dim LstNosazi_sakhteman As New List(Of Global.Biz.SA.ClsNosaziCodeType)
Dim LstNosazi_melk As New List(Of Global.Biz.SA.ClsNosaziCodeType)

LstNosazicode = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)



LstNosazi_apartem = LstNosazicode.Where(Function(f) f.EumNosaziCodeObjType = 4).ToList
LstNosazi_sakhteman = LstNosazicode.Where(Function(f) f.EumNosaziCodeObjType = 3 or  f.EumNosaziCodeObjType = 5 ).ToList
LstNosazi_melk = LstNosazicode.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
LstNosaziSelf =  LstNosazicode.Where(Function(f) f.NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode).ToList


 Dim StrBlackComm as String 	=""	         	         
 Dim BlackList As New List(Of DtoSA.Sh_BlackList)
 BlackList=Info8.GetFunctions.CheckInBlackList(Info8.GetRequest.Info.NidNosaziCode)

 Dim CkeBlakList as Integer =0
 Dim DomainBlakList as String =""



	
 if  Not BlackList.any()  Then
     CkeBlakList=0
 Else
 
	Dim i as Integer 
	For i=0 to BlackList.count-1
	
		if BlackList(i).EumBlackListControl=0 or BlackList(i).EumBlackListControl=CheckExport Then '*
			 
			CkeBlakList=0'*
		Else'
			CkeBlakList=1
			DomainBlakList=BlackList(i).domain
			StrBlackComm=BlackList(i).EnterComments
			Exit For
		End if'*
	Next			
End if	
	

 
  			
   

If LstNosaziSelf(0).EumNosaziCodeObjType=4 And CkeBlakList =0 Then



BlackList_temp=Info8.GetFunctions.CheckInBlackList(LstNosazi_sakhteman(0).NidNosaziCode)

 if  Not BlackList_temp.any()  Then
     CkeBlakList=0
 Else
 
	Dim j as Integer 
	For j=0 to BlackList_temp.count-1
		if BlackList_temp(j).EumBlackListControl=0 or BlackList_temp(j).EumBlackListControl=CheckExport Then '*
		Else'
			CkeBlakList=1	
			DomainBlakList=BlackList_temp(j).domain
			StrBlackComm=BlackList_temp(j).EnterComments
			Exit For
		End if'*
	Next
  End if
  
 		
  
      
 	
	if CkeBlakList=1 Then
	
	
	
	
	
	
	Else
		BlackList_temp=Info8.GetFunctions.CheckInBlackList(LstNosazi_melk(0).NidNosaziCode)
		if  Not BlackList_temp.any()  Then
		     CkeBlakList=0
		 Else
		 
			Dim h as Integer 
			For h=0 to BlackList_temp.count-1
				if BlackList_temp(h).EumBlackListControl=0 or  BlackList_temp(h).EumBlackListControl=CheckExport Then '*
				Else'
					CkeBlakList=1
					DomainBlakList=BlackList_temp(h).domain
					StrBlackComm=BlackList_temp(h).EnterComments
					Exit For	
				End if'*
			Next
		End if			
				
	End if
End if

	 


If LstNosaziSelf(0).EumNosaziCodeObjType=3 And CkeBlakList =0  Then


BlackList_temp=Info8.GetFunctions.CheckInBlackList(LstNosazi_melk(0).NidNosaziCode)


 if  Not BlackList_temp.any()  Then
     CkeBlakList=0
 Else
 
	Dim m as Integer 
	For m=0 to BlackList_temp.count-1
		if BlackList_temp(m).EumBlackListControl=0 or BlackList_temp(m).EumBlackListControl=CheckExport Then '*
		Else'
			CkeBlakList=1
			DomainBlakList=BlackList_temp(m).domain
			StrBlackComm=BlackList_temp(m).EnterComments
			Exit For 	
		End if'*
	Next
  End if			
End if

    			
	
	
   	

Select Case DomainBlakList
    Case "Commission20"
        DomainBlakList ="کمیسیون20"
    Case "Commission77"
        DomainBlakList = "کمیسیون77"
    Case "Estate"
        DomainBlakList = "املاک"
    Case "Monitoring"
        DomainBlakList = "مانیتورینگ"
    Case "PathOccupation"
        DomainBlakList = "اشغال معبر"
    Case "Sara8"
        DomainBlakList = "شهرسازی"
    Case "سرا8"
        DomainBlakList = "شهرسازی"
    Case Else 
        DomainBlakList = "سایر"
    End Select



if  CkeBlakList =0 Then 		         

Else
	Info8.GetOnTickChecklistParameter().Result="false"
	Info8.GetOnTickChecklistParameter().Description= " کد نوسازی در لیست سیاه " & DomainBlakList &" به علت :" & StrBlackComm & "  می باشد "

End if
 
Dim LstOwner As New List(Of Global.DtoSC.Base_Owner)


Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))
' sp 359885

If TmpNosazi.EumNosaziCodeObjType = 4 Then
	Info8.ParameterList.Remove("Revisit_Apartment_NidBase")
    Info8.ParameterList.Add("Revisit_Apartment_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
    LstOwner = Info8.GetRevisit_Apartment.GetBase_Owner()
   
ElseIf TmpNosazi.EumNosaziCodeObjType = 5 Then
	Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
    LstOwner = Info8.GetRevisit_HouseSharing.GetBase_Owner()
ElseIf TmpNosazi.EumNosaziCodeObjType = 2 Then
    Info8.ParameterList.Remove("Revisit_House_NidBase")
    Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
    LstOwner = Info8.GetRevisit_House.GetBase_Owner()
Else
	Dim TmpErrorg As New Global.Biz.SA.clsBizError
	Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)
 
    ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
    Info8.ParameterList.Remove("Revisit_House_NidBase")
    Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(ListParent(0).NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
	LstOwner = Info8.GetRevisit_House.GetBase_Owner()
End If

Dim ii as Integer 


If LstOwner.Count > 0 Then
   For ii = 0 To LstOwner.Count - 1
       If Info8.GetFunctions.CheckInBlackListWithNationalCode(LstOwner(ii).NationalCode) Then
	   		Info8.GetOnTickChecklistParameter().Result="false"
			Info8.GetOnTickChecklistParameter().Description= "مالک در لیست سیاه می باشد "
		End If
   Next
 End If	
 

 
 End Function 
 
Private function LstUserGroupId(ByVal LstUser() as String) as Integer


		
Dim flagb as Integer = 0

Dim GroupIn(),TempStr as String
Dim UserList As New List(Of BIZ.SA.ClsUserGroup)

UserList = Info8.User.UserGroups

Dim i as Integer

if UserList.Any Then 
  For i=0 to UserList.Count-1
  	TempStr +=  UserList(i).GroupGuid.ToString()
   	if i< UserList.Count-1 Then TempStr = TempStr & ","
  Next i
End if

GroupIn=TempStr.Split(",")
For Each a as String in GroupIn

  For Each b As String In LstUser
  
	  If Ucase(b) = Ucase(a) Then flagb=1
  Next
Next

Return flagb
	
End function

Private Function Taghsid(ByVal nidpro as guid,ByVal Check as Integer) as Integer
    Dim kk As New Global.Biz.SC.ClsNidLib

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
        Dim tarikhFich as String 
        
        
        
        Info8.Income_Fiche=Nothing
        TmpAllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(nidpro)

 
 
        Dim Tmptt As String
        Try
        	tmptt = Info8.GetFunctions.GetActiveRevisitBaseInfo(nidpro).NidRevisit.tostring
        Catch ex As Exception
        	Return RtnResult
    	End Try
        
	     
	      Dim strCodeNew as String
	      Dim tmpDistrict as Integer=0
	      
			strCodeNew = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
		   
		    tmpDistrict =  strCodeNew.Split("-")(0)
	        If tmpDistrict <> 80 Then
	
	            Str_Income = "64,78,1,8,15,22,29,36,90,91,92,93,94,95,96,97,98,99,100,43,50,71,100,101,105,106,107,108,109,110,111,112,113,114,115,116"
	        Else
	            Str_Income = "1,7,10,11"
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
	                
	                AllFiche = AllFiche.Where(Function(f) f.EumFicheStatus = 3 Or f.EumFicheStatus = 5).ToList()
	                   
	                If AllFiche.Any Then
	               
				
	                	'Dim NidIncome= Info8.FindParameter("NidIncome")
	                	
						tarikhFich =AllFiche.Max(Function(f) f.BankPaymentDate)
						if tarikhFich="" Then tarikhFich=AllFiche.Max(Function(f) f.ExportPermanentDate) 
						
						For i=0 to AllFiche.count-1 
							Try
	                			Info8.ParameterList.Remove("NidIncome")
	                			Info8.ParameterList.Add("NidIncome", AllFiche(i).NidIncome.tostring)
	                		 Catch ex As Exception
        			     	End Try	
	                	
	                    	Info8.Installment = Nothing'*2544139
	                    	 
	                		Info8.GetInstallment.LoadObj (AllFiche(i).NidFiche,1)
	                		Dim LstInstal As New List(Of Global.DtoSC.Installment_List)
	                	
				            LstInstal = Info8.GetInstallment.GetInstallment_List()
				            
				           
				            if LstInstal.count>0 Then 
				           
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
       
    
      
       Return RtnResult       
    
Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی Taghsid",  ex.Message )

    End Try

End Function


Public Function TaeedTaghsit() As Integer


    Dim Check_com As Integer = 1
    Try
        
       
	
            Dim ListMangerConfim As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimTaeed As List(Of DtoSC.Sh_ManagerConfirm)
            Dim ListMangerConfimCancel As List(Of DtoSC.Sh_ManagerConfirm)

            ListMangerConfim = Info8.GetManagerConfirm.GetSh_ManagerConfirm_List()
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence = 1).ToList
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceManagerConfirm = 16 And f.EumManagerConfirmLicence = 2).ToList
              
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
      
    Catch ex As Exception
   ' plogfile("1","1" )
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*هشدار *", "اطلاعات وارد شده در فرم تایید مدیران برای گواهی نادرست است")
        'msgbox("ملک - بازدید	",ex.tostring)
        Throw ex
        
    End Try
    Return Check_com
End Function


Public Function TaeedMMokateb() As Integer


    Dim Check_com As Integer = 1
    Try
        
       
	
            Dim ListMangerConfim As New List(Of DtoSC.Sh_MokatebatConfirm )
            Dim ListMangerConfimTaeed  As New List(Of DtoSC.Sh_MokatebatConfirm )
            Dim ListMangerConfimCancel As New List(Of DtoSC.Sh_MokatebatConfirm )

            ListMangerConfim = Info8.GetMokatebatConfirm.Sh_MokatebatConfirm_List()
           
       
            
            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = 13 And  f.EumManagerConfirmLicence = 1).ToList
            
       
            
            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = 13 And  f.EumManagerConfirmLicence = 2).ToList
           
             
              
            If ListMangerConfimTaeed.Any Then
            
                If ListMangerConfimCancel.Any Then
                    If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString < ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                        
                        Check_com = 0
                    Else
                        If ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString = ListMangerConfimTaeed.Max(Function(f) f.ConfirmationDate).ToString Then
                            Dim tmpC As String = ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString

                            ListMangerConfimTaeed = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = 13  And f.EumManagerConfirmLicence = 1 And f.ConfirmationDate = tmpC).ToList
                            ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = 13 And f.EumManagerConfirmLicence = 2 And f.ConfirmationDate = tmpC).ToList

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
        
    Catch ex As Exception
     Dim TmpError As String = ex.Message
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "WWهشدار", "اطلاعات وارد شده در فرم تایید مدیران برای مکاتبات نادرست است")
        'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "4444هشدار", TmpError)
        'msgbox("ملک - بازدید	",ex.tostring)
        Throw ex
    End Try
    Return Check_com
End Function

Public Function TaeedM() as Integer
Dim Check_com as Integer =1
Try

 Dim ListMangerConfim As New List(Of DtoSC.Sh_MokatebatConfirm)
  ListMangerConfim = Info8.GetMokatebatConfirm.Sh_MokatebatConfirm_List()
 
'Return 1 
'Dim tmpStr as String =  Info8.GetOnTickChecklistParameter().NidProc.tostring'Info8.GetRequest.Info.NidProc.ToString
'Info8.GetOnTickChecklistParameter().Result="false"	
'	Info8.GetOnTickChecklistParameter().Description =1
'	Exit Function
 ' Dim TmpInfo8 As New Biz.SC.ClsObjectFactory
        		' TmpInfo8.ManagerConfirm = Nothing
 ' Try
  '  Info8.ParameterList.Remove("NidProc")
'	Info8.ParameterList.Add("NidProc", tmpStr)
'Catch ex As Exception
'End Try	
	                	
        		  
       
        		 
'  TmpInfo8.MokatebatConfirm = Nothing
' Info8.GetMokatebatConfirm.LoadObj(Guid.Parse(tmpStr))
  ' Info8.GetOnTickChecklistParameter().Result="false"	
'	Info8.GetOnTickChecklistParameter().Description =2
'	Exit Function
'if ListMangerConfim.count = 0 Then Return 1

 
 Dim ListMangerConfimTaeed As New List(Of DtoSC.Sh_MokatebatConfirm)
 Dim ListMangerConfimTaeedNew As New List(Of DtoSC.Sh_MokatebatConfirm)
 
 Dim ListMangerConfimCancel As New List(Of DtoSC.Sh_MokatebatConfirm)
 

 ListMangerConfim = Info8.GetMokatebatConfirm.Sh_MokatebatConfirm_List()
 'sp 318326
 'sp 318162
 

 


 	
 if ListMangerConfim.count = 0 Then Return 1 
 
 Dim M_D=  ListMangerConfim.Max(Function(f) f.ConfirmationDate).ToString
 ListMangerConfimTaeed=ListMangerConfim.Where(Function(f) f.ConfirmationDate=M_D).ToList
 Dim M_T=  ListMangerConfimTaeed.Max(Function(f) f.ConfirmationTime).ToString
 ListMangerConfimTaeed=ListMangerConfimTaeed.Where(Function(f) f.ConfirmationTime=M_T).ToList
 
 ListMangerConfimTaeed = ListMangerConfim.Where(Function(f)  f.CI_ResourceMokatebatConfirm = 113 And (f.EumManagerConfirmLicence = 3 or f.EumManagerConfirmLicence=1)).ToList
 
 
 if ListMangerConfimTaeed.count>0 Then 
 	Check_com=0 
 	Return Check_com
 	Exit Function
 End if 
 
 'sp 318326
 ListMangerConfimTaeed = ListMangerConfim.Where(Function(f)  f.CI_ResourceMokatebatConfirm = 113 And (f.EumManagerConfirmLicence = 1 )).ToList


 if ListMangerConfimTaeed.count < 2 Then 
 '	Check_com=1 
 '	Return Check_com
 '	Exit Function
 End if 
 ListMangerConfimCancel = ListMangerConfim.Where(Function(f) f.CI_ResourceMokatebatConfirm = 113 And f.EumManagerConfirmLicence = 2).ToList
        
If ListMangerConfimTaeed.Any Then

	If ListMangerConfimCancel.Any Then
		
		Dim tmpC as String=ListMangerConfimCancel.Max(Function(f) f.ConfirmationDate).ToString
		ListMangerConfimCancel=ListMangerConfimCancel.Where(Function(f) f.ConfirmationDate=tmpC).ToList
		Dim tmpT as String=ListMangerConfimCancel.Max(Function(f) f.ConfirmationTime)
	
		ListMangerConfimTaeedNew= ListMangerConfim.Where(Function(f) f.ConfirmationDate>tmpC).ToList
		if ListMangerConfimTaeedNew.count> 2 Then 
			Check_com=0 
			Return Check_com
 			Exit Function
 		Else
 			ListMangerConfim= ListMangerConfim.Where(Function(f) f.ConfirmationDate=tmpC And f.ConfirmationTime>tmpT).ToList
 			if ListMangerConfimTaeedNew.count+ListMangerConfim.count > 2 Then 
 				Check_com=0 
 				Return Check_com
 				Exit Function
 			Else
 				Check_com=1 
 				Return Check_com
 				Exit Function
 			End if 
 		End if	
	Else
		Check_com=0 
		Return Check_com
 		Exit Function
    End if
Else    
	Check_com=1 
	Return Check_com
 	Exit Function    
End if 


Catch ex As Exception
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار","اطلاعات وارد شده در فرم تایید مدیران برای گواهی نادرست است") 
 'msgbox("ملک - بازدید	",ex.tostring)
 Info8.GetOnTickChecklistParameter().Result="false"	
	Info8.GetOnTickChecklistParameter().Description =ex.tostring
	Exit Function
 Throw ex
End Try 
Return Check_com
End Function


Public Function APNumber(ByVal lstChild as List(Of Global.Biz.SA.ClsNosaziCodeType),ByVal NOAp as Integer ) As Boolean

    Try
        
		Dim aa as Integer =0 
		For aa=0 to lstChild.count
			if lstChild(aa).Apartment= NOAp Then Return True		
		Next
      
    Catch ex As Exception
         
    End Try
    Return False
End Function
				

Private Function Fa_NahiyeDaramadi() As Integer
Try

 	Dim i,IndexKey,NahiyeDaramadiTmp As Integer
	Dim Key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)

	Dim P_LapList As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
	Dim P_LapListTmp As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
	Dim P_LapListGharb As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
	Dim LayerListId as String =""
	Dim gu As New Guid
	Dim gu2 as New DtoSC.Base_NosaziCode 
							    
	Dim NidBaseTo As New Guid
	Dim tmpNidR As New Guid
	Dim NidNosaziCode As New Guid
	NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
	Dim tmpNickName() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	tmpNickName = tmpNosaziNickName.Split("-")	
    Dim district , house,Region  as Double
    district=tmpNickName(0)
    Region=tmpNickName(1)
    house =tmpNickName(3)
      					
		Dim ListParent,ListParentDas As New List(Of BIZ.SA.ClsNosaziCodeType)
		ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NidNosaziCode, 0)
		
		ListParentDas= ListParent.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
		
		if ListParentDas.any Then
			Info8.ParameterList.Remove ("Parvandeh_HouseSharing_NidBase")
			Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", ListParentDas(0).NidBase.tostring())
			gu2 = Info8.GetParvandeh_HouseSharing.GetBase_NosaziCode   
				
		Else	
		
			ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
			
			Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
			Info8.ParameterList.Add("Parvandeh_House_NidBase", ListParent(0).NidBase.tostring())
			gu2 = Info8.GetParvandeh_House.GetBase_NosaziCode
		End if	
									

			 
							 
	gu = gu2.NidNosaziCode	
		
	Dim strnosazicode As String = ""
	strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)

	 
		
	Dim KeyObj as Object
					
	if district <> "80" And house > "10000" Then 
		Key_List =Info8.GetMap().GetMapFind(strnosazicode,"1991")
		KeyObj=Key_List(0).Guid	
		IndexKey=1991														
	''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
	
	Else if district = 80 Then
		if House > 10000 Then 
			IndexKey=3710
			key_List =Info8.GetMap().GetMapFind(strnosazicode,"3710")
 			keyObj= Key_List(0).Guid
        ElseIf Region = 80 Then
			IndexKey=3706
			key_List =Info8.GetMap().GetMapFind(strnosazicode,"3706")
 			keyObj= Key_List(0).Guid 
			 
		Else
			IndexKey=5
			key_List =Info8.GetMap().GetMapFind(strnosazicode,"5")
 			
			Try
				keyObj= Key_List(0).Guid
			
			Catch
				IndexKey=839
				key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
 				keyObj= Key_List(0).Guid
 				
			End Try
	 	End if
	''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''														   
	Else	
	
		Try
	
			Key_List =Info8.GetMap().GetMapFind(strnosazicode,"5")
			 
			KeyObj=Key_List(0).Guid	
			IndexKey=5
			
		Catch ex As Exception
		
			msg(ex.message)
		
			 Key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
			 KeyObj=Key_List(0).Guid	
			 IndexKey=839
			 
			 
		End Try
			
   End if
  
 
  
   LayerListId="10265"'188518-677
		    
   P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring,LayerListId) 
   P_LapListTmp =Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring,"10006") 
   P_LapListGharb=Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring,"11740") 
   	
'   	  msg(P_LapList.count .tostring)
   	
   if P_LapList.count >0 Then 
	P_LapList=P_LapList.where (Function (f) f.LayerID=10265).tolist ()'677
	
		
	
	if P_LapList.count >0 Then NahiyeDaramadiTmp =IIf( CStr(P_LapList(0).Code) = "", 0,  P_LapList(0).Code)
	
   End if                          		
     
     Return NahiyeDaramadiTmp
 Catch Ex as Exception
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Fa_NahiyeDaramadi", "خواندن از روی نقشه" & ex.message )
End Try 
End Function

Private Function Fa_Bimeh() As Integer
Try

'msg("222")
'Exit Function

 	Dim i,IndexKey,NahiyeDaramadiTmp As Integer
	Dim Key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)

	Dim P_LapList As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
	Dim P_LapListTmp As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
	Dim P_LapListGharb As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
	Dim LayerListId as String =""
	Dim gu As New Guid
	Dim gu2 as New DtoSC.Base_NosaziCode 
							    
	Dim NidBaseTo As New Guid
	Dim tmpNidR As New Guid
	Dim NidNosaziCode As New Guid
	NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
	Dim tmpNickName() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	tmpNickName = tmpNosaziNickName.Split("-")	
    Dim district , house,Region  as Double
    district=tmpNickName(0)
    Region=tmpNickName(1)
    house =tmpNickName(3)
    
'    msg(district.tostring + " " + Region.tostring + " "  + house.tostring)
 '   Exit Function
      					
		Dim ListParent,ListParentDas As New List(Of BIZ.SA.ClsNosaziCodeType)
		ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(NidNosaziCode, 0)
		
		ListParentDas= ListParent.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
		
		if ListParentDas.any Then
			Info8.ParameterList.Remove ("Parvandeh_HouseSharing_NidBase")
			Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", ListParentDas(0).NidBase.tostring())
			gu2 = Info8.GetParvandeh_HouseSharing.GetBase_NosaziCode   
				
		Else	
		
			ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
			
			Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
			Info8.ParameterList.Add("Parvandeh_House_NidBase", ListParent(0).NidBase.tostring())
			gu2 = Info8.GetParvandeh_House.GetBase_NosaziCode
		End if	
									

			 
							 
	gu = gu2.NidNosaziCode
		
	Dim strnosazicode As String = ""
	strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)

		
	Dim KeyObj as Object
		
    	
	if district <> "80" And house > "10000" Then 
		Key_List =Info8.GetMap().GetMapFind(strnosazicode,"1991")
		KeyObj=Key_List(0).Guid	
		IndexKey=1991														
	''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
	
	Else if district = 80 Then
		if House > 10000 Then 
			IndexKey=3710
			key_List =Info8.GetMap().GetMapFind(strnosazicode,"3710")
 			keyObj= Key_List(0).Guid
        ElseIf Region = 80 Then
			IndexKey=3706
			key_List =Info8.GetMap().GetMapFind(strnosazicode,"3706")
 			keyObj= Key_List(0).Guid 
			 
		Else
			IndexKey=5
			key_List =Info8.GetMap().GetMapFind(strnosazicode,"5")
 			
			Try
				keyObj= Key_List(0).Guid
			
			Catch
				IndexKey=839
				key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
 				keyObj= Key_List(0).Guid
 				
			End Try
	 	End if
	''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''														   
	Else	
	
		Try
	
			Key_List =Info8.GetMap().GetMapFind(strnosazicode,"5")
			 
			KeyObj=Key_List(0).Guid	
			IndexKey=5
			
		Catch ex As Exception
		
			 Key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
			 KeyObj=Key_List(0).Guid	
			 IndexKey=839
		End Try
			
   End if
  
   LayerListId="13866 "'188518-677
		    
   P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring,LayerListId) 
   
   if P_LapList.count >0 Then 
  	 NahiyeDaramadiTmp =IIf( CStr(P_LapList(0).Code) = "", 0,  P_LapList(0).Code)
 Else
 	NahiyeDaramadiTmp=0
   End if 
 Return NahiyeDaramadiTmp
 Catch Ex as Exception
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Fa_Bimeh", "خواندن از روی نقشه" )
End Try 
End Function

Public Function ExecSp(ByVal NameSp as String, ByVal Name as String) 
 
	Dim p_Connection As String = "Data Source=172.16.8.136;Initial Catalog=Sara8M03;Persist Security Info=True;User ID=esup92;Password=zxc@789-sf"
	Dim intRowsAffected As String = 0
	Dim con As System.Data.SqlClient.SqlConnection = New System.Data.SqlClient.SqlConnection(p_Connection)
	
	
	Dim TextSms as String =""
	Dim Count as String=""
	Dim cmd As New System.Data.SqlClient.SqlCommand(NameSp, con)
	cmd.CommandType = CommandType.StoredProcedure
	cmd.Parameters.AddWithValue("@NidProc", Info8.GetRequest.Info.NidProc)
	cmd.Connection = con
	con.Open()
    Dim adp As New SqlDataAdapter(cmd)
    Dim ds As DataSet = New DataSet()
   
    cmd.ExecuteNonQuery()
	adp.Fill(ds, NameSp)
	Dim Ret As String 
	Dim Mobile as String
Try
	Ret= ds.Tables(0).Rows(0).Item(0).ToString
   	
 	TextSms = ds.Tables(0).Rows(0).Item(1).ToString 
 	Mobile = ds.Tables(0).Rows(0).Item(2).ToString 
 	Count = ds.Tables(0).Rows(0).Item(3).ToString 
 Catch
End Try 	
 	
	con.Close()
	con = Nothing
if NameSp <> "[dbo].[SP_CopyOwnerShahkar]" And  NameSp <> "[dbo].[SP_ParKing]" Then
	if NameSp = "[dbo].[SP_BazdidSara10]" Then
	 	Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
	    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
	    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
	    Dim ss as Integer =0
	
'sp 455029
'	Sms(lstOwner(ss).LawyerMobile,"شهروند گرامی: درخواست شما به شماره پرونده " & Info8.GetRequest.Info.Nidworkitem.tostring & " به کارشناس منتخب بازدید شهرداری آقای " & Name & " برای انجام بازدید ارسال گردید ، هزینه خدمات بازدید درخواست شما با احتساب 9% مالیان بر ارزش افزوده مبلغ " & Ret.tostring & " ریال می باشد، لطفا مبلغ فوق پس از هماهنگی با شماره تماس ................ به شماره کارت ................... به نام............ واریز نمایید، نهایتا طی 72 ساعت پس از واریز وجه و اعلام به کارشناس بازدید، پاسخگویی انجام خواهد شد)	
'end sp	   
	   
	    For ss=0 to lstOwner.count-1
	      if lstOwner(ss).LawyerMobile.tostring.Length =11 Then
				Sms(lstOwner(ss).LawyerMobile,"فاکتور بازدید"," مالک محترم مبلغ فاکتور خدمات بازدید   کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " با احتساب 10% مالیات بر  ارزش افزوده: " + Ret.tostring + " ریال می باشد " _
				+ " که توسط کارشناس بازدید دریافت می شود. ضمنا فاکتور حاضر بر مبنای سابقه مجاز ملک/آپارتمان برآورد گردیده که در صورت تغییر گروه ساختمانی فاکتور تغییر خواهد یافت.شهرداری مشهد ")
		  End if 	
	      Sms(lstOwner(ss).OwnerMobile,"فاکتور بازدید"," مالک محترم مبلغ فاکتور خدمات بازدید   کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " با احتساب 10% مالیات بر  ارزش افزوده: " + Ret.tostring + " ریال می باشد " _
				+ " که توسط کارشناس بازدید دریافت می شود. ضمنا فاکتور حاضر بر مبنای سابقه مجاز ملک/آپارتمان برآورد گردیده که در صورت تغییر گروه ساختمانی فاکتور تغییر خواهد یافت.شهرداری مشهد ")
	    Next
	     
	    Sms(Info8.GetRequest.Info.Cellphone,"فاکتور بازدید"," مالک محترم مبلغ فاکتور خدمات بازدید   کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " با احتساب 10% مالیات بر  ارزش افزوده: " + Ret.tostring + " ریال می باشد " _
				+ " که توسط کارشناس بازدید دریافت می شود. ضمنا فاکتور حاضر بر مبنای سابقه مجاز ملک/آپارتمان برآورد گردیده که در صورت تغییر گروه ساختمانی فاکتور تغییر خواهد یافت.شهرداری مشهد ")
	ElseIf NameSp = "[dbo].[SP_Kargozari10]" Then
		Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
		
	    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
	    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
	    Dim ss as Integer =0
	   
	    For ss=0 to lstOwner.count-1
	      if lstOwner(ss).LawyerMobile.tostring.Length =11 Then
				Sms(lstOwner(ss).LawyerMobile,"فاکتور حق الزحمه نماینده فنی"," شهروند گرامی : " + lstOwner(0).LawyerFirstName + " " + lstOwner(0).LawyerLastName + " مبلغ فاکتور خدمات نماینده فنی جهت کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " باشماره ارجاع "+ Info8.GetRequest.Info.NidWorkitem.tostring + " به شرح ذیل میباشد " _
					+"بهای خدمات حق الزحمه نماینده فنی + 9% مالیات ارزش افزوده: " + Ret.tostring + " ریال می باشد .شهرداری مشهد ")
		  End if 		
	      Sms(lstOwner(ss).OwnerMobile,"فاکتور حق الزحمه نماینده فنی"," شهروند گرامی : " + lstOwner(0).LawyerFirstName + " " + lstOwner(0).LawyerLastName + " مبلغ فاکتور خدمات نماینده فنی جهت کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " باشماره ارجاع "+ Info8.GetRequest.Info.NidWorkitem.tostring + " به شرح ذیل میباشد " _
			+"بهای خدمات حق الزحمه نماینده فنی + 9% مالیات ارزش افزوده: " + Ret.tostring + " ریال می باشد .شهرداری مشهد ")
	    Next
	     'Sms("09155193570","bb","countNew" & Ret.tostring  & Info8.GetRequest.Info.NidWorkitem.tostring )
	      Sms(Info8.GetRequest.Info.Cellphone,"فاکتور حق الزحمه نماینده فنی"," شهروند گرامی : "& Info8.GetRequest.Info.RequesterName.tostring & " مبلغ فاکتور خدمات نماینده فنی جهت کد نوسازی " + Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode) + " باشماره ارجاع "+ Info8.GetRequest.Info.NidWorkitem.tostring + " به شرح ذیل میباشد " _
			+"بهای خدمات حق الزحمه نماینده فنی + 9% مالیات ارزش افزوده: " + Ret.tostring + " ریال می باشد .شهرداری مشهد ")			

'sp 455029
'	 ElseIf NameSp = "[SP_ShahrsaziParking]" Then
'		Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
'		
'	    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Info8.GetRequest.Info.NidProc)
'	    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
'	    Dim ss as Integer =0
'	   
'	    For ss=0 to lstOwner.count-1
'	      if lstOwner(ss).LawyerMobile.tostring.Length =11 Then
'				Sms(lstOwner(ss).LawyerMobile,"فاکتور خدمات بازدید","شهروند گرامی: درخواست شما به شماره پرونده "+ Info8.GetRequest.Info.NidWorkitem.tostring +" به کارشناس منتخب بازدید شهرداری آقای "+ Name + " برای انجام بازدید ارسال گردید ، هزینه خدمات بازدید درخواست شما با احتساب 9% مالیان بر ارزش افزوده " _
'					+ "مبلغ " + Ret.tostring + " ریال می باشد لطفا مبلغ فوق پس از هماهنگی با شماره تماس " + Mobile.tostring +" به شماره کارت ایشان واریز نمایید، نهایتا طی 72 ساعت پس از واریز وجه و اعلام به کارشناس بازدید، پاسخگویی انجام خواهد شد . شهرداری مشهد @")
'		  End if 		
'	      Sms(lstOwner(ss).OwnerMobile,"فاکتور خدمات بازدید","شهروند گرامی: درخواست شما به شماره پرونده "+ Info8.GetRequest.Info.NidWorkitem.tostring +" به کارشناس منتخب بازدید شهرداری آقای "+ Name + " برای انجام بازدید ارسال گردید ، هزینه خدمات بازدید درخواست شما با احتساب 9% مالیان بر ارزش افزوده مبلغ "  _
'					 + "مبلغ " + Ret.tostring + " ریال می باشد لطفا مبلغ فوق پس از هماهنگی با شماره تماس " + Mobile.tostring +" به شماره کارت ایشان واریز نمایید، نهایتا طی 72 ساعت پس از واریز وجه و اعلام به کارشناس بازدید، پاسخگویی انجام خواهد شد . شهرداری مشهد@ ")
'	    Next
'	      Sms(Info8.GetRequest.Info.Cellphone,"فاکتور خدمات بازدید","شهروند گرامی: درخواست شما به شماره پرونده "+ Info8.GetRequest.Info.NidWorkitem.tostring +" به کارشناس منتخب بازدید شهرداری آقای "+ Name + " برای انجام بازدید ارسال گردید ، هزینه خدمات بازدید درخواست شما با احتساب 9% مالیان بر ارزش افزوده مبلغ "  _
'					 + "مبلغ " + Ret.tostring + " ریال می باشد لطفا مبلغ فوق پس از هماهنگی با شماره تماس " + Mobile.tostring +" به شماره کارت ایشان واریز نمایید، نهایتا طی 72 ساعت پس از واریز وجه و اعلام به کارشناس بازدید، پاسخگویی انجام خواهد شد . شهرداری مشهد @ ")		
 'end sp 455029  
 
	Else
		
		Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
	    lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))
	    if lstOwner.count>0 Then lstOwner = lstOwner.Where(Function(f) f.OwnerMobile<> Info8.GetRequest.Info.Cellphone).tolist
	    Dim ss as Integer =0
	   	
	    For ss=0 to lstOwner.count-1
	      if lstOwner(ss).LawyerMobile.tostring.Length =11 Then
				Sms(lstOwner(ss).LawyerMobile,"فاکتورنقشه برداری",TextSms)
		  End if 	
	      Sms(lstOwner(ss).OwnerMobile,"فاکتورنقشه برداری",TextSms)
	    Next
	     Sms(Info8.GetRequest.Info.Cellphone,"فاکتورنقشه برداری", TextSms)
	End if
End if  
if NameSp = "[dbo].[SP_ParKing]" Then 
	 AssingTo =ret
	 AssingToUserName =TextSms
	 TmpMobile=Mobile
	 TmpCount=Count
End if 
End Function


Private Function InsertReportA(ByVal pNidNosaziCode As Guid, ByVal pEngTitle As String, ByVal pPerTitle As String, ByVal pReportPath As String, ByVal pOutputNo As String, ByVal pDistrict As String,ByVal Group as Integer)
Try
          ' ReportArchive =1
            Dim tmpExportLicence As New DtoDbSaraAnls.Sara8Sys_ExportLicence
            
            tmpExportLicence.ExportDate = Biz.SA.ClsCommon.CurrentShamsiDateString
            
            tmpExportLicence.ExportTime = Date.Now.Hour.ToString.PadLeft(2, "0") & ":" & Date.Now.Minute.ToString.PadLeft(2, "0") & ":" & Date.Now.Second.ToString.PadLeft(2, "0")
            tmpExportLicence.ExportNidUser = Guid.Parse("00c21306-2043-428f-9f9d-f211ff85fedc")'Info8.User.UserGuid
            tmpExportLicence.ExportUserName = "Admin"'Info8.User.UserName
            
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

              ClsErrorResult = Info8.GetFunctions.ExportLicenceInCentral(tmpExportLicence, pDistrict,Group,Info8.GetRequest.Info.NidWorkItem.tostring, Info8.Security_RequestGuid, Info8.Security_EncryptCode)
            ' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " ClsErrorResult.Count " ,   Info8.GetFunctions.ErrorResult.BizErrors.Count ) 
            ' If ucase(Info8.User.UserGuid.tostring) = ucase("540d119d-14f1-4f65-94d1-7f39fc2b3b48") Then Exit Function 
'	Info8.GetOnTickChecklistParameter().Result="false"	
'	Info8.GetOnTickChecklistParameter().Description =ClsErrorResult.count
'	Exit Function
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

Public Sub msg(message As String)
    
        If message = "" Then
            Info8.GetOnTickChecklistParameter.Result = "true"
        Else
            Info8.GetOnTickChecklistParameter.Description = message 
            Info8.GetOnTickChecklistParameter.Result = "false"
        End If

        'logToFile(nosaziNickName, "MessageContract", message, Info8.GetOnTickChecklistParameter.Result)
End Sub



#Region "Save to archives"


    Private Function InsertReport(nidProc As Guid, clsNidLib As Biz.SC.ClsNidLib,
                                  outputNo As String,
                                  reportPath As String,
                                  archiveDomain As String,
                                  outputEngTitle As String,
                                  outputPerTitle As String,
                                  outputDate As String,
                                  entityId As Integer) As IList(Of Biz.SC.SrvReportService.ClsRenderName)
        Try
    

            Dim outRenderNamesList As New List(Of Biz.SC.SrvReportService.ClsRenderName)
            Dim nidNosaziCode As Guid

			If clsNidLib.RMainCodeInfo.Shop > 0 Then
                nidNosaziCode = clsNidLib.RSenfi.NidNosaziCode
            ElseIf clsNidLib.RMainCodeInfo.Apartment > 0 Then
                nidNosaziCode = clsNidLib.RApartment.NidNosaziCode
            Else
                nidNosaziCode = clsNidLib.RHouse.NidNosaziCode
            End If
            'nidNosaziCode = clsNidLib.RMainCodeInfo.NidNosaziCode
            
               
     

            '-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-
            Dim tmpExportLicence As DtoDbSaraAnls.Sara8Sys_ExportLicence
            
            tmpExportLicence = New DtoDbSaraAnls.Sara8Sys_ExportLicence
            tmpExportLicence.ExportDate = Biz.SA.ClsCommon.CurrentShamsiDateString
            tmpExportLicence.ExportTime = String.Format("{0}:{1}:{2}",
                                                        Date.Now.Hour.ToString.PadLeft(2, "0"),
                                                        Date.Now.Minute.ToString.PadLeft(2, "0"),
                                                        Date.Now.Second.ToString.PadLeft(2, "0"))
             
                                                     
             

            tmpExportLicence.ExportNidUser =Guid.Parse("00c21306-2043-428f-9f9d-f211ff85fedc")
            
               
            tmpExportLicence.ExportUserName = "Admin"'Info8.User.UserName
            
            tmpExportLicence.NidNosaziCode = nidNosaziCode
            tmpExportLicence.NidProc = nidProc
            tmpExportLicence.OutputNo = outputNo
            tmpExportLicence.ReportPath = reportPath
                                                       
         
           
            tmpExportLicence.ArchiveDomain = archiveDomain 'clsNidLib.RMainCodeInfo.District
            tmpExportLicence.OutputEngTitle = outputEngTitle
            tmpExportLicence.OutputPerTitle = outputPerTitle
            tmpExportLicence.OutputDate = outputDate 'Biz.SA.ClsCommon.CurrentShamsiDateString
                 
         
            

            '-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-
            Dim archiveWrappersList As List(Of DtoSC.Archive_Wrapper)
            archiveWrappersList = Info8.GetFunctions.GetArchiveBizCode(nidNosaziCode)
            
            
             
            If archiveWrappersList.Any Then archiveWrappersList = archiveWrappersList.Where(Function(x) x.CI_ArchiveGroup = 1).ToList
            For i As Integer = 0 to archiveWrappersList.count -1
         '       Info8.GetOnTickChecklistParameter().Result = "false"
   '  Info8.GetOnTickChecklistParameter().Description= archiveWrappersList(i).CI_ArchiveGroup
                            
'Exit Function
            Next
       '      Info8.GetOnTickChecklistParameter().Result = "false"
   '  Info8.GetOnTickChecklistParameter().Description= archiveWrappersList.count
                            
'Exit Function
          
            If archiveWrappersList.Any Then
            
           

                For Each archiveWrapper As DtoSC.Archive_Wrapper In archiveWrappersList
                    Dim tmpResult As New List(Of Biz.SC.SrvReportService.ClsRenderName)
                             
        
                    tmpExportLicence.ArchiveBizCode = archiveWrapper.BizCode
                   ' Info8.GetOnTickChecklistParameter().Result="false"
  			'		Info8.GetOnTickChecklistParameter.Description= nidProc.tostring'Info8.Security_EncryptCode.ToString() '"s"'
'Exit Function
               
                 
                    tmpResult = Info8.GetFunctions.ExportLicenceInCentral(tmpExportLicence, archiveDomain, 605, clsNidLib.RRequest.NidWorkItem.ToString(), Info8.Security_RequestGuid,Info8.Security_EncryptCode )'
          
     Info8.GetOnTickChecklistParameter().Result = "false"
     Info8.GetOnTickChecklistParameter().Description= tmpResult.count
                            
Exit Function
		  
         
                    outRenderNamesList.AddRange(tmpResult)
                  
                       	 
                         
         
                                        
	
 

                Next archiveWrapper
         
                
          

            End If

            ' نمایش خطاها
            '-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-
            If (Info8.GetFunctions().ErrorResult IsNot Nothing) Then
                If (Info8.GetFunctions().ErrorResult.BizErrors IsNot Nothing ) Then
                    For Each bizErr As Biz.SA.clsBizError In Info8.GetFunctions().ErrorResult.BizErrors
                       ' Info8.AddError(bizErr.ErrorAction, "خطا درج در آرشیو", bizErr.ErrorKey & ":" & VbCrLf & bizErr.ErrorTitel)
                       
                       
                    Next
                End If
            End If

            '-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-
            Return outRenderNamesList
        Catch ex As Exception
      
           ' Info8.AddError(Biz.SA.EumErrorAction.Warning, "insertReport", ex.ToString)
            	Info8.GetOnTickChecklistParameter().Result="false"
	
	Info8.GetOnTickChecklistParameter.Description=ex.ToString
	Exit Function
            Throw ex
        End Try
    End Function


#End Region
' sp 481228
Public Function Check_ComNama() as Integer
	Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
		Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
		Dim ListParent_M As List(Of BIZ.SA.ClsNosaziCodeType)
		
	Try	
 		TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))	
	 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)
	 	ListParent_M=ListParent
	Catch
	End Try
	 

				Dim M_BaseUsing,TmpM_BaseUsing as New  List(of DtoSc.Base_Using)	
											
'				Try  
'					Info8.ParameterList.Remove("Revisit_Building_NidBase")
'	   				Info8.ParameterList.Add("Revisit_Building_NidBase", Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
			
'				 	M_BaseUsing = Info8.GetRevisit_Building.GetBase_Using
'				 	
'				Catch
				
'					Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
'				    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)							
'					M_BaseUsing = Info8.GetRevisit_HouseSharing.GetBase_Using
					
'				End Try 
		Try
				
				M_BaseUsing = Info8.GetMapControl.GetBase_Using
		Catch e as exception
		
			msg(e.message)
			Exit Function
	
				
		End Try		
				

	'======================================================================================================================================================================================
				
	'		Dim M_BaseUsing,TmpM_BaseUsing as New  List(of DtoSc.Base_Using)
	'		
	'		M_BaseUsing = Info8.GetRevisit_Building.GetBase_Using
	'		TmpM_BaseUsing=M_BaseUsing.where(Function(f) f.CI_UsingGroup=13 ).tolist
	'    	Dim M_UsingArea as Double 
	'    	Dim M_Nama as Integer=0
	'    	
	'    	M_UsingArea=TmpM_BaseUsing.sum(Function (f) f.UsingArea)
	' 	
	 '   	if M_UsingArea >0 Then M_Nama=1
				

	'======================================================================================================================================================================================
				
					
				TmpM_BaseUsing=M_BaseUsing.where(Function(f) f.CI_UsingGroup=13 or f.CI_UsingGroup=15  or f.CI_UsingGroup=14 or f.CI_UsingGroup=19 ).tolist
		    	Dim M_UsingArea as Double 
		    	Dim M_Nama as Integer=0
		    	
		    	M_UsingArea=TmpM_BaseUsing.sum(Function (f) f.UsingArea)		 	
		    	if M_UsingArea >0 Then M_Nama=1
		    	
		    	if M_Nama=0 Then 
		    		TmpM_BaseUsing=M_BaseUsing.where(Function(f) f.CI_UsingGroup=12 ).tolist
		    		M_UsingArea=TmpM_BaseUsing.sum(Function (f) f.UsingArea)
		    		if M_UsingArea >2000  Then M_Nama=1
		    	End if
		    		
		    	if M_Nama=0 And M_BaseUsing.count>0 Then 
		    		M_UsingArea=M_BaseUsing.Max(Function (f) f.BuildingNo)
		   	
		    		if M_UsingArea >1 Then M_Nama=1
		    	End if
		    	'sp 371209 
		      	if M_Nama=0 And M_BaseUsing.count>0 Then 
		    			'sp 414875  
    				Dim LstFloor as New  List(of DtoSc.Base_Using)
    				LstFloor = M_BaseUsing.where(Function (f) f.FloorNo<100).tolist() 
    				
		    		M_UsingArea=LstFloor.Max(Function (f) f.FloorNo) -  M_BaseUsing.Min(Function (f) f.FloorNo) +1
		    		if M_UsingArea >10 Then M_Nama=1
		    	End if 
			    	
		    	
		    	if M_Nama=0 And M_BaseUsing.count>0 Then 
		    		M_UsingArea=M_BaseUsing.sum(Function (f) f.UsingArea)
		    		if M_UsingArea >5000  Then M_Nama=1
		    	End if 	
		    	
		    

			    	
		    	
		    	if M_Nama=0 Then 
		    	
		    		if Info8.GetRequest.Info.ActiveNidZabeteh=Guid.Empty Then
			    	ElseIf Info8.GetRequest.Info.ActiveNidZabeteh.tostring="" Then 
		
					Else
					 
						Dim tmpNidZ As Guid
				  		Dim TmpZabete As New DtoSC.Zabeteh
				  		Dim MapNama as Integer=0
			Try	 
				  		Dim M_ZabetehDetails As  List(Of DtoSC.Zabeteh_Details)
				  		M_ZabetehDetails = New List(Of DtoSC.Zabeteh_Details)
				   	    tmpNidZ = Info8.GetRequest.Info.ActiveNidZabeteh				   	    
			      		Info8.Zabeteh=Nothing
				  		Info8.ParameterList.REMove("NidZabeteh")
				  		Info8.ParameterList.Add("NidZabeteh", tmpNidZ.ToString)
					    Info8.ParameterList.REMove("NidNosaziCode")					    
				        Info8.ParameterList.Add("NidNosaziCode",Ucase(ListParent(1).NidNosaziCode.tostring))	'Info8.GetRevisit_House.GetBase_NosaziCode. ' باید نوسازی ملک باشه که ردیف داشته باشه در دیتا بیس ضابطه
				        			        
				  	 	TmpZabete =Info8.GetZabeteh(True).GetZabeteh
				  		M_ZabetehDetails = Info8.GetZabeteh(True).GetZabeteh_Details
				  		if M_ZabetehDetails.count>0  Then 
							M_ZabetehDetails=M_ZabetehDetails.where(Function(f) f.CI_Zabeteh=246 And f.ValueZabeteh=1).tolist
				  			if M_ZabetehDetails.count>0  Then M_Nama=1
				 		End if	 	
			Catch ex as exception
					
				Info8.GetOnTickChecklistParameter().Result="false"
				Info8.GetOnTickChecklistParameter().Description="چک کنترل کمیته نما"+ex.message
				Exit Function		   	
		    End Try	
						
					End if
					
		   	End if   
	   	
		   	if ControlMap > 0 Then 	M_Nama=1	'sp 454792
		   	Return M_Nama
		   	
End function

Public Function ControlMap() as Integer



  Try

      		Dim BaseEdgePathWidth as Double
            Dim LstNosaziCode_T As New List(Of Global.Biz.SA.ClsNosaziCodeType)
            Dim LstTmpNosaziCode_T As New List(Of Global.Biz.SA.ClsNosaziCodeType)
            
           LstNosaziCode_T = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(guid.parse(Info8.FindParameter("NidNosaziCode")), 0)    
        '   LstNosaziCode_T = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(guid.parse("6c0de3ca-1358-4393-bb1f-83bdbbf241f8"), 0)    
           LstTmpNosaziCode_T = LstNosaziCode_T.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList           
                
            Dim i As Integer = 0
            Dim MapId As Integer = 0
            Dim LayerId As String
           
            Dim Max As Double
            Dim p_OverLapArea As Double
                       
           


	            ' خواندن اطلاعات نقشه
	            Dim gu As New Guid   
	      
	            gu = LstTmpNosaziCode_T(0).NidNosaziCode
	             Dim strnosazicode As String = ""
	            strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
            
            Dim tmpMapFind As New List(Of BIZ.SC.SrvMap.MapService_SrvFindResult)

            tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "5")
            'sp 341714
            'sp 406753 
            	
            if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "839")
            
          	if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "1991")
       		if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3706")
       		if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3710")
       	
' 839  	لایه وضع قدیم
'1991	لایه پروژه های سرمایه گذاری 
'3706	لایه تجمیع موقت قطعه ملکی منطقه ثامن 
'3710	لایه پروژه 

	
     		
            If tmpMapFind.Any Then
             
               Dim key As New Object 
		   	    if Not tmpMapFind is Nothing And tmpMapFind.count > 0 Then   key = tmpMapFind(0).Guid
                Dim TmpSesion As New Global.Biz.SC.SrvMap.ClsGeoSession
		   	    TmpSesion = Nothing
                TmpSesion=Info8.GetMap.GeoSessionAddWKT(key.tostring,tmpMapFind(0).wkt.Tostring,40,23)
				   	   
			   Dim TmpExtend As List(Of Global.Biz.SC.SrvMap.clsF)
				 TmpExtend = Info8.GetMap.GetExtendedProp(TmpSesion.SessionId.Tostring ,10296) '10316				 				 
				 			 				 
				if  TmpExtend.count > 0 Then Return 1	     
 
 
                Dim tmpMapService As New System.Collections.Generic.List(Of BIZ.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
                tmpMapService = Info8.GetMap().GetOverlapListCompleteDataByID(tmpMapFind.First.Guid.ToString, "10297") '??????
                



                If tmpMapService.Any Then
                    Max = 0
                    For i = 0 To tmpMapService.Count - 1
                        Dim Code As String = IIf(IsDBNull(tmpMapService.Item(i).Code), 0, tmpMapService.Item(i).Code)
                       
                        Dim OverLapArea As Double = IIf(IsDBNull(tmpMapService.Item(i).OverLapArea), 0, tmpMapService.Item(i).OverLapArea)
                       
                        If OverLapArea >= Max Then   
                        
	                        MapId = Val(Code)
	                        Max = OverLapArea
	                        Return 1
                        End if
                          
                    ''    if MapId = 1 Then GoTo  Exitif 
                       
                    Next                   
              
                End If
                   
          End if

      Return 0
           
        Catch Ex As Exception
         
            Info8.AddError(BIZ.SA.EumErrorAction.Warning, "کمیته نما", "کنترل نقشه" & VbCrLf & ex.ToString)
        End Try
End function

<DisplayName("خواندن براکف")> Private Function Base_Edg() as Double
Dim LBase_Edge As New List(Of DtoSC.Base_Edge)
    LBase_Edge = Info8.GetBarokaf.GetBase_Edge
    
Dim M_dtJahatA as New List(Of DtoSc.Base_Edge)  
Dim  tmp(),TmpArrey as String 
Dim i,K as Integer
Dim M_PathWidth(10),MaxPathW,M_PathWidthEslah(10) as Double 
   Dim  MaxArz as Double = 0

    if LBase_Edge.any Then M_dtJahatA=LBase_Edge
    
        
    	 For i=1 to 8 
		
						M_dtJahatA = LBase_Edge.Where(Function(f) f.CI_SideCode=i).ToList()
						
						if M_dtJahatA.count>0 Then
						        TmpArrey =iif (isdbnull (M_dtJahatA.Max(Function(f) f.PathWidth)),"0",M_dtJahatA.Max(Function(f) f.PathWidth))
	
								tmp =TmpArrey.Split("+") 
								
								For k = 0 To tmp.Length - 1
									if IsNumeric (tmp(k)) Then
										M_PathWidth(i)+= Convert.ToDouble(tmp(k)) 'Arze Mabar
									Else
										M_PathWidth(i)=0
									End if	
								Next 
					            
						        M_dtJahatA = LBase_Edge.Where(Function(f) f.CI_SideCode=i).ToList()
								TmpArrey =iif (isdbnull (M_dtJahatA.Max(Function(f) f.AfterEditWidth)),"0",M_dtJahatA.Max(Function(f) f.AfterEditWidth))
								tmp =TmpArrey.Split("+") 
								
								For k = 0 To tmp.Length - 1
									if IsNumeric (tmp(k)) Then
										M_PathWidthEslah(i)+= Convert.ToDouble(tmp(k)) 'Arze GozareEslahi
									Else
										M_PathWidthEslah(i)=0
									End if	
								
								Next  
								
								if M_PathWidth(i)>MaxArz Then MaxArz=M_PathWidth(i)
								
								if M_PathWidthEslah(i)>0 And M_PathWidthEslah(i)>M_PathWidth(i)  Then 
								   M_PathWidth(i)=M_PathWidthEslah(i) 
								   MaxArz=M_PathWidth(i)
								End if
								
					    End if
						
					
         Next
   Return MaxArz

End function

Public Function ControlMap1() as Integer
        Try

      
            Dim LstNosaziCode_T As New List(Of Global.Biz.SA.ClsNosaziCodeType)
            Dim LstTmpNosaziCode_T As New List(Of Global.Biz.SA.ClsNosaziCodeType)
            
           LstNosaziCode_T = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(guid.parse(Info8.FindParameter("NidNosaziCode")), 0)        
           LstTmpNosaziCode_T = LstNosaziCode_T.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList           
                
            Dim i As Integer = 0
            Dim MapId As Integer = 0
            Dim LayerId As String
           
            Dim Max As Double
            Dim p_OverLapArea As Double
                       
           

	            ' خواندن اطلاعات نقشه
	            Dim gu As New Guid   
	      
	            gu = LstTmpNosaziCode_T(0).NidNosaziCode
	            Dim strnosazicode As String = ""
	            strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
            
            Dim tmpMapFind As New List(Of BIZ.SC.SrvMap.MapService_SrvFindResult)

            tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "5")
            'sp 341714
            'sp 406753 
            	
            if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "839")
            
          	if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "1991")
       		if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3706")
       		if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3710")
       	
' 839  	لایه وضع قدیم
'1991	لایه پروژه های سرمایه گذاری 
'3706	لایه تجمیع موقت قطعه ملکی منطقه ثامن 
'3710	لایه پروژه 

     		
     		
            If tmpMapFind.Any Then
 
                Dim tmpMapService As New System.Collections.Generic.List(Of BIZ.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
                tmpMapService = Info8.GetMap().GetOverlapListCompleteDataByID(tmpMapFind.First.Guid.ToString, "10297") '??????


                If tmpMapService.Any Then
                    Max = 0
                    For i = 0 To tmpMapService.Count - 1
                        Dim Code As String = IIf(IsDBNull(tmpMapService.Item(i).Code), 0, tmpMapService.Item(i).Code)
                       
                        Dim OverLapArea As Double = IIf(IsDBNull(tmpMapService.Item(i).OverLapArea), 0, tmpMapService.Item(i).OverLapArea)
                       
                        If OverLapArea >= Max Then   
                        
	                        MapId = Val(Code)
	                        Max = OverLapArea
                        End if
                          
                    ''    if MapId = 1 Then GoTo  Exitif 
                       
                    Next                   
              
                End If
   
          End if

         
                 
     ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "MapId", MapId.tostring)                
     ' Exit Function
        '   Exitif:      
             Return MapId 
        Catch Ex As Exception
         
            Info8.AddError(BIZ.SA.EumErrorAction.Warning, "ثبت درخواست", "خواندن از نقشه" & VbCrLf & ex.ToString)
        End Try

End Function

Public Function CheckExistRequest(ByVal NidWorkFlowDeff As String) as Boolean 

if ucase(Info8.FindParameter("NidNosaziCode")) = ucase("62e0b0b3-fd76-4132-91df-9d89b08c4208") Then Return True : Exit Function   '3/4/18/37/1
if ucase(Info8.FindParameter("NidNosaziCode")) = ucase("9C642710-A9CA-44D0-986E-16DA244DE66B") Then Return True : Exit Function  '7/2/28/8/1

	Dim BB As New List(Of Global.DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
'	Dim BB As New List (Of DtoSC.Sh_RequestInfo)
    BB = Info8.GetFunctions.GetAllRequestList(Guid.Parse(Info8.FindParameter("NidNosaziCode")))
    BB = BB.where(Function(f) f.EumRequestStatus = 0).toList()  'جاری بودن

    if BB.any() Then
        
        For Each x as DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult in BB
        
            if Ucase(x.NidWorkflowDeff.tostring) = Ucase(NidWorkFlowDeff) Then
                	
                		Return True
                		Exit Function
                		
            End if
        Next 
      End if
       Return False			' پاسخ منفی یعنی همچین درخواستی جاری وجود ندارد روی این کد نوسازی
             
End Function



' این کد برای کپی چک لیست های نود نقشه برداری است که هر مقداری گرفتن و به مرحله نقص نقشه برداری برگشت کرد مواردی که قبلا تایید یا عدم تایید شده بود نشون بده با توضیحاتش
Public sub CopyNodeChecklistToAnotherNode() 
    Dim GetCheckList As New BIZ.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
    Dim GetCheckListData as New List(of BIZ.SC.ClsNewWorkFlow.data)
	Dim i as Integer = 0
    GetCheckList = Info8.GetFunctions.WF_GetCheckListByNidProc(Info8.GetRequest.Info.Nidproc.tostring)
    GetCheckListData = GetCheckList.data

'if ucase(Info8.Getrequest.info.nidworkflowdeff.tostring) = ucase("2635fe18-fe4c-4a2d-9d05-993756751262") Then
'	GetCheckListData = GetCheckListData.where(Function(f) f.NidCheckList ="c5281b9f-fbe0-40de-a8f2-a263a2236569").tolist
'	msg(GetCheckListData.count.tostring)
'	Exit Sub
'	CopyNodeSaveCheck("1e1db1b4-12b5-4224-b466-81d7e67e4ac1",0,0,"Sys",GetCheckListData(0).Comments)  '
	
'	Else

	    GetCheckListData = GetCheckListData.where(Function(f) f.NidCheckList ="67f75965-b0b3-469a-94b6-9a4fa228c501" Or f.NidCheckList ="23d6e636-9b56-4200-9ef9-d2237b2955f6" Or _
	                                                          f.NidCheckList ="1e1db1b4-12b5-4224-b466-81d7e67e4ac1" Or f.NidCheckList ="f555829e-2fec-42e3-86f5-2ec726de5549" Or _
	                                                          f.NidCheckList ="f0f66760-ab09-4d7f-9d3e-b8f0c40182d2" Or f.NidCheckList ="cf5c846a-5af6-43c7-aaf0-90fcb7d23c16" Or _
	                                                          f.NidCheckList ="177b144c-17e7-47a0-905d-07bb5ae16cbe" Or f.NidCheckList ="88145a70-fa62-47b3-8d1f-54bbc8189c80" Or _
	                                                          f.NidCheckList ="5811e1ba-f3e3-4b1f-8a81-eaaa02b080c1").tolist
	                                                          
	 Try
	 
	    For Each x As BIZ.SC.ClsNewWorkFlow.data In GetCheckListData    
	        if ucase(x.NidCheckList)=ucase("67f75965-b0b3-469a-94b6-9a4fa228c501") Then CopyNodeSaveCheck("f8142a44-1056-4a05-9664-9a10265dea92",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments )  'احراز هویت
	        if ucase(x.NidCheckList)=ucase("23d6e636-9b56-4200-9ef9-d2237b2955f6") Then CopyNodeSaveCheck("51e95d0d-4804-4709-9424-19bc89778a26",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments )  'تکمیل مدارک
	        if ucase(x.NidCheckList)=ucase("1e1db1b4-12b5-4224-b466-81d7e67e4ac1") Then CopyNodeSaveCheck("665ff873-1d4b-4adc-a1a7-fd43fee216da",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments)  'پرداخت فاکتور نقشه برداری توسط مالک
	        if ucase(x.NidCheckList)=ucase("cf5c846a-5af6-43c7-aaf0-90fcb7d23c16") Then CopyNodeSaveCheck("f5eb3d65-d583-4025-8d39-a441ffdefa90",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments)  'انطباق محل (آدرس) با موقعیت پلاک در سیستم شهرسازی
	        if ucase(x.NidCheckList)=ucase("f555829e-2fec-42e3-86f5-2ec726de5549") Then CopyNodeSaveCheck("13fbf86e-ca48-4014-86d4-c8f544e99ce2",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments)  'انطباق مالکیت و پلاک ثبتی
	        if ucase(x.NidCheckList)=ucase("f0f66760-ab09-4d7f-9d3e-b8f0c40182d2") Then CopyNodeSaveCheck("07f76fdb-502a-4fae-8b8f-54bbf4490b8c",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments)  'انطباق حدود و وقوع ملک با اسناد در محل
	        if ucase(x.NidCheckList)=ucase("177b144c-17e7-47a0-905d-07bb5ae16cbe") Then CopyNodeSaveCheck("3085c3b5-d751-41b9-8265-abe75503d808",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments)  'بررسی و اعلام  شمولیت میزان در مسیر
	        if ucase(x.NidCheckList)=ucase("88145a70-fa62-47b3-8d1f-54bbc8189c80") Then CopyNodeSaveCheck("f82048f9-b4a3-43aa-9a00-b7b1d10d839b",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments)  'بررسی و اعلام وقوع در حرائم
	        if ucase(x.NidCheckList)=ucase("5811e1ba-f3e3-4b1f-8a81-eaaa02b080c1") Then CopyNodeSaveCheck("7a79b5c3-a6a0-4a65-98bc-1f7f01b54be3",x.CI_CheckListValue,x.OwnerConfirm,x.OwnerComments,x.Comments)  'تایید بارگذاری نقشه رقومی (ممهور) عرصه ملک و خط طرح
	   		'End if
	    Next 
	    
	 Catch  ex as exception
		msg("HI - " & ex.message & "---" & GetCheckListData.count.tostring)
	 End Try
 
' End if
End sub

Public Function CopyNodeSaveCheck(ByVal NidCheckList as String, ByVal CheckListValue as Integer,ByVal OwnerConfirm as Integer, ByVal OwnerComments as String, ByVal Comments  as String)

	Try		 
'	msg("In")	
		Dim SaveCheckList As New Global.Biz.SC.ClsNewWorkFlow.ClsSaveCheckListValueMessageContract
	
		SaveCheckList.CI_CheckListValue = CheckListValue.tostring
		if Comments is Nothing Then  Comments="1"
		SaveCheckList.Comments=Comments.tostring 
	
		SaveCheckList.NidCheckList=NidCheckList.tostring
		SaveCheckList.NidProc=Info8.GetRequest.Info.Nidproc.tostring
		SaveCheckList.NidUser="00c21306-2043-428f-9f9d-f211ff85fedc"
		SaveCheckList.FullUserName ="Sys"
	
	    SaveCheckList.OwnerConfirm =OwnerConfirm.tostring
		if OwnerComments is Nothing Then  OwnerComments="1"
	    SaveCheckList.OwnerComments=OwnerComments.tostring
	    
	   		 	
		Info8.GetFunctions.WF_SaveCheckListValue(SaveCheckList)
	 '	if Not result Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "خطا ", "نتیجه ذخیره چک لیست منفی می باشد")
	 Catch e as exception
	 	msg("HI4 - " & e.message )
	End Try	    
		   
End Function

'sp 481471
Private Function FunFire ()	
'	Dim tmp222 As New Global.Biz.Communication.ClsFireFighting
'	Dim ReturnFire as Integer = 0
	
			Dim ID as GUID = Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57")
		'	ReturnFire = tmp222.GetInstanceState(Info8.GetRequest.Info.NidWorkItem.tostring  , ID , "Esup") 
			
			Dim Fire As New Global.Biz.Communication.ClsFireFighting
	        Dim ReturnFire As New  Biz.Communication.SrvFireFighting.resualt2
			ReturnFire = Fire.GetReauestState(Info8.GetRequest.Info.NidWorkItem.tostring ,ID,"Esup")			
					'	msg("R: " & ReturnFire.returnCode.tostring)
					'	Exit Function			
			
			if ReturnFire.returnCode= 1 Then 
				msg("پرونده در اتش نشانی  در حال پیگیری می باشد")
			'	msg("1 - تا زمانی که تایید آتش نشانی نشده باشد اجازه تایید ندارید")
			
			ElseIf ReturnFire.returnCode= 2 Then 
			'	msg("پرونده در اتش نشانی تایید شده است ")
			'	Exit Function
			ElseIf ReturnFire.returnCode= 3 Then 
				msg("پرونده در اتش نشانی  رد شده است ")
		'		msg(" 3 - تا زمانی که تایید آتش نشانی نشده باشد اجازه تایید ندارید")
			
		    ElseIf ReturnFire.returnCode= 5 Then 
				msg("پرونده در حال پیگیری می باشد")
		'		msg(" 5 - تا زمانی که تایید آتش نشانی نشده باشد اجازه تایید ندارید")
			
			Else
			'	msg("شماره پرونده در اتش نشانی پیدا نشد :  " + ReturnFire.tostring)
			'	msg(ReturnFire.tostring & " تا زمانی که تایید آتش نشانی نشده باشد اجازه تایید ندارید")
			
			End if			
End Function

'sp 506151
Private function HadNegarWebService

    Dim renovationCode, taskCode, isPerson, LicenseImageBase64, plaqMain, plaqSub , postalCode , Address As String 
    Dim meterage as Double
    Dim FloorNo, UnitCount as Integer 
     

  		Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
  		TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))
  		renovationCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
  		taskCode = Info8.GetRequest.Info.NidWorkItem

	  	Dim districtA as String 
		Dim tmpDistrictA() as String
		Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
		if tmpNosaziNickName<>"" Then 
			tmpDistrictA = tmpNosaziNickName.Split("-")
			districtA=tmpDistrictA(0)
		End if


        Dim HadNegarMC as New BIZ.Communication.HadNegarMessageContract
        Dim clsHadNegar as New BIZ.Communication.ClsHadNegar
        Dim HadNegarResponse as New BIZ.Communication.HadNegarCartInsertResponse
        Dim HadNegarToken as New BIZ.Communication.GetHadNegarTokenResponse
 
 
     '================= Owner Part ==========================
     
        
        



		Dim GetOwner As List (Of DtoSC.Base_Owner)
	if Info8.GetRevisit_House.GetBase_Owner.count>0 Then	
		GetOwner = Info8.GetRevisit_House.GetBase_Owner
	ElseIf	Info8.GetParvandeh_House.GetBase_Owner.count>0 Then
		GetOwner = Info8.GetParvandeh_House.GetBase_Owner
	End if
	
        Dim OwnerList as New List(Of BIZ.Communication.HadNegarOwner)   
        
 Dim FName As String = ""
 Dim LName As String = ""
 Dim Mobail As String = ""
 Dim Code As String = ""
 
 
 Try
   		
 Dim lstOwner As New List(Of DtoDbSaraAnls.UGP_AuthenticationHistory)
 lstOwner = Info8.GetFunctions.GetUGP_AuthenticationHistory(Info8.GetRequest.Info.NidProc)
 

 If lstOwner.Count > 0 Then
 	For Each u as  DtoDbSaraAnls.UGP_AuthenticationHistory in lstOwner
 	
 	 ' Dim Owner  As New Biz.Communication.Owner
 	  Dim Owner as New BIZ.Communication.HadNegarOwner
 	  
'	if Not String.IsNullOrEmpty(u.LawyerFirstName.tostring) Then
		Mobail = u.LawyerMobile  'lstOwner(i).LawyerMobile ' u.LawyerMobile  '
	    FName = u.LawyerFirstName  'lstOwner(i).LawyerFirstName  ' u.LawyerFirstName  '
	    LName = u.LawyerLastName ' lstOwner(i).LawyerLastName '	u.LawyerLastName
	    Code = u.LawyerNationalCode  '	lstOwner(i).LawyerNationalCode ' u.LawyerNationalCode  '		

 	  
	    If FName = Nothing Then FName = u.OwnerFirstName	' lstOwner(i).OwnerFirstName
	    If LName = Nothing Then LName = u.OwnerLastName	'lstOwner(i).OwnerLastName
	    If Mobail = Nothing Then Mobail = if(String.IsNullOrEmpty(u.OwnerMobile.tostring),"0",u.OwnerMobile.tostring)		'lstOwner(i).OwnerMobile
	    If Code = Nothing Then Code = if(String.IsNullOrEmpty(u.OwnerNationalCode.tostring),"0",u.OwnerNationalCode.tostring)	'lstOwner(i).OwnerNationalCode

	
	Try
	
		if TmpNosazi.EumNosaziCodeObjType = 2 or TmpNosazi.EumNosaziCodeObjType = 3 Then
		    isPerson = Info8.GetRevisit_House.GetBase_Owner(0).EumOwnerCharacter.tostring    
			if Not Info8.GetRevisit_House.GetBase_AddressInfo.MainAddress.toString = Nothing Then
			    Owner.address = IIF(isPerson = "1", Info8.GetRevisit_House.GetBase_AddressInfo.MainAddress.toString , " " ) 
			    Owner.companyAddress = IIF(isPerson = "2", Info8.GetRevisit_House.GetBase_AddressInfo.MainAddress.toString , " " ) 
			Else
			 	 Owner.address = " "
				Owner.companyAddress=" "
			End if	
	    
		 ElseIf TmpNosazi.EumNosaziCodeObjType = 5
		
		    isPerson = Info8.GetRevisit_HouseSharing.GetBase_Owner(0).EumOwnerCharacter.tostring   
		    if Not Info8.GetParvandeh_HouseSharing.GetBase_AddressInfo.MainAddress.toString = Nothing Then
			    Owner.address = IIF(isPerson = "1", Info8.GetParvandeh_HouseSharing.GetBase_AddressInfo.MainAddress.toString , " " ) 
			    Owner.companyAddress = IIF(isPerson = "2", Info8.GetParvandeh_HouseSharing.GetBase_AddressInfo.MainAddress.toString , " " ) 
			    
			Else if Not Info8.GetRevisit_HouseSharing.GetBase_AddressInfo.MainAddress.toString = Nothing
			    Owner.address = IIF(isPerson = "1", Info8.GetRevisit_HouseSharing.GetBase_AddressInfo.MainAddress.toString , " " ) 
			    Owner.companyAddress = IIF(isPerson = "2", Info8.GetRevisit_HouseSharing.GetBase_AddressInfo.MainAddress.toString , " " ) 			
			Else
			 	Owner.address = " "
				Owner.companyAddress=" "
			End if						    
		    
		 End if   
	    
	Catch 	'ex As Exception 
	
		'Info8.AddError(BIZ.SA.EumErrorAction.warning, " آدرس ", "ملک مورد نظر فاقد آدرس میباشد")   
	   
		Owner.address = "0"
		Owner.companyAddress="0"

    End Try    
    	    
	    Owner.renovationCode = renovationCode
	    Owner.taskCode = taskCode                   'کد ارجاع-شماره درخواست
	    Owner.isPerson = IIF(isPerson = "1", True , False )                  'true :حقیقی  -  false :حقوقی
	    Owner.firstName = FName 'IIF(isPerson = "0",FName,"")
	    Owner.lastName  = LName 'IIF(isPerson = "0",LName,"")
	    Owner.nationalID = Code
	    Owner.companyNationalID = IIF(isPerson = "2", Code , " " )
	    Owner.companyName = IIF(isPerson = "2", FName + LName , " " ) 
	    Owner.tel = Mobail
	    Owner.mobile = Mobail
	    
	    Owner.companyTel = IIF(isPerson = "2", Mobail , " " ) 
	    Owner.attorney = " "
	    OwnerList.Add(Owner)	    
	    
     Next  

End if  
      
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.warning, "مشخصات مالکین", ex.message)
    End Try        

Dim ownerr as String
Dim result As New System.Text.StringBuilder()

For Each item as Object  In OwnerList
    Dim props = item.GetType().GetProperties()

    For Each p as Object In props
        Dim val As String = CStr(p.GetValue(item, Nothing))
        result.Append(val & ", ")
    Next

    result.AppendLine() ' خط جدید برای هر آیتم لیست
Next

'msg(result.tostring)
'Exit Function


	 Dim plaq  as New list(Of DtoSC.Base_RegisterPlack)
	 Try
		 if asobject = 5 Then
		   plaq = Info8.GetRevisit_HouseSharing.GetBase_RegisterPlack
		 Else
		   plaq = Info8.GetRevisit_House.GetBase_RegisterPlack
		 End if  
	 
		if plaq.any Then
			 For Each X as DtoSC.Base_RegisterPlack in plaq
				 if x.CI_RegisterPlackType = 1  Then  plaqMain = x.RegisterPlackTypeValue
				 if x.CI_RegisterPlackType = 2 Then  plaqSub = x.RegisterPlackTypeValue    
		    Next  
		Else
				plaqMain = 0
				plaqSub = 0	
		End if  
	Catch 
	
	End Try	  
	
	 
    

		Dim PostalCode_List as New list (Of DtoSC.Base_AddressPostCode)
		Try
			 if asobject = 5 Then
			   PostalCode_List = Info8.GetRevisit_HouseSharing.GetBase_AddressPostCode
			 Else
			   PostalCode_List = Info8.GetRevisit_House.GetBase_AddressPostCode
			 End if  
		Catch
		End Try	 
		 
		 if PostalCode_List.count>0 Then
		 	postalCode = PostalCode_List(0).PostCode	
		 Else
		 	postalCode = " "	
		 End if	
	
		
 	
		' ارسال منطقه (عدد اول کد نوسازی ) برای نمایش عکس پروانه
		
	    Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)     	    
		ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.info.NidNosaziCode,0)
		ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).tolist
		Dim NosaziNickName_House = Info8.GetFunctions.GetNosaziNickName(ListParent(0).NidNosaziCode)
			 		
		Dim NosaziNickName_tmp = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.info.NidNosaziCode)
		Dim NosaziRegion_str = NosaziNickName_tmp.Split("-")(0)     
		
			'========================  Map ===========================
	
	Dim Map As New Biz.SC.ClsSettings_Map
	Dim codeMap As Biz.SC.SrvMap.MapService_SrvFindResult
	codeMap = Info8.GetMap.GetMapLatAndLong(Map, NosaziNickName_House, False)
	
	Dim MapXY_str as String
	MapXY_str = codeMap.CenterX.tostring + " " + codeMap.CenterY.tostring
	
'msg(MapXY_str)
'Exit Function		

'=================================================================	 

if 1=2 Then 	 
		Dim LicenseImageBase64TMP As Byte()
	
		Try
		    
			Dim tmpParameters = New Dictionary(Of String, String)
			tmpParameters.Clear()
			tmpParameters.Add("District", NosaziRegion_str.ToString())
			tmpParameters.Add("NidProc", Info8.GetRequest.info.NidProc.ToString())
						
			Dim tmpRenderReport As New List(Of Byte())
			tmpRenderReport = Info8.GetFunctions.RenderReport(tmpParameters, "/Sara8Reports/RptMovafeghatOsooli")
			
			If tmpRenderReport IsNot Nothing Then
			    If tmpRenderReport.Any() Then
			        LicenseImageBase64TMP = tmpRenderReport(0).ToArray() ' Direct assignment
			    End If
			End If
			
        Catch ex As Exception
        
            Info8.AddError(BIZ.SA.EumErrorAction.warning, "Base64", "ارسال پرونده: " & VbCrLf & ex.ToString)
            
        End Try	
        
        Dim base64String As String = Convert.ToBase64String(LicenseImageBase64TMP)      
        
End if           
                   
'=========================================================================================                     
	Dim Base_Using As New List(Of DtoSC.Base_Using)
	Dim ownershipType , ownershipTypeVoucher , RegisterSection as String
	
	if 	asobject = 2 Then 
		Base_Using = Info8.GetRevisit_House().GetBase_Using()

	'	address = Info8.GetParvandeh_House.GetBase_AddressInfo.MainAddress.toString 'Address '//آدرس
	ElseIf asobject = 3 Then 
		Base_Using = Info8.GetRevisit_Building().GetBase_Using()
	ElseIf asobject = 5 Then 
		Base_Using = Info8.GetRevisit_HouseSharing().GetBase_Using()
	'	address = Info8.GetParvandeh_HouseSharing.GetBase_AddressInfo.MainAddress.toString
	ElseIf asobject = 4 Then 
		Base_Using = Info8.GetRevisit_Apartment().GetBase_Using()
	End if
	
	Try 
		Address = Info8.GetRevisit_House.GetBase_AddressInfo.MainAddress.toString 'Address '//آدرس
		ownershipTypeVoucher = Info8.GetRevisit_House().GetBase_CommonEstate().CI_DocType.tostring
		ownershipType = Info8.GetRevisit_House().GetBase_CommonEstate_House().CI_OwnerType.tostring	
		RegisterSection = Info8.GetRevisit_House().GetBase_CommonEstate().RegisterSection.tostring		
	Catch 	
		Address = Info8.GetRevisit_HouseSharing.GetBase_AddressInfo.MainAddress.toString
		ownershipTypeVoucher = Info8.GetRevisit_HouseSharing().GetBase_CommonEstate().CI_DocType.tostring
		ownershipType = Info8.GetRevisit_HouseSharing().GetBase_CommonEstate_House().CI_OwnerType.tostring	
		RegisterSection = Info8.GetRevisit_HouseSharing().GetBase_CommonEstate().RegisterSection.tostring		
	End Try	
	
                         
'msg(Address & " " & ownershipTypeVoucher & " " & ownershipType & " " & RegisterSection)
'Exit Function		 	

	UnitCount = Base_Using.sum(Function(f) f.UnitCount) 	
	FloorNo = Base_Using.Select(Function(f) f.FloorNo).Distinct().Count() 	
	meterage = Base_Using.sum(Function(f) f.UsingArea)


        
        Dim HN_RequestInfo as New BIZ.Communication.HadNegarRequestInfo
            HN_RequestInfo.munFileNo = Info8.GetRequest.Info.NidWorkItem.tostring
            HN_RequestInfo.munRefDate = Biz.SA.ClsCommon.CurrentShamsiDateString    
            HN_RequestInfo.systemDate = DateTime.Now
            HN_RequestInfo.meterage = meterage
            HN_RequestInfo.comMeterage = if(meterage>0,meterage,0)
            HN_RequestInfo.floors = FloorNo
            HN_RequestInfo.units = UnitCount
            

Dim TT as String            
            
       TT &= "munFileNo " & HN_RequestInfo.munFileNo & VbCrLf
       TT &= "munRefDate " & HN_RequestInfo.munRefDate & VbCrLf
       TT &= "systemDate " & HN_RequestInfo.systemDate & VbCrLf
       TT &= "meterage " & HN_RequestInfo.meterage & VbCrLf
       TT &= "comMeterage " & HN_RequestInfo.comMeterage & VbCrLf
       TT &= "floors " & HN_RequestInfo.floors & VbCrLf
       TT &= "units " & HN_RequestInfo.units & VbCrLf
       

' msg(TT)    
' Exit Function 
        
       Dim Files as New List(Of String) From {"12345", "678910"}

 

        HadNegarMC.branchId = 11
        HadNegarMC.cityId = 234
        HadNegarMC.partCode = "بخش مرکزی"
        HadNegarMC.municipalRegionCode = NosaziRegion_str.tostring
        HadNegarMC.plaqMain = plaqMain
        HadNegarMC.plaqSub = plaqSub
        HadNegarMC.registrationPartCode = RegisterSection
        HadNegarMC.ownershipTypeVoucher = ownershipTypeVoucher
        HadNegarMC.ownershipType = ownershipType
        HadNegarMC.address = Address
        HadNegarMC.plaqueAddress = Address
        HadNegarMC.postalCode = postalCode
        HadNegarMC.renovationCode = NosaziNickName_tmp
        HadNegarMC.addressMap = MapXY_str        
        HadNegarMC.owners = OwnerList
        HadNegarMC.requestInfo = HN_RequestInfo
        HadNegarMC.files = Files   
        
Dim str as String 

       str &= "branchId " & HadNegarMC.branchId & VbCrLf
       str &= " cityId " & HadNegarMC.cityId & VbCrLf
       str &= " partCode " & HadNegarMC.partCode & VbCrLf
       str &= " municipalRegionCode " & HadNegarMC.municipalRegionCode & VbCrLf
       str &= " plaqMain " & HadNegarMC.plaqMain & VbCrLf
       str &= " plaqSub " & HadNegarMC.plaqSub & VbCrLf
       str &= " registrationPartCode " & HadNegarMC.registrationPartCode & VbCrLf
       str &= " ownershipTypeVoucher " & HadNegarMC.ownershipTypeVoucher & VbCrLf
       str &= " ownershipType " & HadNegarMC.ownershipType & VbCrLf
       str &= " address " & HadNegarMC.address & VbCrLf
       str &= " plaqueAddress " & HadNegarMC.plaqueAddress & VbCrLf
       str &= " postalCode " & HadNegarMC.postalCode & VbCrLf
       str &= " renovationCode " & HadNegarMC.renovationCode & VbCrLf
       str &= " addressMap " & HadNegarMC.addressMap & VbCrLf
       

' msg(str)    
' Exit Function         
        
        
        Dim Nidproc = Info8.GetRequest().Info.NidProc
        Dim User as New BIZ.SA.ClsUser
        User.UserGuid=Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57")
	    User.UserName="esup"
	    
        Try
            HadNegarResponse = clsHadNegar.CartInsert(Nidproc,HadNegarMC,User,3)
            if HadNegarResponse.success = False Then
                Msg("به وبسرویس حدنگار ارسال نشد: " & HadNegarResponse.errorMessage & " - لطفا مجدد امتحان کنید") ' : " & HadNegarResponse.errorCode)
                Exit Function
            Else
            '	Msg("Send - ResponseCode " & HadNegarResponse.data)    
            End If
        Catch e as Exception
           Msg("HadNegar-Ex: " & e.message) 
        End Try  
 
End function

Public Function  GetBuildingGroup() as Boolean
' این تابع برای مشحص کردن گروه ساختمانی ملک میباشد

Try
		Dim FloorNumber,FloorNuMax,FloorNuMin as Integer
		Dim Metraj as Double
		
 		Dim TmpNosazi As Global.Biz.SA.ClsNosaziCodeType
		Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
		Dim ListParent_M As List(Of BIZ.SA.ClsNosaziCodeType)
		
	Try	
 		TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))	
	 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(TmpNosazi.NidNosaziCode,0)
	 	ListParent_M=ListParent
	Catch
	End Try

				Dim M_BaseUsing,TmpM_BaseUsing as New  List(of DtoSc.Base_Using)								
				Try  
					Info8.ParameterList.Remove("Revisit_Building_NidBase")
	   				Info8.ParameterList.Add("Revisit_Building_NidBase", Info8.GetFunctions.GetNidBase(Info8.GetRequest.Info.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
			
				 	M_BaseUsing = Info8.GetRevisit_Building.GetBase_Using
				 	Metraj = Info8.GetRevisit_Building.GetBase_BuildingInfo.InfrastructureArea
				 	
				Catch
				
					Info8.ParameterList.Remove("Revisit_HouseSharing_NidBase")
				    Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", Info8.GetFunctions.GetNidBase(TmpNosazi.NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)							
					
					M_BaseUsing = Info8.GetRevisit_HouseSharing.GetBase_Using
					Metraj = Info8.GetRevisit_HouseSharing.GetBase_BuildingInfo.InfrastructureArea
					
				End Try 
				
	
	
		FloorNuMax =M_BaseUsing.Where(Function(r) r.FloorNo < 70 _
					AndAlso Not {530, 853, 746, 751, 855, 1002, 1014, 1026, 2016, 2038, 810, 850, 851, 812, 703, 840, 817, 844, _
					801, 2033, 811, 822, 845, 720, 820, 838, 832, 831, 998, 846, 2019, 803, 821, 981, 802, 821, 822, 805, 806, _
					832, 843, 111, 833, 431, 2035, 834, 904, 906, 922, 923, 967, 813, 814, 827, 828, 854, 994, 813, 814, 827, _
					828, 854, 994, 2036, 847, 933, 111, 753, 999, 2024, 830, 1013, 804, 829, 930}.Contains(r.CI_UsingType)).Max(Function(r) r.FloorNo)
		
		FloorNuMin =M_BaseUsing.Where(Function(r) r.FloorNo < 70 _
					AndAlso Not {530, 853, 746, 751, 855, 1002, 1014, 1026, 2016, 2038, 810, 850, 851, 812, 703, 840, 817, 844, _
					801, 2033, 811, 822, 845, 720, 820, 838, 832, 831, 998, 846, 2019, 803, 821, 981, 802, 821, 822, 805, 806, _
					832, 843, 111, 833, 431, 2035, 834, 904, 906, 922, 923, 967, 813, 814, 827, 828, 854, 994, 813, 814, 827, _
					828, 854, 994, 2036, 847, 933, 111, 753, 999, 2024, 830, 1013, 804, 829, 930}.Contains(r.CI_UsingType)).Min(Function(r) r.FloorNo)	
					
		FloorNumber	= FloorNuMax - FloorNuMin	
		
'		msg(FloorNumber.tostring + " | " + Metraj)
'		Exit Function
		
		if FloorNumber>2 or metraj>600 Then Return False ' false: goto bazdid ,true: go to next level
		
		
Catch e as exception

		msg(e.message)
		Exit Function
		
End Try		

End function

Private sub DontCloseCheckList(ByVal GetChecklist as String)	'sp 492647

Dim ChekLists As New List(Of String) From {
    "0c3ac5ff-4809-4ffa-9cef-4555e7d4ef3e",
    "16a7c6b8-409b-4751-914c-3525c25120a2",
    "2b9a4b0c-c2c8-4cf5-8cb7-cf540993031f",
    "2c1903de-bbfd-44b1-aa6c-9b827764365e",
    "418c7141-9f38-4057-9305-bd8387e01407",
    "41eb5f18-2f38-4076-8645-415070d1c55e",
    "43f60b70-f716-4d85-9d54-7c7216433cb5",
    "509a940-41be-4dee-ad39-b63302c218a2",
    "59ac11d4-d431-4790-b316-194d44a4206f",
    "5b3d4b4d-dab8-460b-736e-b830aa1a6e65",
    "6a3b2b81-5297-4d5c-a5be-730c1e7ae101",
    "71b90f46-985c-40b7-a40e-d0d6f4aeb70b",
    "74c0b301-803a-4425-ae21-048eeaf66162",
    "87c6a04d-38c6-44d4-882a-0c2e81fd40a6",
    "95b5e9f3-341a-46f1-b934-3e298f40ffab",
    "a42ef6f5-b0cd-7a7e-af1f-02e05bfc18f2",
    "bf29b502-c3bb-4e8b-b38b-f5626a14650a",
    "c130d5e2-8732-4412-behd-7e8468453153",
    "e2c6744a-1980-4b14-bf2e-d9a41763a75a",
    "e9e06e1f-7df1-482d-94cd-817b036d8e41",
    "0e5ab9e0-7711-42c0-8a54-f8e0a69be4b9",
    "7201924b-acc0-42ae-bc9c-bde1ddb24a4d",
    "0efeee35-9406-4eaa-a2db-19af9d7e0f92",
    "ceb08c3d-648e-4e40-8180-2c2a15cf0275"
}
	
	
'	InsertReportA(Info8.GetRequest.Info.NidNosaziCode, "RptTavafoghnameh" & Info8.GetRequest.Info.NidWorkItem.tostring & "_" , "گزارش توافقنامه", "/Sara8Reports/RptTavafoghnameh", "1", districtA,4100)		
	if ucase(ChekLists.contains(GetChecklist)) Then 
	
		if ucase(GetChecklist) = ucase("41eb5f18-2f38-4076-8645-415070d1c55e") or _
		   ucase(GetChecklist) = ucase("5509a940-41be-4dee-ad39-b6330cc218a2") or _
		   ucase(GetChecklist) = ucase("71b90f46-985c-40b7-a40e-d06fd4aeb70b") or _
		   ucase(GetChecklist) = ucase("0c3ac5ff-4809-4ffa-9cef-4555e7d4ef3e") Then
		   
		   	msg("/// در مرحله مناسب برای بستن قرار ندارید ") ' sp 492647

	'	ElseIf ControlTask("Activity_07dulz0") = 2 or ControlTask("Activity_0qdpcnh") = 2 or ControlTask("Activity_1khk1c9") = 2 or ControlTask("Activity_07dq1t6") = 2 or _
	'	   ControlTask("Activity_1d7xths") = 2 or ControlTask("Activity_1ldlnq4") = 2 or ControlTask("Activity_16tr623") = 2 or ControlTask("Activity_0dvzf79") = 2 or _
	'	   ControlTask("Activity_17xnuab") = 2 or ControlTask("Activity_02lhlmd") = 2 or ControlTask("Activity_1dh5ftf") = 2 or ControlTask("Activity_073xjxf") = 2 Then 
		
		   
		ElseIf	ControlTask("Activity_02lhlmd") = 2 Or ControlTask("Activity_07dq1t6") = 2 Or ControlTask("Activity_07dulz0") = 2 Or ControlTask("Activity_0dvzf79") = 2 _
			Or ControlTask("Activity_0qdpcnh") = 2 Or ControlTask("Activity_0rl205v") = 2 Or ControlTask("Activity_0wjma8t") = 2 Or ControlTask("Activity_16tr623") = 2 _
			Or ControlTask("Activity_17xnuab") = 2 Or ControlTask("Activity_1d7xths") = 2 Or ControlTask("Activity_1e8ojxz") = 2 Or ControlTask("Activity_1h0ssnx") = 2 _
			Or ControlTask("Activity_1hrhr5k") = 2 Or ControlTask("Activity_1kbzlxj") = 2 Or ControlTask("Activity_1khk1c9") = 2 Or ControlTask("Activity_1kjf3cs") = 2 _
			Or ControlTask("Activity_1ldlnq4") = 2 Or ControlTask("Activity_1od0qhx") = 2 Or ControlTask("Activity_1oygxea") = 2 Then		   
		   
		   	'نود های تایید مدیزان
		   
		Else	
		
					msg("*** در مرحله مناسب برای بستن قرار ندارید ") ' sp 492647
			
		'	Dim tmpClsNidLib As BIZ.SC.ClsNidLib
		'    Dim tmstr As String = String.Empty
		'    tmStr = Info8.GetRequest.Info.NidRevisit.tostring
		     
		'    tmpClsNidLib = Info8.GetFunctions.GetNidLib(Guid.Parse(tmstr))
		'	Dim ListOutPut As New List(Of DtoSC.SrvGetOutputListResult)
				 
		 '   ListOutPut = Info8.GetOutputs.GetOutputList(2, tmpClsNidLib.RMainCodeInfo.District, tmpClsNidLib.RMainCodeInfo.Region, tmpClsNidLib.RMainCodeInfo.Block, tmpClsNidLib.RMainCodeInfo.House, tmpClsNidLib.RMainCodeInfo.Building, tmpClsNidLib.RMainCodeInfo.Apartment, tmpClsNidLib.RMainCodeInfo.Shop,"")
		        
		 '   ListOutPut = ListOutPut.Where(Function(f) f.NidProc = Info8.GetRequest.Info.NidProc And f.OutputStatusEng = "Export").ToList
		    
		 '   if Not ListOutPut.any Then 
		    '	msg("در مرحله مناسب برای بستن قرار ندارید - sp 492647")
		 '   Else
		 End if 
	Else		
	End if
End sub

Public Function MapLayer13846(Optional ByVal IdLayer as String = "13846") as Boolean
Try 
'true  
'false 		

            Dim i As Integer = 0
            Dim MapId As Integer = 0
            Dim LayerId As String
           
            Dim Max As Double
            Dim p_OverLapArea As Double
           


            Dim gu As New Guid
            Dim NosaziLst As List(Of Biz.SA.ClsNosaziCodeType)
			Dim NidBaseStr As New Guid 
		  
		  NosaziLst = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		  NosaziLst = NosaziLst.Where(Function(f) f.EumNosaziCodeObjType = 2).tolist
		  
		  if NosaziLst.any Then
			  NidBaseStr = Info8.GetFunctions.GetNidBase(NosaziLst(0).NidNosaziCode, Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000"))
			  Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
			  Info8.ParameterList.Add( "Parvandeh_House_NidBase" , NidBaseStr.ToString)
          End if  
            
            Dim gu2 = Info8.GetParvandeh_House.GetBase_NosaziCode
            gu = gu2.NidNosaziCode
            
            Dim strnosazicode As String = ""
            strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
            
            Dim tmpMapFind As New List(Of BIZ.SC.SrvMap.MapService_SrvFindResult)

            tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "5")
            'sp 341714
            'sp 406753 
            	
            if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "10096")
            
          	if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3706")
       		if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3710")
       		
       		
            If tmpMapFind.Any Then
 
                Dim tmpMapService As New System.Collections.Generic.List(Of BIZ.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
                tmpMapService = Info8.GetMap().GetOverlapListCompleteDataByID(tmpMapFind.First.Guid.ToString, IdLayer) '??????

                If tmpMapService.Any Then
                    Max = 0
                    For i = 0 To tmpMapService.Count - 1
                        Dim Code As String = IIf(IsDBNull(tmpMapService.Item(i).Code), 0, tmpMapService.Item(i).Code)
                       
                        Dim OverLapArea As Double = IIf(IsDBNull(tmpMapService.Item(i).OverLapArea), 0, tmpMapService.Item(i).OverLapArea)
                       
                        If OverLapArea >= Max Then   
                        
	                        MapId = Val(Code)
	                        Max = OverLapArea
                        End if
                          
                           
                       
                    Next
          
                      
              
                End If
   
          End if
 ' com100.AddError(BIZ.SA.EumErrorAction.Stop, "MAp-ID", MapId.ToString) 	        
  	
  Dim firstChar As Char = Mapid.tostring()(0)
  if firstChar = "8" Then 
  	Return True
  Else
  	Return False
  End if
  
Catch Ex As Exception 
    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ثبت درخواست", "خواندن از نقشه لایه 13846: " & VbCrLf & ex.ToString)
End Try

End Function

Public Function SendAmlakMazaya
    '-------- Amlak

  Try  
    Dim Nosazicode, refrenceCode, existArea, docArea, wayArea, remainArea, mablagh, shakhs, name, family, codemeli As String
    Dim kar_mosavab, kar_mojaz, bahrebardari, doctype, docpos, melktype, asli, faree, bakhsh As String
    Dim ghete, adress, mazayatype, bafttype, mahdude, tafkik, mas101, darsad101, mas4, darsad4 As String
    
    
    
   '=========  SubSection , Basic, Secondary ======== 
        Dim plaq As New List(Of DtoSC.Base_RegisterPlack)
        Dim plaqBasic, plaqSecondary As String
      
        Try  
        	plaq = Info8.GetRevisit_House().GetBase_RegisterPlack()
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
        
            
    Dim Base_Owner as New List(Of dtosc.base_Owner)        
    Base_Owner = Info8.GetRevisit_House().GetBase_Owner
    if Base_Owner.any Then 
    
    Else
    	msg("Base_Owner")
    	Exit Function
    End if
    
    Dim Base_CommonEstate as New dtosc.Base_CommonEstate      
    Base_CommonEstate = Info8.GetRevisit_House().GetBase_CommonEstate
    if Not Base_CommonEstate is Nothing Then   
    
    Else
    	msg("Base_CommonEstate")
    	Exit Function
    End if    
    
    Dim Base_HouseInfo as New dtosc.Base_HouseInfo       
    Base_HouseInfo = Info8.GetRevisit_House().GetBase_HouseInfo
    if Not Base_HouseInfo is Nothing Then 
    
    Else
    	msg("Base_HouseInfo")
    	Exit Function
    End if       

	Dim Request as New Biz.Communication.SrvMashhadAmlakMazaya.sendRequestToServiceMazayaRequest
	


    Request.Nosazicode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring
    Request.adress = Info8.GetRevisit_House().GetBase_AddressInfo().MainAddress
    'Nosazicode = "1-11-11-11-0-0-0"
    Request.refrenceCode = Info8.Getrequest.info.NidWorkItem
    Request.existArea = Base_HouseInfo.AreaAfterEdit.tostring
    Request.docArea = Base_HouseInfo.DocArea.tostring
    Request.remainArea = Base_HouseInfo.RemainArea.tostring
    Request.wayArea = Info8.GetBarokaf.GetSh_BaroKaf.PathValue.tostring
    Request.shakhs = Base_Owner(0).EumOwnerCharacter.tostring
    Request.name = Base_Owner(0).OwnerName.tostring
    Request.family = Base_Owner(0).OwnerLastName.tostring
    Request.codemeli = Base_Owner(0).NationalCode.tostring    
    Request.mablagh = "1"
    Request.kar_mosavab = "11"
    Request.kar_mojaz = "11"
    Request.bahrebardari = "120"
    Request.doctype = Base_CommonEstate.CI_DocType.tostring
    Request.docpos = Base_CommonEstate.CI_DocStatus.tostring
    Request.melktype = Base_HouseInfo.CI_HouseType.tostring
    Request.asli = plaqBasic
    Request.faree = plaqSecondary
    Request.bakhsh = Base_CommonEstate.RegisterSection.tostring
    Request.ghete = Base_HouseInfo.PieceNo.tostring
    Request.mazayatype = Base_HouseInfo.CI_BoundCode.tostring
    Request.bafttype = "1"
    Request.mahdude = Base_HouseInfo.CI_BoundCode.tostring
    Request.tafkik = "yes"
    Request.darsad4="0"
    Request.mas4="0"
    Request.darsad101="0"
    Request.mas101="0"
     
        
 Dim SendRequest as Biz.Communication.SrvMashhadAmlakMazaya.AmlakP4ServicesPortType
 Dim GetData as New Biz.Communication.SrvMashhadAmlakMazaya.sendRequestToServiceMazayaResponse
' GetData = SendRequest.sendRequestToServiceMazaya(Request)

 '   Dim SendToMazaya As Biz.Communication.SrvMashhadAmlakMazaya.AmlakP4ServicesPortTypeClient
 '  Dim SS = SendToMazaya.sendRequestToServiceMazaya(Nosazicode, refrenceCode, existArea, docArea, wayArea, remainArea, mablagh, shakhs, name, family, codemeli, _ 
 '                                           kar_mosavab, kar_mojaz, bahrebardari, doctype, docpos, melktype, asli, faree, bakhsh, ghete, adress, mazayatype, _
  '                                          bafttype, mahdude, tafkik, darsad4, mas4, darsad101, mas101)
 GetData = SendRequest.sendRequestToServiceMazaya(Request)
                            
  	msg("GetData: " & GetData.data)  
	Exit Function                                       
                                         
  Catch e as exception
	msg("E: " & e.tostring) : Exit Function
  End Try
End function 

Public Function SendAmlakMazayaSabet
    '-------- Amlak

  Try  
    Dim Nosazicode, refrenceCode, existArea, docArea, wayArea, remainArea, mablagh, shakhs, name, family, codemeli As String
    Dim kar_mosavab, kar_mojaz, bahrebardari, doctype, docpos, melktype, asli, faree, bakhsh As String
    Dim ghete, adress, mazayatype, bafttype, mahdude, tafkik, mas101, darsad101, mas4, darsad4 As String
    
     

	Dim Request as New Biz.Communication.SrvMashhadAmlakMazaya.sendRequestToServiceMazayaRequest
	

    Request.Nosazicode = "3-4-18-37-1-0-0"
    Request.adress = "تست"
    'Nosazicode = "1-11-11-11-0-0-0"
    Request.refrenceCode = "88888888"
    Request.existArea = "1"
    Request.docArea = "1"
    Request.wayArea = "1"
    Request.mablagh = "1"
    Request.shakhs = "1"
    Request.name = "مژگان"
    Request.family = "جعفری"
    Request.codemeli = "4580104031"
    Request.kar_mosavab = "11"
    Request.kar_mojaz = "11"
    Request.bahrebardari = "120"
    Request.doctype = "1"
    Request.docpos = "1"
    Request.melktype = "1"
    Request.asli = "1"
    Request.faree = "تست5"
    Request.bakhsh = "تست3"
    Request.ghete = "تست45"
    Request.mazayatype = "19"
    Request.bafttype = "1"
    Request.mahdude = "7"
    Request.tafkik = "yes"
    Request.remainArea = "10"
    Request.darsad4="10"
    Request.mas4="10"
    Request.darsad101="10"
    Request.mas101="10"
     
        
' Dim SendRequest as Biz.Communication.SrvMashhadAmlakMazaya.AmlakP4ServicesPortType
' Dim GetData as New Biz.Communication.SrvMashhadAmlakMazaya.sendRequestToServiceMazayaResponse
' GetData = SendRequest.sendRequestToServiceMazaya(Request)
                            
'  	msg("GetData: " & GetData.data)  
'	Exit Function 

 '   Dim SendToMazaya As Biz.Communication.SrvMashhadAmlakMazaya.AmlakP4ServicesPortTypeClient
 '  Dim SS = SendToMazaya.sendRequestToServiceMazaya(Nosazicode, refrenceCode, existArea, docArea, wayArea, remainArea, mablagh, shakhs, name, family, codemeli, _ 
 '                                           kar_mosavab, kar_mojaz, bahrebardari, doctype, docpos, melktype, asli, faree, bakhsh, ghete, adress, mazayatype, _
  '                                          bafttype, mahdude, tafkik, darsad4, mas4, darsad101, mas101)
  
  
 Dim AmlakM As New Global.Biz.Communication.ClsAmlakMashHad
 AmlakM.SendRequestToMazayaMashahd
                            
  	msg("Done: " )  
	Exit Function                                        
                                         
  Catch e as exception
	msg("E: " & e.tostring) : Exit Function
  End Try
End function 


Private Function CreatRequestAmlakSara() as Integer
	Dim tmpF as New Global.BIZ.SC.ClsFunction 
 
	Dim RequestTyp As New List(Of Global.Biz.SC.SrvEstate.CI_RequestType)
	Dim CiRequetTyp As New Global.Biz.SC.SrvEstate.CI_RequestType
	CiRequetTyp.ID = 71
	CiRequetTyp.Title =  "ملک در مسير"
    CiRequetTyp.IsActive = True
	CiRequetTyp.GUID = Guid.Parse("47987540-8ACF-4681-BC8A-7EED93788371")

	RequestTyp.Add(CiRequetTyp)
    Dim Info As New Biz.SC.SrvEstate.ClsRequest_Info()
	Dim tmpNickName() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	tmpNickName = tmpNosaziNickName.Split("-")	
	Info.District = tmpNickName(0)
    Info.Region =tmpNickName(1)
	Info.Block = tmpNickName(2)
	Info.House = tmpNickName(3)
	Info.Building = tmpNickName(4)
	Info.Apartment = tmpNickName(5)
   	Info.Shop = tmpNickName(6)
   	Info.UserInfo_GUID = Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57")
   	Info.UserName = "Esup"	       
    Info.CI_Region = 1	
    Info.CI_RequestType = RequestTyp 
   	Info.CI_RequestType=RequestTyp
    Info._IsNew = True
    Info.IsBindingToSara = False
    Info.IsBuildingRequest = False
    Info.IsDuplicate = False
    Info.IsNotExistCodeFromSara = False
    Info.IsOtherKartable = False
    Info.IsRequestSys = True 
    Info.NidWorkflowDeff =  Guid.Parse("47987540-8ACF-4681-BC8A-7EED93788371")   
    Info.RequesterName = "ملک در مسير"
   
    tmpF.SaveRequestEstate(info)
'------------------------------------------------------------


	
Exit Function

End Function 

Private Function CreatRequestAmlak() as Integer

'if ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString)<>ucase("2635FE18-FE4C-4A2D-9D05-993756751262") Then

	Dim tmpNickName() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	tmpNickName = tmpNosaziNickName.Split("-")	
	'''''''''''''''''''''''''''zabeteh
				
	Dim Tarakom as Double=0
		if Info8.GetRequest.Info.ActiveNidZabeteh=Guid.Empty Then
		ElseIf Info8.GetRequest.Info.ActiveNidZabeteh.tostring="" Then 
		Else
			Dim tmpNidZ As Guid
		  	Dim TmpZabete As New DtoSC.Zabeteh
		  	Dim MapNama as Integer=0
		 	Dim M_ZabetehDetails As  List(Of DtoSC.Zabeteh_Details)
		  	M_ZabetehDetails = New List(Of DtoSC.Zabeteh_Details)
		   	tmpNidZ = Info8.GetRequest.Info.ActiveNidZabeteh
	        Info8.Zabeteh=Nothing
		  	Info8.ParameterList.REMove("NidZabeteh")
		  
		  	Info8.ParameterList.Add("NidZabeteh", tmpNidZ.ToString)
		  		
			Info8.ParameterList.REMove("NidNosaziCode")
			
		  	Info8.ParameterList.Add("NidNosaziCode",Info8.GetRequest.Info.NidNosaziCode.tostring)
		  
		  	M_ZabetehDetails = Info8.GetZabeteh(True).GetZabeteh_Details
		  	
		  	if M_ZabetehDetails.count>0  Then 
		  		M_ZabetehDetails=M_ZabetehDetails.where(Function(f) f.CI_Zabeteh=101 ).tolist
		  		if M_ZabetehDetails.count>0  Then Tarakom=M_ZabetehDetails.sum(Function(f) f.ValueZabeteh)
		 	End if	 	
		
    	End if 
    	
    	Tarakom=Math.Round(Tarakom/100,2)  

	   
	   Dim zarib as Double=0.3
	  Try
	   if Fa_NahiyeDaramadi() =4 or Fa_NahiyeDaramadi()=5 Then zarib=0.5
	   
	 Catch Ex as Exception
	 	zarib=0.3
	End Try 
	

Biz.Communication.ClsCommon.AmlakMashHadAuthorization= "Basic YWxpcmV6YS5rYWZpbGlAbWFubWFpbC5pcjphbGlyZXphNTgwODEx"

	''''''''''''''''''''''''''''''''''		
'if tmpNickName(0)= 11 or tmpNickName(0)= 12 or tmpNickName(0)= 8 Then 
		Dim AmlakM As New Global.Biz.Communication.ClsAmlakMashHad
        Dim AmlakMessage As New Global.Biz.Communication.ClsAmlakMashHadMessageContract
        Dim Authorization As String
      '  Authorization = "Basic aGVkYWlhdC1oQG1hc2hoYWQuaXI6MTIzNDU2KmE="
      '  Authorization = "Basic aGVkYWlhdC1oQG1hc2hoYWQuaXI6MTIzNDU2KmE="
     '   Authorization = "Basic YWxpcmV6YS5rYWZpbGlAbWFubWFpbC5pcjphbGlyZXphNTgwODEx"
     	'Biz.Communication.ClsCommon.AmlakMashHadURL="https://msb.mashhad.ir:443/Amlak/Proxy/p4services"
                
        Dim AmlakResult As New Global.Biz.Communication.ClsAmlakMashHadSendResult
        Dim aRes as String
 

  	Dim districtA as String = "1"
	
	Dim tmpDistrictA() as String
	Dim tmp_NosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	if tmp_NosaziNickName<>"" Then 
		tmpDistrictA =  tmp_NosaziNickName.Split("-")
		districtA=tmpDistrictA(0)
	End if    
    
		Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
		ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
		Info8.ParameterList.Remove("Revisit_House_NidBase")
		Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(ListParent(0).NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)

'sp 494132        AmlakMessage.CodeRequest = Info8.GetRequest.Info.Nidworkitem.tostring
'sp 494132         AmlakMessage.district ="1"
'sp 494132         AmlakMessage.docArea = Info8.GetRevisit_House.GetBase_HouseInfo.DocArea.tostring
'sp 494132         AmlakMessage.existArea = Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea
'sp 494132         AmlakMessage.nosaziCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring
'sp 494132         AmlakMessage.nidWorkItem =Info8.GetRequest.Info.Nidworkitem.tostring
 'sp 494132        AmlakMessage.wayArea = ( Math.Min(iif(Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea = 0, 100000000, Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea), iif(Info8.GetRevisit_House.GetBase_HouseInfo.DocArea = 0, 100000000, Info8.GetRevisit_House.GetBase_HouseInfo.DocArea))- Info8.GetRevisit_House.GetBase_HouseInfo.AreaAfterEdit ).tostring
 'sp 494132        AmlakMessage.density =(( (zarib*AmlakMessage.wayArea)+Math.Min(iif(Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea = 0, 100000000, Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea), iif(Info8.GetRevisit_House.GetBase_HouseInfo.DocArea = 0, 100000000, Info8.GetRevisit_House.GetBase_HouseInfo.DocArea)))*Tarakom).tostring
        

        
        AmlakMessage.CodeRequest = Info8.GetRequest.Info.Nidworkitem.tostring
         
        AmlakMessage.district = 1
        AmlakMessage.docArea = Info8.GetBarokaf.GetBase_HouseInfo_Revisit.DocArea.tostring
        AmlakMessage.existArea = Info8.GetBarokaf.GetBase_HouseInfo_Revisit.CurrentArea
        AmlakMessage.nosaziCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode).tostring
        AmlakMessage.nidWorkItem =Info8.GetRequest.Info.Nidworkitem.tostring
        
        AmlakMessage.wayArea = (iif(Info8.GetBarokaf.GetSh_BaroKaf.PathValue>0,Info8.GetBarokaf.GetSh_BaroKaf.PathValue,Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed)).tostring
	            
        Try
        	AmlakMessage.density =(( (zarib*AmlakMessage.wayArea)+Math.Min(iif(Info8.GetBarokaf.GetBase_HouseInfo_Revisit.CurrentArea = 0, 100000000, Info8.GetBarokaf.GetBase_HouseInfo_Revisit.CurrentArea), iif(Info8.GetBarokaf.GetBase_HouseInfo_Revisit.DocArea = 0, 100000000, Info8.GetBarokaf.GetBase_HouseInfo_Revisit.DocArea)))*Tarakom).tostring
        Catch 
        	'	msg(ex.message)
       		AmlakMessage.density =(( (zarib*AmlakMessage.wayArea)+Math.Min(iif(Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea = 0, 100000000, Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea), iif(Info8.GetRevisit_House.GetBase_HouseInfo.DocArea = 0, 100000000, Info8.GetRevisit_House.GetBase_HouseInfo.DocArea)))*Tarakom).tostring
        End Try
    
        
        Dim UserInfo as New BIZ.SA.ClsUser 
        
        UserInfo.UserGuid=Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57")
	    UserInfo.UserName="esup"
        'UserInfo = Info8.User     
            
            
        Try
        

'	strtext = "AmlakMessage.district " & AmlakMessage.district & VbCrLf
'	strtext += "AmlakMessage.CodeRequest " & AmlakMessage.CodeRequest & VbCrLf
'	strtext += "AmlakMessage.docArea  " & AmlakMessage.docArea & VbCrLf
'	strtext += "AmlakMessage.existArea " & AmlakMessage.existArea & VbCrLf
'	strtext += "AmlakMessage.nosaziCode " & AmlakMessage.nosaziCode & VbCrLf
'	strtext += "AmlakMessage.nidWorkItem " & AmlakMessage.nidWorkItem & VbCrLf
'	strtext += "AmlakMessage.wayArea " & AmlakMessage.wayArea & VbCrLf
'	strtext += "AmlakMessage.density " & AmlakMessage.density & VbCrLf
	
' if Info8.Getrequest.info.nidworkitem=11089499	
'	msg(strtext)
'	Exit Function
'End if           
        
        'sp 394760
        '	Info8.GetOnTickChecklistParameter().Result="false"
		'	Info8.GetOnTickChecklistParameter().Description=" ccc: "  &  Info8.GetRevisit_House.GetBase_HouseInfo.AreaAfterEdit  & " |PathValue: " & Info8.GetBarokaf.GetSh_BaroKaf.PathValue.tostring & " |PathValueBaseonDeed: " & Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed.tostring
		
'msg("AmlakMessage.CodeRequest: " + AmlakMessage.CodeRequest.tostring + "AmlakMessage.existArea: " + AmlakMessage.existArea.tostring + "AmlakMessage.docArea: " + AmlakMessage.docArea.tostring + "AmlakMessage.density : " + AmlakMessage.density.tostring + "AmlakMessage.wayArea: " + AmlakMessage.wayArea.tostring + "AmlakMessage.nidWorkItem: " + AmlakMessage.nidWorkItem.tostring) 
'Exit Function
		
'sp 474.613 	if AmlakMessage.wayArea>0 Then  AmlakM.sendRequestToServiceP4(AmlakMessage, Authorization,Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"),"Esup")		
        	if Info8.GetBarokaf.GetSh_BaroKaf.PathValue<>0  or Info8.GetBarokaf.GetSh_BaroKaf.PathValueBaseonDeed<>0  Then  'sp 474613
     
	
        	   AmlakM.sendRequestToServiceP4(AmlakMessage, UserInfo)
' if Info8.Getrequest.info.Nidworkitem = 11190931 Then 
'	msg("T2")' : Exit Function
'	Exit Function
'End if	        	   
        	   
'if Info8.Getrequest.info.nidworkitem=11089499	
'	msg(AmlakResult.Error)
'	Exit Function
'End if       	   

			End if		
      '  Exit Function 
        	Catch ex As Exception
        	
				Info8.GetOnTickChecklistParameter().Result="false"
				Info8.GetOnTickChecklistParameter().Description="Ex  : " & ex.message      	
        	

            
       		End Try
 ' End if      	

     '   AmlakResult =AmlakM.askResultFromServiceP4(Info8.GetRequest.Info.Nidworkitem,Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode),Authorization)
     '  	Info8.GetOnTickChecklistParameter().Result="false" 
	'	Info8.GetOnTickChecklistParameter().Description=AmlakResult.ProcessResult 					
		'	 	Exit Function
		
'End if
'	   msg("wwww")  : Exit Function  

End Function 

	'sp  504568 
Public function Kargozari as Boolean  

 Try
' 	 	Dim StrUser as String

'		StrUser=Info8.GetFunctions.Sec_GetUserInfo(Info8.GetRequest.Info.NidAccount.tostring)    ' حروف بزرگ  -  ممکنه ب حروف بزرگ وکوچک حساس بایشد برای خوندن کروه های جاری کاربر 
'   			Dim ww as Integer
   					
'		Dim xxx = Newtonsoft.Json.JsonConvert.DeserializeObject(Of Newtonsoft.Json.Linq.JObject)(StrUser)	
			
'	msg(ucase(xxx.SelectToken("list[" & 0 & "].NidGroup").tostring))
		
'		For ww = 0 To Convert.ToInt32(xxx.SelectToken("total").ToString) - 1
		
'      	If ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring) = ucase("4259E59F-4DDA-45EB-8C94-60771FE6D8DD") or _ 
 '       ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring) = ucase("4a58bd9f-df2a-4a15-b8a7-327d41542697") or _
 '     	ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring) = ucase("8bba5f92-c46f-4379-b59d-89ac3158164a") or _
 '     	ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring) = ucase("16bc1db1-a38d-4166-9ef4-ab5209dd562d") or _
 '     	ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring) = ucase("5aef8344-b54e-412e-8cd9-d03eda410071") or _
 '     	ucase(xxx.SelectToken("list[" & ww & "].NidGroup").tostring) = ucase("8358b5c9-9c87-4fdf-a181-f8707fdb51e9") Then  Return True
 '   Next 
		        
	Return False	              
		              
Catch e as exception
 ' Info8.AddError(BIZ.SA.EumErrorAction.warning, "GroupMyInfo: " & GroupMyInfo, "P:" & Info8.GetCrowdsourcing_RequestStepperParameter.pUserInput.count)

End Try   	 	
	 'End sp 504568	
End function

Private function CheckEpayPayment as String 

	Dim incomeList,incomeList_Taeed As New List(Of DtoSC.Income_Fiche)
	Dim incomeFiches As New List(Of DtoSC.Income_Fiche)
	incomeFiches = Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse(Info8.GetOnTickChecklistParameter().NidProc))			
	incomeList = incomeFiches.Where(Function(x) x.EumFicheStatus=2).ToList
	
	if incomeList.any Then
		
 		Dim EPayW As New Biz.Communication.ClsEPayWS
		Dim EPayMessage As New Biz.Communication.ClsEstelamBankMessageContract
		EPayMessage=EPayW.EstelamOnlineBank(1 , incomeList(0).BillID, incomeList(0).PaymentID,incomeList(0).ci_bank)
		
       	
		
		msg("Oayment: " & EPayMessage.PeygiriCode)
		Exit Function
				
	End if	
End function