<DisplayName("آپارتمان_پرونده")> Public Function FrmApartment as clsOut 

 Try
'if 1=2 Then



' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", "tmpUsingBuilding " & Info8.GetParvandeh_Apartment.GetBase_Info.NidBase.tostring) : Exit Function      
' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", "New " & Info8.GetParvandeh_Apartment.GetBase_ApartmentInfo.NidBase.tostring ) : Exit Function   
 
 	Info8.ParameterList.Add("Parvandeh_Apartment_NidBase", Info8.GetParvandeh_Apartment.GetBase_ApartmentInfo.NidBase.tostring)
 	 

 
	Try
	
		Dim OtherEq , OtherEq_tmp , TmplisOtherEq216 As New List(Of DtoSC.Base_OtherEquipment)
		OtherEq_tmp=Info8.GetParvandeh_Apartment.GetBase_OtherEquipment()
		
		if OtherEq_tmp.count > 0 Then
			OtherEq = OtherEq_tmp.Where(Function(f) f.CI_OtherEquipmentGroup = 0).ToList
			if OtherEq.count>0 Then Info8.AddError(Biz.SA.EumErrorAction.Stop, "هشدار ", "لطفا اطلاعات سایر امکانات را اصلاح فرمایید (نوع گروه نامشخص نباشد)")
						 							
			TmplisOtherEq216 = OtherEq_tmp.where(Function(f) f.CI_OtherEquipmentType=216).tolist		     	
		    if TmplisOtherEq216.any() Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه","کاربر محترم : مجاز به حذف این کد نمی باشید.")
		    
		End if	
			 							
	Catch e as exception
	 	   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start2", "آپارتمان_پرونده " & e.message )
	       Exit Function  
	End Try					



        'A  If Info8.GetParvandeh_Apartment.GetBase_CommonEstate.CI_DocType = 1 Then
        'A        Info8.AddError(Biz.SA.EumErrorAction.Stop, "هشدار ", "شما اجازه انتخاب نوع سند ثبتی(ملکی ) را ندارید ")
        'A    End If

	'93033		if Info8.GetParvandeh_Apartment.GetBase_Using.count>0 Then Call MassDistanceMain(Info8.GetParvandeh_Apartment.GetBase_Using)

            Dim i As Integer = 0
            Dim tmpBase_AddressPostCode As List(Of DtoSC.Base_AddressPostCode)
	     	tmpBase_AddressPostCode=Info8.GetParvandeh_Apartment.GetBase_AddressPostCode()

	     	If tmpBase_AddressPostCode.Count() > 1 Then
            	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"کد پستی","تنها یک کد پستی وارد کنید")
            End If         
			
     
            Dim tmpBaseUsing As New List(Of DtoSC.Base_Using)
            tmpBaseUsing = Info8.GetParvandeh_Apartment.GetBase_Using() 
            
          
            
            Dim tmpUsingBuilding As List(Of DtoSC.Base_Using) 
            Dim tmpBasFront As List(Of DtoSC.Base_Front) 
            
            tmpUsingBuilding = Info8.GetParvandeh_Apartment.GetBase_Using()
            tmpBasFront = Info8.GetParvandeh_Apartment.GetBase_Front()

             if tmpUsingBuilding.count<=0 Then 
			 	   Info8.AddError(BIZ.SA.EumErrorAction.warning, "هشدار", "کاربری های آپارتمان وارد نشده است" )			                                  
             End if
     	
            
            Dim cc as Integer
			For cc=0 to tmpBasFront.count-1 
				if tmpBasFront(cc).CI_FrontPlace=2 or tmpBasFront(cc).CI_FrontPlace=4 Then
					if  tmpBasFront(cc).CI_FrontType=8 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"توجه","مجاز به انتخاب بالکن تزئینی از سمت حیاط نمی باشید")
				End if	
			Next
			
		    			
					            		 
   
' Info8.AddError(BIZ.SA.EumErrorAction.warning, "Start", tmpUsingBuilding.count)     
            Dim tmpArea as Double = 0
            Dim TmpMax as Integer = 0    
            
                              
       If tmpBaseUsing.Any Then     
       
     		tmpUsingBuilding = tmpUsingBuilding.where(Function(f)  f.FloorNo<50 OrElse f.FloorNo = 100).tolist     
      				
     		TmpMax=tmpUsingBuilding.Max(Function(f) f.FloorNo)   

        		  		
     		tmpUsingBuilding = tmpUsingBuilding.Where(Function(f) f.FloorNo = TmpMax).tolist     	
     		
     			     		
     		tmpArea=tmpUsingBuilding.sum(Function(f) f.UsingArea)
     		
     	'	tmpUsingBuilding = tmpUsingBuilding.where(Function(f) f.FloorNo<>100 AndAlso f.FloorNo<50).tolist
     			   		
     		
     		if tmpArea>50 Then 
     			TmpMax=(TmpMax+1)+((-1)* tmpUsingBuilding.Min(Function(f) f.FloorNo))
     		Else
     			TmpMax=(TmpMax)+((-1)* tmpUsingBuilding.Min(Function(f) f.FloorNo))
		     End if	
		       		     

            
                If tmpBaseUsing.Count > 0 Then
                    For i = 0 To tmpBaseUsing.Count - 1

                        If tmpBaseUsing.Item(i).UsingArea = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "مساحت کاربري رديف " & i+1 & " وارد نشده است")
                        If tmpBaseUsing.Item(i).BuildingNo = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "شماره ساختمان رديف " & i+1 & " وارد نشده است")
                        If tmpBaseUsing.Item(i).CI_UsingGroup = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کاربري اصلي رديف " & i+1 & " وارد نشده است")
                        If tmpBaseUsing.Item(i).CI_UsingType = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کاربري فرعي رديف " & i+1 & " وارد نشده است")
                        If tmpBaseUsing.Item(i).CI_SazehType = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد نوع سازه رديف " & i+1 & " وارد نشده است") 'CI_BuildingType
                        If tmpBaseUsing.Item(i).CI_RoofType = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد نوع سقف رديف " & i+1 & " وارد نشده است") '
                        If tmpBaseUsing.Item(i).CI_BuildingKind = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد نوع ساختمان رديف " & i+1 & " وارد نشده است") '
                        if TmpMax>5 And tmpBaseUsing.Item(i).CI_BuildingKind=1 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "نوع ساختمان در آپارتمان با تعداد طبقه سازگار نیست") 
                     ' @
                        if (tmpBaseUsing.Item(i).CI_UsingType =809 or tmpBaseUsing.Item(i).CI_UsingType = 839)And tmpBaseUsing.Item(i).UnitCount <> 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "برای کاربری سرایه داری و نگهبانی - سرایه داری کاربری ردیف " & i+1 & " تعداد واحد وارد نشود  ")    
                    ' @
                    Next
                End If
         End If
            
 

            Info8.Parvandeh_Apartment.Base_ApartmentInfo.TotalArea= tmpBaseUsing.Sum(Function(f) f.UsingArea)+tmpBasFront.sum(Function(f) f.FrontArea)
        
            i=0
 			Dim tmpBaseOwner As List(Of DtoSC.Base_Owner) = Info8.GetParvandeh_Apartment.GetBase_Owner()
 			
 	
 			
			if tmpBaseOwner.any Then
				if tmpBaseOwner.count>0 Then
					For i=0 to tmpBaseOwner.count -1
						
					if tmpBaseOwner(I).OwnerName = "" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "نام مالک را وارد نمایید **")
                    If tmpBaseOwner(I).OwnerLastName = "" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "نام خانوادگی مالک را وارد نمایید")
                    
                  
                    
					 if tmpBaseOwner.Item(i).EumOwnerCharacter <>2 Then 
					  
						If isdbnull(tmpBaseOwner.Item(i).NationalCode) Then 
							Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا کد ملی را وارد کنید") 
						ElseIf tmpBaseOwner.Item(i).NationalCode ="" Then 
							Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا کد ملی را وارد کنید") 
						Else
'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start2",CheckMeli(tmpBaseOwner.Item(i).NationalCode))	: Exit Function	
						  
							if CheckMeli2(tmpBaseOwner.Item(i).NationalCode) = False Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد ملی معتبر نمی باشد")
					
						
						
						End if
					End if		
					Next
				End if
			Else
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "اطلاعات مالک را وارد نمایید")	
			End if		
								 	

  

'---------------------------------------------------------------------------------	

 		'	Dim TmpInfo8 As New Biz.SC.ClsObjectFactory
		
		'	TmpInfo8.Parvandeh_Apartment=Nothing
			
       
        '	TmpInfo8.ParameterList.Add("NidProc", Info8.FindParameter("NidpRoc"))
        	
 	        	

	
		'	Info8.ParameterList.Add("Parvandeh_Apartment_NidBase",  Info8.GetParvandeh_Apartment.GetBase_ApartmentInfo.NidBase.ToString)

    

'End if '1=2			 
			 
    ' sp 499747     
'	if Info8.GetParvandeh_Apartment.GetBase_ApartmentInfo.ToftImpart<=0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی - پرونده(آپارتمان)", "مقدار سهم العرصه آپارتمان را وارد کنید - sp 499747")  
    'end  sp 499747    			
 
		 			 
        Catch ex As Exception
            Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی - فرمول آپارتمان - پرونده", ex.message)
        End Try
        

 End Function
 
Private Function CheckMeli(ByVal MeliCode As String) As Boolean
Dim GroupIn7(),TempStr7 as String
Dim i as Integer
if MeliCode="" Then MeliCode=0

		Dim StrUser as String=""
		

 '   
		Dim CurrentGroup7 as String
		Dim UserList7 As New List(Of BIZ.SA.ClsUserGroup)
		UserList7 = Info8.User.UserGroups
					
		
		
		if UserList7.Any Then 
		 	For i=0 to UserList7.Count-1
		 		TempStr7 +=  UserList7(i).GroupGuid.ToString()
		   		if i< UserList7.Count-1 Then TempStr7 = TempStr7 & ","
		  	Next i
		End if
		
	 
	
		GroupIn7=TempStr7.Split(",")
	  ' 	Dim a as Boolean = False
		For Each bb As String In GroupIn7
			If  Lcase(bb)="4a58bd9f-df2a-4a15-b8a7-327d41542697" or Lcase(bb)="5f4030ce-99fb-4731-892b-0d0f525a910e" or Lcase(bb)="e4fd6027-2a09-4b82-b812-6644160636c5" _
			 or Lcase(bb)="59434e63-ffd2-4de9-b26d-e795c44a4f6a" or Lcase(bb)="5aef8344-b54e-412e-8cd9-d03eda410071" or Lcase(bb)="16bc1db1-a38d-4166-9ef4-ab5209dd562d" _
			 or Lcase(bb)="8bba5f92-c46f-4379-b59d-89ac3158164a" Then
			 Return True
			End if
		Next
		


        Dim MCode As String = MeliCode
        If MCode.Length <> 10 Then
            Return False
        ElseIf MCode="0000000000" or MCode= "1111111111" or MCode="2222222222" or MCode="3333333333" or MCode="4444444444" _
        or MCode="9999999999" or MCode="8888888888" or MCode="7777777777" or MCode="6666666666" or MCode="5555555555" _
        or MCode="0123456789" Then
        	     Return False
        Else
            Dim jj As Integer = 10
            Dim Control As Integer = 0
            Dim Sum1 As Long = 0
            For kk As Integer = 0 To MCode.Length - 2
            	if isnumeric(MCode(kk)) Then 
            	
                	Sum1 += Val(MCode(kk)) * jj
                	jj -= 1
                Else
                	Return False
                End if		
            Next
               Control = IIf((Sum1 Mod 11) < 2, Sum1 Mod 11, 11 - (Sum1 Mod 11))
            If Control = Val(MCode(9)) Then
                Return True
            Else
                Return False
            End If
        End If

    End Function
    
Private Function CheckMeli2(ByVal MeliCode As String) As Boolean


Dim GroupIn7(),TempStr7 as String
Dim i as Integer
if MeliCode="" Then MeliCode=0

		Dim StrUser as String=""

Try	
		
	Try	
		
	'	Try	
	'	
			StrUser=Info8.GetFunctions.Sec_GetGroupList(Info8.User.UserGuid.tostring).tostring
					
			
	'	Catch e as exception
		'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"هشدار 1",e.message)
		'	Exit Function			
	'	End Try 
		 		
		Dim xxx = Newtonsoft.Json.JsonConvert.DeserializeObject(Of Newtonsoft.Json.Linq.JObject)(StrUser)
		
		For i = 0 To Convert.ToInt32(xxx.SelectToken("total").ToString) - 1
						
			TempStr7 +=  xxx.SelectToken("list[" & i & "].NidGroup").tostring 
			
	     	if i < Convert.ToInt32(xxx.SelectToken("total").ToString) - 1 Then TempStr7 = TempStr7 & ","	     	
	     	
	    Next	
	    
   Catch
   
		Dim CurrentGroup7 as String
		Dim UserList7 As New List(Of BIZ.SA.ClsUserGroup)
		UserList7 = Info8.User.UserGroups
					
		
		
		if UserList7.Any Then 
		 	For i=0 to UserList7.Count-1
		 		TempStr7 +=  UserList7(i).GroupGuid.ToString()
		   		if i< UserList7.Count-1 Then TempStr7 = TempStr7 & ","
		  	Next i
		End if
		
	End Try	
	
	 
	
		GroupIn7=TempStr7.Split(",")
	  ' 	Dim a as Boolean = False
		For Each bb As String In GroupIn7
			If  Lcase(bb)="4a58bd9f-df2a-4a15-b8a7-327d41542697" or Lcase(bb)="5f4030ce-99fb-4731-892b-0d0f525a910e" or Lcase(bb)="e4fd6027-2a09-4b82-b812-6644160636c5" _
			 or Lcase(bb)="59434e63-ffd2-4de9-b26d-e795c44a4f6a" or Lcase(bb)="5aef8344-b54e-412e-8cd9-d03eda410071" or Lcase(bb)="16bc1db1-a38d-4166-9ef4-ab5209dd562d" _
			 or Lcase(bb)="8bba5f92-c46f-4379-b59d-89ac3158164a" Then
			 Return True
			End if
		Next		

        Dim MCode As String = MeliCode
        If MCode.Length <> 10 Then
            Return False
        ElseIf MCode="0000000000" or MCode= "1111111111" or MCode="2222222222" or MCode="3333333333" or MCode="4444444444" _
        or MCode="9999999999" or MCode="8888888888" or MCode="7777777777" or MCode="6666666666" or MCode="5555555555" _
        or MCode="0123456789" Then
        	     Return False
        Else
            Dim jj As Integer = 10
            Dim Control As Integer = 0
            Dim Sum1 As Long = 0
            For kk As Integer = 0 To MCode.Length - 2
            	if isnumeric(MCode(kk)) Then 
            	
                	Sum1 += Val(MCode(kk)) * jj
                	jj -= 1
                Else
                	Return False
                End if		
            Next
               Control = IIf((Sum1 Mod 11) < 2, Sum1 Mod 11, 11 - (Sum1 Mod 11))
            If Control = Val(MCode(9)) Then
                Return True
            Else
                Return False
            End If
        End If
        
       
        
Catch e as exception

End Try        

    End Function
اعتبار سنجی درخواست
Dim AssingTo as String
Dim AssingToUserName as String
Dim jobAV as Boolean=False
Dim LstUserKar() AS String  
Dim NIDAgent As New Guid 
Dim NidAgent_NameFamily as String

<DisplayName("درخواست")> Public Function FrmSh_Request as clsOut 
  Try  
  
   If ucase(Info8.User.UserGuid.ToString()) = "E7D536CF-3FE6-4F23-9C05-CE6EEC87987F" Then Exit Function  'ms Ghafarian

'sp 492647
if ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString) = ucase("a3678031-d273-4569-a223-fd2b39d46f39") Then 
	if Info8.GetRequest.Info.CI_Requester=14 Then SaveCheck("2c1903de-bbfd-44b1-aa6c-9b82764365ce",2,0)
End if 'end sp 492647
  

'     If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
'       Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", Info8.Getrequest.Info.NidNosaziCode.tostring)
'       Exit Function
'   End If
  
'sp 490168
'if 1=2 Then
if ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString) = ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then 
	Try

			
		'	Info8.GetRequest.Info.NidRevisit = Guid.NewGuid()
	         
	Catch e as exception
		   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی- اصلاح نقشه", e.message) 
		   Exit Function
	End Try

End if
'End if 
'end sp 490168 

'sp 483504
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") Then  ' فاصا حساب
	if MapHarimJame3("10320") = 1 Then	
		 LstUserKar={"8bba5f92-c46f-4379-b59d-89ac3158164a","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d" _
		 			 ,"9d612183-5bf3-439f-ade3-ce501dd08be2","81caa6f8-1da9-428b-a0c7-8c17e705b08e","b40f1faf-5885-457b-9217-ddac6f01898e" _
		 			 ,"91152337-9688-4618-9d9a-6401925743a9","4a58bd9f-df2a-4a15-b8a7-327d41542697"}
		 if LstUserGroupId(LstUserKar)=0 Then
		 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "شما مجاز به ثبت درخواست مفاصا حساب در خارج از لایه حریم نمیباشید - sp 483504") : Exit Function
		 End if	
	End if	 
End if	 
'end sp		
	
  
  'sp 489120
if	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("8F844ABD-2642-4854-9A33-8E61B6F67BA7") or _
	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("CCCFBD5F-B245-408F-9C74-E2A21170AA2E") or _
	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("994AD909-C2B7-476B-9BE2-8821EA1C9841") or _
	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("E10EDCBA-7945-4DDD-8306-9BAA37529AE2")  Then  
	
	   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "امکان ثبت درخواست وجود ندارد . sp 489120 ") 
	   Exit Function
	   	
	End if
  'end sp 489120
  
  
  if  1 = 2 Then 'sp 483504
	  'sp 477179
	 if	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") Then
	
		   	LstUserKar={"4a58bd9f-df2a-4a15-b8a7-327d41542697"}
		'sp 477192   	
			if LstUserGroupId(LstUserKar)=1 Then	
			'if ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
		   		if MapHarimJame3("10320") = 1 Then	    
					Dim TmpError As New Global.Biz.SA.clsBizError
					TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
					TmpError.ErrorTitel = "درخواست مفاصا حساب برای گروه کارشناس کمیسیون ماده صد روی کدهای حریم قابل انجام نیست - sp 477179"
					TmpError.ErrorKey = ""
					Info8.ErrorResult.BizErrors.Add(TmpError)
					Exit Function				 
				End if	
			End if
	End if	  'end sp 477179 
End if   'end sp 483504

  	
' Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "امکان ثبت درخواست وجود ندارد .") 
'sp 449289
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("C83F6228-987B-4B5E-8906-56F7CD87EA7C")Then
   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "امکان ثبت درخواست وجود ندارد .") 
   Exit Function
End if
'

'sp 476884
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("0BD1CA17-17DE-4F91-8544-250F40105DF2") Then 
  	if CheckTarhTafzili()=1 Then
  '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"Is 1", "") 
  		Dim TmpError As New Global.Biz.SA.clsBizError
		TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
		TmpError.ErrorTitel = "بدلیل داشتن درخواست جاری یا بایگانی موقت از گروه پروانه احداث یا توسعه امکان ثبت درخواست متقاضی مزایا طرح تفضیلی را ندارید - sp 476884"
		TmpError.ErrorKey = 1000
		Info8.ErrorResult.BizErrors.Add(TmpError)
		Exit Function 
	End if
End if


'sp 469654
Dim strCodePp as String
Dim tmpShopP as Integer
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("3157211F-C484-48E9-B65C-6BF3271A9F15") or _
   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("6CC12A36-D0FE-476B-8BD5-EFBF3B29401A") or _
   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("3B155802-5BD4-4261-A6F3-B65B5177BF5F") Then
   
		strCodePp = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
		tmpShopP=strCodePp.Split("-")(6)
		if tmpShopP=0 Then 
			Dim TmpError As New Global.Biz.SA.clsBizError
			TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
			TmpError.ErrorTitel = "شما مجاز به ثبت این نوع درخواست نمی باشید از سامانه سرا 10 اقدام نمایید"
			TmpError.ErrorKey = 1000
			Info8.ErrorResult.BizErrors.Add(TmpError)
			Exit Function
		End if
End if			 
'end sp 469654


'sp448002-B6
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("5B9A549D-C94D-43EF-BAB6-D67E5BEEC843") _
 or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("F088552B-7787-4FFE-AE7D-63F412D68B24")Then 
 jobAV=True
End if
if jobAV=True Then'*
         Try
            Dim HasSabeghe as Boolean=False
			Dim jj,bb As Integer
			Dim AllFiche As List(Of Global.DtoSC.Income_Fiche)
			Dim FicheKoll As List(Of Global.DtoSC.Income_Fiche)
		    Dim TmpListReq As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
		    Dim AllCalculation  As New List(Of DtoSC.Income_Calculation)
		     TmpListReq= Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
		        'if TmpListReq.count >0 Then TmpListReq=TmpListReq.where (Function (f) f.eumrequeststatus<>3 ).tolist ''ابطال نباشد 	

		        'Sp455250 
		         Dim Arr_IncomeAccountGroup as Integer()
		         Dim strCodeW as String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	             Dim dis as Integer =  strCodeW.Split("-")(0)  
		      	If dis <> 80 Then
		      	'مالکیت فیش پرداخت اول و شهرداری باشد
                       Arr_IncomeAccountGroup = {1,8,15,22,29,36,43,50,57,64,71,78,90,91,92,93,94,95,96,97,98,99,100,101,105,106,107,108,109,110,111,112,113,114,115,116,123,130,131,132,133,134,135,136,137,138,139,140,141,142,150,155,122,159,163}
                  Else
                       Arr_IncomeAccountGroup = {1,7,10,11,131,132,133,134,135,136,137,138,139,140,141,142,150,155,122,162,163}
                  End If
              'plogAHM("TmpListReq.count ",TmpListReq.count)              
		      if TmpListReq.count >0 Then
			       For jj =0 to TmpListReq.count-1 
				        Info8.Income_Fiche=Nothing
				       ' plogAHM("nidProc ",Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse(TmpListReq(2).nidproc.tostring)).count.toString)
				       ' plogAHM("nidProc ",Guid.Parse(TmpListReq(1).nidproc.tostring).toString)
				        if String.IsNullOrEmpty(TmpListReq(jj).nidproc.tostring)=False And CheckTypeRequest(TmpListReq(jj).NidWorkflowDeff.ToString)=1 Then
				              	AllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse(TmpListReq(jj).nidproc.tostring))
					        	if AllFiche.count>0 Then AllFiche=AllFiche.Where(Function(f) (f.EumFicheStatus=3 or f.EumFicheStatus=5 or f.EumIncomePaymentType=4) And f.Payable>0 And Arr_IncomeAccountGroup.contains(f.CI_IncomeAccountGroup)).ToList() 
				                if AllFiche.count>0 Then HasSabeghe=True 
				              '  plogAHM("AllFiche.count ",AllFiche.count)
				        End if 
		            if HasSabeghe=True Then Exit For
                   Next
		        End if
		     
		   
		         
		    '    if TmpListReq.count >0 Then
			'       For jj =0 to TmpListReq.count-1 
			'	        Info8.Income_Fiche=Nothing
			'	        if String.IsNullOrEmpty(TmpListReq(jj).nidproc.tostring)=False Then
				'	    	AllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(Guid.Parse(TmpListReq(jj).nidproc.tostring))
				'	    	if AllFiche.count>0 Then AllFiche=AllFiche.Where(Function(f) (f.EumFicheStatus=3 or f.EumFicheStatus=5 or f.EumIncomePaymentType=4) And f.Payable>0).ToList() 
					'    	if AllFiche.count>0 Then
						'	    	For bb=0 to AllFiche.count-1
							'    	   AllCalculation = Info8.GetIncome().GetIncome_Calculation_InNidIncome(AllFiche(bb).NidIncome)  
							 '   	   if AllCalculation.count>0 Then  AllCalculation=AllCalculation.Where(Function(f) (f.CI_IncomeCalculation=100088 or f.CI_IncomeCalculation=1260 or f.CI_IncomeCalculation=1261 or f.CI_IncomeCalculation=1254 or f.CI_IncomeCalculation=100017 ) And f.SysValue>0).ToList()  '**2669316
							  '  	   if AllCalculation.count>0 Then HasSabeghe=True :Exit For
			    	           '     Next
	    	               ' End if
	              '   End if
	               '  if HasSabeghe=True Then Exit For
                   'Next
		       ' End if
		        
		 
		        'sp454607
		        'plogAHM("HasSabeghe ",HasSabeghe)
		        'plogAHM("CheckGroupPathOccupation ",CheckGroupPathOccupation)
		        if HasSabeghe=False And CheckGroupPathOccupation<>2 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "کاربرگرامی:",  " سابقه استفاده از محل فعالیت برای صنف اعلامی مشاهده نشد ")
   
   	        Catch Ex As Exception
		      Info8.AddError(BIZ.SA.EumErrorAction.Warning, "ee:",  ex.ToString)
		 End Try
End if'*

if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("24B35887-28E0-45C5-8952-B8E796067CFC")  Then
   	if String.IsNullOrEmpty(Info8.GetRequest.Info.RequesterAddress.ToString) =True Then 
   	   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "لطفا آدرس را وارد نمائید #") 
   	ElseIf String.IsNullOrEmpty(Info8.GetRequest.Info.TelephoneNo.ToString) =True Then 
   	   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "لطفا شماره تلفن  را وارد نمائید #") 
   	ElseIf String.IsNullOrEmpty(Info8.GetRequest.Info.Cellphone.ToString) =True Then 
   	   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "لطفا شماره همراه را وارد نمائید #") 
   	ElseIf String.IsNullOrEmpty(Info8.GetRequest.Info.RequesterNationalCode .ToString) =True Then 
   	   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "لطفا کدملی را وارد نمائید #") 
   	ElseIf String.IsNullOrEmpty(Info8.GetRequest.Info.PostCode.ToString) =True Then 
   	   Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "لطفا کد پستی را وارد نمائید #") 
   	End if            
End if

'sp 472933
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("24B35887-28E0-45C5-8952-B8E796067CFC") or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") Then
	
	Dim UL , ULtmp As New List(Of BIZ.SA.ClsUserGroup)			'گروه کاربری یوزر رو میگیره 
	ULtmp = Info8.User.UserGroups
 	UL = ULtmp.where(Function(f) ucase(f.GroupGuid.tostring)=ucase("4a58bd9f-df2a-4a15-b8a7-327d41542697")).tolist()	'جامع اداره نظارت-کارشناس کمیسیون
	if UL.count>0 Then 
		if Info8.GetRequest.Info.CI_Requester<>5 Then
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی", "فقط نوع متقاضی , مدیریت مهندسی و نظارت بر ساخت و سازها اجازه ثبت دارد - 472933") 
		Else
		
		End if
	End if	
End if


'sp 406753 

	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= ucase("24b35887-28e0-45c5-8952-b8e796067cfc") Then	
			
			Dim Layer13628_Condition = GettyMap() 'GetMapPrice_New("13628")
			if Layer13628_Condition = False Then
				Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی- sp 509653", "کد نوسازی در محدوده شهری میباشد،امکان ثبت درخواست اعمال قانون وجود ندارد") : Exit Function
			End if	
	
'sp 509653	  if  MapHarimJame3()=0 Then
'sp 509653		Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "اعتبارسنجی 406753", "  امکان ثبت درخواست وجود ندارد  با لایه حریم طرح جامع سوم همپوشانی ندارد 1.")
'sp 509653	  Else
	  
	'			Dim LstUserKar() AS String   
	   	     
			   	LstUserKar={"4a58bd9f-df2a-4a15-b8a7-327d41542697","4259E59F-4DDA-45EB-8C94-60771FE6D8DD"}
			'sp 477192   	
				if LstUserGroupId(LstUserKar)=1 Then
					if CheckTekrari = False Then
						Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "اعتبارس~", "  امکان ثبت درخواست تکراری وجود ندارد */* .")
						Exit Function
					Else
					End if	
			'end sp 477192
				Else
					Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "اعتبارس~", "  امکان ثبت درخواست وجود ندارد - .") : Exit Function
				End if 
'sp 509653	End if 	
	'	Call user()
	'	CreatTask(Info8.getRequest.Info.nidproc.tostring, "Activity_0dmjjqy",AssingTo,AssingToUserName) : Exit Function
	End if 
  	'sp  424450
	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("83f0fac4-2268-4005-8d52-34266dd9f595") or  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("79903151-af7c-4a73-9f18-6eb915f3293f") Then
	   	Dim LstUserKar() AS String   
'sp 451493	   	     
	   	LstUserKar={"094C16A1-CC90-4403-83DC-1A4DFB616EF1","CCFDF653-4751-404B-87C6-2BACC851BAC6","8E0E273D-B34C-46A7-97B2-2C6F3A4D6D72" _
	   	,"FE7DAC28-D9D2-4D31-8273-311B3E1DFB0E","37A86AE6-364A-48F2-A991-3713ACDB7B1A","959192EB-44CB-4194-9C5A-3C6C15C2B29D" _
	   	,"959192EB-44CB-4194-9C5A-3C6C15C2B29D","B474B169-07A1-41B3-AB95-48793FA8C241","607FB4CB-FEE7-4D68-BA76-786D3594F26C" _
	   	,"CDC05AC4-ECE6-47E3-ADDF-920AEC44BEFA","288524DB-06AE-4DCB-ABF9-990C3937EC42","D4F90C48-0975-45AE-BE18-AB3010E195FA" _
	   	,"F619A8BD-F192-401E-9DA6-BBDBBC1BFBF7","EDFF9387-19BF-427A-8985-C26ECB75C356","C93C6043-3059-443D-9288-DEEF8688A40B" _
	   	,"55EEF9D9-E5BC-49AB-AD8E-FC4C1F25FBBA","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d" _ 
		,"8bba5f92-c46f-4379-b59d-89ac3158164a","4a58bd9f-df2a-4a15-b8a7-327d41542697","81d43cf6-c638-488f-a471-fff5280c954d" _
		,"267e08f6-3faa-471b-aff5-0d0bdf186762"," a5c6ce90-e1ab-4c2f-bec5-60ca8e6d7daf","7ba10b77-4d81-4464-8783-ec446f14cab7" _
		,"922870db-3970-42d5-a755-8c8a11f2133c","30c87b1e-cb62-40de-852e-fc400ef8530b","af3d222a-25f7-42ba-9c58-048da74c8000" _
		,"d7ba1563-670d-4ece-b11d-dbc5f7d45057"}
		if LstUserGroupId(LstUserKar)=1 Then
		Else
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "010اعتبارس~", "  امکان ثبت درخواست وجود ندارد .") : Exit Function
		End if 
	End if  
	
  	

 
 	Dim LstUserFaniH() AS String
	LstUserFaniH={"b40f1faf-5885-457b-9217-ddac6f01898e"} 
	if LstUserGroupId(LstUserFaniH)=1 Then
		Info8.getRequest.Info.CI_ShahrsazRequestType=1
	End if 
	

	'sp  413161 ,454623, 457544
	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") or  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("C4C75308-5055-460B-999E-D2E7FDDE217E") Then
	   	
	   	Dim LstUserKar() AS String 
	   	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") Then '457544 مفاصا
	   	
	   		Dim strCodeW as String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)	'sp 481163 start
	        Dim dis as Integer =  strCodeW.Split("-")(0)  
		    If dis = 80 Then	 
		   	  		 
	   			LstUserKar={"5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d","8bba5f92-c46f-4379-b59d-89ac3158164a","4a58bd9f-df2a-4a15-b8a7-327d41542697","8358b5c9-9c87-4fdf-a181-f8707fdb51e9"}
	   		Else	  
	   		 	Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "اعتبار سنجی 80", "  امکان ثبت این درخواست از سرا 8 فقط برای منطقه ثامن میباشد  برای ثبت درخواست لطفا از سرا 10 اقدام نمایید.") : Exit Function	          
	   	 '   	LstUserKar={"9d612183-5bf3-439f-ade3-ce501dd08be2","81caa6f8-1da9-428b-a0c7-8c17e705b08e","91152337-9688-4618-9d9a-6401925743a9","83d509f4-f968-4103-ab24-f30e6bf03ac9","b40f1faf-5885-457b-9217-ddac6f01898e","7857a335-e3b9-41ae-ae08-e4af9361cd09","3346573f-55b8-407d-9153-eb96afb28371"} 
			End if	'end sp 481163 
		Else
		  	LstUserKar={"81caa6f8-1da9-428b-a0c7-8c17e705b08e","91152337-9688-4618-9d9a-6401925743a9","83d509f4-f968-4103-ab24-f30e6bf03ac9","b40f1faf-5885-457b-9217-ddac6f01898e","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d","8bba5f92-c46f-4379-b59d-89ac3158164a","4a58bd9f-df2a-4a15-b8a7-327d41542697","7857a335-e3b9-41ae-ae08-e4af9361cd09","3346573f-55b8-407d-9153-eb96afb28371"} 
		End if
		if LstUserGroupId(LstUserKar)=1 Then
		Else
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "010اعتبارسنجی", "  امکان ثبت درخواست وجود ندارد .") : Exit Function
		End if 
	End if  
	
	'sp 410413 
	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("de10d8ed-3989-4e6c-ad37-f5d915bc73d6")  Then
	   	Dim LstUserKar() AS String                                                                 
	   	LstUserKar={"5aef8344-b54e-412e-8cd9-d03eda410071","8bba5f92-c46f-4379-b59d-89ac3158164a","16bc1db1-a38d-4166-9ef4-ab5209dd562d","dd1e6da0-4ba3-41dc-ac1d-85e7ba15d529","4a58bd9f-df2a-4a15-b8a7-327d41542697"} 
		if LstUserGroupId(LstUserKar)=1 Then
		Else
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "10اعتبارسنجی", "  امکان ثبت درخواست وجود ندارد .") : Exit Function
		End if 
	End if  
	
	'sp 449020 
  	
  	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f088552b-7787-4ffe-ae7d-63f412d68b24") Then
  		Dim strCodePe as String
  		Dim tmpMantPe as Integer=0
		strCodePe = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
  		tmpMantPe=strCodePe.Split("-")(0)
  		if tmpMantPe<>80 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "**اعتبارسنجی80", " ** امکان ثبت درخواست وجود ندارد .") : Exit Function


  		Dim LstUserSamen() AS String   
	   	     
	   	LstUserSamen={"5aef8344-b54e-412e-8cd9-d03eda410071","8bba5f92-c46f-4379-b59d-89ac3158164a"}
		if LstUserGroupId(LstUserSamen)=1 Then
		Else
			Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "اعتبارس~80", "  امکان ثبت درخواست وجود ندارد .") : Exit Function
		End if 
  	
  	End if 
	
	'sp 400161 
	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="E5F7F86A-0D02-4A01-A01C-19F13F6BEDB9" or _
	   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="297949D0-9E25-419E-AD7C-62A7034910A1" or _
	   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="6481A0F2-BED0-45B3-A9E6-6734B5671787" or _
	   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="EBCCF491-D89D-46E6-BEE7-6F4459C01329" or _
	   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="3EBB3B53-972C-425F-9939-80E76AAB6BFB" or _
       ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="3636B5FF-0BF6-4CBC-BBD3-875DB4A1BB11" or _
       ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="6C67F07C-84DA-41CB-A454-BAC84078EA9F" or _
	   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="D0127432-AA14-4A16-BA07-D6A6C7DF5C5D" or _
	   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="F5F8B45F-A8D0-4384-9ED2-42DC747A1A20" Then
	   	'sp 400415 
	   	Dim strCodePe as String
  		Dim tmpMantPe as Integer=0
		strCodePe = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
  		tmpMantPe=strCodePe.Split("-")(0)
  		if tmpMantPe<>80 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "**اعتبارسنجی", " ** امکان ثبت درخواست وجود ندارد .") : Exit Function
	End if  
	'sp 404659 
'sp 405724
'	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="5437358A-F86B-48D8-A823-168388815F79" or _
'	   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="F5CAE479-5301-47B9-A7FB-845B1FBCB2C2" or _
'	   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="E81BBAEF-ED4B-4E44-9F73-DB45F23BC7F9" Then
'	   	Dim strCodePe as String
'  		Dim tmpMantPe as Integer=0
'		strCodePe = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
'  		tmpMantPe=strCodePe.Split("-")(0)
'  		if tmpMantPe=9 or tmpMantPe=11 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "اعتبارسنجی", " ** امکان ثبت درخواست وجود ندارد .") : Exit Function
'	End if  
	'sp 400411 
   if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="9D2031E0-8ACE-4CB3-A563-05FC1D42C8DF" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="82FBD9BC-EBA4-495C-99FF-22B035FB97FF" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="B0602DD4-FEF9-4AEB-A14A-251B3E86A6D4" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="01AAA0B7-8193-41D6-84F9-437FA39306A7" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="04DF58F1-731B-4371-8D53-47435F681362" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="AB69C90F-FBAF-42FC-9414-7DAE032A1182" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="6F00FA9D-02A3-43F9-9365-A7F42E9039E9" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="D1B85DEF-3F07-4B29-B99B-BF47D0A283F8" or _
	  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="677FBEE4-B05C-4FA6-AB19-CE897739195A" Then  
	  	
	   	Dim strCodePe as String
  		Dim tmpMantPe as Integer=0
		strCodePe = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
  		tmpMantPe=strCodePe.Split("-")(0)
  		if tmpMantPe<>80 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "اعتبارسنجی", " ** امکان ثبت درخواست وجود ندارد .") : Exit Function
	End if  


	'sp 418126
if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="1E386CF1-F6BC-4C10-99B5-03F50EEF8104" or _
   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="79903151-AF7C-4A73-9F18-6EB915F3293F" Then 
	Dim LstUserKar() AS String
	'sp 415726
	LstUserKar={"f9623472-adfc-45a9-a1b8-ddd1bffd36a7","8364897d-8836-4004-866d-514959163786","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","CDD67927-5EE9-4BEB-8C0E-830AF3F90AB0","c7ff2f46-79da-4e9d-92ce-7580a2d00a07","08d20b53-d68a-4d22-a0e0-b43eec51461e","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d","8bba5f92-c46f-4379-b59d-89ac3158164a","e4fd6027-2a09-4b82-b812-6644160636c5","dd1e6da0-4ba3-41dc-ac1d-85e7ba15d529","4a58bd9f-df2a-4a15-b8a7-327d41542697"} 
	if LstUserGroupId(LstUserKar)=1 Then
	Else
		Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*اعتبارسنجی", " ** امکان ثبت درخواست وجود ندارد .") : Exit Function
	End if 
End if 

 
	'sp 395214   ,sp 418126
'if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="83F0FAC4-2268-4005-8D52-34266DD9F595" or _
 '  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="1E386CF1-F6BC-4C10-99B5-03F50EEF8104" or _
 '  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="79903151-AF7C-4A73-9F18-6EB915F3293F" Then 
'	Dim LstUserKar() AS String
'	LstUserKar={"8364897d-8836-4004-866d-514959163786","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","c7ff2f46-79da-4e9d-92ce-7580a2d00a07","08d20b53-d68a-4d22-a0e0-b43eec51461e","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d","8bba5f92-c46f-4379-b59d-89ac3158164a","e4fd6027-2a09-4b82-b812-6644160636c5","dd1e6da0-4ba3-41dc-ac1d-85e7ba15d529","4a58bd9f-df2a-4a15-b8a7-327d41542697"} 
'	if LstUserGroupId(LstUserKar)=1 Then
'	Else
'		Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*اعتبارسنجی", " ** امکان ثبت درخواست وجود ندارد .") : Exit Function
'	End if 
'End if 
if  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)="C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB" Then
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "* امکان ثبت درخواست وجود ندارد .")
	Exit Function
End if 

if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"D6F25EBA-45A5-4851-ACED-7EED0105E977" And _
   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9" And _
   ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>"B3407F6F-967D-47DE-A8EB-9A7AB98BDA75" Then

  if MapAmlak()<>"0" Then
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", " ** امکان ثبت درخواست وجود ندارد . جهت تعیین تکلیف به سازمان زمین و مسکن مراجعه فرمایید")
	Exit Function
  End if
End if  


		Dim GroupInShahkar(),TempStrShahkar as String
		Dim CurrentGroupShahkar as String
		Dim UserListShahkar As New List(Of BIZ.SA.ClsUserGroup)
		UserListShahkar = Info8.User.UserGroups
		Dim iShahkar as Integer
		Dim IspolicsakhtemanShahkar as Integer
		IspolicsakhtemanShahkar=0
		if UserListShahkar.Any Then 
			For iShahkar=0 to UserListShahkar.Count-1
				TempStrShahkar +=  UserListShahkar(iShahkar).GroupGuid.ToString()
				if iShahkar< UserListShahkar.Count-1 Then TempStrShahkar = TempStrShahkar & ","
			Next iShahkar
		End if
		GroupInShahkar=TempStrShahkar.Split(",")
		For Each bb As String In GroupInShahkar
			If Lcase(bb)="4a58bd9f-df2a-4a15-b8a7-327d41542697" or Lcase(bb)="5f4030ce-99fb-4731-892b-0d0f525a910e" or Lcase(bb)="e4fd6027-2a09-4b82-b812-6644160636c5" _
			or Lcase(bb)="59434e63-ffd2-4de9-b26d-e795c44a4f6a" or Lcase(bb)="3c6497be-9795-40ed-9c17-257071751614" or Lcase(bb)="16bc1db1-a38d-4166-9ef4-ab5209dd562d" _
			or Lcase(bb)="5aef8344-b54e-412e-8cd9-d03eda410071" or Lcase(bb)="8bba5f92-c46f-4379-b59d-89ac3158164a" Then
				IspolicsakhtemanShahkar=1
			End if	
		Next
'sp 368074						
  if IspolicsakhtemanShahkar=0 And  Info8.Request.Info.CI_Requester<>8 And Info8.Request.Info.CI_Requester<>5 And Info8.Request.Info.CI_Requester<> 12 Then 
	Dim strCodeP as String
  	Dim tmpHouseP as Integer=0
	strCodeP = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
  	tmpHouseP=strCodeP.Split("-")(3)
  	'sp 374236
  	if tmpHouseP<1000 And ucase(Info8.User.UserGuid.ToString()) <> ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then   
		if  GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 45 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 23 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 90  And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 58 Then 						

			 Dim shahkar as New Biz.Communication.ClsShahkar()
			 'plogAHM("GetShahkarStatus ",shahkar.GetShahkarStatus(Info8.getRequest.Info.Cellphone, Info8.getRequest.Info.RequesterNationalCode).toString)
			 	 
			 
	'		 if Not shahkar.GetShahkarStatus(Info8.getRequest.Info.Cellphone, Info8.getRequest.Info.RequesterNationalCode) Then 
	'		 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی شاهکار @@", " اطلاعات شاهکار صحیح نمیباشد " ) : Exit Function
	'		 End if
		End if
	End if 		 
  End if 
  Plogfile("A***","FrmSh_Request")
'sp 418126 'ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F") or _ ' 
  if BIZ.SA.ClsCommon.CurrentShamsiDateString>"1400/05/01"  Then 
  	
     if   BIZ.SA.ClsCommon.CurrentShamsiDateString<"1401/01/00" And (ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") or _ 
		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("CB2A1920-7B97-4D20-9BBE-12D50F40BB5A") or _ 
		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("097C8C41-BA8C-4B60-8B2F-6BED0221F573") or _ 
		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("2635FE18-FE4C-4A2D-9D05-993756751262") or _ 
		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") or _ 
		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("307E2C2F-99DC-4AB3-A6A4-AF7A5D731581") or _ 
		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("8D20BE44-78AB-4001-BB94-F064D22B9CE1"))  Then 
		'SP 382181 ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7") or _ 
			Dim LstUserKar() AS String
			'sp 415726
			LstUserKar={"c7ff2f46-79da-4e9d-92ce-7580a2d00a07","f9623472-adfc-45a9-a1b8-ddd1bffd36a7","8364897d-8836-4004-866d-514959163786","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","CDD67927-5EE9-4BEB-8C0E-830AF3F90AB0","08d20b53-d68a-4d22-a0e0-b43eec51461e","05aed9ee-74d6-4300-a1b3-c7d37cfb15af","3a718066-1c33-445e-9b03-e0277f6cc756"} 
	 		if LstUserGroupId(LstUserKar)=1 Then
	 		Else
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "@اعتبارسنجی سرا10 ",  "شما مجاز به ثبت این نوع درخواست نمی باشید از سامانه ایساپ اقدام نمایید") : Exit Function
			End if 
	Else			
	  	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334")  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی سرا10 ", "شما مجاز به ثبت این نوع درخواست نمی باشید از سامانه ایساپ اقدام نمایید") : Exit Function
	  	Dim strCodeP as String
	  	Dim tmpHouseP as Integer=0
	  	'sp 364087
	  	'sp 366414
	  	'sp 368327
	 
	  	strCodeP = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	  	tmpHouseP=strCodeP.Split("-")(3)
	  	'sp 387397
	  	if strCodeP.Split("-")(0)=11 Then
	  		if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("D0127432-AA14-4A16-BA07-D6A6C7DF5C5D") or _
	  		ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("F5F8B45F-A8D0-4384-9ED2-42DC747A1A20") Then 
	  			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی سرا10 ", "شما مجاز به ثبت این نوع درخواست نمی باشید از سامانه ایساپ اقدام نمایید") : Exit Function
	  		End if 
	  	End if
	  	
	  	if tmpHouseP<1000 And(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("307E2C2F-99DC-4AB3-A6A4-AF7A5D731581") or _
	  	ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("CB2A1920-7B97-4D20-9BBE-12D50F40BB5A")) Then 
	  		if strCodeP.Split("-")(0) =9 or strCodeP.Split("-")(0)=11 Then
	  			Dim LstUserKar() AS String
	  			'sp 415726
				LstUserKar={"c7ff2f46-79da-4e9d-92ce-7580a2d00a07","","8364897d-8836-4004-866d-514959163786","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","CDD67927-5EE9-4BEB-8C0E-830AF3F90AB0","08d20b53-d68a-4d22-a0e0-b43eec51461e","05aed9ee-74d6-4300-a1b3-c7d37cfb15af","3a718066-1c33-445e-9b03-e0277f6cc756"} 
		 		if LstUserGroupId(LstUserKar)=1 Then
		 		Else
		 			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*اعتبارسنجی سرا10 ", "شما مجاز به ثبت این نوع درخواست نمی باشید از سامانه ایساپ اقدام نمایید") : Exit Function
		 		End if		
	  		Else
			
	  	  			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "@اعتبارسنجی سرا10 ",  "شما مجاز به ثبت این نوع درخواست نمی باشید از سامانه ایساپ اقدام نمایید") : Exit Function
	  	  		
	  	  	End if
	  	End if  		
	 End if
 End if 
 	 
 if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("E10EDCBA-7945-4DDD-8306-9BAA37529AE2")  Then 
  	Dim LstUserKar() AS String
   'add  B40F1FAF-5885-457B-9217-DDAC6F01898E  sp 395107
	LstUserKar={"bc6e8457-64ca-4bc4-9bcd-b2e4ecabc5b0","10be4733-1179-49ac-978f-8336391bc04a","81caa6f8-1da9-428b-a0c7-8c17e705b08e","9d612183-5bf3-439f-ade3-ce501dd08be2","91152337-9688-4618-9d9a-6401925743a9","83d509f4-f968-4103-ab24-f30e6bf03ac9","B40F1FAF-5885-457B-9217-DDAC6F01898E"} 
	if LstUserGroupId(LstUserKar)=1 Then
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "@اعتبارسنجی هیت فنی ",  "شما مجاز به ثبت این نوع درخواست نمی باشید ") : Exit Function
  	End if
  End if  			 
 	 
  'sp 350666
 ' if ucase(Info8.user.userguid.tostring)<>"E7D536CF-3FE6-4F23-9C05-CE6EEC87987F" Then  
	  if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("112265bc-f578-4f76-9685-ee54e739ae45") or _
	  	 ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("3708270c-b7d9-495e-b72a-20d072d26d52") or _
	  	 ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("44bf6d6c-a541-4d45-bc9e-9745aa0adaeb") or _
	  	 ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("4f56c548-1cf6-4c12-ac40-4f1319be03c9") or _
	  	 ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("2635FE18-FE4C-4A2D-9D05-993756751262") or _
	  	 ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("097C8C41-BA8C-4B60-8B2F-6BED0221F573") Then 
	  
	  	 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی سرا10 ", "شما مجاز به ثبت این نوع درخواست نمی باشید @@") : Exit Function
	  End if
'  End if	 
  
  'sp 317052
  'request test amir zarrin
  'sp 350051 
 
  if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=42 Then
  	if Ucase(Info8.GetRequest.Info.NidNosaziCode.Tostring) <>"D805B144-0C1B-4CB6-BFCA-86C5F27B3A99" Then 
  		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی..", "بر روی کد مناسب ثبت درخواست نشده است")
  	Else
  		Dim LstUserTahatorNew() AS String
	    LstUserTahatorNew={"083a32a0-0fe4-45b6-8366-ed07ed0af346"} 
		 if LstUserGroupId(LstUserTahatorNew)=0 Then
		 	 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی تهاتر", "شما مجاز به ثبت این نوع درخواست نمی باشید") : Exit Function
		 Else
		 	Exit Function	 
		 End if  
  		 
  	End if 
  End if
   Plogfile("1***","FrmSh_Request")
  
  '''sp 253354
if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) = 23  Then ' بررسی ملک
	Call DafaterTashilGari()
End if
  
  if Info8.GetRequest.Info.IsRegisterFromUGP =Nothing Then 
  TmpArchiveGroup=0
  Dim strCode as String
  Dim tmpDistrict as Integer=0
  strCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
		    tmpDistrict =  strCode.Split("-")(0)
  
  	Dim JamSepari as Boolean 
	JamSepari=False
	Dim Parking as Boolean 
	Parking=False
	Dim NidWDeff7 as String 
	NidWDeff7=Lcase(Info8.Request.Info.NidWorkflowDeff.tostring)
	
	'sp 350887
	  Plogfile("2***", GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)))
if 	tmpDistrict<>80 Then
	 if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=109 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=7 _
  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=9 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=18  _
  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=19 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=40 _
  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=54 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=9 _
  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=69 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=70 _
  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=83  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*@", " مجاز به ثبت درخواست نمی باشید")
 '358690 if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=107 or _
'358690'	 GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=111 Then Exit Function
  
End if 	
   Plogfile("2***","FrmSh_Request")
  
	
	'172796----با سلام لطفا امکان ثبت این گردشکار برای کلیه مناطق فراهم شود  172796
'	if ucase(NidWDeff7)=ucase("bb0854be-4002-4c0c-975c-04b4b8485334") And tmpDistrict<> 11 And tmpDistrict<> 8 Then 
	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*", "شما مجاز به ثبت این نوع درخواست نمی باشید ")
	'	Exit Function
'	End if



if  GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=90 Then
	'sp 322131
	Dim LstUserKar() AS String
'sp 334302
'sp 341398
	 LstUserKar={"7c17bd44-195e-4276-8461-360639885f3e","8bba5f92-c46f-4379-b59d-89ac3158164a","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d","e4fd6027-2a09-4b82-b812-6644160636c5"} 
	 if LstUserGroupId(LstUserKar)=1 Then
	 	if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=90 or jobAV=True   Then
	 	Else	
	    	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی <>", "شما مجاز به ثبت این نوع درخواست نمی باشید TT") : Exit Function
	    End if	
	 End if   
'sp 322131

'303594
Else
'sp 322131
	Dim LstUserKar() AS String
'sp 334302
'sp 341398
	 LstUserKar={"8bba5f92-c46f-4379-b59d-89ac3158164a","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d","e4fd6027-2a09-4b82-b812-6644160636c5", _
	 "8358b5c9-9c87-4fdf-a181-f8707fdb51e9","4259E59F-4DDA-45EB-8C94-60771FE6D8DD"} 
	 if LstUserGroupId(LstUserKar)=1 Then
	 	if  GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=100 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=88 _
	 	 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=75  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=91 _
	 	 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=77  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=32 _
	 	 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=78  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=101 _
	 	 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=106 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=84 _
	 	 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=104 or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") _
	 	 or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("c47465ab-9d56-4bff-be7a-63d2a8ce42d3") _
	 	 or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("442fa5bb-49ce-43b7-9783-a4b58dc2ebe2") or jobAV=True Then
	 
	 	 'sp 411461
	 	 'sp 430436
	   Else	
	   	If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
	   	Else
	   	 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی T<>", "شما مجاز به ثبت این نوع درخواست نمی باشید") : Exit Function
	   	End if 
	   End if 
	 End if   
'sp 322131
'303594
End if

'sp 469256
'if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("c47465ab-9d56-4bff-be7a-63d2a8ce42d3") Then	 
'	Dim LstUserKar() AS String
	' LstUserKar={"8bba5f92-c46f-4379-b59d-89ac3158164a","5aef8344-b54e-412e-8cd9-d03eda410071"}
	'if LstUserGroupId(LstUserKar)=1 Then
	' if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("c47465ab-9d56-4bff-be7a-63d2a8ce42d3") or jobAV=True Then
	' 
	'' 	   Else	
	'   	 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی S<>", "شما مجاز به ثبت این نوع درخواست نمی باشید") : Exit Function
	'   End if 
	 'End if 
'End if
	'end sp 469256 
	
Dim LstUserTahator() AS String
	 LstUserTahator={"3A6740C8-BC28-4464-97CC-CFF40E7CF1F7"} 
	 if LstUserGroupId(LstUserTahator)=0 Then
	 	if  GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=42 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی تهاتر", "شما مجاز به ثبت این نوع درخواست نمی باشید") : Exit Function
	 End if   
'303594

Dim LstUserDolati() AS String
'sp 334302
'add  B40F1FAF-5885-457B-9217-DDAC6F01898E  sp 395107
 LstUserDolati={"8bba5f92-c46f-4379-b59d-89ac3158164a","e4fd6027-2a09-4b82-b812-6644160636c5","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d","81caa6f8-1da9-428b-a0c7-8c17e705b08e","91152337-9688-4618-9d9a-6401925743a9","B40F1FAF-5885-457B-9217-DDAC6F01898E"} 
if LstUserGroupId(LstUserDolati)=1 And Info8.Request.Info.CI_Requester=8 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی نوع درخواست", "شما مجاز به انتخاب 'ادارات دولتی' نمی باشید ") : Exit Function



'282038
	 Dim LstUserPolis() AS String
 
	 LstUserPolis={"4A58BD9F-DF2A-4A15-B8A7-327D41542697","5aef8344-b54e-412e-8cd9-d03eda410071","16bc1db1-a38d-4166-9ef4-ab5209dd562d","8bba5f92-c46f-4379-b59d-89ac3158164a"} 
	 if LstUserGroupId(LstUserPolis)=1 Then
	 	if  GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 100 _
	 	And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 75 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 91 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 77 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 32 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 78 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 90 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 101 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 106 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 84 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 88 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 66 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 104 _
		And ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)<>Ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") _
		And ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring)<>ucase("24b35887-28e0-45c5-8952-b8e796067cfc") _
		And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>Ucase("c47465ab-9d56-4bff-be7a-63d2a8ce42d3") _
		And jobAV=False _ 
		Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی&", "شما مجاز به ثبت این نوع درخواست نمی باشید Test " + " | WorkFlow: " + Info8.getRequest.Info.NidWorkflowDeff.tostring + " | JobAV: " + jobAV.tostring ) : Exit Function
		'sp 430436
		'SP 406753
	 End if   
	 
   
'260895
	 Dim LstUserKarg() AS String
	 'sp 334302
	 LstUserKarg={"8bba5f92-c46f-4379-b59d-89ac3158164a","16BC1DB1-A38D-4166-9EF4-AB5209DD562D","5aef8344-b54e-412e-8cd9-d03eda410071"} 
	 if LstUserGroupId(LstUserKarg)=1 Then
	 	if  GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 100 _
	 	And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 75 _
	 	And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 84 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 88 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 77 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 78 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 101 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 106 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 90 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 91 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 32 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 66 _
		And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 104 _
		And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>Ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") _
		And ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)<>Ucase("c47465ab-9d56-4bff-be7a-63d2a8ce42d3") _
		And jobAV=False _
		 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی9", "شما مجاز به ثبت این نوع درخواست نمی باشید") : Exit Function
		 'sp 430436
	 End if   

'227515
'301500
'303742
'376373
'376373if tmpDistrict=1 or tmpDistrict=11 or tmpDistrict=12 or tmpDistrict=8 or tmpDistrict=10 Then
'376373	 Dim LstUserT() AS String
'376373	 'sp 301500 
'376373	 '"3A718066-1C33-445E-9B03-E0277F6CC756"
'376373	 LstUserT={"81CAA6F8-1DA9-428B-A0C7-8C17E705B08E","91152337-9688-4618-9D9A-6401925743A9","9D612183-5BF3-439F-ADE3-CE501DD08BE2"} 
'376373	 if LstUserGroupId(LstUserT)=1 Then
'376373	' Else
'376373	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring) =ucase("D84FA3D1-54FD-4D24-8000-758A300B2441") Then 
'376373	Else
'376373	   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی..", "شما مجاز به ثبت این نوع درخواست نمی باشید لطفا از سامانه شهروند سپاری اقدام نمایید")
'376373	    Exit Function
'376373	 End if    
'376373	 End if  
'376373 End if 
'274697
'    Dim LstUserT() AS String '274128
'	Dim LstUser8() as String
'	Dim LstUser9() as String
'	Dim LstUser10() as String
'	
'	Dim LstUser8_8() as String
'	Dim LstUser9_8() as String
'	Dim LstUser10_8() as String
'	Dim LstUser11_8() as String
'	Dim LstUser12_8() as String
'	Dim LstUser13_8() as String
'	
'	Dim LstUser1_1() as String
'	Dim LstUser2_1() as String
'	Dim LstUser3_1() as String
'	Dim LstUser4_1() as String
'	Dim LstUser5_1() as String 
'	
'	Dim LstUser1_11() as String
'	Dim LstUser2_11() as String
'	Dim LstUser3_11() as String
'	Dim LstUser4_11() as String
'	
'	Dim LstUser1_10() as String
'	Dim LstUser2_10() as String
'	Dim LstUser3_10() as String
'	Dim LstUser4_10() as String
'	Dim LstUser5_10() as String
'	Dim LstUser6_10() as String
'	
'	LstUserT={"3EA579D1-68D0-406B-BDD5-C5FBCFD1C1E8"}
'	LstUser1_1={"C7FF2F46-79DA-4E9D-92CE-7580A2D00A07"}
'	LstUser2_1={"3E8630FF-6553-4417-8675-FED217BB18E2"}
'	LstUser3_1={"8E0E273D-B34C-46A7-97B2-2C6F3A4D6D72"}
'	LstUser4_1={"8A111A55-112B-42A7-BEBE-F47E6228B14A"}
'	LstUser5_1={"8E0E273D-B34C-46A7-97B2-2C6F3A4D6D72"}
'	
'	LstUser1_11={"C7FF2F46-79DA-4E9D-92CE-7580A2D00A07"}
'	LstUser2_11={"D15D3B38-6D89-4060-A015-330E60DA2704"}
'	LstUser3_11={"8E0E273D-B34C-46A7-97B2-2C6F3A4D6D72"}
'	LstUser4_11={"6CEF70FB-48C3-40F2-B7D1-F2479A3E3A57"}
'	
'	LstUser1_10={"C7FF2F46-79DA-4E9D-92CE-7580A2D00A07"}
'	LstUser2_10={"094C16A1-CC90-4403-83DC-1A4DFB616EF1"}
'	LstUser3_10={"FDD795FB-E45F-4DB4-8D3B-B14AE12E1FF7","B9534154-B723-4494-BC89-012C78329832"}
'	LstUser4_10={"6CEF70FB-48C3-40F2-B7D1-F2479A3E3A57"}
'	LstUser5_10={"8364897d-8836-4004-866d-514959163786","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86"}
'	LstUser6_10={"FDD795FB-E45F-4DB4-8D3B-B14AE12E1FF7","B9534154-B723-4494-BC89-012C78329832"}
'	
'	LstUser12_8={"FDD795FB-E45F-4DB4-8D3B-B14AE12E1FF7","9B486E38-6D00-4A22-BAA6-51A19AD4D142"}
'	LstUser8={"E4FD6027-2A09-4B82-B812-6644160636C5" ,"59434E63-FFD2-4DE9-B26D-E795C44A4F6A"}
'	LstUser9={"600BA67A-00A7-42E6-A074-57FC9BA13C7F" ,"A70A5740-01F8-4370-9FB6-7E8260DF3253","D0146965-8F21-4EA2-A4FB-C6CDAC3F0382"}
'	'235389 am
'	LstUser10={"042BA0DA-F4F8-4033-A110-BCF9B5FCB5DB" ,"600BA67A-00A7-42E6-A074-57FC9BA13C7F",
'	"DE4611AF-8CAB-4217-B26A-BF8E04FDD35B","2070504A-4849-4163-83C0-6CDBB910BEA0",
'	"C7FF2F46-79DA-4E9D-92CE-7580A2D00A07","8364897d-8836-4004-866d-514959163786","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86",
'	"C23F3270-E2B9-4767-AB64-DBF591EA7334","0BD22C4C-7645-4C9E-A5BC-A10C5A9ECEFE",
'	"5D71230D-9EE0-4D83-8300-6D11028C269D"}
'	LstUser8_8={"8364897d-8836-4004-866d-514959163786","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86","3672a58c-c75b-451c-96dc-9c6c88aa3d86"}
'	LstUser9_8={"E4FD6027-2A09-4B82-B812-6644160636C5"}
'	LstUser13_8={"59434E63-FFD2-4DE9-B26D-E795C44A4F6A"}
'	LstUser10_8={"C6E0171F-7247-49CD-8A2B-B88BAB8AF667","A4861359-14FF-480E-81CF-7A8B13A02C4A","09D30EC3-B9CF-426F-B41B-C0C81A0B8D0A"}
'	LstUser12_8={"C6E0171F-7247-49CD-8A2B-B88BAB8AF667","A4861359-14FF-480E-81CF-7A8B13A02C4A","09D30EC3-B9CF-426F-B41B-C0C81A0B8D0A","59434E63-FFD2-4DE9-B26D-E795C44A4F6A"}
'	LstUser11_8={"C7FF2F46-79DA-4E9D-92CE-7580A2D00A07","1A59D5D9-6091-4641-97FA-8B0D01972333"}
'	if (ucase(NidWDeff7)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") or ucase(NidWDeff7)=ucase("032DD2F4-DC70-44F4-88FD-119580EC9884") or _
'		ucase(NidWDeff7)=ucase("CB2A1920-7B97-4D20-9BBE-12D50F40BB5A") or ucase(NidWDeff7)=ucase("E10EDCBA-7945-4DDD-8306-9BAA37529AE2") or _
'		ucase(NidWDeff7)=ucase("4BFCFAE5-ACF9-4159-9BF1-1B2C0781ED0E") or ucase(NidWDeff7)=ucase("307E2C2F-99DC-4AB3-A6A4-AF7A5D731581") or _
'		ucase(NidWDeff7)=ucase("F5F8B45F-A8D0-4384-9ED2-42DC747A1A20") or ucase(NidWDeff7)=ucase("C28A7410-B48A-4199-BA3A-B5254A20C1F5") or _
'		ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") or ucase(NidWDeff7)=ucase("27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7") or _
'		ucase(NidWDeff7)=ucase("C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB") or ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A") or _
'		ucase(NidWDeff7)=ucase("06E28FB5-E844-45E4-9FBA-631C1C324081") or ucase(NidWDeff7)=ucase("D0127432-AA14-4A16-BA07-D6A6C7DF5C5D") or _
'		ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") or ucase(NidWDeff7)=ucase("08BB48AD-8AA1-4BE1-8D81-EA966DAA5035") or _
'		ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F") or ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") or _
'		ucase(NidWDeff7)=ucase("F5CAE479-5301-47B9-A7FB-845B1FBCB2C2") or ucase(NidWDeff7)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") Or _
'		ucase(NidWDeff7)=ucase("3B155802-5BD4-4261-A6F3-B65B5177BF5F") or ucase(NidWDeff7)=ucase("DAACD02A-5B8D-4C40-9FFB-DE24B605D084") or _
'		ucase(NidWDeff7)=ucase("83F0FAC4-2268-4005-8D52-34266DD9F595") or ucase(NidWDeff7)=ucase("2C1EE596-5D29-4C0A-802A-1C2492B77A9F")) _
'		And (tmpDistrict =12 or (tmpDistrict =8 And BIZ.SA.ClsCommon.CurrentShamsiDateString >"1398/08/17") or (tmpDistrict =1 And BIZ.SA.ClsCommon.CurrentShamsiDateString >"1398/12/19") _
'	    or (tmpDistrict =10 And BIZ.SA.ClsCommon.CurrentShamsiDateString >"1398/12/27") or (tmpDistrict =11 And BIZ.SA.ClsCommon.CurrentShamsiDateString >"1398/12/23") )Then 
'			
'		if Map(3) And tmpDistrict =12 Then 
'	
'		Else 
'		
'		'	if ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") And set_UserGroupId(Info8.User.UserGroups)=9 Then 
'   		'	Else
'   		'		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی88888888888", "شما مجاز به ثبت این نوع درخواست نمی باشید لطفا از سامانه شهروند سپاری اقدام نمایید")
'		'		Exit Function
'   		'	End if 
'   			
'   		    if LstUserGroupId(LstUserT)=1 Then
'   		    
'   		    ElseIf LstUserGroupId(LstUser6_10)=1 And tmpDistrict =10 Then 
'   			ElseIf ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") And LstUserGroupId(LstUser9)=1 And tmpDistrict =12 Then 
'   			
'   			ElseIf ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") And LstUserGroupId(LstUser11_8)=1 And tmpDistrict =8 Then 
'   			ElseIf ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A") And LstUserGroupId(LstUser10)=1 And tmpDistrict =12 Then
'   			ElseIf ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A") And LstUserGroupId(LstUser13_8)=1 And tmpDistrict =8 Then   
'   			ElseIf LstUserGroupId(LstUser8_8)=1 And tmpDistrict =8 Then
'   			'246020 ElseIf LstUserGroupId(LstUser8_8)=1 And tmpDistrict =8 Then
'   			ElseIf (ucase(NidWDeff7)=ucase("83F0FAC4-2268-4005-8D52-34266DD9F595") OR ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F") ) And LstUserGroupId(LstUser12_8)=1 And tmpDistrict =8  Then
'			ElseIf LstUserGroupId(LstUser8)=1 And tmpDistrict =12 Then
'   				if ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") or ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") _
'   				or (NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A") or  ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") Then
'   				Else
'   					
'	   				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*7777", "شما مجاز به ثبت این نوع درخواست نمی باشید لطفا از سامانه شهروند سپاری اقدام نمایید")
'					Exit Function
'						
'   				End if
'   			'245971	
'   		   ElseIf LstUserGroupId(LstUser9_8)=1 And tmpDistrict =8 Then
'   				if ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") or ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") Then
'   			
'   		    	Else
'   					if ucase(NidWDeff7)=ucase("F5CAE479-5301-47B9-A7FB-845B1FBCB2C2") And LstUserGroupId(LstUser12_8)=1 Then 
'   					Else	
'		   				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی77", "شما مجاز به ثبت این نوع درخواست نمی باشید لطفا از سامانه شهروند سپاری اقدام نمایید")
'						Exit Function
'					End if	
'   		    	End if 
'   		'245956	
'   		    ElseIf ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") And LstUserGroupId(LstUser10_8)=1 And tmpDistrict =8 Then
'   		    
'   		    ElseIf (ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") OR ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6")OR ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A")OR _
'   		    ucase(NidWDeff7)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") OR ucase(NidWDeff7)=ucase("CB2A1920-7B97-4D20-9BBE-12D50F40BB5A")OR ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F")OR _
'   		    ucase(NidWDeff7)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") OR ucase(NidWDeff7)=ucase("307E2C2F-99DC-4AB3-A6A4-AF7A5D731581")OR ucase(NidWDeff7)=ucase("27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7")OR _
'   		    ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") OR ucase(NidWDeff7)=ucase("F5CAE479-5301-47B9-A7FB-845B1FBCB2C2") ) And LstUserGroupId(LstUser1_1)=1 And tmpDistrict =1 Then
'   		    ElseIf (ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") or ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") or ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A") _
'   		    ) And LstUserGroupId(LstUser3_1)=1 And tmpDistrict =1 Then  '267886
'   		    
'   		    ElseIf (ucase(NidWDeff7)=ucase("83F0FAC4-2268-4005-8D52-34266DD9F595") or ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F") or ucase(NidWDeff7)=ucase("6B26CCDD-4FF5-4B9D-BF15-128312E7A08A") or _
'   		    ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") or ucase(NidWDeff7)=ucase("D84FA3D1-54FD-4D24-8000-758A300B2441") or ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") or _
'   		    ucase(NidWDeff7)=ucase("2AE54B83-03DB-4DF5-A983-01A761395252") or ucase(NidWDeff7)=ucase("C83F6228-987B-4B5E-8906-56F7CD87EA7C") ) And LstUserGroupId(LstUser4_1)=1 And tmpDistrict =1 Then  '274126
'   		    
'   		    ElseIf (ucase(NidWDeff7)=ucase("D84FA3D1-54FD-4D24-8000-758A300B2441") OR ucase(NidWDeff7)=ucase("2AE54B83-03DB-4DF5-A983-01A761395252") OR ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6")OR _ 
'   		    ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") ) And LstUserGroupId(LstUser2_1)=1 And tmpDistrict =1 Then
'   		    
'   		    ElseIf (ucase(NidWDeff7)=ucase("83F0FAC4-2268-4005-8D52-34266DD9F595") OR ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F") OR ucase(NidWDeff7)=ucase("6B26CCDD-4FF5-4B9D-BF15-128312E7A08A") _ 
'   		    ) And LstUserGroupId(LstUser5_1)=1 And tmpDistrict =1 Then
'		    
'		    ElseIf (ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") OR ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6")OR ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A")OR _
'   		    ucase(NidWDeff7)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") OR ucase(NidWDeff7)=ucase("CB2A1920-7B97-4D20-9BBE-12D50F40BB5A")OR ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F")OR _
'   		    ucase(NidWDeff7)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") OR ucase(NidWDeff7)=ucase("307E2C2F-99DC-4AB3-A6A4-AF7A5D731581")OR ucase(NidWDeff7)=ucase("27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7")OR _
'   		    ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") ) And LstUserGroupId(LstUser1_11)=1 And tmpDistrict =11 Then
'			
'			ElseIf ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9")  And LstUserGroupId(LstUser4_11)=1 And tmpDistrict =11 Then
'			
'			ElseIf (ucase(NidWDeff7)=ucase("D84FA3D1-54FD-4D24-8000-758A300B2441") OR ucase(NidWDeff7)=ucase("2AE54B83-03DB-4DF5-A983-01A761395252") OR ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") _
'			Or ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") Or ucase(NidWDeff7)=ucase("E10EDCBA-7945-4DDD-8306-9BAA37529AE2") OR ucase(NidWDeff7)=ucase("F5CAE479-5301-47B9-A7FB-845B1FBCB2C2") _
'			 ) And LstUserGroupId(LstUser2_11)=1 And tmpDistrict =11 Then
'			 
'			ElseIf (ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") or ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") or ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A") _
'			Or ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F") or ucase(NidWDeff7)=ucase("83F0FAC4-2268-4005-8D52-34266DD9F595") ) _
'   		    And LstUserGroupId(LstUser3_11)=1 And tmpDistrict =11 Then
'		  
'		    ElseIf (ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") OR ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6")OR ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A")OR _
'   		    ucase(NidWDeff7)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") OR ucase(NidWDeff7)=ucase("CB2A1920-7B97-4D20-9BBE-12D50F40BB5A")OR ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F")OR _
'   		    ucase(NidWDeff7)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") OR ucase(NidWDeff7)=ucase("307E2C2F-99DC-4AB3-A6A4-AF7A5D731581")OR ucase(NidWDeff7)=ucase("27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7")OR _
'   		    ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") OR ucase(NidWDeff7)=ucase("F5CAE479-5301-47B9-A7FB-845B1FBCB2C2") ) And LstUserGroupId(LstUser1_10)=1 And tmpDistrict =10 Then
'			
'			ElseIf ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9")  And LstUserGroupId(LstUser4_10) And tmpDistrict =10 Then
'			
'			ElseIf (ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F") or ucase(NidWDeff7)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") or ucase(NidWDeff7)=ucase("307E2C2F-99DC-4AB3-A6A4-AF7A5D731581")or _
'			ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A") or ucase(NidWDeff7)=ucase("D0127432-AA14-4A16-BA07-D6A6C7DF5C5D")or ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") OR _
'			ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") OR ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A")OR _
'   		    ucase(NidWDeff7)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") OR ucase(NidWDeff7)=ucase("CB2A1920-7B97-4D20-9BBE-12D50F40BB5A")OR ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F")OR _
'   		    ucase(NidWDeff7)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9") OR ucase(NidWDeff7)=ucase("307E2C2F-99DC-4AB3-A6A4-AF7A5D731581")OR ucase(NidWDeff7)=ucase("27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7")OR _
'   		    ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9") OR ucase(NidWDeff7)=ucase("F5CAE479-5301-47B9-A7FB-845B1FBCB2C2")) And LstUserGroupId(LstUser5_10) And tmpDistrict =10 Then
'			
'			ElseIf (ucase(NidWDeff7)=ucase("D84FA3D1-54FD-4D24-8000-758A300B2441") OR ucase(NidWDeff7)=ucase("2AE54B83-03DB-4DF5-A983-01A761395252") OR ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") _
'			or ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") OR ucase(NidWDeff7)=ucase("E10EDCBA-7945-4DDD-8306-9BAA37529AE2") OR ucase(NidWDeff7)=ucase("F5CAE479-5301-47B9-A7FB-845B1FBCB2C2") ) And LstUserGroupId(LstUser3_10)=1 And tmpDistrict =10 Then
'			 
'			ElseIf (ucase(NidWDeff7)=ucase("DE10D8ED-3989-4E6C-AD37-F5D915BC73D6") or ucase(NidWDeff7)=ucase("5AEC0F88-6381-41F3-A5B6-636C8941886E") or ucase(NidWDeff7)=ucase("677FBEE4-B05C-4FA6-AB19-CE897739195A") _
'			or ucase(NidWDeff7)=ucase("6EB5AC8B-EC9E-4F20-A0AA-4371FED29BB9")or ucase(NidWDeff7)=ucase("83F0FAC4-2268-4005-8D52-34266DD9F595") or ucase(NidWDeff7)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F") _
'			 ) And LstUserGroupId(LstUser2_10)=1 And tmpDistrict =10 Then  '269954
'		    
'		    Else 
'				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی6666*", "شما مجاز به ثبت این نوع درخواست نمی باشید لطفا از سامانه شهروند سپاری اقدام نمایید")
'				Exit Function
'		    End if		
'		End if	
'	End if
'	
'	'192630	
'274697



	if GetIdWorkflow(ucase(NidWDeff7))=85 Then'ehdas masajed
	    if  Set_UserGroupId(Info8.User.UserGroups)=6 Then 
			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید ")
		 Exit Function
		End if 
	End if
	Plogfile("A",GetIdWorkflow(ucase(NidWDeff7)))
	Plogfile("00UserId",Set_UserGroupId(Info8.User.UserGroups))
			
	if GetIdWorkflow(ucase(NidWDeff7))=66 Then'هیئت فنی
	   if  Set_UserGroupId(Info8.User.UserGroups)=7 Then 
			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید ")
		 Exit Function
		End if 
	End if
	if 	GetIdWorkflow(ucase(NidWDeff7))=55 Then Exit Function
	
	if  GetIdWorkflow(ucase(NidWDeff7))=94  _
	 or GetIdWorkflow(ucase(NidWDeff7))=51 Then 
		Dim jj as Integer	
		Dim kk_N As Integer
	    Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	    
	    TmpListReq_N= Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
	    ''200726
	    if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3  ).tolist ''ابطال نباشد 	 
	    For jj =0 to TmpListReq_N.count-1 
	    	if 1=2 Then 'Taghsid(TmpListReq_N(jj).nidproc,1)
	    		Dim TmpError As New Global.Biz.SA.clsBizError
                TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
                TmpError.ErrorTitel = "اين ملك داراي بدهي اقساط مي باشد و پاسخ گويي به هرگونه استعلام كه منجر به تغيير مالكيت و نقل و انتقال ملك شود، منوط به پرداخت كل بدهي و تصفيه حساب كامل مي باشد"
                TmpError.ErrorKey = 1000
                Info8.ErrorResult.BizErrors.Add(TmpError) 
	    	End if
	    Next
	    if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 And f.eumrequeststatus<>2 ).tolist ''ابطال نباشد 	
	     

		For kk_N = 0 To TmpListReq_N.Count - 1
			
			if GetSara8WorkflowShahrvand(TmpListReq_N(kk_N).NidWorkflowDeff.ToString,0) = 1 Then
			
	
		   ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجیA", "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید") :	Exit Function
		    '	Dim TmpError As New Global.Biz.SA.clsBizError
            '    TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
            '    TmpError.ErrorTitel = "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید"
            '    TmpError.ErrorKey = 1000
             '   Info8.ErrorResult.BizErrors.Add(TmpError)
           End if     
		Next
		
		
	'	Exit Function
	''''test	Exit Function	
		Dim asBuildingChildNidBase_N As New List(Of BIZ.SA.ClsNosaziCodeType)
		Dim asBuildingChildNidBase_New As New List(Of BIZ.SA.ClsNosaziCodeType)
		
	
		
		asBuildingChildNidBase_N = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		'melk
		
	
		if asBuildingChildNidBase_N.any Then  asBuildingChildNidBase_New = asBuildingChildNidBase_N.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList()
		
	
		if asBuildingChildNidBase_New.any Then TmpListReq_N =Info8.GetRequest.GetAllRequestList(asBuildingChildNidBase_New(0).NidNosaziCode) 	 				        
		 ''200726
		if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3  ).tolist ''ابطال نباشد 	 
	    For jj =0 to TmpListReq_N.count-1 
	    	if 1=2 Then
	    		Dim TmpError As New Global.Biz.SA.clsBizError
                TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
                TmpError.ErrorTitel = "اين ملك داراي بدهي اقساط مي باشد و پاسخ گويي به هرگونه استعلام كه منجر به تغيير مالكيت و نقل و انتقال ملك شود، منوط به پرداخت كل بدهي و تصفيه حساب كامل مي باشد"
                TmpError.ErrorKey = 1000
                Info8.ErrorResult.BizErrors.Add(TmpError) 
	    	End if
	    Next

		if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 And f.eumrequeststatus<>2 ).tolist ''ابطال نباشد 	
		
		For kk_N = 0 To TmpListReq_N.Count - 1
		
		
		
			if GetSara8WorkflowShahrvand(TmpListReq_N(kk_N).NidWorkflowDeff.ToString,1) = 1 Then
			'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجیM", "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید") :	Exit Function
				'208467
				if 1=2 Then
					Dim TmpError As New Global.Biz.SA.clsBizError
	                TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
	                TmpError.ErrorTitel = "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید"
	                TmpError.ErrorKey = 1000
	                Info8.ErrorResult.BizErrors.Add(TmpError)
	            End if    
            End if    
		Next 
		'sakhteman
	
	'		Dim tmpFileStreamcdsqn As New System.IO.FileStream( "C:\logC\nosazi.txt", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite)
'Dim tmpBinaryWritercdsqn As New System.IO.BinaryWriter(tmpFileStreamcdsqn)
'tmpBinaryWritercdsqn.Write ("nn" &  Info8.GetRequest.Info.NidNosaziCode.tostring )
'tmpFileStreamcdsqn.Close()
		
		asBuildingChildNidBase_N = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
	 
		if asBuildingChildNidBase_N.any Then  asBuildingChildNidBase_New = asBuildingChildNidBase_N.Where(Function(f) f.EumNosaziCodeObjType = 3).ToList()
		if asBuildingChildNidBase_New.any Then 	 TmpListReq_N =Info8.GetRequest.GetAllRequestList(asBuildingChildNidBase_New(0).NidNosaziCode) 	 				        
	
			 ''200726
		if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3  ).tolist ''ابطال نباشد 	 
	    For jj =0 to TmpListReq_N.count-1 
	    '200726
	    	if 1=2 Then
	    		Dim TmpError As New Global.Biz.SA.clsBizError
                TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
                TmpError.ErrorTitel = "اين ملك داراي بدهي اقساط مي باشد و پاسخ گويي به هرگونه استعلام كه منجر به تغيير مالكيت و نقل و انتقال ملك شود، منوط به پرداخت كل بدهي و تصفيه حساب كامل مي باشد"
                TmpError.ErrorKey = 1000
                Info8.ErrorResult.BizErrors.Add(TmpError) 
	    	End if
	    Next
	    
		if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 And f.eumrequeststatus<>2 ).tolist ''ابطال نباشد 	
			'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجیB", "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید") :	Exit Function
			 
'sp 432362 
		if (MapMosharekati()<> 0) And (uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") or _
		uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9")or UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("5D7FEE70-04D1-4047-86C8-546BD7618503")or _
		uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F")or UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("D8F77DFA-ECE9-4F52-9AEE-3458D5D91272")or _
		UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("D9E55CD5-922B-4D56-80C8-7FE5F7178C95"))Then 	
		Else	
			For kk_N = 0 To TmpListReq_N.Count - 1
				if GetSara8WorkflowShahrvand(TmpListReq_N(kk_N).NidWorkflowDeff.ToString,1) = 1 Then 
			'208467
					if 1=2 Then
						Dim TmpError As New Global.Biz.SA.clsBizError
		                TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
		                TmpError.ErrorTitel = "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید"
		                TmpError.ErrorKey = 1000
		                Info8.ErrorResult.BizErrors.Add(TmpError)
	                End if
	            End if    
			Next 
		End if	
	End if	
	if GetIdWorkflow(ucase(NidWDeff7))=105 or GetIdWorkflow(ucase(NidWDeff7))=7 or GetIdWorkflow(ucase(NidWDeff7))=112 _
	or GetIdWorkflow(ucase(NidWDeff7))=94  or GetIdWorkflow(ucase(NidWDeff7))=51 Then JamSepari=True
	

	if JamSepari=True Then 
	  if Info8.User is Nothing Then 	
		
		Exit Function
	  Else 
	  	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجیsh", "شما مجاز به ثبت این نوع درخواست نمی باشید ")
	  	Exit Function	
	  End if 	
	End if 	 	
'sp 345593
    if  Set_UserGroupId(Info8.User.UserGroups)=5 Then 
		if GetIdWorkflow(ucase(NidWDeff7))<> 58 And GetIdWorkflow(ucase(NidWDeff7))<> 90 Then 
			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجیD", "شما مجاز به ثبت این نوع درخواست نمی باشید ")
			Exit Function
		End if 
	End if	
	if GetIdWorkflow(ucase(NidWDeff7))=88  or GetIdWorkflow(ucase(NidWDeff7))=66 Then Parking=True
	
'177925	if ucase(NidWDeff7)=ucase("F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9") And Set_UserGroupId(Info8.User.UserGroups)=4 And tmpDistrict=10 Then 
'177925		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "**اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید لطفا از شهروند سپاری اقدام نمایید")
'177925  		Exit Function
'177925  	End if	
  ''''''''''''''''''''''''''''''''''''
  'And Set_UserGroupId(Info8.User.UserGroups)=4 hedayat
  if GetIdWorkflow(ucase(NidWDeff7))=83  And tmpDistrict=10 And BIZ.SA.ClsCommon.CurrentShamsiDateString>"1397/05/26" Then 
	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "**اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید لطفا از شهروند سپاری اقدام نمایید")
  	Exit Function
 End if	
 
 if GetIdWorkflow(ucase(NidWDeff7))=83  And tmpDistrict<>10  And BIZ.SA.ClsCommon.CurrentShamsiDateString>"1397/06/00" Then 
	if tmpDistrict<>80  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "**اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید لطفا از شهروند سپاری اقدام نمایید") :  	Exit Function
 End if	
 ' //// GetSh_Workflow(Guid pNidWorkflowDeff)
  Dim polickargozar as Integer =0
 
  '112474
 ' if Info8.GetRequest.Info.NidWorkItem ="1405416"  Then insertReport(Info8.GetRequest.Info.NidNosaziCode, "RptBazdidSys" , "بازدید سیستمی", "/Sara8Reports/RptRevisitApartment", "13", "1",13)
																										
' IF Set_UserGroupId(Info8.User.UserGroups) <> 3 And ucase(Info8.Request.Info.NidWorkflowDeff.tostring)="F5CAE479-5301-47B9-A7FB-845B1FBCB2C2" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "**اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید")
if JamSepari=False Then 
  if Set_UserGroupId(Info8.User.UserGroups) <> 3  And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=45 Then  
  	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "**اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید")
  	Exit Function
  End if
End if  	
  if    GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=7 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=94 _
  	 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=51 or JamSepari = True Then 
  Else
    Dim UserList7_New As New List(Of BIZ.SA.ClsUserGroup)
	 UserList7_New = Info8.User.UserGroups
	 Dim i_New as Integer
	 Dim User128768 as Boolean =False
  	if UserList7_New.Any Then 
	 	For i_New=0 to UserList7_New.Count-1
			If "3346573F-55B8-407D-9153-EB96AFB28371" = ucase(UserList7_New(i_New).GroupGuid.ToString()) Then User128768=True
			'278466
			If "4A58BD9F-DF2A-4A15-B8A7-327D41542697" = ucase(UserList7_New(i_New).GroupGuid.ToString()) Then User128768=True
		Next i_New
		'217334
		For i_New=0 to UserList7_New.Count-1
			If ucase("c7ff2f46-79da-4e9d-92ce-7580a2d00a07") = ucase(UserList7_New(i_New).GroupGuid.ToString()) Then User128768=True
		Next i_New
	End if

	  if User128768= False Then 
	  	if ReqTafkik_A()= False Then 
	  		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا ابتدا درخواست عدم خلاف و پایانکار ساختمان مربوطه را تعیین تکلیف نمایید" )
	  		Exit Function
	  	End if	
	  End if	
  
  
  	if CStr(Info8.Request.Info.TelephoneNo)="" Then
  		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا مقدار شماره تلفن را وارد نمایید " )
  		Exit Function
  	End if	
  	if CStr(Info8.Request.Info.Cellphone)="" Then 
  		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا مقدار شماره موبایل را وارد نمایید " )
  		Exit Function
  	Else
  	'155113
  		if Len(Info8.Request.Info.Cellphone)<>11 Then 
	  			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی*", "لطفا مقدار شماره موبایل را اصلاح نمایید " )
	  			Exit Function
	  	End if	
  		if CStr(Info8.Request.Info.Cellphone).Substring(0,2)<> "09"	Then 
  			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا مقدار شماره موبایل را اصلاح نمایید " )
  			Exit Function
  		
  		End if 	
  		'155113
  	End if	
  End if	
  Dim checkBlackJamiee as Integer=0
  if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=7  _
  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=94 _
  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=51 _
  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=77  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=78  Then checkBlackJamiee=1
  'sp 326269
  if checkBlackJamiee=0 And JamSepari= False Then
  	if Info8.GetFunctions.CheckInUserBlackListWithUserID(Info8.User.UserGuid) = False Then checkBlackJamiee=1
 End if
'aaa
 'Exit Function
  
 if checkBlackJamiee=1 And JamSepari=False Then
  
 
		  Dim EdgeLst As New List(Of DtoSC.Base_Edge)
		  Dim OwnerLst As New List(Of DtoSC.Base_Owner)
		  Dim AddLst As New DtoSC.Base_AddressInfo
		  Dim NosaziLst As List(Of Biz.SA.ClsNosaziCodeType)
		  Dim NidBaseStr As New Guid 
		  UserInSabtShardari=False
		  
		  NosaziLst = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		  NosaziLst = NosaziLst.Where(Function(f) f.EumNosaziCodeObjType = 2).tolist
		  
		  if NosaziLst.any Then
			  NidBaseStr = Info8.GetFunctions.GetNidBase(NosaziLst(0).NidNosaziCode, Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000"))
			  Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
			  Info8.ParameterList.Add( "Parvandeh_House_NidBase" , NidBaseStr.ToString)
			  
			  EdgeLst = Info8.GetParvandeh_House.GetBase_Edge
			  OwnerLst = Info8.GetParvandeh_House.GetBase_Owner
			  AddLst = Info8.GetParvandeh_House.GetBase_AddressInfo
			  'sp 414735
			   Dim UserCom as Integer=0
			  if "5aec0f88-6381-41f3-a5b6-636c8941886e"=Lcase(Info8.Request.Info.NidWorkflowDeff.tostring) Then
			 	Dim LstUserComplet() AS String
				LstUserComplet={"4a58bd9f-df2a-4a15-b8a7-327d41542697"} 
				if LstUserGroupId(LstUserComplet)=1 Then
					UserCom=1
				End if 
			  End if
			  'sp 452287
			  if Ucase(Info8.Request.Info.NidWorkflowDeff.tostring) = Ucase("F16712D1-9938-4692-AB59-3CD090D70B8F") or Ucase(Info8.Request.Info.NidWorkflowDeff.tostring) = Ucase("24B35887-28E0-45C5-8952-B8E796067CFC") Then
			 	Dim LstUserComplet() AS String
				LstUserComplet={"4a58bd9f-df2a-4a15-b8a7-327d41542697"} 
				if LstUserGroupId(LstUserComplet)=1 Then
					UserCom=1
				End if 			  
			  End if
			  If EdgeLst.Count = 0 And OwnerLst.Count = 0 And AddLst.MainAddress = "" And UserCom=0 And ucase(Info8.Request.Info.NidWorkflowDeff.tostring)<>ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc")  Then
			    Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "اطلاعات ملک تکمیل نمی باشد لطفا اطلاعات پرونده ملک تکمیل گردد " )
			    Exit Function
			  End If
		   Else
		   	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", " ملک باید ایجاد و اطلاعات تکمیل گردد " )
		   	Exit Function
		  End if 
		
			Dim TmPUserIdGroup7 as Integer =0
			
		 	NidWDeff7=Lcase(Info8.Request.Info.NidWorkflowDeff.tostring)
		 	if GetIdWorkflow(ucase(NidWDeff7))=7  _
		 	or GetIdWorkflow(ucase(NidWDeff7))=94   _
		 	or GetIdWorkflow(ucase(NidWDeff7))=51 Then Exit Function
		 
		 	
		 if GetIdWorkflow(ucase(NidWDeff7))=56	Then
		 
		 '''''''''''''''''''''''
		 Dim M_DatePay, M_DateBank, M_Date As String
		 Dim kk As Integer
		        Dim TmpListReq As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
		        TmpListReq= Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
		        if TmpListReq.count >0 Then TmpListReq=TmpListReq.where (Function (f) f.eumrequeststatus<>3 ).tolist ''ابطال نباشد 	
		        
		        For kk = 0 To TmpListReq.Count - 1
		            If GetSara8Workflow(TmpListReq(kk).NidWorkflowDeff.ToString) = 1 Then
		            
		            	Info8.ParameterList.REMove("Nroc") 
		            	Info8.ParameterList.REMove("NidRevisit") 
		                Info8.ParameterList.Add("NidRevisit", TmpListReq(kk).NidRevisit.ToString)
		                Info8.ParameterList.Add("NidProc", TmpListReq(kk).NidProc.ToString)
		                
		               ' Info8.ParameterList.Add("NidProc", Info8.GetIncome.NidIncome.ToString)
		                Dim AllFiche As List(Of Global.DtoSC.Income_Fiche)
		                Info8.Income_Fiche=Nothing
		                AllFiche = Info8.GetIncome_Fiche.GetAll_Fiche(TmpListReq(kk).NidProc)
		                
		                AllFiche = AllFiche.Where(Function(f) f.BankPaymentDate < "1393/01/01" Or f.PaymentDate < "1393/01/01" ).ToList
		               
		                M_DatePay = AllFiche.Max(Function(f) f.PaymentDate)
		                M_DateBank = AllFiche.Max(Function(f) f.BankPaymentDate)
		                
		                If M_Date < M_DatePay Then
		                    If M_DatePay > M_DateBank Then
		                        M_Date = M_DatePay
		                    Else
		                        M_Date = M_DateBank
		                    End If
		                ElseIf M_Date < M_DateBank Then
		                    If M_DatePay > M_DateBank Then
		                        M_Date = M_DatePay
		                    Else
		                        M_Date = M_DateBank
		                    End If
		                End If
		             End if   
		        Next
		        
		        
		 'if M_Date<"1393/01/01" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "/اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید فیش تایید شده بابت پروانه در سال 92 ندارید")
		 '''''''''''''''''''''''''''''''
		 		Dim TmPUserIdGroup7_M as Integer =0
		 		Dim GroupIn7(),TempStr7 as String
				Dim CurrentGroup7 as String
				Dim UserList7 As New List(Of BIZ.SA.ClsUserGroup)
				if JamSepari=False Then UserList7 = Info8.User.UserGroups
				
				
			    Dim i as Integer
			
				if UserList7.Any Then 
				   	For i=0 to UserList7.Count-1
				   		TempStr7 +=  UserList7(i).GroupGuid.ToString()
				   		if i< UserList7.Count-1 Then TempStr7 = TempStr7 & ","
				   	Next i
			    End if
			    GroupIn7=TempStr7.Split(",")
		    	 For Each bb As String In GroupIn7
				
			   		If "3346573F-55B8-407D-9153-EB96AFB28371" =  Ucase(bb) Then TmPUserIdGroup7_M=1
				 Next
				 '217334
				 For Each bb As String In GroupIn7
				
			   		If ucase("c7ff2f46-79da-4e9d-92ce-7580a2d00a07") =  Ucase(bb) Then TmPUserIdGroup7_M=1
				 Next 
				 if TmPUserIdGroup7_M =0 Then 
				 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*/اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید")
				 	Exit Function
				 End if	
		
		 End if	
		 
		
		 Rtype= GetSara8Workflow(NidWDeff7)
		 Rtype=GuidWorkFlow
		Rgroup=M_ShahrsaziArchiveGroup	
			
		
		  'if Not (NidWDeff7="01aaa0b7-8193-41d6-84f9-437fa39306a7" or NidWDeff7="e6c9db23-6cea-4161-813e-078193082dd9"  _
		  '			or NidWDeff7="c4c75308-5055-460b-999e-d2e7fdde217e" or NidWDeff7="fe80bd5c-f87b-495a-8f13-77ee05cbfd6d" _
		  '			or NidWDeff7="01e1d2a4-a299-4c88-8270-8419e81d5977" _
		  '			or NidWDeff7="b53b60fc-db43-40e5-b954-0756de68c498" or NidWDeff7="2518b6d1-54bb-4c94-beac-2ba4d19bb100" _
		  '			or NidWDeff7="1bea53f3-426c-4423-af29-1fcd2ae1aa62" or NidWDeff7="c4c75308-5055-460b-999e-d2e7fdde217e" _
		 '			or NidWDeff7="045bea33-0a29-4f35-8cc1-672417f49dc1" or NidWDeff7="d84fa3d1-54fd-4d24-8000-758a300b2441" _
		 '			or NidWDeff7="008df040-922b-41a6-a3b2-7f35b1c870bb" or NidWDeff7="8b4af4ad-a7e7-4330-bd2c-f39b3a07adfd" _
		 '			or NidWDeff7="9d2031e0-8ace-4cb3-a563-05fc1d42c8df" or NidWDeff7="8b4af4ad-a7e7-4330-bd2c-f39b3a07adfd" _
		 '			or NidWDeff7="11ad7cfc-b3e8-48e3-8ec5-88de9abdd5aa" or ucase(NidWDeff7)="53E7F83A-59A2-4C0D-805A-7BE86E1DC56D" _
		 '			or NidWDeff7="e6c9db23-6cea-4161-813e-078193082dd9" or NidWDeff7="7c90f477-7515-4ad3-aefe-fd8f1fe4d11a" _
		'			or ucase(NidWDeff7)="008DF040-922B-41A6-A3B2-7F35B1C870BB" or ucase(NidWDeff7)="8DCD2E0C-8E5E-48A4-BAD6-8C2A1B4901B1" _
		'			or ucase(NidWDeff7)="5DC809DF-16C5-459B-B6F8-C28A049A32AC" or ucase(NidWDeff7)="8B4AF4AD-A7E7-4330-BD2C-F39B3A07ADFD" _
		'			or NidWDeff7="8b26053c-5554-48fb-99fb-62431aa06c07" or NidWDeff7="53e7f83a-59a2-4c0d-805a-7be86e1dc56d" _
		'			or ucase(NidWDeff7)="5AEC0F88-6381-41F3-A5B6-636C8941886E" _
		'			or ucase(NidWDeff7)="559AAE89-1052-4394-8D0A-6105A26B04F3" or ucase(NidWDeff7) ="0358FB22-3DA1-4642-A138-0D17AD16635A")  Then 
		'aaaaaa			
	'	Exit Function
	
	
 'sp 333438 ============================================================			
  'sp  333707 if Rtype=189	Then
   'sp  333707  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"اعتبارسنجی","به استناد سمپای 333438 امکان ثبت درخواست بهره برداری موقت صنفی وجود ندارد .") 
   'sp  333707   Exit Function
  'sp  333707 End if			
  '======================================================================
	
	
	
	
	
	
		 
		if Not (Rtype=190 or Rtype=202 or Rtype=201 or Rtype=200 or Rtype=203 or Rtype=116 or Rtype=117 or Rtype=194 or Rtype=118 	) And JamSepari=False And Parking=False		Then 
					
					Dim GroupIn7(),TempStr7 as String
					Dim CurrentGroup7 as String
					Dim UserList7 As New List(Of BIZ.SA.ClsUserGroup)
					
					if JamSepari=False  Then UserList7 = Info8.User.UserGroups
					TmPUserIdGroup7=0
					
				    Dim i as Integer
				    
				    if UserList7.Any Then 
				    	For i=0 to UserList7.Count-1
				    		TempStr7 +=  UserList7(i).GroupGuid.ToString()
				    		if i< UserList7.Count-1 Then TempStr7 = TempStr7 & ","
				    	Next i
				    End if
				    GroupIn7=TempStr7.Split(",")
		    
				 For Each bb As String In GroupIn7
					If "E4FD6027-2A09-4B82-B812-6644160636C5" =  Ucase(bb) Then TmPUserIdGroup7=1 ' وصول درآمد 
					If ucase("16bc1db1-a38d-4166-9ef4-ab5209dd562d") =  Ucase(bb) Then TmPUserIdGroup7=1 ' وصول درآمد
					If ucase("5aef8344-b54e-412e-8cd9-d03eda410071") =  Ucase(bb) Then TmPUserIdGroup7=1 ' وصول درآمد
					'sp 334302
					If ucase("8bba5f92-c46f-4379-b59d-89ac3158164a") =  Ucase(bb) Then TmPUserIdGroup7=1 ' وصول درآمد
					if "3C6497BE-9795-40ED-9C17-257071751614" =  Ucase(bb) Then TmPUserIdGroup7=1 ' وصول درآمد 
					If "59434E63-FFD2-4DE9-B26D-E795C44A4F6A" =  Ucase(bb) Then polickargozar=1  '  پلیس ساختمان - کارگزار
				 Next
				 
				 if (Rtype=151 or Rtype=189) Then '81265
				 	if polickargozar=1 Then 
				 		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "دفاتر /اعتبارسنجی*", "شما مجاز به ثبت این نوع درخواست نمی باشید")
				 		Exit Function
				 	End if	
				 ElseIf TmPUserIdGroup7 =1  Then 
				  	if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=90  or  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") or ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=Ucase("c47465ab-9d56-4bff-be7a-63d2a8ce42d3") or jobAV=True Then
				  	'sp 430436 
				  	Else
				  		Dim LstUserPKargozar() AS String
	 					LstUserPKargozar={"3C6497BE-9795-40ED-9C17-257071751614"}
	 					 
	 					if LstUserGroupId(LstUserPKargozar)=1 And Rgroup =1 Then 
				  		Else
					 		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "دفاتر /اعتبارسنجی$", "شما مجاز به ثبت این نوع درخواست نمی باشید")
					 		Exit Function
					 	End if	
				 	End if	
				 End if
		   End if
		   
		 '  Not (Rtype=200 or Rtype=203 ) And '150161
		   if  Info8.GetRequest.GetHasNidProc(Info8.GetRequest.Info.NidProc).tostring=False And JamSepari=False And Parking=False Then  'sampa 79457
		   		Dim GroupIn7(),TempStr7 as String
				Dim CurrentGroup7 as String
				Dim UserList7 As New List(Of BIZ.SA.ClsUserGroup)
				if JamSepari=False Then UserList7 = Info8.User.UserGroups
				Dim i as Integer
				if UserList7.Any Then 
				 	For i=0 to UserList7.Count-1
				 		TempStr7 +=  UserList7(i).GroupGuid.ToString()
				   		if i< UserList7.Count-1 Then TempStr7 = TempStr7 & ","
				  	Next i
				End if
				GroupIn7=TempStr7.Split(",")
		    	TmPUserIdGroup7=0
				For Each bb As String In GroupIn7
					If "C65C30A2-B7B8-4AF5-95DC-E0BE1F2AF6E9" =  Ucase(bb) Then TmPUserIdGroup7=1
				Next
				  
				if TmPUserIdGroup7 =1 Then 
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "لغو موقت دفاتر /اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید")'76616
					Exit Function
				End if	
		   End if 
		 '-----------------------------  87648
		   if (GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=89)  And Info8.GetRequest.GetHasNidProc(Info8.GetRequest.Info.NidProc).tostring=False And JamSepari=False And Parking=False Then  'sampa 79457
		   		Dim GroupIn7(),TempStr7 as String
				Dim CurrentGroup7 as String
				Dim UserList7 As New List(Of BIZ.SA.ClsUserGroup)
				if JamSepari=False Then UserList7 = Info8.User.UserGroups
				Dim i as Integer
				if UserList7.Any Then 
				 	For i=0 to UserList7.Count-1
				 		TempStr7 +=  UserList7(i).GroupGuid.ToString()
				   		if i< UserList7.Count-1 Then TempStr7 = TempStr7 & ","
				  	Next i
				End if
				GroupIn7=TempStr7.Split(",")
		    	TmPUserIdGroup7=0
				For Each bb As String In GroupIn7
				     'add  b40f1faf-5885-457b-9217-ddac6f01898e sp 395107
					If "81caa6f8-1da9-428b-a0c7-8c17e705b08e" =  Lcase(bb) or "b40f1faf-5885-457b-9217-ddac6f01898e" =  Lcase(bb) Then TmPUserIdGroup7=1
				Next
				  
				if TmPUserIdGroup7 =1 Then 
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "   New دفاتر /اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید")'87648
					Exit Function
				End if	
		   End if  
		  
		'-----------------------------------------------   
		' 		if UserIdGroup=7 Then 
		' 			Dim NidWDeff as String 
		' 			NidWDeff=Lcase(Info8.Request.Info.NidWorkflowDeff.tostring)
		' 			'	Info8.AddError(BIZ.SA.EumErrorAction.warning, "NidWDeff",NidWDeff.tostring) 
		' 			if Not (NidWDeff="01aaa0b7-8193-41d6-84f9-437fa39306a7" or NidWDeff="e6c9db23-6cea-4161-813e-078193082dd9"  or NidWDeff="c4c75308-5055-460b-999e-d2e7fdde217e" _
		' 			or NidWDeff="045bea33-0a29-4f35-8cc1-672417f49dc1" or NidWDeff="d84fa3d1-54fd-4d24-8000-758a300b2441" _
		' 			or NidWDeff="008df040-922b-41a6-a3b2-7f35b1c870bb" or NidWDeff="8b4af4ad-a7e7-4330-bd2c-f39b3a07adfd" _
		' 			or NidWDeff="9d2031e0-8ace-4cb3-a563-05fc1d42c8df" or NidWDeff="8b4af4ad-a7e7-4330-bd2c-f39b3a07adfd" _
		' 			or NidWDeff="e6c9db23-6cea-4161-813e-078193082dd9" or NidWDeff="7c90f477-7515-4ad3-aefe-fd8f1fe4d11a"  ) Then 
		' 				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید  ")
		'			End if
		' 		End if 
		 		
		 	'	--------------------------------------
		
		if JamSepari=False Then 		
			if Set_UserGroupId(Info8.User.UserGroups) =1 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) <> 69 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 95   Then ' درخواست استعلام
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "شما مجاز به ثبت درخواست استعلام می باشید")
				Exit Function
			End if	
		End if 
		'aaaa
	'	Exit Function    
		if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 76 Then 
			
			Dim TmPUserIdGroup as Integer =0
		
		   if GetIdWorkflow(Info8.Request.Info.NidWorkflowDeff.tostring)=68 or GetIdWorkflow(Info8.Request.Info.NidWorkflowDeff.tostring)=57 _
		   or GetIdWorkflow(Info8.Request.Info.NidWorkflowDeff.tostring)=87 or GetIdWorkflow(Info8.Request.Info.NidWorkflowDeff.tostring)=13 Then 
		  
				  Dim Dafater() as String ={"C1397F3C-F9B4-4574-A021-5869118C7B64","8EC98CDB-17C5-4D10-9FB1-C43F4F288F11",
						"FC0C43BA-0D7F-4FD8-BE9D-F3BE22E43761","0D9C6E87-F03C-4AAC-8C8F-CE5007FD67EB","022311BC-88F0-43D3-AA48-CF6852134C3F"}
			
				 For Each bc As String In Dafater
			   		If bc = Ucase(Info8.User.JobLocationGuid.ToString()) Then TmPUserIdGroup=1
				 Next
				  
				 if TmPUserIdGroup =1 Then 
				 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*اعتبارسنجی", "شما مجاز به ثبت این نوع درخواست نمی باشید")
				 	Exit Function
				 End if	
		   End if
		
		    
		
		        Dim BlackList As New List(Of DtoSA.Sh_BlackList)
		        if Info8.Request.Info.NidWorkflowDeff.tostring<>"88543dab-f235-4b44-84b4-2776173cf0b4" Then  BlackList=Info8.GetFunctions.CheckInBlackList(Info8.GetRequest.Info.NidNosaziCode)
		        
		      
		        Dim CkeBlakList as Integer =0
		        Dim DomainBlakList as String =""
		       
				if  Not BlackList.any()  Then
		        
		        	CkeBlakList=0
				Else
					Dim i as Integer 
					For i=0 to BlackList.count-1
						if BlackList(i).EumBlackListControl<> 1 Then '*
							CkeBlakList=0'*
						Else'
							CkeBlakList=1
							DomainBlakList=BlackList(i).domain
						End if'*
					Next			
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
		        if  CkeBlakList =0 Then 'لیست سیاه
		        		
		        		if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=7 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=94 _
		        		or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)) =51 Then JamSepari=True
			      	   ''''ثبت درخواست  تکراری
				        Dim  TmpListReq As  List(Of  DtoSC.Sh_RequestInfo)
				        Dim  TmpReq As   DtoSC.Sh_RequestInfo
				        
				   	if JamSepari=False Then 
				      
				        If Not Info8.GetRequest.GetHasNidProc(Info8.GetRequest.Info.NidProc) Then
			           
				            TmpListReq =Info8.GetRequest.GetActiveRequestList(Info8.GetRequest.Info.NidNosaziCode)
				            Dim ReqGroupOld,ReqGroupNew as Integer 
				            Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
	    
	    					TmpListReq_N= Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
	    
	    					if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 And f.eumrequeststatus<>2 ).tolist '' , دایم ابطال نباشد 	 

				            Dim TmpReq_N As New  DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult
	    
						    'sp 432362 
							if (MapMosharekati()<> 0) And (uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") or _
							uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9")or UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("5D7FEE70-04D1-4047-86C8-546BD7618503")or _
							uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F")or UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("D8F77DFA-ECE9-4F52-9AEE-3458D5D91272")or _
							UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("D9E55CD5-922B-4D56-80C8-7FE5F7178C95"))Then 	
							Else	
				           ' For Each TmpReq in  TmpListReq
					            For Each TmpReq_N in  TmpListReq_N
					                Dim M_ShahrsaziArchiveGroupParvanehold,M_ShahrsaziArchiveGroupParvanehNew as Integer
					                M_ShahrsaziArchiveGroupParvanehNew=0
					                M_ShahrsaziArchiveGroupParvanehold=0
					            	ReqGroupOld= GetSara8Workflow(TmpReq_N.NidWorkflowDeff.tostring)
					            	M_ShahrsaziArchiveGroupParvanehold=M_ShahrsaziArchiveGroupParvaneh
					            	ReqGroupNew= GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)
					            	M_ShahrsaziArchiveGroupParvanehNew=M_ShahrsaziArchiveGroupParvaneh
					            '	Plogfile(Ucase(TmpReq.NidWorkflowDeff.tostring) ,UCase(Info8.Request.Info.NidWorkflowDeff.tostring))
					            	
					
							     	if Ucase(TmpReq_N.NidWorkflowDeff.tostring) = UCase(Info8.Request.Info.NidWorkflowDeff.tostring) Then
							     	'125441
							     	'242760
							     	'	if Not NotPolice(Info8.User.UserGroups)  And (Rtype=117 or Rtype=118 )Then 
							     	'	Else
							     '	278466
							     		Dim LstUserTNew() AS String
		 								LstUserTNew={"4A58BD9F-DF2A-4A15-B8A7-327D41542697","4259E59F-4DDA-45EB-8C94-60771FE6D8DD"} 
		 								if LstUserGroupId(LstUserTNew)=1 And ucase((Info8.User.UserGuid).tostring)<>ucase("6ca61715-320a-4b6b-9a3b-3c340f42047b") Then
		 								Else
							        		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "*&&امکان ثبت درخواست تکراري موجود نمي باشد")
							        		Exit Function
							        	End if 	
							        '	End if 	
							        ElseIf (ReqGroupOld =ReqGroupNew or (ReqGroupOld=2 And ReqGroupNew=3 ) or (ReqGroupOld=3 And ReqGroupNew=2 )) And 	 GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 76  Then  
							        	'Msgbox ("R2" , TmpReq.Nidworkitem.tostring )
							        	if ReqGroupNew <> 6 Then 
							        	'125441
							        	'242760
							        	'	if Not NotPolice(Info8.User.UserGroups)  And (Rtype=117 or Rtype=118)  Then 
							     		'	Else
							     		'	278466
							     			Dim LstUserTNew() AS String
			 								LstUserTNew={"4A58BD9F-DF2A-4A15-B8A7-327D41542697"} 
			 								if LstUserGroupId(LstUserTNew)=1 Then
			 								Else
			 									
							        				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "**امکان ثبت درخواست برای یک گروه موجود نمي باشد")
							        				Exit Function	
							        		End if	
							        	'	End if 	
							        	End if
							        ElseIf M_ShahrsaziArchiveGroupParvanehold	= M_ShahrsaziArchiveGroupParvanehNew And (ReqGroupOld <> ReqGroupNew And ReqGroupNew<>1) And M_ShahrsaziArchiveGroupParvanehNew=1 Then 
							        	if NotPolice(Info8.User.UserGroups)  Then  
							        		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "**امکان ثبت درخواست موجود نمي باشد لطفا درخواست پروانه جاری را تعیین تکلیف فرمایید")
							        		Exit Function
							        	End if	
							        End if	
					            Next
					        End if     
				            ''''یا فرزند ان درخواست های پدر 
				          
				            Dim t =Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0,0, Guid.Parse("00000000-0000-0000-0000-000000000000"))		
				            
						         	If t.EumNosaziCodeObjType = 3 Then
						         		 Dim asBuildingChildNidBase As New List(Of BIZ.SA.ClsNosaziCodeType)
						         		asBuildingChildNidBase = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
						         		
										if asBuildingChildNidBase.any Then  asBuildingChildNidBase = asBuildingChildNidBase.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList()
										
										if asBuildingChildNidBase.any Then TmpListReq =Info8.GetRequest.GetActiveRequestList(asBuildingChildNidBase(0).NidNosaziCode) 	 
			
						         	ElseIf t.EumNosaziCodeObjType = 2 Then
						         		 Dim asBuildingChildNidBase As New List(Of BIZ.SA.ClsNosaziCodeType)
						         		asBuildingChildNidBase = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
						         		
										if asBuildingChildNidBase.any Then  asBuildingChildNidBase = asBuildingChildNidBase.Where(Function(f) f.EumNosaziCodeObjType = 3).ToList()
										
										if asBuildingChildNidBase.any Then TmpListReq =Info8.GetRequest.GetActiveRequestList(asBuildingChildNidBase(0).NidNosaziCode) 	 
									End if 	
							'sp 432362 
							if (MapMosharekati()<> 0) And (uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("BB0854BE-4002-4C0C-975C-04B4B8485334") or _
							uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("7B60BD7D-5116-4B6A-8B6C-8EE303345AD9")or UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("5D7FEE70-04D1-4047-86C8-546BD7618503")or _
							uCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("79903151-AF7C-4A73-9F18-6EB915F3293F")or UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("D8F77DFA-ECE9-4F52-9AEE-3458D5D91272")or _
							UCase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("D9E55CD5-922B-4D56-80C8-7FE5F7178C95"))Then 	
							Else		
						            For Each TmpReq in TmpListReq
								        Dim M_ShahrsaziArchiveGroupParvanehold,M_ShahrsaziArchiveGroupParvanehNew as Integer
								       	M_ShahrsaziArchiveGroupParvanehNew=0
						                M_ShahrsaziArchiveGroupParvanehold=0
						            	ReqGroupOld= GetSara8Workflow(TmpReq.NidWorkflowDeff.tostring)
						            	M_ShahrsaziArchiveGroupParvanehold=M_ShahrsaziArchiveGroupParvaneh
						            	ReqGroupNew= GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)
						            	M_ShahrsaziArchiveGroupParvanehNew=M_ShahrsaziArchiveGroupParvaneh
						            	
								       				
								       	if TmpReq.NidWorkflowDeff = Info8.Request.Info.NidWorkflowDeff Then
								       	'125441
								       	'242760
								       	'	if Not NotPolice(Info8.User.UserGroups)  And (Rtype=117 or Rtype=118 ) Then 
								     	'	Else
								     	'	278466
								     			Dim LstUserTNew() AS String
				 								LstUserTNew={"4A58BD9F-DF2A-4A15-B8A7-327D41542697"} 
				 								if LstUserGroupId(LstUserTNew)=1 Then
				 								Else
									       			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "#امکان ثبت درخواست تکراري موجود نمي باشد")
									       			Exit Function
									       		End if	
								       	'	End if	
								       	ElseIf (ReqGroupOld=ReqGroupNew  or (ReqGroupOld=2 And ReqGroupNew=3 ) or (ReqGroupOld=3 And ReqGroupNew=2 )) And 	 GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 76   Then  
								       		'Msgbox ("R" , TmpReq.Nidworkitem.tostring )
								       		if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> 6 Then 
								       		'125441
								       		'242760
								       	'		if Not NotPolice(Info8.User.UserGroups)  And (Rtype=117 or Rtype=118)Then 
								     		'	Else
								     		'	278466
								     			Dim LstUserTNew() AS String
				 								LstUserTNew={"4A58BD9F-DF2A-4A15-B8A7-327D41542697"} 
				 								if LstUserGroupId(LstUserTNew)=1 Then
				 								Else
								       				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "امکان ثبت درخواست برای یک گروه موجود نمي باشد")
								       				Exit Function
								       			End if	
								       		'	End if	
								    		End if
								    	ElseIf M_ShahrsaziArchiveGroupParvanehold	= M_ShahrsaziArchiveGroupParvanehNew And (ReqGroupOld <> ReqGroupNew And ReqGroupNew<>1) And M_ShahrsaziArchiveGroupParvanehNew=1 Then 
								        	if NotPolice(Info8.User.UserGroups)  Then 
								        	   '	278466
								     			Dim LstUserTNew() AS String
				 								LstUserTNew={"4A58BD9F-DF2A-4A15-B8A7-327D41542697"} 
				 								if LstUserGroupId(LstUserTNew)=1 Then
				 								Else	
									        		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "##امکان ثبت درخواست موجود نمي باشد لطفا درخواست پروانه جاری را تعیین تکلیف فرمایید")
									        		Exit Function
									        	End if	
								        	End if	
			
								        End if	
						            Next
						       End if   
				        End  If
			        End if
			        
			        Dim TmpMap as Boolean
			        TmpMap=Map(0) 
		        
			       if Rtype= 117 And polickargozar=1 Then TmpMap=True
			       '292893
			       if Rtype= 118 And LstUserGroupId(LstUserPolis)=1 Then TmpMap=True				       
			     '  Info8.AddError(BIZ.SA.EumErrorAction.Stop, "*TmpMap", TmpMap)
					if TmpMap Then ' باید بر روی نقشه باشد					
					if  Map(1) And Set_UserGroupId(Info8.User.UserGroups) <> 2 Then 
						Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "امکان ثبت درخواست بر روی لایه ابشار نمی باشید")
						Exit Function
					End if	
						'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی",Info8.Request.Info.NidWorkflowDeff.tostring)
		  			if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=69 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=95  _
		  		    or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=105 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=7  _
		  			or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=94    _
		  			or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=51 Then ' درخواست استعلام
		  			''''''''''''''''''''''''''''''''
		       				 Dim a As New BIZ.SB.ClsDutyCalculate
			            Dim str As String = String.Empty
			           
			      		
			          '  Boolean BIZ.SB.ClsDutyCalculate.HasDept(System.Guid, EumDutyType, Int32, System.Guid, System.String)	Stop
					'	Dim a As New BIZ.SB.ClsDutyCalculate
			          ' Dim str As String =""
			            Dim k As BIZ.SA.ClsEnum.EumDutyType
			           
			          ''' if a.HasDept(Info8.GetRequest.Info.NidNosaziCode, k.Renovation, 1385, Info8.User.UserGuid, str) Then 
			    	   	'''	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی","شما مجاز به ثبت درخواست نمی باشید ملک دارای بدهی نوسازی می باشد")
			    	  '''  End if		
			    	    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
		  			 
						Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
						Dim TmpNosazi As New BIZ.SA.ClsNosaziCodeType
						TmpNosazi=Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(tmpNosaziNickName)
		                Dim TypeObject as Integer = Convert.ToInt64(TmpNosazi.EumNosaziCodeObjType )
		               
		
						if TypeObject=2 Then
							 Dim TmpListNosazi As New List(Of BIZ.SA.ClsNosaziCodeType)
							 Dim aa As BIZ.SA.ClsNosaziCodeType
							 
							 Dim M_EumBaseInfoGroup As BIZ.SA.ClsEnum.EumBaseInfoGroup
		           			 Dim M_EumRevisitGroup As BIZ.SA.ClsEnum.EumRevisitGroup
		           			 Dim Revisitstr As String = "00000000-0000-0000-0000-000000000000"
							 aa=Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,M_EumBaseInfoGroup.Parvandeh,M_EumRevisitGroup.None,Guid.Parse(Revisitstr))
							 
							 TmpListNosazi=Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(aa.NidNosaziCode,0)
						 
							 TmpListNosazi=TmpListNosazi.where(Function (f) f.EumNosaziCodeObjType=3 or f.EumNosaziCodeObjType=5).tolist
							 ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, " TmpListNosazi.count", TmpListNosazi.count)
							 if TmpListNosazi.count >0 Then 
							 	Info8.ParameterList.Add("Parvandeh_Building_NidBase", TmpListNosazi(0).NidBase.ToString)
							 	
							 	if Info8.GetParvandeh_Building.GetBase_Using.count >0 Then 
							 		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "شما مجاز به ثبت درخواست بر روی ملک نمی باشید ( ملک دارای ساختمان می باشد )")
							 		Exit Function
							 	End if
							 End if		
						End if
						 'sp 353757
						if 	tmpDistrict<>80 Then 
							if Not(Gavahi(TypeObject))  Then
								Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "شما مجاز به ثبت درخواست نمی باشید , فاقد گواهی بعد از 87 می باشد ")
								Exit Function
							End if	
						End if	
			           
				
					End if
		
				   Else
				   	  if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") Then 	'sp 484834
							Info8.AddError(BIZ.SA.EumErrorAction.Stop, "***اعتبارسنجی", "ملک روی نقشه نمی باشد" )
							Exit Function				   	  		
 					  Else		
							Info8.AddError(BIZ.SA.EumErrorAction.Stop, "***اعتبارسنجی", "ملک روی نقشه نمی باشد" )
							Exit Function
					  End if	
				  End if
			
				
				
				Else
					
					Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "**کد نوسازی در لیست سیاه " & DomainBlakList &" می باشد مجاز به ثبت درخواست نمی باشید")
		        	Exit Function	
					
			    End if
			    if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=7 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=94 _
			    or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=51 Then Exit Function
			    
			    
			    Dim IsTaeed as Integer = 0
			    Dim Attach as String =""
			    Try
			    	Attach=Info8.GetRequest.Info.NidProc_Attached.tostring
			    Catch 
			    	Attach=""
			    End Try 
			    
			    if Attach<>"" Then 
			    	IsTaeed = TaeedMGavahi1(Attach)
			    	if IsTaeed=0 Then 
			    		Info8.AddError(BIZ.SA.EumErrorAction.Stop ,"هشدار","درخواست پیوستی تایید گواهی شده و مجاز به انتخاب آن نمی باشید")
			    		Exit Function
			    	End if	
				End if
			    
				
				if (GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=96) Then
				
		 			if (Info8.Request.Info.MainRequesterDate) is Nothing Then 
		 				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا تاریخ نامه را وارد کنید")
		 				Exit Function
		 			End if	
		 			if (Info8.Request.Info.MainRequesterName) is Nothing Then
		 				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا نام صاحب نامه را وارد کنید")
		 				Exit Function
		 			End if	
		 			if (Info8.Request.Info.MainRequesterNo) is Nothing Then 
		 				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا نام شماره نامه را وارد کنید")
		 				Exit Function
		 			End if	
		 		End if
		 		
			 	if JamSepari=False Then 
			       
				 	if (Info8.Request.Info.PostCode is Nothing) Then 
				 	if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 45 Then 
						 Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کاربر محترم : کد پستی  را وارد نمایید .") 
						 Exit Function
					End if	 
					Else
					    if Info8.Request.Info.PostCode.ToString.Length > 10 or Info8.Request.Info.PostCode.ToString.Length < 10 Then 
					    	if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 45 Then  
					    	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کاربر محترم : کد پستی باید 10 رقمی باشد  .")
					    	Exit Function
					    End if	
					    End if	
					End if
				End if
				
				if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=74 or _
				GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=73 or _
				GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=72 or
				GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=79 or _
				GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=71  Then 
					Call Set_UserGroupId()
					if UserInSabtShardari=False And ucase(Info8.user.userguid).tostring <> ucase("E7D536CF-3FE6-4F23-9C05-CE6EEC87987F") Then  
						Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کاربر محترم : شما مجاز به ثبت این نوع درخواست نمی باشید .") 
						Exit Function
					End if	
				End if 
				
				
				
					
		 		
		End if
		'121299
		'if 	Ucase(Info8.Request.Info.NidWorkflowDeff.tostring) ="B6A28C29-6761-4719-8B62-C8575F88F572" Then  
			'if tmpDistrict <> 10  Then 
			'	if tmpDistrict <> 6  Then 
			'		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "درخواست مورد نظر در منطقه 10 قابل استفاده است") 
			'	End if
		'	End if		
	'	End if	
		'if Info8.GetRequest.Info.RequesterNationalCode ="1111111111" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد ملی نامعتبر است.") '78222
			'121299
		if  Info8.GetRequest.Info.RequesterNationalCode ="0123456789" or Info8.GetRequest.Info.RequesterNationalCode ="1111111111" or  _
			Info8.GetRequest.Info.RequesterNationalCode ="2222222222" or Info8.GetRequest.Info.RequesterNationalCode ="3333333333" or  _
			Info8.GetRequest.Info.RequesterNationalCode ="4444444444" or Info8.GetRequest.Info.RequesterNationalCode ="5555555555" or  _
			Info8.GetRequest.Info.RequesterNationalCode ="6666666666" or Info8.GetRequest.Info.RequesterNationalCode ="7777777777" or  _
			Info8.GetRequest.Info.RequesterNationalCode ="8888888888" or Info8.GetRequest.Info.RequesterNationalCode ="9999999999" or  _
			Info8.GetRequest.Info.RequesterNationalCode ="0000000000" or len(Info8.GetRequest.Info.RequesterNationalCode)<>10 _
			or Not CheckMeli( Info8.GetRequest.Info.RequesterNationalCode) Then 
				Dim Ispolicsakhteman as Integer =0 
				if JamSepari=False Then 
		
						Dim GroupIn7(),TempStr7 as String
						Dim CurrentGroup7 as String
						Dim UserList7 As New List(Of BIZ.SA.ClsUserGroup)
						if JamSepari=False Then UserList7 = Info8.User.UserGroups
						Dim i as Integer
						if UserList7.Any Then 
						 	For i=0 to UserList7.Count-1
						 		TempStr7 +=  UserList7(i).GroupGuid.ToString()
						   		if i< UserList7.Count-1 Then TempStr7 = TempStr7 & ","
						  	Next i
						End if
						GroupIn7=TempStr7.Split(",")
					   	
						For Each bb As String In GroupIn7
				  	     	'If Ucase(bb)="DEB84AB9-E2E1-44A7-9F94-19BAAC68440C" or Ucase(bb)="2D8A3986-E9B2-4D01-933C-2023F6D32D3A" _
							'or Ucase(bb)="29BBC2BC-085A-44D5-9632-2C11F1B165DF" or Ucase(bb)="9B570D30-92F7-4376-8971-39164869AEA9" _
							'or Ucase(bb)="2D8362F5-A6AE-449F-86CD-4256B6E3EFC9" or Ucase(bb)="B3B790FB-712C-4684-8EA3-552D60ADC547" _
							'or Ucase(bb)="84E83CFF-394B-4C50-8845-7A01AAC1A568" or Ucase(bb)="A4861359-14FF-480E-81CF-7A8B13A02C4A" _
							'or Ucase(bb)="78C37250-ECE0-4C7B-94DB-8EE5CDCE3B00" or Ucase(bb)="089B3654-1472-4EB0-8A27-B892AF9437F4" _
							'or Ucase(bb)="7958BD45-8EE9-49D3-9C68-CA6CC712B85B" or Ucase(bb)="2994C372-F22C-4FE3-B99F-D8189D61CDE2" _
							'or Ucase(bb)="172ED681-5AD4-4401-819D-FE73A7A2F933" Then Ispolicsakhteman=1
							'84673 sampa
						'	sp 334302
							If Lcase(bb)="4a58bd9f-df2a-4a15-b8a7-327d41542697" or Lcase(bb)="5f4030ce-99fb-4731-892b-0d0f525a910e" or Lcase(bb)="e4fd6027-2a09-4b82-b812-6644160636c5" _
							or Lcase(bb)="59434e63-ffd2-4de9-b26d-e795c44a4f6a" or Lcase(bb)="3c6497be-9795-40ed-9c17-257071751614" or Lcase(bb)="16bc1db1-a38d-4166-9ef4-ab5209dd562d" _
							or Lcase(bb)="5aef8344-b54e-412e-8cd9-d03eda410071" or Lcase(bb)="8bba5f92-c46f-4379-b59d-89ac3158164a" Then
							 Ispolicsakhteman=1
							End if
						Next
						
				End if '83244 sampa
				'msgbox ("Ispolicsakhteman" , Ispolicsakhteman)		  
				if Ispolicsakhteman=0 And  (Info8.Request.Info.CI_Requester<>8 And Info8.Request.Info.CI_Requester<>12) Then 
				'150745
					if  GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 45 And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 23 _
					And GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))<> 90  Then 
						Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کد ملی نامعتبر است.") '78222
						Exit Function
					End if	
				End if	
		
		End if 
Else
	if  Info8.GetRequest.GetHasNidProc(Info8.GetRequest.Info.NidProc) Then 
 
	Else
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "کاربر در لیست کنترلی می باشد  مجاز به ثبت درخواست نمی باشید")
		Exit Function
	End if	
End if		 
if 	GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=109 or  _
	GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=53 or _
	GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=83 Then 
	
		If Info8.Request.Info.TransferDang IsNot Nothing Then

			if Info8.Request.Info.TransferDang.tostring="" or Info8.Request.Info.TransferDang.tostring="0" Then
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا دانگ مورد انتقال را وارد نمایید")
				Exit Function
			End if
		Else
			Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا دانگ مورد انتقال را وارد نمایید")
			Exit Function	
		End if	
End if
'125441
if Info8.Request.Info.CI_Requester =5 Then Info8.Request.Info.RequesterName ="مدیریت مهندسی و نظارت بر ساخت و سازها"
 	
 	Dim tmpNosaziNickNamesms As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	Dim TmpNosazisms As New BIZ.SA.ClsNosaziCodeType
	TmpNosazisms=Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(tmpNosaziNickNamesms)
	Dim TypeObjectsms as Integer = Convert.ToInt64(TmpNosazisms.EumNosaziCodeObjType )
	if TypeObjectsms=2 or TypeObjectsms=3 Then 
		if M_ShahrsaziArchiveGroup=1 or M_ShahrsaziArchiveGroup=2 Then 
		    
			 Dim p_RequestNo as String=""
			 
			 p_RequestNo = Info8.GetRequest.Info.NidWorkItem
			 Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
			 tmpSMS.BizCode = tmpNosaziNickNamesms.tostring()
			 tmpSMS.Number = Info8.GetRequest.Info.Cellphone
			 tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
			 tmpSMS.SMSType = "ثبت درخواست"
			 if tmpDistrict=9 Then 
			 	if M_ShahrsaziArchiveGroup =1 Then 
			 		tmpSMS.Text ="شهروند گرامی اقا/ خانم " + Info8.GetRequest.Info.RequesterName.tostring + "پرونده شما با شماره ارجاع" + p_RequestNo +" در تاریخ " + BIZ.SA.ClsCommon.CurrentShamsiDateString + "در دفتر پیشخوان شهرسازی برای دریافت پروانه تشکیل شد. زمان پاسخگویی به شما در گام اول: 2 الی 3 روز کاری"
			 	Else 	
			 		tmpSMS.Text ="شهروند گرامی اقا/ خانم " + Info8.GetRequest.Info.RequesterName.tostring + "پرونده شما با شماره ارجاع" + p_RequestNo +" در تاریخ " + BIZ.SA.ClsCommon.CurrentShamsiDateString + "در دفتر پیشخوان شهرسازی برای دریافت پایانکار بهره‌برداری تشکیل شد. زمان پاسخگویی به شما در گام اول: اگر ملک، در مسیر نداشته باشد و کاربری زمین، خدماتی نباشد، 7 روز کاری، اگر کاربری ملک، خدماتی باشد 3 روز کاری به زمان پاسخگویی اضافه می شود و اگر قسمتی از ملک، در مسیر طرح های شهرسازی باشد، 60 روز کاری به زمان اولیه اضافه می شود."
			 	End if	

			 Else 
			 	tmpSMS.Text ="شهروند گرامی اقا/ خانم " + Info8.GetRequest.Info.RequesterName.tostring + " درخواست شما بر روی کد نوسازی " + tmpNosaziNickNamesms.tostring() + " با شماره ارجاع " + p_RequestNo + " با موفقیت ثبت گردید "
			 End if
			 tmpSMS.UserID = Info8.User.UserGuid.ToString
			 tmpSMS.UserName = Info8.User.UserName
			
			
			  Dim smsErrorResult As New BIZ.SA.ClsErrorResult
			                    ' Send SMS
			  
			'144581  	smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
			
			                    ' Check Error
			 '144581	If smsErrorResult.BizErrors.Any Or smsErrorResult.Requirements.Any Then
			 '144581	Dim ii as Integer 
			'144581	 	For ii=0 to smsErrorResult.BizErrors.Count -1 
			'144581	 		Info8.AddError(BIZ.SA.EumErrorAction.Warning, "SMssS", smsErrorResult.BizErrors(ii).ErrorTitel ) 
			'144581	 		
			'144581	 	Next
			'144581	 	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "SMS", "اختلال در ارسال پیامک")
			'144581 	Else
			 '144581   	Info8.AddError(BIZ.SA.EumErrorAction.Warning, "SMS", "ارسال پیامک با موفقیت انجام شد")
			'144581 	End If
		End if
	End if		 	

Dim Test = GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)	
'''''''''''''''''''''''''''''
 
        Dim Mant As Integer = 0
        Select Case tmpDistrict
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
       
'''''''''''''''''''''''''''''
if User137(Info8.User.UserGuid.tostring,TmpArchiveGroup)=1 Then
	if  TmpArchiveGroup>10 Then '(tmpDistrict=9 or tmpDistrict=8 or tmpDistrict=3 or tmpDistrict=12 or tmpDistrict=5) Andadd 8-3 -9  161246-161249-161250
	'a if Ucase(Info8.User.UserGuid.tostring) ="540D119D-14F1-4F65-94D1-7F39FC2B3B48"  or Ucase(Info8.User.UserGuid.tostring) ="7332DA55-0A3F-446B-8103-E71BACB5B969" Then 
	  
		  Dim Fava137 As New Biz.Communication.ClsFava137
		  Dim Fava137Message As New Biz.Communication.ClsFava137MessageContract
		  Fava137Message.key = "5tgb6yhn^YHN%TGB"
		  Fava137Message.Name = Info8.GetRequest.Info.RequesterName 
		  Fava137Message.LastName = " "
		 
		  Fava137Message.ReferenceNum = Info8.GetRequest.Info.NidWorkItem
		  Fava137Message.LevelNum = 1
		  Fava137Message.TypeOfReq=TmpArchiveGroup
		  
		 
		  Fava137Message.OrgCellPhon=Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name.tostring
		  '''''
		  
		  Fava137Message.CityCode=Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name.tostring
		  Fava137Message.Subject="1134"
		  Fava137Message.SubSubject=TmpArchiveGroup 
		  Fava137Message.Zone=Mant
		  
		  Dim NosaziLst As List(Of Biz.SA.ClsNosaziCodeType)
	 	  NosaziLst = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		  NosaziLst = NosaziLst.Where(Function(f) f.EumNosaziCodeObjType = 2).tolist
			  
		  if NosaziLst.any Then 
		  	
	 		  Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
	 		  Info8.ParameterList.Add( "Parvandeh_House_NidBase" , Info8.GetFunctions.GetNidBase(NosaziLst(0).NidNosaziCode, Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000")).ToString)
	 	 Else
	 	 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "137اعتبارسنجی", "137 مشکل در ارسال به 137 ")
	 	 	Exit Function
	 	 End if 	
	
		  
		  
		  Try
		  	Fava137Message.Adress=Info8.GetParvandeh_House.GetBase_AddressInfo.MainAddress.ToString 
	 	  Catch ex As Exception
				Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "لطفا ادرس ملک را اصلاح نمایید") : Exit Function
	       End Try
	 	  if Info8.GetParvandeh_House.GetBase_PreCodeInfo.Count>0 Then 
		  	Fava137Message.RenovationCode=Info8.GetParvandeh_House.GetBase_PreCodeInfo(0).PreCode	
	 	  Else
	    	Fava137Message.RenovationCode="0"	
	 	  End if 	
	 	  Fava137Message.RenovationCodeNew=Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)	
		 
		  if TmpArchiveGroup=1839 or TmpArchiveGroup=1882 or TmpArchiveGroup=1871 or TmpArchiveGroup=1874 or TmpArchiveGroup=1873 or TmpArchiveGroup=1883 Then 
			  Fava137Message.LevelExplain =	 "شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" درخواست پروانه شما  در دفتر پیشخوان " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " تشکیل شد. تا 2 ساعت دیگر،مامور بازدید شما مشخص خواهدشد (درصورت ضرورت بازدید ملک) "  
		  ElseIf TmpArchiveGroup=1838 or TmpArchiveGroup=1869 or TmpArchiveGroup=1870 Then 
			 Fava137Message.LevelExplain = "شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" پرونده شما در دفتر پیشخوان " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " برای دریافت پایانکار تشکیل شد. تا 2 ساعت آتی، مأمور بازدید شما مشخص خواهد شد. "  
		  ElseIf TmpArchiveGroup=1875 or TmpArchiveGroup=1876 or TmpArchiveGroup=1880 Then 
			  Fava137Message.LevelExplain =	 "شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" درخواست گواهی انتقال ملک/ پایانکار تفکیکی شما در دفتر پیشخوان شماره " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " ثبت شد حداکثر تا دوساعت دیگر مامور بازدید شما مشخص خواهد شد " 
		  ElseIf TmpArchiveGroup=1881 or TmpArchiveGroup=1872  Then 
		  
			  Fava137Message.LevelExplain =	 " شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  + " شماره ارجاع: " + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" درخواست پروانه شما در دفتر پیشخوان شهرسازی شماره " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " تا 2 ساعت دیگر،مامور بازدید شما مشخص خواهدشد (در صورت ضرورت بازدید ملک)"  	  	   	  
		  ElseIf TmpArchiveGroup=1877 or TmpArchiveGroup=1878 or TmpArchiveGroup=1879 Then 
		  
			  Fava137Message.LevelExplain =	 " شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  + " شماره ارجاع: " + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" درخواست تایید سابقه مجاز شما در دفتر پیشخوان شهرسازی شماره " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " ثبت شد حداکثر 2روزکاری تایید سابقه مجاز شما انجام خواهد شد."  	  	   	  	  
		  Else
		  	  Fava137Message.LevelExplain =" "
		  End if 
		  
		  Fava137Message.Regdate = Date.Now.Date
		  Fava137Message.TrackingDate =AddDateForHolidays(GetShamsiDate(Date.Now.Date.tostring), 1) 'Date.Now.Date.AddDays(1)
		  Fava137Message.CellPhone = Info8.GetRequest.Info.Cellphone
		 
		  '180233  If Fava137.SaveRequestnew(Fava137Message) Then
		  '02/11 If Fava137.SaveRequest(Fava137Message) Then
		  '	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "137اعتبارسنجی", "137 موفقیت")
		  	Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
				 tmpSMS.BizCode = strCode
				 tmpSMS.Number = Info8.GetRequest.Info.Cellphone
				 tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
				 tmpSMS.SMSType = "ثبت درخواست"
				 if TmpArchiveGroup=1839 or TmpArchiveGroup=1882 or TmpArchiveGroup=1871 or TmpArchiveGroup=1874 or TmpArchiveGroup=1873 or TmpArchiveGroup=1883  Then 
					  tmpSMS.Text =	  "شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  			+ "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" درخواست پروانه شما  در دفتر پیشخوان " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " تشکیل شد. تا 2 ساعت دیگر،مامور بازدید شما مشخص خواهدشد (درصورت ضرورت بازدید ملک) "  
				  ElseIf TmpArchiveGroup=1838 or TmpArchiveGroup=1869 or TmpArchiveGroup=1870 Then 
					 tmpSMS.Text =	 "شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
					  + "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" پرونده شما در دفتر پیشخوان " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " برای دریافت پایانکار تشکیل شد. تا 2 ساعت آتی، مأمور بازدید شما مشخص خواهد شد. "  
				  ElseIf TmpArchiveGroup=1875 or TmpArchiveGroup=1876 or TmpArchiveGroup=1880 Then 
			  			tmpSMS.Text= "شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  			+ "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" درخواست گواهی انتقال ملک/ پایانکار تفکیکی شما در دفتر پیشخوان شماره " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + "ثبت شد حداکثر تا دوساعت دیگر مامور بازدید شما مشخص خواهد شد "  	  	  
			  	 ElseIf TmpArchiveGroup=1881 or TmpArchiveGroup=1872  Then 
		  
			  			tmpSMS.Text =	 "شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  			+ "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" درخواست پروانه شما در دفتر پیشخوان شهرسازی شماره " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " تا 2 ساعت دیگر،مامور بازدید شما مشخص خواهدشد (در صورت ضرورت بازدید ملک) "  	  	   	  
				 ElseIf TmpArchiveGroup=1877 or TmpArchiveGroup=1878 or TmpArchiveGroup=1879 Then 
		  
			  		tmpSMS.Text =	 "شماره گام: 1" + VbCrLf + " نام و نام خانوادگی: "  + Info8.GetRequest.Info.RequesterName.tostring + VbCrLf _
			  		+ "شماره ارجاع:" + Info8.GetRequest.Info.NidWorkItem.tostring + VbCrLf +" درخواست تایید سابقه مجاز شما در دفتر پیشخوان شهرسازی شماره " + Info8.GetFunctions.GetUser(Info8.User.UserGuid).User.JobLocations.Name + " ثبت شد حداکثر 2روزکاری تایید سابقه مجاز شما انجام خواهد شد. "  	  	   	  	  
	
				  End if 
				 
				 tmpSMS.UserID = Info8.User.UserGuid.ToString
				 tmpSMS.UserName = Info8.User.UserName
				
				
				  Dim smsErrorResult As New BIZ.SA.ClsErrorResult
				  smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
	'180233	  Else
		 '   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "137اعتبارسنجی", "137 مشکل در ارسال به 137 ")
	'180233	  End If
		   
				
	  End if	
	End if	
Else 'jamieeeeeeeeeeeeee
'	Dim jj as Integer	
'	Dim kk_N As Integer
'	Dim TmpListReq_N As New List(Of DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
'	TmpListReq_N= Info8.GetRequest.GetAllRequestList(Info8.GetRequest.Info.NidNosaziCode)
'	if TmpListReq_N.count >0 Then TmpListReq_N=TmpListReq_N.where (Function (f) f.eumrequeststatus<>3 And f.eumrequeststatus<>2 ).tolist ''ابطال نباشد 	
'	if  ucase(Info8.Request.Info.NidWorkflowDeff.tostring)=ucase("35CFFB97-67A3-488E-8A7A-BEC93C459B99") Then 
'		For kk_N = 0 To TmpListReq_N.Count - 1
'			if GetSara8WorkflowShahrvand(TmpListReq_N(kk_N).NidWorkflowDeff.ToString,0) = 1 Then
'			   Dim TmpError As New Global.Biz.SA.clsBizError
'	           TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'	           TmpError.ErrorTitel = "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید"
'	           TmpError.ErrorKey = 1000
'	           Info8.ErrorResult.BizErrors.Add(TmpError)
'	        End if     
'		Next
'	End if	
'	Dim  TmpListReq As  List(Of  DtoSC.Sh_RequestInfo)
'	Dim  TmpReq As   DtoSC.Sh_RequestInfo
'	
'	  TmpListReq =Info8.GetRequest.GetActiveRequestList(Info8.GetRequest.Info.NidNosaziCode)
'	  if TmpListReq.count>0 And ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <>ucase("83406869-9A8B-4B80-9A13-F74F946FF84F") Then 
'		  Dim ReqGroupOld,ReqGroupNew as Integer 
'		  For Each TmpReq in TmpListReq
'			 Dim M_ShahrsaziArchiveGroupParvanehold,M_ShahrsaziArchiveGroupParvanehNew as Integer
'			 M_ShahrsaziArchiveGroupParvanehNew=0
'			 M_ShahrsaziArchiveGroupParvanehold=0
'			 ReqGroupOld= GetSara8Workflow(TmpReq.NidWorkflowDeff.tostring)
'			 M_ShahrsaziArchiveGroupParvanehold=M_ShahrsaziArchiveGroupParvaneh
'			 ReqGroupNew= GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)
'			 M_ShahrsaziArchiveGroupParvanehNew=M_ShahrsaziArchiveGroupParvaneh
'			 if Ucase(TmpReq.NidWorkflowDeff.tostring) = UCase(Info8.Request.Info.NidWorkflowDeff.tostring) Then
'				
'				   Dim TmpError As New Global.Biz.SA.clsBizError
'		           TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'		           TmpError.ErrorTitel = "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید"
'		           TmpError.ErrorKey = 1000
'		           Info8.ErrorResult.BizErrors.Add(TmpError)
'					
'		     ElseIf (ReqGroupOld =ReqGroupNew or (ReqGroupOld=2 And ReqGroupNew=3 ) or (ReqGroupOld=3 And ReqGroupNew=2 )) And 	 Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"A3B24BD2-4CB2-4011-9900-3070AD088A6F"  And  Ucase(TmpReq.NidWorkflowDeff.ToString)<>"A3B24BD2-4CB2-4011-9900-3070AD088A6F" Then  
'				if ReqGroupNew <> 6 Then 
'				
'						Dim TmpError As New Global.Biz.SA.clsBizError
'		           		TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'		           		TmpError.ErrorTitel = "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید"
'		           		TmpError.ErrorKey = 1000
'		           		Info8.ErrorResult.BizErrors.Add(TmpError)
'					 	
'				End if
'			 ElseIf M_ShahrsaziArchiveGroupParvanehold	= M_ShahrsaziArchiveGroupParvanehNew And (ReqGroupOld <> ReqGroupNew And ReqGroupNew<>1) And M_ShahrsaziArchiveGroupParvanehNew=1 Then 
'			
'				   Dim TmpError As New Global.Biz.SA.clsBizError
'		           TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'		           TmpError.ErrorTitel = "به دلیل داشتن درخواست مشابه در شهرسازی تا اتمام درخواست مذکور مجاز به ثبت درخواست نمی باشید"
'		           TmpError.ErrorKey = 1000
'		           Info8.ErrorResult.BizErrors.Add(TmpError)
'			
'			 End if	
'	      Next
'   End if
'   If Not Info8.GetRequest.GetHasNidProc(Info8.GetRequest.Info.NidProc) Then
'	  TmpListReq =Info8.GetRequest.GetActiveRequestList(Info8.GetRequest.Info.NidNosaziCode)
'	  Dim ReqGroupOld,ReqGroupNew as Integer 
'	  For Each TmpReq in TmpListReq
'		 Dim M_ShahrsaziArchiveGroupParvanehold,M_ShahrsaziArchiveGroupParvanehNew as Integer
'		 M_ShahrsaziArchiveGroupParvanehNew=0
'		 M_ShahrsaziArchiveGroupParvanehold=0
'		 ReqGroupOld= GetSara8Workflow(TmpReq.NidWorkflowDeff.tostring)
'		 M_ShahrsaziArchiveGroupParvanehold=M_ShahrsaziArchiveGroupParvaneh
'		 ReqGroupNew= GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)
'		 M_ShahrsaziArchiveGroupParvanehNew=M_ShahrsaziArchiveGroupParvaneh
'		 if Ucase(TmpReq.NidWorkflowDeff.tostring) = UCase(Info8.Request.Info.NidWorkflowDeff.tostring) Then
'			
'			Dim TmpError As New Global.Biz.SA.clsBizError
'		    TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'		    TmpError.ErrorTitel = "امکان ثبت درخواست تکراري موجود نمي باشد"
'		    TmpError.ErrorKey = 1000
'		    Info8.ErrorResult.BizErrors.Add(TmpError)
'			Exit Function
'		   
'		 ElseIf (ReqGroupOld =ReqGroupNew or (ReqGroupOld=2 And ReqGroupNew=3 ) or (ReqGroupOld=3 And ReqGroupNew=2 )) And 	 Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"A3B24BD2-4CB2-4011-9900-3070AD088A6F"  And 	 Ucase(TmpReq.NidWorkflowDeff.ToString)<>"A3B24BD2-4CB2-4011-9900-3070AD088A6F" Then  
'			if ReqGroupNew <> 6 Then 
'				
'				Dim TmpError As New Global.Biz.SA.clsBizError
'		    	TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'		    	TmpError.ErrorTitel = "امکان ثبت درخواست برای یک گروه موجود نمي باشد"
'		    	TmpError.ErrorKey = 1000
'		    	Info8.ErrorResult.BizErrors.Add(TmpError)
'				Exit Function
'			
'			End if
'		 ElseIf M_ShahrsaziArchiveGroupParvanehold	= M_ShahrsaziArchiveGroupParvanehNew And (ReqGroupOld <> ReqGroupNew And ReqGroupNew<>1) And M_ShahrsaziArchiveGroupParvanehNew=1 Then 
'			
'			Dim TmpError As New Global.Biz.SA.clsBizError
'		    TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'		    TmpError.ErrorTitel = "امکان ثبت درخواست موجود نمي باشد لطفا درخواست پروانه جاری را تعیین تکلیف فرمایید"
'		    TmpError.ErrorKey = 1000
'		    Info8.ErrorResult.BizErrors.Add(TmpError)
'			Exit Function
'		 End if	
'	  Next
				            ''''یا فرزند ان درخواست های پدر 
				           
'	  Dim t =Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0,0, Guid.Parse("00000000-0000-0000-0000-000000000000"))		
'	  If t.EumNosaziCodeObjType = 3 Then
'		Dim asBuildingChildNidBase As New List(Of BIZ.SA.ClsNosaziCodeType)
'		asBuildingChildNidBase = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
'	  	if asBuildingChildNidBase.any Then  asBuildingChildNidBase = asBuildingChildNidBase.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList()
'		if asBuildingChildNidBase.any Then TmpListReq =Info8.GetRequest.GetActiveRequestList(asBuildingChildNidBase(0).NidNosaziCode) 	 
'			
'	  ElseIf t.EumNosaziCodeObjType = 2 Then
'		Dim asBuildingChildNidBase As New List(Of BIZ.SA.ClsNosaziCodeType)
'		asBuildingChildNidBase = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
'		if asBuildingChildNidBase.any Then  asBuildingChildNidBase = asBuildingChildNidBase.Where(Function(f) f.EumNosaziCodeObjType = 3).ToList()
'		if asBuildingChildNidBase.any Then TmpListReq =Info8.GetRequest.GetActiveRequestList(asBuildingChildNidBase(0).NidNosaziCode) 	 
'	  End if 	
'	  For Each TmpReq in TmpListReq
'		  Dim M_ShahrsaziArchiveGroupParvanehold,M_ShahrsaziArchiveGroupParvanehNew as Integer
'		  M_ShahrsaziArchiveGroupParvanehNew=0
'		  M_ShahrsaziArchiveGroupParvanehold=0
'		  ReqGroupOld= GetSara8Workflow(TmpReq.NidWorkflowDeff.tostring)
'		  M_ShahrsaziArchiveGroupParvanehold=M_ShahrsaziArchiveGroupParvaneh
'		  ReqGroupNew= GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString)
'		  M_ShahrsaziArchiveGroupParvanehNew=M_ShahrsaziArchiveGroupParvaneh
'		  if TmpReq.NidWorkflowDeff = Info8.Request.Info.NidWorkflowDeff Then
'			 
'			 Dim TmpError As New Global.Biz.SA.clsBizError
'		     TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'		     TmpError.ErrorTitel = "امکان ثبت درخواست تکراري موجود نمي باشد"
'		     TmpError.ErrorKey = 1000
'		     Info8.ErrorResult.BizErrors.Add(TmpError)
'			 Exit Function
'		  ElseIf (ReqGroupOld=ReqGroupNew  or (ReqGroupOld=2 And ReqGroupNew=3 ) or (ReqGroupOld=3 And ReqGroupNew=2 )) And 	 Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)<>"A3B24BD2-4CB2-4011-9900-3070AD088A6F"  And 	 Ucase(TmpReq.NidWorkflowDeff.ToString)<>"A3B24BD2-4CB2-4011-9900-3070AD088A6F" Then  
'			 if GetSara8Workflow(Info8.GetRequest.Info.NidWorkflowDeff.ToString) <> 6 Then 
'				 Dim TmpError As New Global.Biz.SA.clsBizError
'			     TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'			     TmpError.ErrorTitel = "امکان ثبت درخواست برای یک گروه موجود نمي باشد"
'			     TmpError.ErrorKey = 1000
'			     Info8.ErrorResult.BizErrors.Add(TmpError)
'				
'				Exit Function
'			 End if
'		  ElseIf M_ShahrsaziArchiveGroupParvanehold	= M_ShahrsaziArchiveGroupParvanehNew And (ReqGroupOld <> ReqGroupNew And ReqGroupNew<>1) And M_ShahrsaziArchiveGroupParvanehNew=1 Then 
'			
'			Dim TmpError As New Global.Biz.SA.clsBizError
'			TmpError.ErrorAction = Global.Biz.SA.EumErrorAction.Stop
'			TmpError.ErrorTitel = "امکان ثبت درخواست موجود نمي باشد لطفا درخواست پروانه جاری را تعیین تکلیف فرمایید"
'			TmpError.ErrorKey = 1000
'			Info8.ErrorResult.BizErrors.Add(TmpError)
'			Exit Function
'		  End if	
'	
'		Next
'	 End if
End if 


if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("24b35887-28e0-45c5-8952-b8e796067cfc")  Then
	Call user()

	CreatTask("Activity_0dmjjqy",AssingTo,AssingToUserName)
End if 

 '  عملکرد مهندس ناظر 
If ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring) = ucase("4cd6e92b-ccba-436c-8ec0-867557a409b7") Then
	Try
		IsHere = 1
	'	Call AddRevisit
	 	CreatTask("Activity_0f3q4ad"  ,"EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57",  "Esup")  'malek		
	 	if Info8.GetRequest.Info.CI_Requester=14 Then
	 		Info8.GetRequest.Info.IsConfirmAuthentication =1 
	 	End if		
	Catch e  as exception
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", "ثبت درخواست با مشکل با شکست مواجه شد -  لطفا دوباره امتحان کنید  ")  
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", " | " & e.message)  
	End Try			   
 End if	
 
  '  مادهع 14
If ucase(Info8.GetRequest.Info.NidWorkflowDeff.tostring) = ucase("4c052600-bab2-48f8-a1c7-5df81741281c") Then
	Try
		IsHere = 1
		
		Call AddRevisit
	
	 	CreatTask("Activity_126rga3"  ,"EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57",  "Esup")  'malek		
	 	if Info8.GetRequest.Info.CI_Requester=14 Then
	 		Info8.GetRequest.Info.IsConfirmAuthentication =1 
	 	End if		
	Catch e  as exception
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", "ثبت درخواست با مشکل با شکست مواجه شد -  لطفا دوباره امتحان کنید  ")  
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", " | " & e.message)  
	End Try			   
 End if	
 
	
Sms(Info8.GetRequest.Info.Cellphone,"حراست"," حراست شهرداری با شماره مستقیم 1887 آماده دریافت اخبار مربوط به تخلفات دفاتر و کارکنان می باشد ") 
 Catch ex As Exception
 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "req  فرمول درخواست" & ex.tostring)
 End Try
         
 End Function
 


 
' تابع معادل سازی نوع درخواست


Private Function GetSara8Workflow (ByVal TmpS as String ) as Integer
Try 
	
	
	
'1838	صدور گواهی پایانکار بهره برداری    Fire
'1839	صدور پروانه احداث بنا				Fire
'1869	گواهی عدم خلاف					Fire
'1870	گواهی مفاصا حساب      		Fire
'1871	پروانه تجدید بنا					Fire
'1872	تجدید پروانه						Fire
'1873	تمدید -اصلاح پروانه
'1874	اصلاح پروانه					Fire
'1875	گواهی پایانکار تفکیکی آپارتمان
'1876	گواهی انتقال آپارتمان / تجارتی
'1877	تایید سابقه مجار پروانه
'1878	تایید سابقه مجاز پایانکار
'1879	تایید سایقه مجاز تسویه حساب
'1880	گواهی انتقال
'1881	تجدید - اصلاح پروانه				 Fire 
'1882 پروانه توسعه بنا/ تغيير بهره برداري      Fire
'1883 پروانه دیوارکشی 
TmpArchiveGroup=0
	Select Case Ucase(TmpS)   'Info8.GetRequest.Info.NidWorkflowDeff.ToString
 
		Case "53379B94-1411-4DC8-AEE1-012D4A9B43A7"	,"C28A7410-B48A-4199-BA3A-B5254A20C1F5"     'آماده سازي	
			GuidWorkFlow=182
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6
			
		Case "1BEA53F3-426C-4423-AF29-1FCD2AE1AA62" ,"2518B6D1-54BB-4C94-BEAC-2BA4D19BB100","C4C75308-5055-460B-999E-D2E7FDDE217E","5AEC0F88-6381-41F3-A5B6-636C8941886E"	'گواهي مفاصاحساب	
			GuidWorkFlow=118
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=2
		'198264	TmpArchiveGroup=1870																					'گواهي پايانکار بهره برداري 	
		Case "82FBD9BC-EBA4-495C-99FF-22B035FB97FF","B0602DD4-FEF9-4AEB-A14A-251B3E86A6D4","04DF58F1-731B-4371-8D53-47435F681362","AB69C90F-FBAF-42FC-9414-7DAE032A1182", "01AAA0B7-8193-41D6-84F9-437FA39306A7","9D2031E0-8ACE-4CB3-A563-05FC1D42C8DF","11ad7cfc-b3e8-48e3-8ec5-88de9abdd5aa","11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA","677FBEE4-B05C-4FA6-AB19-CE897739195A"
			GuidWorkFlow=116
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1838
		Case "0BC14A0D-B94A-449D-B7AB-28E42852E120","8445278B-A9E0-47A7-8E5E-D1028FB6DB08"	,"559AAE89-1052-4394-8D0A-6105A26B04F3" ,"0358FB22-3DA1-4642-A138-0D17AD16635A","83406869-9A8B-4B80-9A13-F74F946FF84F"											'استعلام الکترونيک	
			GuidWorkFlow=175
			M_ShahrsaziArchiveGroup=4
			M_ShahrsaziArchiveGroupParvaneh=4
		Case "A3B24BD2-4CB2-4011-9900-3070AD088A6F"												'تاييد سابقه مجاز	
			GuidWorkFlow=140
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6
		Case "B617A65E-F4BF-4ACC-AEFA-3133D1AA92F1"	,"045BEA33-0A29-4F35-8CC1-672417F49DC1","1472660E-7313-4FC8-8B1D-455A81478F20","975E3267-1597-4A3F-884D-E72C42E843CA","E530B5D8-6283-4E95-B655-6DB83377A6E7","C83F6228-987B-4B5E-8906-56F7CD87EA7C"		'مجوز صدور بهره برداري موقت - صنفي
			GuidWorkFlow=189
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6
		Case "9F91A6D9-1E14-43AA-9968-39A99798D463"												'اقامتي موقت	
			GuidWorkFlow=150
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6
		Case "B54E1F02-AB96-46D7-AAC0-41691054CC96","8B26053C-5554-48FB-99FB-62431AA06C07","BA5A32EC-C23B-4E0D-8087-7D083D181644"		'اراضي غيرمحصور و رها	
			GuidWorkFlow=190
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6
		Case "F5F8B45F-A8D0-4384-9ED2-42DC747A1A20"												'ايجاد کد آپارتمان 	
			GuidWorkFlow=196
			M_ShahrsaziArchiveGroup=7
			M_ShahrsaziArchiveGroupParvaneh=7
		Case "8ECF01D4-A93C-44A7-B52B-4685634C54C6"	, "68C87B74-097E-4EC0-83D1-7723C403B1F8","5A659F4C-8DC6-4283-A496-BD20C952A810","A1C05500-1D9F-4CDF-B3C5-D26B115CC2D5","F32B14FF-E1E8-479B-8A9B-979BF74F7579","18313B50-F76D-428A-BAE4-134A918B2600","D8A215F8-2E28-45A0-A826-5ED886F16CCF","F0D7598B-7692-44B3-BD99-B18B870ACC35","3CA97718-4AFE-4752-A892-8B5EF88F176C"	'پروانه احداث بنا 
			GuidWorkFlow=110
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1839
		Case "BB0854BE-4002-4C0C-975C-04B4B8485334"	'پروانه ساختماني
			GuidWorkFlow=111
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
		Case "A24A966E-0BCB-432F-8474-4ED76CF93A79" , "D9E55CD5-922B-4D56-80C8-7FE5F7178C95","679C82EB-4227-45D5-966D-A62C7FE580EB","457475B0-E461-475E-89FC-1FB85FD41FEC","938EE557-4E3B-43EE-A9EE-ED12D17351DC","7321AE5D-C65E-48DD-A8D5-3BF4773923E7","307E2C2F-99DC-4AB3-A6A4-AF7A5D731581","D62680B7-5038-4066-B651-73B3F94C225F"	'پروانه توسعه بنا	
			GuidWorkFlow=128
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1882
		Case "297949D0-9E25-419E-AD7C-62A7034910A1","3EBB3B53-972C-425F-9939-80E76AAB6BFB"," EBCCF491-D89D-46E6-BEE7-6F4459C01329","6C67F07C-84DA-41CB-A454-BAC84078EA9F","E5F7F86A-0D02-4A01-A01C-19F13F6BEDB9","D0127432-AA14-4A16-BA07-D6A6C7DF5C5D"
			GuidWorkFlow=183
			M_ShahrsaziArchiveGroup=2	
			M_ShahrsaziArchiveGroupParvaneh=2
			TmpArchiveGroup=1875
		Case "3636B5FF-0BF6-4CBC-BBD3-875DB4A1BB11"	,"6C67F07C-84DA-41CB-A454-BAC84078EA9F"	'گواهي پايانکار تفکيکي آپارتمان	
			GuidWorkFlow=183
			M_ShahrsaziArchiveGroup=2				
			M_ShahrsaziArchiveGroupParvaneh=1
		Case "F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9","083062F2-6C96-4F21-8955-0FAE1F367704"
			GuidWorkFlow=131																			'گواهي انتقال
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=2
			TmpArchiveGroup=1876
		Case "7318F0DB-13DD-40E4-BB89-6C4C7A23FF62","1426A8E6-68BC-4B2F-8E82-7C478D5CAB14"	,"CDAA2D6E-1184-4983-A516-A8A2FF92795C","EF8464E4-38F5-4AAB-8DB5-094D6B5E1398","3157211F-C484-48E9-B65C-6BF3271A9F15","032DD2F4-DC70-44F4-88FD-119580EC9884","BDA49CCE-DFE8-4ECC-8EEC-C6D6A1EB6A76","2C1EE596-5D29-4C0A-802A-1C2492B77A9F","F9DE191A-959F-4C2E-8092-931FEB3589EB","C4F9A87D-2FA8-4E42-9B74-4480C47B1241"
			GuidWorkFlow=131																			'گواهي انتقال
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=2
			TmpArchiveGroup=1880
		Case "EE64A610-9142-4E2B-953F-6E65B522A9E1"
			GuidWorkFlow=131																			'گواهي انتقال
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=2
			TmpArchiveGroup=1880	
		Case "35CFFB97-67A3-488E-8A7A-BEC93C459B99","BDD563C8-1EB2-40EC-8811-19DB03908488"
			GuidWorkFlow=131																			'گواهي انتقال
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=2
		
		Case "85DF30CD-BE53-466E-AA76-749A40831BA5","62B24930-E3B9-455E-889F-74B32274C320","3B155802-5BD4-4261-A6F3-B65B5177BF5F","CC12A36-D0FE-476B-8BD5-EFBF3B29401A","4351C20C-643A-4A50-A4BD-AEBCEC5EADF8"		'استعلام ادارات	
			GuidWorkFlow=30
			M_ShahrsaziArchiveGroup=4
			M_ShahrsaziArchiveGroupParvaneh=4
		Case "9E7565F4-21C3-4497-887F-A751274EDEEF"	,"7FF7C349-A773-4A45-8F52-F8593B467A96","E34D362D-D8F9-4481-B1C2-E9F880A08582","8012EFB5-5C8C-4BF4-8F3D-E25168B17A24"		'گواهي تفکيک	
			GuidWorkFlow=158																		
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=2
			
		Case "DEFFC4C0-D37E-4CC3-94FD-ABA24FAAEAB2","FCA0F71C-6040-46AA-B927-F3ED908BAE80"		'ساير	
			GuidWorkFlow=144
			M_ShahrsaziArchiveGroup=7
			M_ShahrsaziArchiveGroupParvaneh=7
		Case "88E98ADA-C0F2-48B5-A724-4B455E445303","7D38106F-7362-4283-BDD9-0AC4E4BE2384","5BD4AD80-D3A5-4F87-81CB-652363740C1D","8AEAD964-2A11-4072-9F17-0BAB8E4511C5","CBB8C800-70E6-432B-B338-1F229A365712","CB807F34-5A6F-445E-9E7F-C32FC9F1338B","16C77278-30A1-4375-80DC-7EE8367B39FD"	'تجدید پروانه
			GuidWorkFlow=114
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1872
		Case "4C1AAC11-CF11-4867-B467-C4B6DF47F972"	,"FFF07F64-7B35-43BA-B363-95332389273B"		'مجوز خدماتي	
			GuidWorkFlow=13
			M_ShahrsaziArchiveGroup=5
			M_ShahrsaziArchiveGroupParvaneh=5
		Case "CB2A1920-7B97-4D20-9BBE-12D50F40BB5A","0C2B0730-504F-44AD-A067-6AF8AA1A3132"	
			GuidWorkFlow=195
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1881
		Case "0B53D213-CB83-4E5C-9319-C551CCB187E9"	,"3DBF0B9B-7A1A-4459-81F2-6FC6FC208FCF","AA2E5691-CD31-452C-A019-ADE4494EC258","B96261EE-E426-45D1-9118-41D02210CC14","B956EA48-27DC-4B8F-893D-9BC6289EFB16","CC1FBC44-7EDD-41E7-A64B-3D6DF21E855E","27CFF1AD-5936-407E-A420-5B8B71F94566"		'تجديد  اصلاح پروانه	
			GuidWorkFlow=195
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1873
		Case "72A5E955-CC6D-43E9-811B-C6FF410CA009","8EBA8AAB-A256-4EEC-B1E5-DA115686A9B0"		'تعيين بروکف	
			GuidWorkFlow=156
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6
		Case "2CA83747-919F-4FC2-B3DB-D37BA6272320"												'گواهي تجميع 	
			GuidWorkFlow=145
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6																					'بهره برداري موقت	
		Case "B825F591-1E4E-4FD0-A9B7-D437C56C9BE4","66B37DC0-8081-4745-ADEA-6514AB47DE6F","3D4A5DB8-13C1-47A4-BA37-D49E042A1E5F","FE80BD5C-F87B-495A-8F13-77EE05CBFD6D","01E1D2A4-A299-4C88-8270-8419E81D5977","2AE54B83-03DB-4DF5-A983-01A761395252"

			GuidWorkFlow=151
			M_ShahrsaziArchiveGroup=6'3 sampa 61850
			M_ShahrsaziArchiveGroupParvaneh=6
		Case "F57C96A0-697F-410C-AA37-DFDE36A29C47","710510E5-D8B3-459D-9CAC-DBE13C7DB8DE","F5B285CC-8792-498C-A6C0-0D1571EEB793"  ,"E0B7EF-E10C-4727-BF96-4399B6A37196","27F2E6C5-498B-47F4-8E1A-B83EFDD2EAA7"		'پروانه ديوارکشي	
			GuidWorkFlow=133
			M_ShahrsaziArchiveGroup=7
			M_ShahrsaziArchiveGroupParvaneh=7
			TmpArchiveGroup=1883
		Case "5CB085FB-C9CB-4D1E-A98F-DFF8E76A618A","994AD909-C2B7-476B-9BE2-8821EA1C9841"		'مجوز تعميرات	
			GuidWorkFlow=132
			M_ShahrsaziArchiveGroup=7
			M_ShahrsaziArchiveGroupParvaneh=7
		Case "676E0202-F41E-44E9-98C7-E586072F8C17","577CB127-0C16-4F4D-B645-A7B4D0EC713B","5482C28A-025D-40FB-A928-B62EAD670ACE"		'گواهي افراز	
			GuidWorkFlow=193
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6																					'گواهي عدم خلاف	
		Case "80134AAF-F070-4706-AC6A-E82778B7029C","5F07A635-B569-47F7-A97F-2AA2C36AFE37","FBE3AD14-934C-4F8F-8BA2-5C07BFB4696D","E6C9DB23-6CEA-4161-813E-078193082DD9","53E7F83A-59A2-4C0D-805A-7BE86E1DC56D","DE10D8ED-3989-4E6C-AD37-F5D915BC73D6","FBE3AD14-934C-4F8F-8BA2-5C07BFB4696D"

			GuidWorkFlow=117
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1869	
		Case "9A15158D-0369-452C-B73D-E8AC3295FE10","D84FA3D1-54FD-4D24-8000-758A300B2441"	'گواهي مفاصا بهره برداري صنفي	
			GuidWorkFlow=194
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6				
		Case "214CE534-3326-4C0F-9CF5-F6EC3844B847","9660D3B0-B41A-4B2A-BA3F-75F0E5B78085","22F8F7D9-71D9-4FBA-90F5-E43AD0EDC746","102D761E-C698-4C9B-A5A9-C6D1B57EBBCC","86D5016B-AB5D-4577-B318-791A083AFC5F","1A9B0C43-C8C5-4A08-B2F2-E123CD4C370C","26ADD6E3-9EEF-4757-87F2-2E9091AB730C","3411EFED-3AEF-4619-8DED-1C4D845572C1","AA458943-252C-444F-802E-F038156A208C","D8F77DFA-ECE9-4F52-9AEE-3458D5D91272","1661EA82-19A6-4ED6-9EE1-63B16C5FDA8D","CCCFBD5F-B245-408F-9C74-E2A21170AA2E"
			GuidWorkFlow=111																			'پروانه تجديد بنا	
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1871
		Case "08C2BE75-FAF3-457E-9B33-C97681FA419D" 											'پروانه پيشنهادي
			GuidWorkFlow=1
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
		Case "0082452E-08E3-432C-9FCE-3E9915F253D1"												'پايانکار پيشنهادي
			GuidWorkFlow=9
			M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=2
		Case "20286CC7-9313-4363-8C70-0C7814B79CC9","E33F2C9A-C0A7-46F5-B4A1-6CBDA0FE419D","17FD1A79-AC10-4B37-8D4F-1FAD7550F2C4","30F756C2-2AAF-4543-B98D-FBAB4BC916FD" ,"AB164972-4C84-445E-A8FD-D576578E24B1","E1AC3D8B-4B4E-4837-A7EE-302CB70F5FA3","7B60BD7D-5116-4B6A-8B6C-8EE303345AD9","5D7FEE70-04D1-4047-86C8-546BD7618503","C6230C76-3DEB-41E3-8F68-DEEA920763D1"		'اصلاح پروانه 
			GuidWorkFlow=112
			M_ShahrsaziArchiveGroup=1
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1874
	    Case "0D1C1D1A-CD60-4592-BAD3-685B2AFF2D98"	'گواهي استحکام بنا  
		    GuidWorkFlow=198
		    M_ShahrsaziArchiveGroup=2	
		    M_ShahrsaziArchiveGroupParvaneh=2
	
		Case "A3B24BD2-4CB2-4011-9900-3070AD088A6F" ,"87783CD5-DCF5-40A6-B4B5-5228B62C4D11"' 	تاييد سابقه مجاز
			GuidWorkFlow=140
			M_ShahrsaziArchiveGroup=6
			M_ShahrsaziArchiveGroupParvaneh=6
		Case "008DF040-922B-41A6-A3B2-7F35B1C870BB","79903151-AF7C-4A73-9F18-6EB915F3293F" '	تاييد سابقه مجاز پروانه
			GuidWorkFlow=203
		    M_ShahrsaziArchiveGroup=6
		    M_ShahrsaziArchiveGroupParvaneh=6
		   	TmpArchiveGroup=1877	
		Case "8DCD2E0C-8E5E-48A4-BAD6-8C2A1B4901B1","AE758A56-C5AC-4FC7-AEE9-C24C252F758C","6B26CCDD-4FF5-4B9D-BF15-128312E7A08A","F75B08E0-6F1A-4762-8D1F-4271F648DEA0" '	تاييد سابقه تسویه حساب (بدون گواهي)
			GuidWorkFlow=202
		    M_ShahrsaziArchiveGroup=6	
			M_ShahrsaziArchiveGroupParvaneh=6
			TmpArchiveGroup=1879
		Case "5DC809DF-16C5-459B-B6F8-C28A049A32AC","1E386CF1-F6BC-4C10-99B5-03F50EEF8104" '	تاييد سابقه قبل از 52
			GuidWorkFlow=201
		    M_ShahrsaziArchiveGroup=6	
			M_ShahrsaziArchiveGroupParvaneh=6
		Case "8B4AF4AD-A7E7-4330-BD2C-F39B3A07ADFD","83F0FAC4-2268-4005-8D52-34266DD9F595","A06E7139-ACC1-46A1-97F1-D4C92563E55E"	 '	تاييد سابقه مجاز پايانکار
			GuidWorkFlow=200
		    M_ShahrsaziArchiveGroup=6	
			M_ShahrsaziArchiveGroupParvaneh=6
		    TmpArchiveGroup=1878
		Case "F5CAE479-5301-47B9-A7FB-845B1FBCB2C2" 
			GuidWorkFlow=0
		    M_ShahrsaziArchiveGroup=0	
			M_ShahrsaziArchiveGroupParvaneh=0
		    TmpArchiveGroup=1838   
		Case "C5E8F6C0-48DA-4B3E-8184-4BD6BDD96CEB"
			 GuidWorkFlow=0
		    M_ShahrsaziArchiveGroup=6	
			M_ShahrsaziArchiveGroupParvaneh=1
		Case "B2F0CC45-B0F2-4A1F-A1AF-9B8120E4F58A" 
			 GuidWorkFlow=1
		    M_ShahrsaziArchiveGroup=6	
			M_ShahrsaziArchiveGroupParvaneh=1
			TmpArchiveGroup=1	
		Case ucase("3708270c-b7d9-495e-b72a-20d072d26d52") ,ucase("44bf6d6c-a541-4d45-bc9e-9745aa0adaeb"),ucase("4f56c548-1cf6-4c12-ac40-4f1319be03c9"),ucase("112265bc-f578-4f76-9685-ee54e739ae45")  
			 GuidWorkFlow=0
		    M_ShahrsaziArchiveGroup=4	
			M_ShahrsaziArchiveGroupParvaneh=4 
		Case "097C8C41-BA8C-4B60-8B2F-6BED0221F573",ucase("2635FE18-FE4C-4A2D-9D05-993756751262")  
			 GuidWorkFlow=0
		    M_ShahrsaziArchiveGroup=1	
			M_ShahrsaziArchiveGroupParvaneh=1  '454682
		Case ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1")  
			 GuidWorkFlow=0
		    M_ShahrsaziArchiveGroup=2
			M_ShahrsaziArchiveGroupParvaneh=2 	
		Case ucase("0fc9224b-94da-4156-89c2-c9e7c7acdfda")
			 GuidWorkFlow=200
		    M_ShahrsaziArchiveGroup=6	
			M_ShahrsaziArchiveGroupParvaneh=6
		Case ucase("25f1f6fa-354f-46b4-be0f-3497e1f82e20")  
			 GuidWorkFlow=203
		    M_ShahrsaziArchiveGroup=6	
			M_ShahrsaziArchiveGroupParvaneh=6
		Case ucase("c4afbbd2-5b4d-4451-8a5d-a50d4a1260ce")  
			 TmpArchiveGroup=2	
			 M_ShahrsaziArchiveGroup=2	
		Case ucase("d1b85def-3f07-4b29-b99b-bf47d0a283f8")
		   '  GuidWorkFlow = 500  
			 TmpArchiveGroup=1838
			 M_ShahrsaziArchiveGroup=2
	      Case  Ucase("cdcf2ac8-4527-4ffe-89cc-4e3be8a01071") 'کمیسیون ماده 5
			GuidWorkFlow=218
			 M_ShahrsaziArchiveGroup=1	
			 M_ShahrsaziArchiveGroupParvaneh=1	 		 	  	 
		Case Else
			M_ShahrsaziArchiveGroup=6    
			M_ShahrsaziArchiveGroupParvaneh=6
	End Select
	
	''
    Return M_ShahrsaziArchiveGroup
    
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"GetSara8Workflow", "اجرای فرمول درآمد-GetSara8Workflow")
End Try
End Function

 
Private function Map(ByVal TmpInput as Integer) as Boolean
 ''''''''''''
 Try
 		'	if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then Return True 

            ' خواندن اطلاعات نقشه
            Dim gu As New Guid
           
            Dim i As Integer
            Dim key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
 			Dim P_LapList As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
 			Dim KeyObj as guid
             
            Dim TmpNosazi As New BIZ.SA.ClsNosaziCodeType
            Dim TypeObject as Integer
           
            if Info8.GetRequest.Info.NidNosaziCode.tostring="00000000-0000-0000-0000-000000000000" Then
           		Return True
            End if 
            TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))
                 
            if  TmpNosazi is Nothing Then
            
            Else
            	TypeObject= TmpNosazi.EumNosaziCodeObjType
            End if
    
			Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
			Dim ListParentDastgah As List(Of BIZ.SA.ClsNosaziCodeType)
            ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
            ListParentDastgah = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 5  ).tolist
            ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2 ).tolist
          	
          	if ListParentDastgah.any() Then 
          		gu=ListParentDastgah(0).NidNosaziCode 
          	Else	
            	if ListParent.any() And TypeObject<>5 Then
            		gu=ListParent(0).NidNosaziCode 
            	Else
           			gu = Info8.GetRequest.Info.NidNosaziCode 'gu2.NidNosaziCode
				End if	
			End if 
			

				 
            Dim strnosazicode As String = ""
            strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
            
            
            Dim str() as String=strnosazicode.split("-")
            Dim M_Districk,M_House,M_Region as Double 
            M_Districk= str(0)
            M_Region=str(1)
			M_House= str(3)
	
			if M_Districk <> 80 And M_House > 10000 Then 
	        	key_List =Info8.GetMap().GetMapFind(strnosazicode,"1991")
	        	'sp 406753  اعمال قانون
 				if key_List.count=0 Then 
	 				key_List =Info8.GetMap().GetMapFind(strnosazicode,"10096")
	 			End if 																
	     	ElseIf M_Districk = 80 Then
	     		if M_Region=80 Then 
	     			key_List =Info8.GetMap().GetMapFind(strnosazicode,"3706")
 				
	     		Else	
	     			if M_House > 10000 Then	
	     				key_List =Info8.GetMap().GetMapFind(strnosazicode,"3710")
 					Else
	        			key_List =Info8.GetMap().GetMapFind(strnosazicode,"5")
	        			if key_List.count=0 Then 
	 						key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
	 					End if
            		
			   		End if	
			   	End if																
	     	Else	
	     		

	     		
	     		key_List =Info8.GetMap().GetMapFind(strnosazicode,"5")
	     		'sp 406753  اعمال قانون
	     		if key_List.count=0 Then 
	 				key_List =Info8.GetMap().GetMapFind(strnosazicode,"10096")
	 			End if 	
	     		if key_List.count=0 Then 
	 			'	Info8.AddError(BIZ.SA.EumErrorAction.Warning, " خواندن از روی نقشه ","ملک بر روی وضع موجود نمیباشد . در صورت نمایش نقشه ، از لایه موجود قدیم میباشد" )
            		if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 109 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 83 _
	            	or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 106  _
	            	or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 104  _
	            	or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 78  _
	            	or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 91  _
					or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 75  _
					or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 77  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 53  _
					or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 91  _
					or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 32  or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 78  _
					or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 58 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 113 _
					 Then 
            			key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
            		 	
            		ElseIf Rtype=117 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))= 104  or _
            		 ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") Then 	'sp 484834
            		           		 
         		 
						Try
								
							key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
	 						keyObj = key_List(0).Guid	 						
	 						P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(keyObj.tostring ,"3618") 	 						 						
	 						
	 					if key_List.count=0 Then 
	 						key_List =Info8.GetMap().GetMapFind(strnosazicode,"10096")		 						
	 					End if 
	 					
	 						if P_LapList.count=0 Then 
								key_List =Info8.GetMap().GetMapFind(strnosazicode,"5")								
	            		 	End if
	            		 	'sp 406753  اعمال قانون	
	            		 	if key_List.count=0 Then 
	 							key_List =Info8.GetMap().GetMapFind(strnosazicode,"10096")		 							
	 						End if 	
	            		Catch
		            		
	            			key_List =Info8.GetMap().GetMapFind(strnosazicode,"5")
	            		 		
	            		End Try
	           ' 	ElseIf  ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)=ucase("f16712d1-9938-4692-ab59-3cd090d70b8f") Then 	'sp 484834
	           ' 		Try
			'				key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
	 			'			keyObj = key_List(0).Guid	 						
	 			'			P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(keyObj.tostring ,"3618") 	 
	 			'		Catch
	 			'			
				'		End Try	 								            	
	            	End if 
            	End if
            
 			End if
 			
			
			
 			if  key_List.count=0  And M_ShahrsaziArchiveGroup<>1 And M_ShahrsaziArchiveGroup<>6 Then key_List =Info8.GetMap().GetMapFind(strnosazicode,"10096")	
			
			if TmpInput =1 Then 
	           	P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring,"10006") 
			  	if P_LapList.count >0 Then 
			  		Return True
			  	Else
			  		Return False
			  	End if		
			End if   	
			if TmpInput = 3 Then 
	           	P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(KeyObj.tostring,"678") 
			  	if P_LapList.count >0 Then
			  		Dim ij as Integer=0
			  		For ij =0 to P_LapList.count-1
			  			if P_LapList(ij).Code=45 Then 
			  				Return True
			  			Else
			  				Return False
			  			End if
			  		Next		
			  	Else
			  		Return False
			  	End if		
			End if   			
            if GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=90 Then Return True  	 		
            
            
            if key_List.count>0 Then
           		Return True
           	Else
           		Return False
           	End if 
        
           
 Catch Ex as Exception
     Info8.AddError(BIZ.SA.EumErrorAction.Stop, " خواندن از روی نقشه ", Ex.message )
 End Try   
''''''''''''''''''
 End Function 

Private function NotPolice(ByVal GuidUser as List(Of BIZ.SA.ClsUserGroup)) as Boolean
 ''''''''''''
 Try

        
Dim flagb as Boolean = True
Dim k as Integer=0
if GuidUser.any Then
      For k=0  to GuidUser.count -1 
          
	    If "59434E63-FFD2-4DE9-B26D-E795C44A4F6A" = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = False
	   	End if
	Next
End if

	Return flagb   
         
                
 Catch Ex as Exception
     Info8.AddError(BIZ.SA.EumErrorAction.Stop, " پلیس ", Ex.message )
 End Try   
''''''''''''''''''
 End Function 
Private Function   Gavahi (ByVal TypObj as Integer ) as  Boolean 
'msgbox( " S ", "S" )
 Dim Dt as  List(Of DtoSC.Base_ShahrsaziArchive) 
 Dim TmpDt as New List(Of DtoSC.Base_ShahrsaziArchive) 
 Dim TmpDv as DataView
 Dim tmstr As String = String.Empty
 Dim CI_HouseStatus as Integer=0
 tmStr = "00000000-0000-0000-0000-000000000000"
 Dim NidBaseTo As Guid
 Dim NidNosaziCode As Guid
 Dim TmpEumBaseInfoGroup As Byte
 Dim TmpEumRevisitGroup As Byte
 Dim tmpNidR as Guid
 Dim tmpNidRevisit As Guid
  
 NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
 TmpEumBaseInfoGroup = CByte(BIZ.SA.ClsEnum.EumBaseInfoGroup.Parvandeh)
 
 tmpNidRevisit = Guid.Parse(tmstr)
 TmpEumRevisitGroup = CByte(BIZ.SA.ClsEnum.EumRevisitGroup.Revisit)

 NidBaseTo = Info8.GetFunctions.GetNidBase(NidNosaziCode,BIZ.SA.ClsEnum.EumBaseInfoGroup.Parvandeh,BIZ.SA.ClsEnum.EumRevisitGroup.Revisit,tmpNidR)


     
 if TypObj = 4 Then
 	Info8.ParameterList.Remove ("Parvandeh_Apartment_NidBase") 
  	Info8.ParameterList.Add("Parvandeh_Apartment_NidBase", NidBaseTo.ToString)
  	Dt=Info8.GetParvandeh_Apartment.GetBase_ShahrsaziArchive
 ElseIf TypObj=5 Then
 	Info8.ParameterList.Remove ("Parvandeh_HouseSharing_NidBase")
 	Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", NidBaseTo.ToString)
 	Dt=Info8.GetParvandeh_HouseSharing.GetBase_ShahrsaziArchive
 ElseIf TypObj=3 Then
 	Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
 	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetFunctions.GetNosaziCode_InNidBase(NidBaseTo).NidNosaziCode,0)
    'ListParent=Info8.GetFunctions.GetParents_InNidBase(NidBaseTo)
    ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
    NidBaseTo=ListParent(0).NidBase
    Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
 	Info8.ParameterList.Add("Parvandeh_House_NidBase", NidBaseTo.ToString)
 	Dt=Info8.GetParvandeh_House.GetBase_ShahrsaziArchive

 ElseIf TypObj=2 Then
 	Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
 	Info8.ParameterList.Add("Parvandeh_House_NidBase", NidBaseTo.ToString)
 	Dt=Info8.GetParvandeh_House.GetBase_ShahrsaziArchive 
 	CI_HouseStatus=Info8.GetParvandeh_House.GetBase_HouseInfo.CI_HouseStatus
 	'Return True '62031 sampa 
 End if		

'- TmpDt=Dt.Where(Function(f) f.CertificateDate > "1386/00/00" And (f.Ci_ShahrsaziArchiveGroup = 2 Or f.Ci_ShahrsaziArchiveGroup = 3 Or f.Ci_ShahrsaziArchiveGroup = 4)).ToList
'msgbox( " a_c ", TmpDt.count )
'-if TmpDt.count>0 Then
'-	Return True
'-Else
'-	Return False
'-End if
'--------
 Dim T_AG as String="0"
 Dim T_AG_4 as String="0"
 Dim Sal as String="0"
 Dim Mah as String="0"
 Dim Roz as String="0"
 
 if CI_HouseStatus=1 And TypObj=2  Then '66607
  	TmpDt=Dt.Where(Function(f) f.Ci_ShahrsaziArchiveGroup = 1 Or f.Ci_ShahrsaziArchiveGroup = 2 Or f.Ci_ShahrsaziArchiveGroup = 3 Or f.Ci_ShahrsaziArchiveGroup = 4).ToList	
 Else
  	TmpDt=Dt.Where(Function(f) f.Ci_ShahrsaziArchiveGroup = 2 Or f.Ci_ShahrsaziArchiveGroup = 3 Or f.Ci_ShahrsaziArchiveGroup = 4).ToList	
 End if 
 TmpDt=TmpDt.where(Function (f) f.CI_ShahrsaziArchiveKind <> 200 And f.CI_ShahrsaziArchiveKind <> 201 And f.CI_ShahrsaziArchiveKind <> 202 _
      And f.CI_ShahrsaziArchiveKind <> 203 And f.CI_ShahrsaziArchiveKind <> 140).ToList
	 
	 if TmpDt.count > 0 Then
	  	T_AG=TmpDt.max(Function(f) f.CertificateDate)
	 End if


	if T_AG <> "0" Then 
	'	 Sal=T_AG.split("/")(0) + 4
	'	 Mah=T_AG.split("/")(1)
	'	 Roz=T_AG.split("/")(2)
	'	 T_AG_4= sal + "/" + Mah + "/" + Roz
'	End if


 'if T_AG_4 > BIZ.SA.ClsCommon.CurrentShamsiDateString Then 
 if T_AG > "1386/12/30" Then 
 	Return True
 Else
 	Return False
 End if
'----------
Else
	Return False
 End if	
End Function

Private function Set_UserGroupId(ByVal GuidUser as List(Of BIZ.SA.ClsUserGroup)) as Integer


Dim flagb as Integer = 0
Dim k as Integer=0
if GuidUser.any Then
      For k=0  to GuidUser.count -1 
           
	    If "07CB5805-89BB-4359-89D7-0EEA300EC125" = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = 1
	   	End if
	Next
End if



if GuidUser.any Then
      For k=0  to GuidUser.count -1 
          
	    If "5EDFFC89-399C-45E1-9854-51E9320E2209" = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = 2
	   	End if
	Next
End if

if GuidUser.any Then
      For k=0  to GuidUser.count -1 
            
	    If "528CC532-2EBC-4154-9272-32BE517D4033" = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = 3
	   	End if
	Next
End if
'جمع سپاری انتقال منط 10
if GuidUser.any Then
      For k=0  to GuidUser.count -1 
       'add  B40F1FAF-5885-457B-9217-DDAC6F01898E sp 395107    
	    If "4057BDF9-401C-488E-B119-15D14C604677" = UCase(GuidUser(k).GroupGuid.tostring) And ("81CAA6F8-1DA9-428B-A0C7-8C17E705B08E"= UCase(GuidUser(k).GroupGuid.tostring) or "B40F1FAF-5885-457B-9217-DDAC6F01898E"= UCase(GuidUser(k).GroupGuid.tostring)) Then
	   		flagb = 4
	   	End if
	Next
End if
'دکل
if GuidUser.any Then
      For k=0  to GuidUser.count -1 
           
	    If ucase("f8dea22c-b013-4967-ba3a-b13579c3e43d") = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = 5
	   	End if
	   	'sp 334186
	   	If ucase("4E4007B2-CF59-4115-8092-DEC313D8629D") = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = 5
	   	End if
	   	If ucase("7c17bd44-195e-4276-8461-360639885f3e") = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = 5
	   	End if
	   	'sp 334186
	Next
End if

'احداث مساجد 192630
if GuidUser.any Then
      For k=0  to GuidUser.count -1 
      ' ' add  18BF4080-5035-431B-9954-76041FFC7025  sp 357061
         'بازدید دفاتر-  مسولین دفاتر- دفاتر مهندسی- دفاتر نقشه برداری-ویرایش -کارشناسان بازدید دفاتر(غیر منتخب)- مسولین شهرسازی دفاتر- جانشین مسولین دفاتر-دفاتر بدون ثبت درخواست 253551
         'add  B40F1FAF-5885-457B-9217-DDAC6F01898E sp 395107
	    If ucase("18BF4080-5035-431B-9954-76041FFC7025") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("57273cb4-83f4-4aaf-9954-a3bbfde832e5") = UCase(GuidUser(k).GroupGuid.tostring) or ucase(GuidUser(k).GroupGuid.tostring) = ucase("18bf4080-5035-431b-9954-76041ffc7025") or ucase("6cef70fb-48c3-40f2-b7d1-f2479a3e3a57") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("1C459B84-D2FF-4C08-9472-0A7CE4768CAF") = UCase(GuidUser(k).GroupGuid.tostring) _
	    or ucase("91152337-9688-4618-9d9a-6401925743a9") = UCase(GuidUser(k).GroupGuid.tostring) _
	    or ucase("91152337-9688-4618-9D9A-6401925743A9") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("81CAA6F8-1DA9-428B-A0C7-8C17E705B08E") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("B40F1FAF-5885-457B-9217-DDAC6F01898E") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("57FCC431-0349-456A-B898-A7C0904D1522") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("BC6E8457-64CA-4BC4-9BCD-B2E4ECABC5B0") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("9CE3D600-426F-4EF7-929B-BADC3E04AB54") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("9D612183-5BF3-439F-ADE3-CE501DD08BE2") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("C65C30A2-B7B8-4AF5-95DC-E0BE1F2AF6E9") = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = 6
	   	End if
	Next
End if

'اهیت فنی 200500
if GuidUser.any Then
      For k=0  to GuidUser.count -1 
        'B40F1FAF-5885-457B-9217-DDAC6F01898E sp 395107
	    If   ucase("91152337-9688-4618-9D9A-6401925743A9") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("81CAA6F8-1DA9-428B-A0C7-8C17E705B08E") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("B40F1FAF-5885-457B-9217-DDAC6F01898E") = UCase(GuidUser(k).GroupGuid.tostring) or ucase("9D612183-5BF3-439F-ADE3-CE501DD08BE2") = UCase(GuidUser(k).GroupGuid.tostring) Then
	   		flagb = 7
	   	End if
	Next
End if

'کارگذار درامد
if GuidUser.any Then
      For k=0  to GuidUser.count -1 
           
	    If "E4FD6027-2A09-4B82-B812-6644160636C5" = UCase(GuidUser(k).GroupGuid.tostring) _
	     or "3C6497BE-9795-40ED-9C17-257071751614" = UCase(GuidUser(k).GroupGuid.tostring) _
	   	 or "59434E63-FFD2-4DE9-B26D-E795C44A4F6A" = UCase(GuidUser(k).GroupGuid.tostring) _
	   	 or ucase("16bc1db1-a38d-4166-9ef4-ab5209dd562d") = UCase(GuidUser(k).GroupGuid.tostring) _
	   	 or ucase("5aef8344-b54e-412e-8cd9-d03eda410071") = UCase(GuidUser(k).GroupGuid.tostring) _
	   	 or ucase("8bba5f92-c46f-4379-b59d-89ac3158164a") = UCase(GuidUser(k).GroupGuid.tostring) Then 
	   		flagb = 8
	   	End if
	Next
End if

'amin پیشخوان ستاد و پیشخوان ناحیه3 منطقه 12 ---230193
if GuidUser.any Then
      For k=0  to GuidUser.count -1 
           
	    If "600BA67A-00A7-42E6-A074-57FC9BA13C7F" = UCase(GuidUser(k).GroupGuid.tostring) _
	   	 or "A70A5740-01F8-4370-9FB6-7E8260DF3253" = UCase(GuidUser(k).GroupGuid.tostring) Then 
	   		flagb = 9
	   	End if
	Next
End if

	Return flagb
	
End function

Private function LstUserGroupId(ByVal LstUser() as String) as Integer
' 1 in Group
'0 Not in

Dim flagb as Integer = 0
Dim GroupIn(),TempStr as String
Dim UserList As New List(Of BIZ.SA.ClsUserGroup)

UserList = Info8.User.UserGroups
'Info8.AddError(BIZ.SA.EumErrorAction.warning, "UserList:", UserList.count)
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

Public Function SetDtMap (ByVal TmpFeatureTarhs As List(Of BIZ.SC.GeographyInfo.GeoCalculationResponse))
	Dim i,CountNoRepeat as Integer 
	Dim Repeated as Boolean=False
	Dim TmpLayerId,TmpSubLayerId,TmpLayerArea as Object
	Dim codeArrayOld(5) as String 
	  
	For i = 0 To TmpFeatureTarhs(0).IntersectingFeatures.Count - 1
            Repeated = False
            TmpLayerId = TmpFeatureTarhs(0).IntersectingFeatures(i).TheFeature.TheLayer.ID
            TmpSubLayerId = IIf(CStr(TmpFeatureTarhs(0).IntersectingFeatures(i).TheFeature.Code) = "", "", TmpFeatureTarhs(0).IntersectingFeatures(i).TheFeature.Code)
            TmpLayerArea = TmpFeatureTarhs(0).IntersectingFeatures(i).IntersectionArea
            
            
        Next
End function 


Public Function ReqTafkik_A () as Boolean

	if 	   GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=110 or GetIdWorkflow(ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring))=83  Then	
	
			Dim NosaziLst As List(Of Biz.SA.ClsNosaziCodeType)
		 	Dim  TmpListReq As  List(Of  DtoSC.Sh_RequestInfo)
	        Dim  TmpReq As   DtoSC.Sh_RequestInfo
			TmpListReq =Info8.GetRequest.GetActiveRequestList(Info8.GetRequest.Info.NidNosaziCode)
		    NosaziLst = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		 	NosaziLst = NosaziLst.Where(Function(f) f.EumNosaziCodeObjType = 3).tolist
		 	if NosaziLst.any Then
		 		TmpListReq =Info8.GetRequest.GetActiveRequestList(NosaziLst(0).NidNosaziCode)
					For Each TmpReq in TmpListReq
						
						if 	   GetIdWorkflow(Ucase(TmpReq.NidWorkflowDeff.tostring))=104 _
							or GetIdWorkflow(Ucase(TmpReq.NidWorkflowDeff.tostring))=111 or GetIdWorkflow(Ucase(TmpReq.NidWorkflowDeff.tostring))=83 Then 
								
								Return False
						End if	
					Next 
			 End if
	End if		 
	 Return True
End function 

Private Function InsertReport(ByVal pNidNosaziCode As Guid, ByVal pEngTitle As String, ByVal pPerTitle As String, ByVal pReportPath As String, ByVal pOutputNo As String, ByVal pDistrict As String,ByVal Group as Integer)
Try

            Dim tmpExportLicence As New DtoDbSaraAnls.Sara8Sys_ExportLicence
            tmpExportLicence.ExportDate = Biz.SA.ClsCommon.CurrentShamsiDateString
            tmpExportLicence.ExportTime = Date.Now.Hour.ToString.PadLeft(2, "0") & ":" & Date.Now.Minute.ToString.PadLeft(2, "0") & ":" & Date.Now.Second.ToString.PadLeft(2, "0")
            tmpExportLicence.ExportNidUser = Info8.User.UserGuid
            tmpExportLicence.ExportUserName = Info8.User.UserName
            tmpExportLicence.NidNosaziCode = pNidNosaziCode
            
            ' BizCode
            Dim TmpList As New List(Of DtoSC.Archive_Wrapper)
            TmpList = Info8.GetFunctions.GetArchiveBizCode(pNidNosaziCode)
            TmpList=TmpList.where(Function(f) f.CI_ArchiveGroup=1).tolist
           
            If TmpList.Any Then
                If TmpList.Count = 1 Then
                    tmpExportLicence.ArchiveBizCode = TmpList.First.BizCode
                ElseIf TmpList.Count > 0 Then
                    Dim mm As Integer = 0
                    For mm = 0 To TmpList.Count - 1
                        If TmpList.Item(mm).BizCode.Length < 32 Then
                            tmpExportLicence.ArchiveBizCode = TmpList.Item(mm).BizCode
                            Exit For
                        End If
                    Next
                End If
            End If

            tmpExportLicence.NidProc = Info8.Request.Info.NidProc  'New Guid(Info8.FindParameter("NidProc").ToString)
            
            tmpExportLicence.OutputEngTitle=pEngTitle '& "$" & Info8.GetRequest.Info.NidWorkItem  
            tmpExportLicence.OutputDate = Biz.SA.ClsCommon.CurrentShamsiDateString
            tmpExportLicence.OutputNo = pOutputNo
            
            tmpExportLicence.ArchiveDomain = pDistrict
            tmpExportLicence.OutputPerTitle =pPerTitle
            tmpExportLicence.ReportPath = pReportPath
            
            Dim ClsErrorResult As Biz.SA.ClsErrorResult
             
            ClsErrorResult = Info8.GetFunctions.ExportLicenceInCentral(tmpExportLicence, pDistrict,Group, Info8.Security_RequestGuid, Info8.Security_EncryptCode)
           
Catch ex As Exception
    Info8.AddError(Biz.SA.EumErrorAction.Stop, "insertReport", ex.ToString)
End Try
End Function

Private Function GetSara8WorkflowShahrvand (ByVal TmpS as String, ByVal Check as Integer  ) as Integer
Try 
	Dim ControlWorkflow as Integer=0
	Select Case Ucase(TmpS)   
 
		Case "E6C9DB23-6CEA-4161-813E-078193082DD9","5F07A635-B569-47F7-A97F-2AA2C36AFE37","FBE3AD14-934C-4F8F-8BA2-5C07BFB4696D","53E7F83A-59A2-4C0D-805A-7BE86E1DC56D","80134AAF-F070-4706-AC6A-E82778B7029C","DE10D8ED-3989-4E6C-AD37-F5D915BC73D6"    'گواهي عدم خلاف
			 ControlWorkflow=1
			
		                                                                                                                                                                                                         
		Case "9D2031E0-8ACE-4CB3-A563-05FC1D42C8DF","82FBD9BC-EBA4-495C-99FF-22B035FB97FF","B0602DD4-FEF9-4AEB-A14A-251B3E86A6D4","01AAA0B7-8193-41D6-84F9-437FA39306A7","04DF58F1-731B-4371-8D53-47435F681362","AB69C90F-FBAF-42FC-9414-7DAE032A1182","11AD7CFC-B3E8-48E3-8EC5-88DE9ABDD5AA","677FBEE4-B05C-4FA6-AB19-CE897739195A"  'گواهي پايانکار بهره برداري
			 ControlWorkflow=1
		
																			
		Case "B53B60FC-DB43-40E5-B954-0756DE68C498","1BEA53F3-426C-4423-AF29-1FCD2AE1AA62","2518B6D1-54BB-4C94-BEAC-2BA4D19BB100","5AEC0F88-6381-41F3-A5B6-636C8941886E","C4C75308-5055-460B-999E-D2E7FDDE217E" '	گواهي مفاصا حساب
			 ControlWorkflow=1
		
		Case "EF8464E4-38F5-4AAB-8DB5-094D6B5E1398","032DD2F4-DC70-44F4-88FD-119580EC9884","3157211F-C484-48E9-B65C-6BF3271A9F15","7318F0DB-13DD-40E4-BB89-6C4C7A23FF62","1426A8E6-68BC-4B2F-8E82-7C478D5CAB14","CDAA2D6E-1184-4983-A516-A8A2FF92795C" 'گواهي انتقال
			if Check=1 Then ControlWorkflow=1
			
		Case "083062F2-6C96-4F21-8955-0FAE1F367704","F47FA5B3-58AA-41C9-AD7C-FF46CBF410A9" 'گواهي انتقال آپارتمان / تجارتی											
			if Check=0 Then ControlWorkflow=1
			
		Case "E5F7F86A-0D02-4A01-A01C-19F13F6BEDB9","297949D0-9E25-419E-AD7C-62A7034910A1","EBCCF491-D89D-46E6-BEE7-6F4459C01329","3EBB3B53-972C-425F-9939-80E76AAB6BFB","3636B5FF-0BF6-4CBC-BBD3-875DB4A1BB11","6C67F07C-84DA-41CB-A454-BAC84078EA9F","D0127432-AA14-4A16-BA07-D6A6C7DF5C5D" 'گواهي پايانکار تفکيکي آپارتمان
			if Check=0 Then ControlWorkflow=1	
	End Select
    Return ControlWorkflow
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"GetSara8Workflow", "اجرای فرمول درآمد-GetSara8Workflow")
End Try
End Function

'============================== New Validation---> sampa 303043 =============================

Private Function DafaterTashilGari ( ) 
Try 
	
	  Dim CodeTashilGaran As Integer = MapTashilGaran()
	  Dim check as Integer=0	
	 	
	 	if CodeTashilGaran = 2 Then 
	  		 For k As Integer= 0 To Info8.User.UserGroups.Count - 1
            	if ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) =ucase("938df8ae-76f2-4975-9f96-a48b144c2c13") Then check=1 
            Next	
        End if    
        
        if CodeTashilGaran = 3 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("c04d73f3-960a-4c9a-aefc-09a3b4e4901a") Then check=1 
            Next	
        End if    
        
       	if CodeTashilGaran = 4 Then 
	  	    For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		  If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase( "25bf6a7c-210e-4e84-b15c-5efaf7af1800" ) Then check=1 
            Next	
        End if    
        
        if CodeTashilGaran = 5 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  	     If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("fc64f7bc-f1c7-4fd7-8c08-e6c03cf6ed37" ) Then check=1 
            Next	
        End if    
        
       	if CodeTashilGaran = 6 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  	     If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("7c118b8e-a600-43f7-bb16-d5c5c879e4f5" ) Then check=1 
           Next	
        End if    
        
        if CodeTashilGaran = 7 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  	  	 If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("7639fe65-d63a-4173-93df-6cd403494862") Then check=1 
           Next	
        End if 
           
    	if CodeTashilGaran = 8 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  	   	If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("d8d13842-fc38-4051-a942-13c8a5c757c1" ) Then check=1 
           Next	
        End if    
        
      	if CodeTashilGaran = 9 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("1d145a30-e0b5-4fce-9fd1-1238f9025965" ) Then check=1 
            Next	
        End if    
            	
      	if CodeTashilGaran = 10 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("7c118b8e-a600-43f7-bb16-d5c5c879e4f5" ) Then check=1 
            Next	
        End if    
            	
       	if CodeTashilGaran = 11 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("34e50b21-579d-4c9f-9ffb-5b175d126064" ) Then check=1 
            Next	
        End if    
            	
        if CodeTashilGaran = 12 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		  If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("34e50b21-579d-4c9f-9ffb-5b175d126064" ) Then check=1 
            Next	
        End if    
        
       	if CodeTashilGaran = 13 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  	   		If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("411d8f06-3b1e-42cb-8225-667a5f8fda2f" ) Then check=1 
            Next	
        End if    
        
        if CodeTashilGaran = 14 Then 
	  	   For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("2f80a7fa-f59c-4c54-81e5-73d169a4fd1a" ) Then check=1 
            Next	
        End if    
            	
         if CodeTashilGaran = 15 Then 
	  		 For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 	If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("7932c939-4121-4be9-9703-fa4d6387507b") Then check=1 
            Next	
        End if    
        
          if CodeTashilGaran = 16 Then 
	  		 For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 	If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("1fe20cd9-d447-42a3-ada5-fc0213e51f71" ) Then check=1 
            Next	
        End if    
            	
           if CodeTashilGaran = 17 Then 
	  		 For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 	If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ""  Then check=1 
            Next	
        End if    
            
          if CodeTashilGaran = 18 Then 
	  		 For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		 If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("817bc1f3-fd97-40b9-9c96-7194e40695cf" ) Then check=1 
            Next	
        End if    
            	
          if CodeTashilGaran = 19 Then 
	  		 For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		   If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("2d4ba060-cd8d-45f6-820d-a3d2aff8b62c") Then check=1 
            Next	
        End if   
         'sp 336797
         if CodeTashilGaran = 21 Then 
	  		 For k As Integer= 0 To Info8.User.UserGroups.Count - 1
	  		   If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("0aad3b27-d108-4482-8acd-03b3f290a00d") Then check=1 
	  		   If ucase(Info8.User.UserGroups(k).GroupGuid.ToString()) = ucase("2f80a7fa-f59c-4c54-81e5-73d169a4fd1a") Then check=1 
            Next	
        End if   
         
	  	if check=0 Then  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"توجه17", "شما مجاز به ثبت درخواست نمی باشید")
          

	  
	
Catch
	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"DafaterTashilGari", "اجرای فرمول درآمد-DafaterTashilGari")
End Try
End Function

<DisplayName("خواندن نقشه")> Public Function MapTashilGaran() as Integer
        Try
        
'if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then Return Convert.ToInt32(True) 
        
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
          	if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3706")
       		if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3710")
       		
            If tmpMapFind.Any Then
 
                Dim tmpMapService As New System.Collections.Generic.List(Of BIZ.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
                tmpMapService = Info8.GetMap().GetOverlapListCompleteDataByID(tmpMapFind.First.Guid.ToString, "10581") '??????

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
                 
                     
                 
             Return MapId 
        Catch Ex As Exception
         
            Info8.AddError(BIZ.SA.EumErrorAction.Warning, "ثبت درخواست", "خواندن از نقشه" & VbCrLf & ex.ToString)
        End Try

    End Function
    
    
 Private function MapAmlak() as String
 ''''''''''''
'if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then Return "1"
 Try
 
 		Dim NidNosaziCode as String = Info8.GetRequest.Info.NidNosaziCode.tostring
 		Dim Nidl = Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Guid.Parse(NidNosaziCode), 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000"))
        Dim lstNosazi,lstNosaziDas As New List(Of Global.Biz.SA.ClsNosaziCodeType)
        lstNosazi = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Guid.Parse(NidNosaziCode), 0)
        lstNosaziDas = lstNosazi.Where(Function(f) f.EumNosaziCodeObjType = 5).ToList
        lstNosazi = lstNosazi.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
        
        Dim Nidb = Info8.GetFunctions.GetNidBase(lstNosazi(0).NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000"))
        Info8.ParameterList.Remove ("Parvandeh_House_NidBase")
        Info8.ParameterList.Add("Parvandeh_House_NidBase", Nidb.ToString)
        if lstNosaziDas.count>0 Then  Nidb = Info8.GetFunctions.GetNidBase(lstNosaziDas(0).NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000"))
        Info8.ParameterList.Remove ("Parvandeh_HouseSharing_NidBase")
        Info8.ParameterList.Add("Parvandeh_HouseSharing_NidBase", Nidb.ToString)
         
       
       Dim Tmp as String="0" 
       Tmp="0"  
           Dim LayerListStr as String
            ' خواندن اطلاعات نقشه
            Dim i As Integer
            Dim IndexKey as Integer 
           

            Dim gu As New Guid
            Dim gu2 as New DtoSC.Base_NosaziCode
            if lstNosaziDas.count>0 Then
            	gu2=Info8.GetParvandeh_HouseSharing.GetBase_NosaziCode
            Else
           		gu2=Info8.GetParvandeh_House.GetBase_NosaziCode
            
			End if
            gu = gu2.NidNosaziCode
            
           
        
            Dim strnosazicode As String = ""
            strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)
            msgbox("strnosazicode:",strnosazicode)
           
           '
           'ghovati add samen 80-2-2-10489 tell abdolalizadeh
             Dim district,house,Region as Integer 
			     district=Info8.GetParvandeh_House.GetBase_NosaziCode_House.District
			     Region=Info8.GetParvandeh_House.GetBase_NosaziCode_House.Region
			     house=Info8.GetParvandeh_House.GetBase_NosaziCode_House.House
           '
           '
           
           
         Dim key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
         Dim key as Object 
         IndexKey=5
         LayerListStr="11704,10485,5"
           
	        	IndexKey=5
	        	LayerListStr="11704,10485,5"
	          	key_List=Info8.GetMap().GetMapFind(strnosazicode,"5")
	    	  
	    	    '
            	'sp 371707
            	 if key_List.count=0  Then
            	    if Ucase(Info8.GetRequest.Info.NidWorkflowDeff.ToString)="5AEC0F88-6381-41F3-A5B6-636C8941886E" Then
            	         key_List =Info8.GetMap().GetMapFind(strnosazicode,"10096")
            	    End if
            	 End if
            	 '
            	 '
			
			
			    Dim strCodeNew as String
				Dim tmpDistrict as Integer=0
				strCodeNew = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
				tmpDistrict =  strCodeNew.Split("-")(0)
		 	    msgbox("tmpDistrict:",tmpDistrict)
			
			
			
			    if  house> 10000 And tmpDistrict=80 Then key_List =Info8.GetMap().GetMapFind(strnosazicode,"3710")'samen
           
           
            	if Not key_List is Nothing And key_List.count > 0  Then 'a
            	    key = key_List(0).Guid
            	 Else
            		 
            	 key_List =Info8.GetMap().GetMapFind(strnosazicode,"839")
            	 
            
            	 if Not key_List is Nothing And key_List.count > 0  Then  'b
            		 	IndexKey=839
            		 	LayerListStr="11704,10485,839"
            		     key = key_List(0).Guid
            		  Else
            		  	key_List =Info8.GetMap().GetMapFind(strnosazicode,"1991")
            		  	
            	 		if Not key_List is Nothing And key_List.count > 0 Then 'c
            		 		IndexKey=1991
            		 		LayerListStr="11704,10485,1991"
            		     	key = key_List(0).Guid
            		  	Else
            		  	  key_List =Info8.GetMap().GetMapFind(strnosazicode,"3710")
            		  	  
            	 		  if Not key_List is Nothing And key_List.count > 0 Then 'd
            		 		IndexKey=3710
            		 		LayerListStr="11704,10485,3710"
            		     	key = key_List(0).Guid
            		      Else	
            		      	key_List =Info8.GetMap().GetMapFind(strnosazicode,"3706")
            		  	  
            	 		    if Not key_List is Nothing And key_List.count > 0 Then 'e
	            		 		IndexKey=3706
	            		 		LayerListStr="11704,10485,3706"
	            		     	key = key_List(0).Guid
	            		    Else
	            		    	'sp 406753  اعمال قانون
            		  	  		key_List =Info8.GetMap().GetMapFind(strnosazicode,"10096")
            		  	  
            	 		  		if Not key_List is Nothing And key_List.count > 0 Then 'f
            		 			IndexKey=10096
            		 			LayerListStr="11704,10485,10096"
            		     		key = key_List(0).Guid 	
            		     		Else		
            		        		if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring) <> Ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " * توجه ","ملک مورد نظر روی نقشه جانمایی نشده است . " )
            		        			if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring) = Ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then Return "0" : Exit Function  ' return "0" sampa 489811
            		        End if 'f	
            		        End if	'e
            		      End if  'd
            		    End if 'c
	                 End if 'b
              	End if 'a
         	
            Dim P_LapList As New  List(Of Biz.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
            Dim TmpFeatureTarhs As List(Of BIZ.SC.GeographyInfo.GeoCalculationResponse)
            P_LapList = Info8.GetMap().GetOverlapListCompleteDataByID(key.tostring,LayerListStr)
            
            P_LapList=P_LapList.where ( Function (f) f.LayerID=10485).tolist   
	
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
	     
 if Tmp<>"0" Then 
	   	P_LapList=P_LapList.where ( Function (f) f.LayerID=11704).tolist   
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
		     
   End if   	
   
   Return Tmp
                
 Catch Ex as Exception
     Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبار سنجی","5555555خواندن  لایه با خطا" & Ex.message)
End Try   
''''''''''''''''''
 End Function 
 <DisplayName("  مشارکتی خواندن نقشه")> Public Function MapMosharekati() as Integer
        Try
        
'if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then Return Convert.ToInt32(True) 
        
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
          	if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3706")
       		if Not tmpMapFind.Any Then tmpMapFind = Info8.GetMap().GetMapFind(strnosazicode, "3710")
       		
            If tmpMapFind.Any Then
 
                Dim tmpMapService As New System.Collections.Generic.List(Of BIZ.SC.SrvMap.MapService_SrvGetOverLapByguidWithLayerResult)
                tmpMapService = Info8.GetMap().GetOverlapListCompleteDataByID(tmpMapFind.First.Guid.ToString, "3570") '??????

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
                 
                     
                 
             Return MapId 
        Catch Ex As Exception
         
            Info8.AddError(BIZ.SA.EumErrorAction.Warning, "ثبت درخواست", "خواندن از نقشه" & VbCrLf & ex.ToString)
        End Try

    End Function

 <DisplayName("حریم جامع سوم خواندن نقشه")> Public Function MapHarimJame3(Optional ByVal IdLayer as String = "10320") as Integer
        Try
' if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then Return Convert.ToInt32(True)        
	'0  خارج حریم
	'1 داخل حریم		
        
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
                 
                     
                 
             Return MapId 
        Catch Ex As Exception
         
            Info8.AddError(BIZ.SA.EumErrorAction.Warning, "ثبت درخواست", "خواندن از نقشه" & VbCrLf & ex.ToString)
        End Try

    End Function

 Public Function CreatTask(ByVal NIdNod as String ,ByVal RevisitAgent as String,ByVal RevisitAgentName as String )

	Try
		Dim NewSara As New Global.Biz.SC.ClsNewWorkFlow.ClsGetWorkFlowCommandMessageContract
		NewSara.NidProc = Info8.getRequest.Info.nidproc.tostring
		NewSara.AssingTo = RevisitAgent
		NewSara.AssingToUserName = RevisitAgentName
		NewSara.CreatedBy = "EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57"
		NewSara.CreatedByName = "Esup"
		NewSara.EumAssingType = "0"
		NewSara.NidNode = NIdNod
		
	 	Dim Retursara as Boolean
	    Retursara = Info8.GetFunctions.WF_Formula_CreateSimpleTask(NewSara)
	    
	Catch e as exception
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "SaveCheckError", "CreatTask: ثبت درخواست با مشکل با شکست مواجه شد -  لطفا دوباره امتحان کنید  ") 
			
	End Try  

End Function

Public Function User () 
    
	Dim District as Integer 
	
	District=Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, Global.Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Global.Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000")).District
	Dim Region as Integer 
	Region=Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.Request.Info.NidNosaziCode, Global.Biz.SA.ClsEnum.EumBaseInfoGroup.Parvandeh, Global.Biz.SA.ClsEnum.EumRevisitGroup.None, Guid.Parse("00000000-0000-0000-0000-000000000000")).Region
	
	
	IF District = 1 Then
		AssingTo = "b3765f13-e747-495e-a9cf-b37eff270f3d"
	    AssingToUserName="پلیس ساختمان منطقه1"   	  	   	   		 	  		
	    	
    End if 
  
  	IF District = 2   Then
		AssingTo = "79b309ec-9138-4791-8c5f-e2cede3bf6be"
	    AssingToUserName="پلیس ساختمان منطقه2"   	  
    	
     End if 
 
    IF District = 3 Then
    	AssingTo = "27877a17-e31c-44be-9794-7f4f4bcfda39"
	    AssingToUserName="پلیس ساختمان منطقه3"   	
    
   End if
    IF District = 4 Then
    	AssingTo = "bb8d8f82-eb95-4750-8a10-0b5b8db34664"
	    AssingToUserName="پلیس ساختمان منطقه4"
    
   End if
    

    IF District = 5  Then
    	AssingTo = "4160ef42-d161-419f-ad94-b670a2b0c266"
	    AssingToUserName="پلیس ساختمان منطقه5"
    
     End if
   

    IF District = 6 Then
    	AssingTo = "f8412116-7b66-4ec5-99ab-856bdb0fe2c2"
	    AssingToUserName="پلیس ساختمان منطقه6"
   End if
    IF District = 7 Then
    	AssingTo = "624cf02f-95f9-4141-bf3c-bcf61e2e822a"
	    AssingToUserName="پلیس ساختمان منطقه7"
    End if
    
   	IF District = 8 Then
    	AssingTo = "09d30ec3-b9cf-426f-b41b-c0c81a0b8d0a"
	    AssingToUserName="پلیس ساختمان منطقه8"
     End if
 
    IF District = 9 Then
    	AssingTo = "73616d68-4941-40ae-a144-097158d760f0"
	    AssingToUserName="پلیس ساختمان منطقه9"
     End if
   
    IF District = 10 Then
    	AssingTo = "b8ec6b82-2c26-4696-ab4b-c507f8ca74ca"
	    AssingToUserName="پلیس ساختمان منطقه10"
     End if
	
    IF District = 11 Then
    	AssingTo = "ee3ca46e-fd09-4f42-98d5-f128f02f1cf7"
	    AssingToUserName="پلیس ساختمان منطقه11"
     End if
   
    IF District = 12 Then
    	AssingTo = "9403456e-05f3-4a4b-a033-2d459e769751"
	    AssingToUserName="پلیس ساختمان منطقه12"
    End if

End Function

'SP455250
 Public Function CheckTypeRequest(ByVal TmpGuidReq as String ) as Integer 
Try

   if String.IsNullOrEmpty( TmpGuidReq) Then   Return 0
'نوع درخواسته های بهره برداری موقت و استفاده از محل فعالیت و مجور بهرهبرداری موقت صنفی					'sp 480891 guid پایینی
   if ucase(TmpGuidReq)=ucase("2AE54B83-03DB-4DF5-A983-01A761395252") or ucase(TmpGuidReq)=ucase("5b9a549d-c94d-43ef-bab6-d67e5beec843") or ucase(TmpGuidReq)=ucase("1472660E-7313-4FC8-8B1D-455A81478F20") or ucase(TmpGuidReq)=ucase("B617A65E-F4BF-4ACC-AEFA-3133D1AA92F1") or ucase(TmpGuidReq)=ucase("66B37DC0-8081-4745-ADEA-6514AB47DE6F") or ucase(TmpGuidReq)=ucase("045BEA33-0A29-4F35-8CC1-672417F49DC1") or ucase(TmpGuidReq)=ucase("E530B5D8-6283-4E95-B655-6DB83377A6E7") or ucase(TmpGuidReq)=ucase("FE80BD5C-F87B-495A-8F13-77EE05CBFD6D") or ucase(TmpGuidReq)=ucase("01E1D2A4-A299-4C88-8270-8419E81D5977") or ucase(TmpGuidReq)=ucase("B825F591-1E4E-4FD0-A9B7-D437C56C9BE4") or ucase(TmpGuidReq)=ucase("3D4A5DB8-13C1-47A4-BA37-D49E042A1E5F") or ucase(TmpGuidReq)=ucase("C83F6228-987B-4B5E-8906-56F7CD87EA7C") or ucase(TmpGuidReq)=ucase("975E3267-1597-4A3F-884D-E72C42E843CA")  Then 
   Return 1
  Else Return 0
  
  End if
 Catch Ex As Exception
            Info8.AddError(BIZ.SA.EumErrorAction.Warning, "ثبت درخواست", ex.ToString)
 End Try

 End Function

'sp 476884
Private function CheckTarhTafzili as Integer

Try

		Dim TmpNosaziCode_Creat As New List (Of Global.Biz.SA.ClsNosaziCodeType)
'        Dim LstNosaziCode_House As New List(Of Global.Biz.SA.ClsNosaziCodeType)
        Dim LstTmpNosaziCode_Building As New List(Of Global.Biz.SA.ClsNosaziCodeType)		
		
        TmpNosaziCode_Creat = Info8.GetFunctions.GetParvandeh_Children_InNidNosaziCode(Info8.Getrequest.info.Nidnosazicode, 0)  
        
	        
        ' خطا اعتبار سنجی کامنت شد بررسی شود fj14040717    
        'LstNosaziCode_House = TmpNosaziCode_Creat.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList		
		LstTmpNosaziCode_Building = TmpNosaziCode_Creat.Where(Function(f) f.EumNosaziCodeObjType = 3).ToList
		
		
		
		Dim BB As New List(Of Global.DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
		Dim CC As New List(Of Global.DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
		Dim CCtmp As New List(Of Global.DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
		
		
	    CC = Info8.GetFunctions.GetAllRequestList(Info8.Getrequest.info.Nidnosazicode)	
	    CCtmp = (Info8.GetFunctions.GetAllRequestList(LstTmpNosaziCode_Building(0).NidNosaziCode))	
	    'این درخواست در خد ملک ثبت میشه و درخواست های در حد ساختمان بررسی هم باید بررسی شود	       	           	 
	           	       
	    BB = CC.where(Function(f) f.EumRequestStatus = 0 or f.EumRequestStatus = 1).toList() 'check Jari And Baygani movaqat bodan
	    BB = CCtmp.where(Function(f) f.EumRequestStatus = 0 or f.EumRequestStatus = 1).toList() 'check Jari And Baygani movaqat bodan	     
	     	   
	    if BB.any() Then
	    
	  '  Info8.AddError(BIZ.SA.EumErrorAction.warning,"BB Cunt: ", BB(0).nidworkflowdeff.tostring) 
	   
	        For Each x as DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult in BB
	        
	            if  Ucase(x.NidWorkflowDeff.tostring) = Ucase("2635fe18-fe4c-4a2d-9d05-993756751262") or _
	           		Ucase(x.NidWorkflowDeff.tostring) = Ucase("097c8c41-ba8c-4b60-8b2f-6bed0221f573") or _
	                Ucase(x.NidWorkflowDeff.tostring) = Ucase("8d20be44-78ab-4001-bb94-f064d22b9ce1") Then
	                
	                 '  Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NidWorkflowDeff: ", "Yes")    
	                	
	                		Return 1
	                		Exit Function
	            End if
	        Next 
	      End if
	      	      
	      
	       Return 0

	 Catch e as exception
	  ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "طرح تفضیلی", e.message)
       Exit Function
	 End Try 

End function

Public function ChangeNickName (ByVal tmpNosaziNickName as String)  as String  
  Dim tmpNickName() as String
  if tmpNosaziNickName<>"" Then 
  '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NIdWorkF",tmpNosaziNickName)
	 tmpNickName = tmpNosaziNickName.Split("-")
	 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NIdWorkF",convert.todouble(tmpNickName(6)))
	 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NIdWorkF",convert.todouble(tmpNickName(5)))
	 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NIdWorkF",convert.todouble(tmpNickName(4)))
	 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NIdWorkF",convert.todouble(tmpNickName(3)))
	 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NIdWorkF",convert.todouble(tmpNickName(2)))
	 '	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NIdWorkF",convert.todouble(tmpNickName(1)))
	 '		Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NIdWorkF",tmpNickName(6) + "-" + tmpNickName(5))
	  Return	(tmpNickName(6)) + "-" + (tmpNickName(5))  + "-" + (tmpNickName(4)) _
	  + "-" + (tmpNickName(3))  + "-" + (tmpNickName(2))  + "-" + (tmpNickName(1))  + "-" + (tmpNickName(0))
			
	' Return	convert.todouble(tmpNickName(6)) + "-" + convert.todouble(tmpNickName(5))  + "-" + convert.todouble(tmpNickName(4)) _
	'  + "-" + convert.todouble(tmpNickName(3))  + "-" + convert.todouble(tmpNickName(2))  + "-" + convert.todouble(tmpNickName(1))  + "-" + convert.todouble(tmpNickName(0))
		
  End if
  
End function

'sp 477192
Private function CheckTekrari as Boolean

'If ucase(Info8.FindParameter("NidNosaziCode")) = ucase("62e0b0b3-fd76-4132-91df-9d89b08c4208") Then Return True : Exit Function

			Dim BB As New List(Of Global.DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult)
					
		    BB = Info8.GetFunctions.GetAllRequestList(Info8.Getrequest.info.Nidnosazicode)
		   
		    BB = BB.where(Function(f) f.EumRequestStatus = 0 or f.EumRequestStatus = 1).toList() 'جاری و بایگانی موقت بودن 
		
		    if BB.any() Then
		
		        
		        For Each x as DtoSC.SrvGetAllRequestList_InNidNosaziCodeResult in BB
		        
		            if  Ucase(x.NidWorkflowDeff.tostring) = Ucase("24b35887-28e0-45c5-8952-b8e796067cfc") Then		'اعمال قانون
			            Return False
			            Exit Function
		            End if 
		         Next
		    End if
		    
	    
		    
	    Return  True      
	    
	    
          
End function

'sp 509653 

Private Function GetValue(dict As Dictionary(Of String, Object), key As String) As String
    If dict Is Nothing Then Return "0" 'String.Empty
    If dict.ContainsKey(key) AndAlso dict(key) IsNot Nothing Then
        Return iif(dict(key).ToString()="","0",dict(key).ToString())
    End If
    Return "0" 'String.Empty
End Function

Public function msgm(ByVal a as String,ByVal b as String)
   If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
       Info8.AddError(BIZ.SA.EumErrorAction.Stop, a, b)
      ' Exit Function
   End If
End function

Public Function GetMapPrice_New(ByVal Inputed_LayerID as String) as Boolean

Try	
	Dim OverlapNid As String
	Dim tmpMapFind As New global.BIZ.SC.ClsMap_Sara10()
	
         Dim key_List As New List(Of Biz.SC.SrvMap.MapService_SrvFindResult)
         Dim key as Object 	

				Dim Price as String=""
				price = "0-0-0"
							
	       		Dim LayerID as Integer
	            Dim LayerListStr as String
	            ' خواندن اطلاعات نقشه
	            Dim i As Integer
	            Dim IndexKey as Integer 
	           
    Dim nidNosaziCode As Guid
	Dim gu As New Guid
	Dim gu2 as New DtoSC.Base_NosaziCode 
    
    Dim TmpNosaziCode_Creat , ListParent,ListParent_HouseSharing As List(Of BIZ.SA.ClsNosaziCodeType)
   ' Dim TmpNosaziCode_Creat As New Global.Biz.SA.ClsNosaziCodeType           
    
	ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.Getrequest.info.NidNosaziCode,0)	 		
    ListParent_HouseSharing = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 5).tolist
    ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).tolist
    
    
'if Not Info8.GetNosaziParameter_Calc.IsCollective Then
  ' If ucase(Info8.User.UserGuid.ToString()) = ucase("407adfbd-e9e8-4fe6-a0e9-7c9f6793e580") Then 
  '     Info8.AddError(BIZ.SA.EumErrorAction.Stop, "ListParent:", ListParent.count.tostring)
  '     Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Nosazi:", Info8.GetEachInfoType_House.GetBase_NosaziCode.NidNosaziCode.tostring)
  '     Exit Function
  ' End If
'End if     


    if ListParent_HouseSharing.any()  Then
      gu=ListParent_HouseSharing(0).NidNosaziCode 
    Else
	    if ListParent.any()  Then
	      gu=ListParent(0).NidNosaziCode 
	    Else
	       gu2 = Info8.GetEachInfoType_House.GetBase_NosaziCode
		   gu = gu2.NidNosaziCode
		End if
	End if
					
	           
	        
	            Dim strnosazicode As String = ""
	            strnosazicode = Info8.GetFunctions.GetNosaziNickName(gu)	
	            	
	Dim M_Districk,M_House as Integer
	Dim LayerIn as String
	M_Districk=strnosazicode.Split("-")(0)
	
	M_House=strnosazicode.Split("-")(3) 'ملک	            			
			
			Dim ResultMapFind2 as New List(Of Dictionary(Of String, Object))
			Dim LayerList_Str As New List(Of Integer) 
				
				LayerListStr+=Inputed_LayerID 
				
					
	 			'LayerListStr.add(13937)
	            if M_House > 10000 Then
	            	if M_Districk<>80 Then
	            		LayerIn &= "1991-" 
		            	LayerID=1991
		            	LayerListStr+=",1991"
			     		ResultMapFind2 = tmpMapFind.MapFind2(LayerID," And Code='" & strnosazicode & "'", "", "1")		
	            	Else 
	            		LayerIn &= "3710-"	
			     		LayerID=3710
			     		LayerListStr+=",3710"
			     		ResultMapFind2 = tmpMapFind.MapFind2(LayerID," And Code='" & strnosazicode & "'", "", "1")	
			     	End if			      
		        Else
		        	LayerIn &= "5-"
		        	LayerID=5
		        	LayerListStr+=",5"
		          	ResultMapFind2 = tmpMapFind.MapFind2(LayerID," And Code='" & strnosazicode & "'", "", "1")			    	
				End if  

					   																
				if ResultMapFind2.count<=0 Then 
				 	LayerIn &= "839-"
		        	LayerID=839
		        	LayerListStr+=",839"
		          	ResultMapFind2 = tmpMapFind.MapFind2(LayerID," And Code='" & strnosazicode & "'", "", "1")	
		          				 
				 	if ResultMapFind2.count<=0 Then 
					 	LayerIn &= "3706-"
			        	LayerID=3706
			        	LayerListStr+=",3706"
			          	ResultMapFind2 = tmpMapFind.MapFind2(LayerID," And Code='" & strnosazicode & "'", "", "1")	
				 							 		
				 	End if
	   			End if      
	   			

	   			if ResultMapFind2.count<=0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "GetMapPrice_New", "ملک مورد نظر جانمایی نشده است - Key " & strnosazicode ) : Exit Function
	   			
	   			
	   			Dim tmp() as String
				tmp = LayerListStr.Split(",")
				Dim j as Integer = 0
				For j = 0 To tmp.Count - 1
				
				  LayerList_Str.add(tmp(j))
				Next
				
'msgm("LayerList_Str1",LayerList_Str(1))	   				
'msgm("LayerList_Str",LayerList_Str(0))
'msgm("LayerID",LayerID)
'msgm("ResultMapFind2",ResultMapFind2(0)("Nid"))	 				
	   			
	'		Dim InputOverLap As New BIZ.SC.GetOverLapByguidWithLayerModel With {
	'		        .Nid = Guid.Parse(ResultMapFind2(0)("Nid")),
	'		        .VazeMojoodId = convert.toint32(LayerID),
	'		        .LayerIds = LayerList_Str,
	'		       .BufferRadius= 1	  
	'        	}   
	        	 '      	     	
        	 
Dim InputOverLap As New Biz.SC.GetOverLapByguidWithLayerModel
'Dim LstLayer As New List(Of Integer)From {12845} 
if Not key_List is Nothing And key_List.count > 0 Then 
	InputOverLap.Nid=Guid.Parse(key)
Else
	InputOverLap.Nid=Guid.Parse(ResultMapFind2(0)("Nid"))
End if
	
InputOverLap.VazeMojoodId = LayerID
InputOverLap.LayerIds = LayerList_Str	        	 
InputOverLap.BufferRadius = 0
	 
'	msgm("Count: " , tmpMapFind.GetOverLapByguidWithLayer(InputOverLap).count)        	 
        Dim OverLapArea As Double = 0
        Dim tmpOverLapArea As Double = 0	        				
		Dim ResultOverLap As Dictionary(Of Integer, BIZ.SC.OverLapByguidWithLayerData) = tmpMapFind.GetOverLapByguidWithLayer(InputOverLap)     
		 		   				
	        For Each kvp As KeyValuePair(Of Integer, BIZ.SC.OverLapByguidWithLayerData) In ResultOverLap
	           
	            Dim overlap As BIZ.SC.OverLapByguidWithLayerData = kvp.Value
	 '    msgm("لایه", kvp.Key & "overlap.count " & overlap.count )
	            ' اگر هیچ داده‌ای در هم‌پوشانی نیست، رد شو
	            If overlap Is Nothing OrElse overlap.count <= 0 Then Continue For
	             
	       
	     '  msgm( "overlap.data " , overlap.data)     
	            ' برای هر لایه‌ای که با وضع موجود همپوشانی دارد
	            For Each layerObj As Object In overlap.data
	                Try
	                    ' تبدیل آبجکت لایه به دیکشنری
	                    Dim layerJson As String = Newtonsoft.Json.JsonConvert.SerializeObject(layerObj)
	                    Dim LayerDict As Dictionary(Of String, Object) =
	                    Newtonsoft.Json.JsonConvert.DeserializeObject(Of Dictionary(Of String, Object))(layerJson)
msgm("LayerList_Str",LayerList_Str(0) & LayerList_Str(1) )	                    
'msgm("Code_Zone",GetValue(LayerDict, "Code_Zone"))	


	                    ' دریافت Nid لایه همپوشانی
	                    OverlapNid = GetValue(LayerDict, "Nid")
	                    Dim overLapAreaStr As String = GetValue(LayerDict, "Code_Zone")
	                    
	                    Dim PartCode = overLapAreaStr.split("_")(0)
	                    if val(PartCode) = 1 Then
	                    	Return False
	                    Else
	                    	Return True
	                    End if
	                   ' Double.TryParse(overLapAreaStr, OverLapArea)
	                    
	                   ' if kvp.Key=val(Inputed_LayerID) Then
	                    '	msgm("Price_N: ",GetValue(LayerDict, "Name") & " _ OverLapArea:  " & GetValue(LayerDict, "OverLapArea"))
	                    '	OverLapArea = val(GetValue(LayerDict, "Code_Zone"))
'msgm("OverLapArea",OverLapArea)
'msgm("tmpOverLapArea",tmpOverLapArea)
	                    	
	                   	'	if OverLapArea>tmpOverLapArea Then
	                   	'		Price = (GetValue(LayerDict, "Name")).tostring
	                   	'		tmpOverLapArea=val(GetValue(LayerDict, "OverLapArea"))

	                   	'	End if

	               	  '  End if
	               	  
	                   
	
	                Catch ex As Exception
	                   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "تابع همپوشانی", ex.Message)
	                    Continue For
	                End Try
	            Next
	        Next   
	        
	'	msgm("Price " & mabna ,Price)	             				
	      ' Return Price 	
	       			
	Catch ex as exception
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "***تابع همپوشانی", ex.Message)
	End Try 
End Function

 Public Function GettyMap(Optional ByVal IdLayer as String = "13628") as Boolean
        Try
' if ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)= ucase("6e8e4e56-a0e7-458b-9842-e25ddc55d9bc") Then Return Convert.ToInt32(True)        
	'0  خارج حریم
	'1 داخل حریم		
        
            Dim i As Integer = 0
            Dim MapId As Integer = 0
            Dim LayerId As String
           
            Dim Max As Double
            Dim p_OverLapArea As Double
                    Dim Code As String
                    Dim OverLapArea As Double           


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
                       OverLapArea  = IIf(IsDBNull(tmpMapService.Item(i).OverLapArea), 0, tmpMapService.Item(i).OverLapArea)
                       
                        If OverLapArea >= Max Then                           	                        
	                        Max = OverLapArea
	                        Code  = IIf(IsDBNull(tmpMapService.Item(i).Code), 0, tmpMapService.Item(i).Code)
                        End if                        
                      	
                      '	msgm("Code " & Code ,"OverLapArea0 " & tmpMapService.Item(0).OverLapArea & " | :OverLapArea1 " & tmpMapService.Item(1).OverLapArea )
						                       
                    Next                              
              
                End If
   
          End if
          
      '    msgm("Code " & Code ,"OverLapArea0" )
          	Dim CodePart =  Code.split("-")(0)
	        if val(CodePart) = 1 Then 
	        	Return False
	        End if                                      
                 
             Return True 
        Catch Ex As Exception
         
            Info8.AddError(BIZ.SA.EumErrorAction.Warning, "GettyMap Func", "خواندن از نقشه " & VbCrLf & ex.ToString)
        End Try

    End Function

Public function AddRevisit  as Integer
	Try
	

			 		Dim DateNow as Date = Date.Now
			        Dim UserTMP As Biz.SA.ClsUser
			        UserTMP.UserGuid = Guid.Parse("EEF07F67-63D9-41D6-90D6-E6BF2D7C9A57")
			        UserTMP.UserName = "Esup"
			        
			        Dim RevisiAgentT As New DtoSC.Sh_Revisit
			        
			        RevisiAgentT.NidRevisitAgent = Guid.Parse("7862F32C-A3E8-4C0A-95F5-4161A0671B35")
			        RevisiAgentT.NidProc = Info8.GetRequest.Info.NidProc
			        RevisiAgentT.NidNosaziCode = Info8.GetRequest.Info.NidNosaziCode
			        RevisiAgentT.NidUser = Guid.Parse("B2D0F9C2-2158-4D2E-8A7A-89388BCA500D")
			        RevisiAgentT.UserName = "شهروند سپاری"
			        RevisiAgentT.RevisitDate = Biz.SA.ClsCommon.ConvertDateTimeToPersionString(DateNow.AddDays(1))
			        RevisiAgentT.RevisitTimeFrom = "8:00"
					RevisiAgentT.RevisitTimeTo = "9:00"
					
			        Info8.GetFunctions.AddRevisitAgentForRequest(RevisiAgentT, UserTMP)
			        
	
	Catch e  as exception
	'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Start", "ثبت درخواست با مشکل با شکست مواجه شد - AddRevisit -  لطفا دوباره امتحان کنید  ")  
	End Try			    
End Function