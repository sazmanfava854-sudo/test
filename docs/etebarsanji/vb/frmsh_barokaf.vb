<DisplayName("بروکف")> Public Function frmsh_barokaf() As clsOut

   'sp 496536  -  ایجاد ردیف مامور بازدید برای درخواست های اصلاح نقشه  در سرا 10
if Info8.GetRequest.Info.IsSara10Request  Then
 	if ucase(Info8.GetRequest.Info.NidWorkFlowDeff.ToString) = ucase("6E8E4E56-A0E7-458B-9842-E25DDC55D9BC") Then 
 	 		
 		Try
		
		     '   Dim RRevist As New Biz.SC.ClsAssignRevisit()
		 
		        Dim UserTMP As Biz.SA.ClsUser
		        UserTMP = Info8.User
		        
		        Dim RevisiAgentT As New DtoSC.Sh_Revisit
		        
		        RevisiAgentT.NidRevisitAgent = Info8.User.UserGuid 
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
End if	'end sp 496536


 if Info8.GetRequest.Info.NidWorkItem="10103017" Then Call GetArchiveDWG()

 ' sp 342479
if Info8.GetRequest.Info.IsSara10Request And 1=2  Then 
	 Dim BizCode = "21060"
	 Dim Workspace_Name = "sara8"
	 Dim Workspace_Pass = "ark@2993"
	 Dim ArchiveCoreURL = "http://shahr1.mashhad.ir:8073/archive/"
	
	 Dim ArchiveProxy As New Global.ArchiveCoreLib.ClsArchiveProxy(ArchiveCoreURL, Workspace_Name, Workspace_Pass)
	 
	 Dim result_login = ArchiveProxy.Login()
	
	 If result_login Then
		 
		 Dim Response = ArchiveProxy.GetFiles(BizCode)
		'  Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "**********", Response.Files.Count ) : Exit Function
	          ' For Each item As Dto.ClsFile In Response.Files
	                
	            '    Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "**********", Response.Files(0).CreatedDate ) : Exit Function
	        '    Next
	  End If
	
	Dim str as biz.SA.ClsNosaziCodeType
	Dim district as Integer=1
	Dim ArcWrrap As List(Of Global.DtoSC.Archive_Wrapper)
	Dim Nidnosazimelak as guid
	Dim ListParent As New List(Of BIZ.SA.ClsNosaziCodeType)
	ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
	ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
	Nidnosazimelak=ListParent(0).NidNosaziCode 
	ArcWrrap = Info8.GetFunctions.GetArchiveBizCode(Nidnosazimelak)
	Dim TmpUrlFromBizCodeResult As New List(Of Global.Biz.SC.SrvArchive.SrvGetUrlFromBizCodeResult)
	Dim LstArchive As New List(Of Global.BIZ.SC.SrvArchive.ClsImageItem)
	if ArcWrrap.count >0 Then  
		ArcWrrap=ArcWrrap.where (Function (f) f.CI_ArchiveGroup =1 ).tolist ()
		if ArcWrrap.count >0 Then
			Dim tmpSerchArchive As New Global.Biz.SC.SrvArchive.ClsSearchDataFile
		    Dim tmpSerchArchiveOut As New  Global.Biz.SC.SrvArchive.ClsSearchDataFile
		    Dim TmpG As New List(Of Guid) 
		    TmpG.Add(Guid.Parse("540D119D-14F1-4F65-94D1-7F39FC2B3B48"))
			tmpSerchArchive.BizCode =  ArcWrrap.min(Function (f) f.BizCode) 'ArcWrrap(0).BizCode.tostring
			tmpSerchArchive.DomainName = district
			tmpSerchArchive.AllGuids = TmpG
			tmpSerchArchive.UserID ="540D119D-14F1-4F65-94D1-7F39FC2B3B48"
			tmpSerchArchive.UserName ="tofighnezhad"
			tmpSerchArchiveOut = Info8.GetFunctions.GetArchiveDataFileByBizCode(tmpSerchArchive, district)		
			TmpUrlFromBizCodeResult = tmpSerchArchiveOut.Result.GetFile
		'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"S"  & tmpSerchArchive.BizCode , "AAA" & TmpUrlFromBizCodeResult.count) : Exit Function
	        TmpUrlFromBizCodeResult = TmpUrlFromBizCodeResult.Where(Function(f) (f.NidEntity = 1040 ) ).ToList
	        
		 End if 
	  End if 
  	  Dim LstUserTahator() AS String
 	  LstUserTahator={"8923D414-828D-4462-B3C3-2B85D9ACB7CC","B385422F-8FDD-4B1A-80A3-FB25027F8A65", "ac01f67f-fae6-4cad-8b61-aeb2c3e345ab" _
 	  ,"b0a03cc1-4db9-446e-b2bc-44128c594c85","8FAA82DD-3F37-45A9-A536-F4BCCD9F6E2C","B315159F-1824-4DB7-957A-125AE4695923","6CD4C355-4D12-4CEB-8CA4-4690BF31253D" _
 	  ,"45e29c8e-9f62-4933-8a77-cb012ff21d4c","C0B05F70-5A08-41F5-BFD3-5783FA4E2160","EA722297-661A-4E06-B367-DD1F3D1E3CE3","A3DD1FD6-0D77-409B-BD49-77CBD841CA70" _
 	  ,"7422715a-7cc9-48fd-a6ee-a032c2fc524c", "4e110b2d-e0eb-448f-a417-2a005ae96ec0"} 
 	'  if LstUserGroupId(LstUserTahator)=1 Then
 	'	if  TmpUrlFromBizCodeResult.count=0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی Dwg", "نقشه بردار محترم، لطفا ابتدا فایل dwg عرصه بارگذاری گردد ") : Exit Function
 	'  End if   
End if	 

 Dim timeM As New System.Diagnostics.Stopwatch
        Dim milliSec2M As Long = 0
        timeM.start()
        milliSec2M = timeM.ElapsedMilliseconds()
'Plogfile("t1",
Darmasir=0
'Info8.AddError(BIZ.SA.EumErrorAction.Stop, " Dwg", "aa ") : Exit Function
Sara10=Info8.GetRequest.Info.IsSara10Request  
 ' if Sara10=True Then Exit Function
   Try
    Dim NidWDefff as String 'sp219143 -jahate khandane Gardeshkar function  GetSara8Workflow(NidWDefff) call shod
	NidWDefff=ucase(Info8.getRequest.Info.NidWorkflowDeff.tostring)
    NidWDefff=GetSara8Workflow(NidWDefff)
   
    
	    Dim LstUserNot15() AS String
		LstUserNot15={"AC01F67F-FAE6-4CAD-8B61-AEB2C3E345AB"}

	Dim LstUser15() AS String
	LstUser15={"B0A03CC1-4DB9-446E-B2BC-44128C594C85"} 
	 Dim strCodeW as String
	 Dim dis,Reg as Integer
	 dis=0
	 reg=0
     strCodeW = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	 dis =  strCodeW.Split("-")(0)
	 Reg =  strCodeW.Split("-")(1)

	 if Info8.GetRequest.Info.IsSara10Request And 1=2  Then 
  	
  		Dim AmlakM As New Global.Biz.Communication.ClsAmlakMashHad
        Dim AmlakMessage As New Global.Biz.Communication.ClsAmlakMashHadMessageContract
        Dim Authorization As String
        Authorization = "Basic aGVkYWlhdC1oQG1hc2hoYWQuaXI6YXNkQDEyMw=="
        Dim AmlakResult As New Global.Biz.Communication.ClsAmlakMashHadSendResult
       
		Dim ListParent As List(Of BIZ.SA.ClsNosaziCodeType)
		ListParent=Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode,0)
		ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
		Info8.ParameterList.Remove("Revisit_House_NidBase")
		Info8.ParameterList.Add("Revisit_House_NidBase", Info8.GetFunctions.GetNidBase(ListParent(0).NidNosaziCode, 1, 0, Info8.GetRequest.Info.NidRevisit).tostring)
 

        AmlakMessage.CodeRequest = Info8.GetRequest.Info.Nidworkitem
        AmlakMessage.density = "50"
        AmlakMessage.docArea = Info8.GetRevisit_House.GetBase_HouseInfo.DocArea
        AmlakMessage.existArea = Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea
        AmlakMessage.nosaziCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
        AmlakMessage.nidWorkItem =Info8.GetRequest.Info.Nidworkitem
        AmlakMessage.wayArea = iif(isdbnull(Info8.GetBarokaf.GetSh_BaroKaf.PathValue),0 ,Info8.GetBarokaf.GetSh_BaroKaf.PathValue)
		 Dim UserInfo as New BIZ.SA.ClsUser 
          
        UserInfo = Info8.User
        AmlakResult = AmlakM.sendRequestToServiceP4(AmlakMessage,UserInfo)
      '  Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "**********", "D") : Exit Function
   End if     

	 If Sara10 = True Then 
	  If CheckEditSara10("7666b59f-54a4-465e-879f-2c6672f01332",Info8.GetRequest.Info.NidProc.tostring ) = True Then     Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*اعتبارسنجي", "با توجه به تاييد چک ليست ( کنترل نحوه اعمال چيدمان مجاز ) امکان تغيير اطلاعات وجود ندارد  ") : Exit Function
	 End If
	 ' Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "**********", "wS") : Exit Function
  
	 'sp 303990	 
	 'sp 318138  
	 
 if Sara10=False Then
 
   if dis=3 And (Reg=1 or Reg=2 or Reg=3 or Reg=5 or Reg=6 or Reg=7 or Reg=8 or Reg=27 or Reg=25 or Reg=20 or Reg=21 or Reg=26) And LstUserGroupId(LstUser15)=1 Then 
		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "در حوزه 1,2,3,5,6,7,8,27,25,20,21,26 نمی باشد امکان ویرایش برای دفتر شما مقدور تیست") : Exit Function
	   
   End if
   
   if dis=3 And (Reg=4 or Reg=9 or Reg=10 or Reg=30 or Reg=31 or Reg=32 or Reg=28 or Reg=29 or Reg=33 or Reg=34 or Reg=35 ) And LstUserGroupId(LstUserNot15) =1 Then 
	 Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*اعتبارسنجی", "در حوزه 4,9,10 ,30 ,31,32,28,29,33,34,35 نمی باشد امکان ویرایش برای دفتر شما مقدور تیست") : Exit Function
	 Exit Function
  End if
 	

 


	'	 if dis=2 And (Reg=1 or Reg=2 or Reg=3) And LstUserGroupId(LstUserNot15)=1 Then 
	'	 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "در حوزه 1,2,3 می باشد امکان ویرایش برای دفتر شما مقدور تیست") : Exit Function
	'			 Exit Function
	'	 End if
	'	 if dis=2 And Reg<>1 And Reg<>2 And Reg<>3 And LstUserGroupId(LstUser15) =1 Then 
	'	 	Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*اعتبارسنجی", "در حوزه 1,2,3 نمی باشد امکان ویرایش برای دفتر شما مقدور تیست") : Exit Function
	'			 Exit Function
	'	 End if
 End if		 
Msgbox("a",2)
'-.-.-.-.-.-.-.-.--.-.-.-.-.-.-.-.-.-.-..--..-.-.-.--.-.-.-.-.-.-.-.-.-.- 
    
  Dim F_changeb as Boolean=True 'False
  '2988644-فیلد عمق اول و پشت جبهه تحت هر شرایطی قابل ویرایش باشد
'  if Sara10=False And 
'sp 364258 if ( Info8.User.UserGuid.tostring = "540d119d-14f1-4f65-94d1-7f39fc2b3b48" or Info8.User.UserGuid.tostring ="6f99c240-c92b-4fb3-99db-6760750167eb")  Then '
        Dim TmpNosazid As New BIZ.SA.ClsNosaziCodeType
        Dim TypeObjectt as Integer	
        Dim TmpListBase_EdgeN As New List(Of DtoSC.Base_Edge)
        Dim ListBase_Edgep As New List(Of DtoSC.Base_Edge)    
	    Dim TmpbInfo8 As New Biz.SC.ClsObjectFactory
	    Dim rr As Integer
	    Dim tmpt As String
	    Dim F_Omghb as Boolean=False
        Dim F_sayerb as Boolean=False
        Dim check_depth1 as  Boolean = False
 	       TmpNosazid = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))
       	if  TmpNosazid is Nothing Then
        Else
           	TypeObjectt= TmpNosazid.EumNosaziCodeObjType
        End if
	    If  TypeObjectt<>4 And TypeObjectt<>6 Then 'Info8.GetRequest.Info.RequestDate > "1394/04/08" And
	    
            

	    tmpt = Info8.FindParameter("NidRevisit")
         Dim aq = Info8.GetFunctions.GetNidLib(Guid.Parse(tmpt))
	    TmpbInfo8._District  =1        
        TmpbInfo8.Revisit_House = Nothing
       
         
    
        TmpbInfo8.ParameterList.Add("NidProc", Info8.FindParameter("NidpRoc"))
        TmpbInfo8.ParameterList.Add("NidRevisit", Info8.FindParameter("NidRevisit"))
         Info8.ParameterList.REMove("Revisit_House_NidBase")
        Info8.ParameterList.Add("Revisit_House_NidBase", aq.RHouse.NidBase.ToString)
        TmpbInfo8.ParameterList.Add("Revisit_House_NidBase", aq.RHouse.NidBase.ToString)
       
		TmpbInfo8.GetRevisit_House._nidBase =Guid.Parse (aq.RHouse.NidBase.ToString)
        TmpListBase_EdgeN = TmpbInfo8.GetRevisit_House.GetBase_Edge()
             
        ListBase_Edgep = Info8.GetBarokaf.GetBase_Edge


        if TmpbInfo8.GetRevisit_House.GetBase_HouseInfo.CurrentArea <> Info8.GetBarokaf.GetBase_HouseInfo_Revisit.CurrentArea Then F_changeb=False
        if TmpbInfo8.GetRevisit_House.GetBase_HouseInfo.DocArea  <> Info8.GetBarokaf.GetBase_HouseInfo_Revisit.DocArea Then F_changeb=False
        if TmpbInfo8.GetRevisit_House.GetBase_HouseInfo.RemainArea  <> Info8.GetBarokaf.GetBase_HouseInfo_Revisit.RemainArea Then F_changeb=False
        if TmpbInfo8.GetRevisit_House.GetBase_HouseInfo.MamarArea  <> Info8.GetBarokaf.GetBase_HouseInfo_Revisit.MamarArea Then F_changeb=False
        if TmpbInfo8.GetRevisit_House.GetBase_HouseInfo.GlasshouseArea  <> Info8.GetBarokaf.GetBase_HouseInfo_Revisit.GlasshouseArea Then F_changeb=False
        
        If Not TmpListBase_EdgeN Is Nothing Then
            TmpListBase_EdgeN = TmpListBase_EdgeN.OrderByDescending(Function(f) f.CI_SideCode).tolist()
        End If
        If Not ListBase_Edgep Is Nothing Then
            ListBase_Edgep = ListBase_Edgep.OrderByDescending(Function(f) f.CI_SideCode).tolist()
        End If

        If Not ListBase_Edgep Is Nothing And F_Omghb = False Then

            If TmpListBase_EdgeN Is Nothing Then
                F_Omghb = True
                F_sayerb = True
                F_changeb =False
               
            Else
             If TmpListBase_EdgeN.count<> ListBase_Edgep.count Then 
             		F_sayerb = True
             Else
                For rr = 0 To TmpListBase_EdgeN.count - 1
                                        
                    If TmpListBase_EdgeN(rr).CI_SideCode <> ListBase_Edgep(rr).CI_SideCode Then F_sayerb = True
                    If TmpListBase_EdgeN(rr).CI_PathType <> ListBase_Edgep(rr).CI_PathType Then F_sayerb = True
                    If TmpListBase_EdgeN(rr).EdgeBarCurrent <> ListBase_Edgep(rr).EdgeBarCurrent Then F_sayerb = True
                    If TmpListBase_EdgeN(rr).EdgeLenCurrent <> ListBase_Edgep(rr).EdgeLenCurrent Then F_sayerb = True
                    If TmpListBase_EdgeN(rr).PathWidth <> ListBase_Edgep(rr).PathWidth Then F_sayerb = True
                    If TmpListBase_EdgeN(rr).AfterEditWidth <> ListBase_Edgep(rr).AfterEditWidth Then F_sayerb = True
                    If TmpListBase_EdgeN(rr).AfterEditBarLen <> ListBase_Edgep(rr).AfterEditBarLen Then F_sayerb = True
                    If TmpListBase_EdgeN(rr).AfterEditSideLen <> ListBase_Edgep(rr).AfterEditSideLen Then F_sayerb = True
                    '
                    If TmpListBase_EdgeN(rr).Depth1 <> ListBase_Edgep(rr).Depth1 Then F_Omghb = True
                    If TmpListBase_EdgeN(rr).Depth3 <> ListBase_Edgep(rr).Depth3 Then F_Omghb = True
                    
                 
                    
                    
                   msgbox("F_Omghb*:","Tmp Depth1:" & TmpListBase_EdgeN(rr).Depth1 & "Depth1:" & ListBase_Edgep(rr).Depth1 & "Tmp Depth3:" & TmpListBase_EdgeN(rr).Depth3 & "Depth3:" & ListBase_Edgep(rr).Depth3  )
                   
                    
                Next
              End if  
            End If
        End If                        
        'TmpbInfo8.ParameterList.REMove("Revisit_House_NidBase")
        
       
      
      if F_Omghb=True And F_sayerb=False Then  F_changeb =True
      if F_sayerb=True Then F_changeb =False
     ' msgbox("a@:","F_Omghb:" & F_Omghb  & "F_sayerb:" & F_sayerb & "F_changeb:" & F_changeb)
	   End if
	  '2988644 agr  F_Omgh=true and F_sayer=false  hata dar soorate dhastane fiche taeed  edit shavad
     
	   
 'sp 364258 End if
  
  '-.-.-.-.-.-.-.-.--.-.-.-.-.-.-.-.-.-.-..--..-.-.-.--.-.-.-.-.-.-.-.-.-.-
   
' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "GuidWorkFlow" & GuidWorkFlow , FicheTaeed(Info8.GetRequest.info.NidProc.tostring)) 
 
   Msgbox("a",3) 
   			Dim ListBase_Edgepp As New List(Of DtoSC.Base_Edge)  
'sp 499747   		if Info8.GetBarokaf.GetBase_Edge.count>0 Then
        	
'sp 499747           	ListBase_Edgepp = Info8.GetBarokaf.GetBase_Edge   			
'sp 499747      			Dim rrr as Integer = 0
'sp 499747           	For rrr = 0 To ListBase_Edgepp.count - 1        	        		
'sp 499747           		 if ListBase_Edgepp(rrr).Depth1>0 Then  check_depth1 = True
'sp 499747           	Next 
'sp 499747           	if Not check_depth1 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی - بروکف", "مقدار عمق اول را وارد کنید - sp 499747 ")
'sp 499747           End if	
    
 
    if Map(0) Then 
    Else
        if  Sara10=False Then
    	    if Map(1) And Set_UserGroupId(Info8.User.UserGroups) <> 2 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " اعتبارسنجی ", "ملک روی نقشه نمی باشد" ) : Exit Function
    	End if
	End if

      Dim Dt2 As New List(Of DtoSC.Base_Edge)
        Dt2 = Info8.GetBarokaf.GetBase_Edge
        Dim kk As Integer
    if Dt2.count>0 Then
      For kk = 0 To Dt2.Count - 1

          if CheckChar (CStr( Dt2(kk).AfterEditBarLen)) =False Then  Exit For 
          if CheckChar (CStr( Dt2(kk).AfterEditBarLen))=False  Then Exit For
          if CheckChar (CStr( Dt2(kk).AfterEditSideLen)) =False  Then Exit For
          if CheckChar (CStr( Dt2(kk).BarLenEffective)) =False  Then Exit For
          if CheckChar (CStr( Dt2(kk).EdgeAndSideCurrent)) =False  Then Exit For
          if CheckChar (CStr( Dt2(kk).EdgeAndSideDoc)) =False  Then Exit  For
          if CheckChar (CStr( Dt2(kk).EdgeBarCurrent)) =False  Then Exit For
          if CheckChar (CStr( Dt2(kk).EdgeBarDoc)) =False Then Exit For
          if CheckChar (CStr( Dt2(kk).EdgeLenCurrent)) =False Then  Exit For
          if CheckChar (CStr( Dt2(kk).EdgeLenDoc)) =False Then Exit For
          if CheckChar (CStr( Dt2(kk).Remain)) =False  Then Exit For
          if CheckChar (CStr( Dt2(kk).RemainBarLen)) =False  Then  Exit For
          if CheckChar (CStr( Dt2(kk).RemainSideLen)) =False  Then Exit For
                
      Next                 
   End if
   
   'new sampa 155062-161108
    Dim NN As Integer
   if Dt2.Count>0 Then
    For  NN = 0 To Dt2.Count - 1
    	Try 
     		Dim LenC = Dt2(NN).EdgeLenCurrent.ToString
        Catch
       		Info8.AddError(BIZ.SA.EumErrorAction.Stop, " اعتبارسنجی ", "لطفا طول ضلع موجود را وارد نمائید" ) : Exit Function
        End Try  
    	if Dt2(NN).EdgeLenCurrent.ToString=""  or CStr(Dt2(NN).EdgeLenCurrent)="0" Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " اعتبارسنجی ", "لطفا طول ضلع موجود را وارد نمائید" ) : Exit Function
    	'msgbox("y##","CI_PathType:" &  Dt2(NN).CI_PathType & "EntranceWith:" & Dt2(NN).EntranceWith )
    	if Dt2(NN).CI_PathType=11 And (Dt2(NN).EntranceWith=0 or Dt2(NN).EntranceWith.ToString="" )    Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " اعتبارسنجی ", " لطفا عرض معبر ورودی دوربرگرادن در ستون عرض معبر ورودی وارد شود " ) : Exit Function'sp227306
    	
    	'sp 293579
    	if (Dt2(NN).CI_PathType=4 or Dt2(NN).CI_PathType=5 ) And (Dt2(NN).EntranceWith=0 or Dt2(NN).EntranceWith.ToString="" )    Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " اعتبارسنجی ", " لطفا عرض معبر ورودی میدان در ستون عرض معبر ورودی وارد شود " ) : Exit Function
    	
      '  if Dt2(NN).CI_PathType=11  And IsNumeric(Dt2(NN).CI_ValBlockFord)=False  Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " اعتبارسنجی ", " لطفا  درعرض معبر ورودی دوربرگرادن در ستون معبر در بلوک دارایی  فقط عدد وارد شود " )
    Next
  End if
    
    		
    
        Dim tmpTaeed As Integer = 0
     
        If TaeedMGavahi() = 1 Then
            If TaeedMLayehe() = 1 Then
                If TaeedMSolh() = 1 Then
                    tmpTaeed = 1
                Else
                    tmpTaeed = 0
                End If
                
            Else
                tmpTaeed = 0
            End If
        Else
            tmpTaeed = 0
        End If
        
     'Msgbox("NidProc:",Info8.GetRequest.info.NidProc.tostring)    
     'Msgbox("FicheTaeed:",FicheTaeed(Info8.GetRequest.info.NidProc.tostring))    
     'function FicheTaeed In Formool MovafeghatOsooli neveshteshode  
    if FicheTaeed(Info8.GetRequest.info.NidProc.tostring)=0  Then' And (GuidWorkFlow=116 or GuidWorkFlow=117 or GuidWorkFlow=118 or M_ShahrsaziArchiveGroup=1) Then '---'219143 196469 116 117 118
     	if F_changeb=False Then
		   Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "به علت داشتن فیش تایید شده مجاز به تغییر نمی باشید ") 
		   Exit Function
		ElseIf F_changeb=True Then
		   Info8.AddError(BIZ.SA.EumErrorAction.warning, "اعتبارسنجی", "به علت داشتن فیش تایید شده مجاز به تغییر نمی باشید ")
		End if 
	End if	 
    
 Msgbox("a",4)
    

        If tmpTaeed = 0 Then
        ' if F_changeb=False Then
           Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "تایید مدیر شده است مجاز به تغییر نمی باشید ") : Exit Function
        Else
            Dim CurrentArea As Double = 0.5
            Dim DocArea As Double = 0.5
            Dim BaroKaf As Double = 0
            Dim Masahat As Double = 0
           
            Dim S_Amlak As Double = 0
            Dim tmp As String
            Dim tmStrP As String

            tmp = Info8.FindParameter("NidRevisit")

            'Dim tmpNidRevisit as New guid
            'tmpNidRevisit= Info8.GetFunctions.GetActiveRevisitBaseInfo(Guid.Parse(tmStrP)).NidRevisit

            Dim a = Info8.GetFunctions.GetNidLib(Guid.Parse(tmp))
            ' Dim a = Info8.GetFunctions.GetNidLib(tmpNidRevisit)
			Info8.ParameterList.Remove("Revisit_House_NidBase")
            Info8.ParameterList.Add("Revisit_House_NidBase", a.RHouse.NidBase.ToString)


            Info8.GetBarokaf.Base_HouseInfo_Config.RemainArea = 0


            Dim i As Integer
            Dim strCode As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
            Dim HouseCode = strCode.Split("-")(3)

            Dim AreaKol As Double = 0
			'msgbox("RequestDate", Info8.GetRequest.Info.RequestDate)
				'msgbox("Nidp111",Info8.GetRequest.Info.NidProc.tostring)
				'msgbox("NidpByp",Info8.FindParameter("NidProc"))
				''''''''''''''''''''''''''''''''''''''''''''
				
					Dim UserList As New List(Of BIZ.SA.ClsUserGroup)
				   if Sara10=False Then UserList = Info8.User.UserGroups
				    
				    Dim ii as Integer
				    Dim TmpU as Integer=0 
				    Dim TmpUEdit as Integer=0 
				    Dim NotMap as Integer=0
				    if UserList.Any Then 
				    	For ii=0 to UserList.Count-1
				    		'253551
				    	' add  18BF4080-5035-431B-9954-76041FFC7025  sp 357061
				    		if ucase(UserList(ii).GroupGuid.ToString()) = ucase("3a718066-1c33-445e-9b03-e0277f6cc756") or ucase(UserList(ii).GroupGuid.ToString()) = ucase("055ee619-0e97-4454-961f-32805805dd09") or ucase(UserList(ii).GroupGuid.ToString()) = "57FCC431-0349-456A-B898-A7C0904D1522"  or ucase(UserList(ii).GroupGuid.ToString()) = "18BF4080-5035-431B-9954-76041FFC7025"  or  ucase(UserList(ii).GroupGuid.ToString()) = ucase("57273cb4-83f4-4aaf-9954-a3bbfde832e5") _
				    		or 	ucase(UserList(ii).GroupGuid.ToString()) = ucase("18bf4080-5035-431b-9954-76041ffc7025") or ucase(UserList(ii).GroupGuid.ToString()) = ucase("6cef70fb-48c3-40f2-b7d1-f2479a3e3a57") or ucase(UserList(ii).GroupGuid.ToString()) = ucase("79947A5B-350B-48E1-9B27-AABC16A94D0F") Then TmpUEdit=1
				    		'if ucase(UserList(ii).GroupGuid.ToString()) = "61892BC0-C5DF-4D0A-B1DA-C0F10B659878"  Then TmpUEdit=1  poshtibaniformulha
				    		if ucase(UserList(ii).GroupGuid.ToString()) = "994E26BE-41F1-4BBF-ABDC-EB63D451E5F7"  or ucase(UserList(ii).GroupGuid.ToString()) = "FBAE3C14-6E44-428B-B939-302032DB0E0D" OR ucase(UserList(ii).GroupGuid.ToString()) ="460C6A1B-C550-4FB5-B03A-4EC35B889521" Then 'Or  ucase(UserList(ii).GroupGuid.ToString()) = "61892BC0-C5DF-4D0A-B1DA-C0F10B659878" 
				    			TmpUEdit=1	
				    		End IF
				    			'msgbox("@@@ " & TmpU ,ucase(UserList(ii).GroupGuid.ToString()) & " AAA:" & CStr(ucase(UserList(ii).GroupGuid.ToString()) = "61892BC0-C5DF-4D0A-B1DA-C0F10B659878"  ))
				    	Next ii
				    End if
				   'Info8.AddError(BIZ.SA.EumErrorAction.warning, "TmpU:", TmpU)
				    '172788
		'aaaaaa	   if TmpUEdit=0 And Sara10=False And  ucase(Info8.User.UserGuid.tostring) <> ucase("CB32C037-2DD0-429A-B801-D9F29E6E7507") Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی**", "شما مجاز به تغییر دیتا نمی باشید ") : Exit Function
				    '	msgbox("TmpU",TmpU)

		'169117	if TmpU=1 And CStr(Info8.GetBarokaf.GetSh_BaroKaf.KorokiNumber)="1" Then NotMap=1
				'''''''''''''''''''''''''''''''''''''''''''''
			'325041 if Info8.GetBarokaf.GetSh_BaroKaf.KorokiNumber ="" Then
		'325041		Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا مبلغ کارشناسی بروکف  به ریال وارد گردد  (در فیلد شماره کروکی) ") : Exit Function
		'325041	Else
		'325041		if Not IsNumeric(Info8.GetBarokaf.GetSh_BaroKaf.KorokiNumber) Then 	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا مبلغ کارشناسی بروکف  به ریال وارد گردد (فقط عدد)") : Exit Function
			
			
		'325041	End if	
			Dim TmpNosazi As New BIZ.SA.ClsNosaziCodeType
	        Dim TypeObject as Integer	
	 		TmpNosazi = Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode))
	       	if  TmpNosazi is Nothing Then
	        Else
	           	TypeObject= TmpNosazi.EumNosaziCodeObjType
	        End if
	       
           ' If Info8.GetRequest.Info.RequestDate > "1394/04/08" And NotMap=0  And TypeObject<>4 And TypeObject<>6 Then
               
                Dim Dt As List(Of DtoSC.Base_Edge) = Info8.GetBarokaf.GetBase_Edge'Info8.GetRevisit_House.GetBase_Edge
            
            '93986	AreaKol = MapPriceNew(dt, 2, 1, Info8.GetBarokaf.Base_HouseInfo_Config.DocArea)
			

	            Dim MaxArea, MinArea As Double
	            If AreaKol <= 250 Then
	                MaxArea = AreaKol + (AreaKol * 0.05) '42533
	                MinArea = AreaKol - (AreaKol * 0.05)
	            Else
	                MaxArea = AreaKol + (AreaKol * 0.05)
	                MinArea = AreaKol - (AreaKol * 0.05)
	            End If
				If Info8.GetRevisit_House.GetBase_HouseInfo.DocArea <= MaxArea And Info8.GetRevisit_House.GetBase_HouseInfo.DocArea >= MinArea Then
	                AreaKol = Info8.GetRevisit_House.GetBase_HouseInfo.DocArea
	            ElseIf Info8.GetRequest.Info.RequestDate > "1394/04/08" And AreaKol <> -1200  And math.abs(AreaKol -Info8.GetRevisit_House.GetBase_HouseInfo.DocArea)> 0.05*Info8.GetRevisit_House.GetBase_HouseInfo.DocArea Then
	            
	            'AA	Info8.AddError(BIZ.SA.EumErrorAction.Warning, " توجه ","مساحت موجود : " & CStr( AreaKol) &  "مساحت سند : " & CStr( Info8.GetRevisit_House.GetBase_HouseInfo.DocArea ) & " اختلاف مساحت موجود و سند  بیشتر از 5 درصد می باشد قابلیت پاسخگویی ندارد ")     
	           
	           End If
                 If Info8.GetRequest.Info.RequestDate > "1394/04/08" And AreaKol <> -1200 Then

                'AA	Info8.GetRevisit_House.GetBase_HouseInfo.CurrentArea = Math.Round(AreaKol, 2)
                'AA	Info8.GetBarokaf.Base_HouseInfo_Config.CurrentArea = Math.Round(AreaKol, 2)
                End if 
                
                ''' H
                if Dt.count>0 Then
	               	For i = 0 To Dt.count - 1
	                	If Dt(i).PathWidth > 0  And  Dt(i).CI_PathType =0 Then
	                    	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا از جهتی که عرض معبر وارد نموده نوع معبر را نیز وارد نمایید ") : Exit Function
	                    End if 
	                Next
				End if
           ' Else
            'Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*a", "a5") 
                If HouseCode < 10000 Then
                   ' Dim Dt As List(Of DtoSC.Base_Edge) = Info8.GetBarokaf.GetBase_Edge
                    '========sp435306
                  
               		''' H
               		if Dt.count>0 Then
		               		For i = 0 To Dt.count - 1
		                    	If (Dt(i).PathWidth > 0 or Dt(i).AfterEditWidth > 0) And  Dt(i).CI_PathType =0 Then
		                        	Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی", "لطفا از جهتی که عرض معبر وارد نموده نوع معبر را نیز وارد نمایید ") : Exit Function
		                        End if 
		                    Next
                   End if
                
                End If
        '    End If
		
	        Dim TajavozBemabar as Double=0
	    
            If Not (a.RDastgah Is Nothing) Then

                ' مساحت موجود
                Dim tmpCurrentArea As New DtoSC.Base_HouseInfo
                Info8.ParameterList.Add("Revisit_HouseSharing_NidBase", a.RDastgah.NidBase.ToString)
                tmpCurrentArea =Info8.GetBarokaf.GetBase_HouseInfo_Revisit'Info8.GetRevisit_House.GetBase_HouseInfo'Info8.GetRevisit_HouseSharing.GetBase_HouseInfo




                If Not (isdbnull(tmpCurrentArea.CurrentArea)) Then
                    CurrentArea = iif(tmpCurrentArea.CurrentArea < 0.5, 0, tmpCurrentArea.CurrentArea)
                End If

                If Not IsDBNull(tmpCurrentArea.DocArea) Then
                    DocArea = iif(tmpCurrentArea.DocArea < 0.5, 0, tmpCurrentArea.DocArea)
                End If
                
                
                ''RE 10028405
                '
                If Not IsDBNull(Info8.GetBarokaf.GetSh_BaroKaf.ToGangwayArea) Then
                    TajavozBemabar = Info8.GetBarokaf.GetSh_BaroKaf.ToGangwayArea
                End If
                '
                '
                if TajavozBemabar>0 Then CurrentArea=math.max(CurrentArea-TajavozBemabar,0)
                
                
                
                
                
                ' Info8.AddError(BIZ.SA.EumErrorAction.warning, "Masahat1",Masahat)
                If CurrentArea = 0 And DocArea = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " بروکف", "//مساحت ملک صحیح نمی باشد") : Exit Function
               'RE 4354384  Masahat = Math.Min(iif(CurrentArea = 0, 1000000, CurrentArea), iif(DocArea = 0, 1000000, DocArea))
                Masahat = Math.Min(iif(CurrentArea = 0, 100000000, CurrentArea), iif(DocArea = 0, 100000000, DocArea))
                '	 Info8.AddError(BIZ.SA.EumErrorAction.warning, "Masahat2",Masahat)

                If Not IsDBNull(tmpCurrentArea.AreaAfterEdit) Then
                    Darmasir = Info8.GetBarokaf.GetSh_BaroKaf.PathValue
                    'sp 317213
                    Dim lisOtherEqMabar As New List(Of DtoSC.Base_OtherEquipment)
                    lisOtherEqMabar=Info8.GetBarokaf.GetBase_OtherEquipment
                    lisOtherEqMabar= lisOtherEqMabar.where(Function(f) f.CI_OtherEquipmentType=139).tolist
                   'sp 317213 ghovati   tell  hesami  comment shod 99/10/16  if lisOtherEqMabar.count>0 Then Darmasir + = lisOtherEqMabar.sum(Function(f) f.OtherEquipmentValue)
                    
                    If Info8.GetRevisit_House.GetBase_NosaziCode_House.District = 80 Then Darmasir += Info8.GetBarokaf.GetSh_BaroKaf.GreenArea
                    'comment sp 356574      میزان در حریم   S_Amlak = iif(isdbnull(Info8.GetBarokaf.GetSh_BaroKaf.ToadjusentArea), 0, Info8.GetBarokaf.GetSh_BaroKaf.ToadjusentArea)
                    If S_Amlak > 0 Then Darmasir += S_Amlak
                End If
 
                If math.round(Masahat - Darmasir, 2) < 0 Then
                    Info8.GetBarokaf.Base_HouseInfo_Config.AreaAfterEdit = 0
                Else
                    Info8.GetBarokaf.Base_HouseInfo_Config.AreaAfterEdit = math.round(Masahat - Darmasir, 2)
                End If
                
            Else
                
                
                
                Dim tmpCurrentArea As DtoSC.Base_HouseInfo = Info8.GetBarokaf.GetBase_HouseInfo_Revisit'Info8.GetRevisit_House.GetBase_HouseInfo
                If Not IsDBNull(tmpCurrentArea.CurrentArea) Then
                    CurrentArea = iif(tmpCurrentArea.CurrentArea < 0.5, 0, tmpCurrentArea.CurrentArea)
                End If
                
                If Not IsDBNull(tmpCurrentArea.DocArea) Then
                    DocArea = iif(tmpCurrentArea.DocArea < 0.5, 0, tmpCurrentArea.DocArea)
                End If
                
                '10028405
                '
                 If Not IsDBNull(Info8.GetBarokaf.GetSh_BaroKaf.ToGangwayArea) Then
                    TajavozBemabar = Info8.GetBarokaf.GetSh_BaroKaf.ToGangwayArea
                End If
                '
                '
                if TajavozBemabar>0 Then CurrentArea=math.max(CurrentArea-TajavozBemabar,0)
               'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",1)
               'Info8.AddError(BIZ.SA.EumErrorAction.Stop, " DocArea",Info8.GetBarokaf.GetSh_BaroKaf.PathValue)


                If CurrentArea = 0 And DocArea = 0 Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, " بروکف", "مساحت ملک صحیح نمی باشد+") : Exit Function

                Masahat = Math.Min(iif(CurrentArea = 0, 100000000, CurrentArea), iif(DocArea = 0, 100000000, DocArea))

                If Not IsDBNull(tmpCurrentArea.AreaAfterEdit) Then
                    Darmasir = Info8.GetBarokaf.GetSh_BaroKaf.PathValue
                    'sp 317213
                    Dim lisOtherEqMabar As New List(Of DtoSC.Base_OtherEquipment)
                    lisOtherEqMabar=Info8.GetBarokaf.GetBase_OtherEquipment
                    lisOtherEqMabar= lisOtherEqMabar.where(Function(f) f.CI_OtherEquipmentType=139).tolist
                   'sp 317213 ghovati   tell  hesami  comment shod 99/10/16 if lisOtherEqMabar.count>0 Then Darmasir + = lisOtherEqMabar.sum(Function(f) f.OtherEquipmentValue)
                    
                    
                    If Info8.GetRevisit_House.GetBase_NosaziCode_House.District = 80 Then Darmasir += Info8.GetBarokaf.GetSh_BaroKaf.GreenArea
                   'comment sp 356574 میزان در حریم   S_Amlak = iif(isdbnull(Info8.GetBarokaf.GetSh_BaroKaf.ToadjusentArea), 0, Info8.GetBarokaf.GetSh_BaroKaf.ToadjusentArea)
                    If S_Amlak > 0 Then Darmasir += S_Amlak
                End If
                'Msgbox ("%%%%%" , "Masahat:" & Masahat & "Darmasir:" & Darmasir & "dar:" & Info8.GetBarokaf.GetSh_BaroKaf.PathValue  )


                If math.round(Masahat - Darmasir, 2) < 0 Then
                    Info8.GetBarokaf.Base_HouseInfo_Config.AreaAfterEdit = 0
                Else
                    Info8.GetBarokaf.Base_HouseInfo_Config.AreaAfterEdit = math.round(Masahat - Darmasir, 2)
                End If

            End If
            '-
            
            
             'sp435306==================================

 
	                Dim TAfterEditWidth as Boolean = False
	                'sp 439527 
	                if Darmasir >0 And  math.round(Masahat - Darmasir, 2) >0 Then TAfterEditWidth= True
	            '	if Dt.count>0 Then
		           '    For i = 0 To Dt.count - 1
		           '       If iif(isdbnull(Dt(i).AfterEditWidth), "0", Dt(i).AfterEditWidth) > "0" Then TAfterEditWidth = True
		           '    Next
		          '  End if
                    
                    Dim RemainSideLen As Boolean = False

                    If Darmasir > 0 And Darmasir < Masahat Then
                        RemainSideLen = False
                        if Dt.count>0 Then
	                        For i = 0 To Dt.count - 1
	                            If iif(isdbnull(Dt(i).AfterEditSideLen), "0", Dt(i).AfterEditSideLen) > "0" Then RemainSideLen = True
	                            msgbox("a","RemainSideLen:" & RemainSideLen)
	                        Next
                        End if
                    End If

                    Dim RemainBarLen As Boolean = False
                    msgbox("a","Darmasir:" & Darmasir & "Masahat:" & Masahat)
                    If Darmasir > 0 And Darmasir < Masahat Then
                        RemainBarLen = False
                        if Dt.count>0 Then
	                        For i = 0 To Dt.count - 1
	                            If iif(isdbnull(Dt(i).AfterEditBarLen), "0", Dt(i).AfterEditBarLen) > "0" Then RemainBarLen = True
	                            msgbox("a","RemainBarLen:" & RemainBarLen)
	                        Next
	                    End if
                    End If
                    
                    msgbox("","Dt:" & Dt.count & "TAfterEditWidth:" & TAfterEditWidth &  "RemainBarLen:" & RemainBarLen & "RemainSideLen:" &  RemainSideLen)
                    '5337382 If (RemainBarLen = False Or RemainSideLen = False) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی***", "ملک در مسیر دارد لطفا اطلاعات اضلاع و بر پس از رعایت بر اصلاحی را کامل نمایید") : Exit Function
               		' Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*a", TAfterEditWidth) 
               		 'Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*a", RemainBarLen) 
               		' Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*a", RemainSideLen) 
               		if TAfterEditWidth And (RemainBarLen = False Or RemainSideLen = False) Then Info8.AddError(BIZ.SA.EumErrorAction.Stop, "اعتبارسنجی##", "ملک  در مسیر دارد لطفا اطلاعات اضلاع و بر پس از رعایت بر اصلاحی را کامل نمایید") : Exit Function

'========================================
            
            
        End If
        'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",2)
  
	  if Darmasir>0 And  Sara10=True  Then   SaveCheck ("98535fe3-d2cc-48d4-94b7-17927e71e932", 2,0)
	  'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",3)
	  'Info8.AddError(BIZ.SA.EumErrorAction.Stop, "Darmasir", "D") 
        
       
      
        Dim tmptxx as String '*n
        tmptxx = Info8.FindParameter("NidRevisit")'*n
        
        
         Dim aqz = Info8.GetFunctions.GetNidLib(Guid.Parse(tmptxx))'*n
         
  		Dim TmpInfo8 As New Biz.SC.ClsObjectFactory
  		'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 49)
        TmpInfo8.Revisit_House = Nothing
        TmpInfo8._District = 1
        'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 49000)
       ' TmpInfo8.GetRevisit_House._District =1
        'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 491)
        
        'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 492)
        TmpInfo8.ParameterList.Add("NidProc", Info8.FindParameter("NidpRoc"))
        TmpInfo8.ParameterList.Add("NidRevisit", Info8.FindParameter("NidRevisit"))
        TmpInfo8.ParameterList.Add("Revisit_House_NidBase", aqz.RHouse.NidBase.ToString)
        
       'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",4) 

        Dim TmpAreaAfterEdit As Double =0
        'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 50)
        
            TmpAreaAfterEdit=TmpInfo8.GetRevisit_House.GetBase_HouseInfo.AreaAfterEdit
            
        'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 51)
        
        
        Dim AreaAfterEdit As Double = Info8.GetBarokaf.Base_HouseInfo_Config.AreaAfterEdit
       
       	Dim TmpListBase_Edge As New List(Of DtoSC.Base_Edge)
		Dim ListBase_Edge As New List(Of DtoSC.Base_Edge)

        TmpListBase_Edge = TmpInfo8.GetRevisit_House.GetBase_Edge()
		ListBase_Edge = Info8.GetBarokaf.GetBase_Edge'Info8.GetRevisit_House.GetBase_Edge()
		'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 52)
		
		
		
				
		Dim F_Edge As Boolean = False
        kk=0

        If Not TmpListBase_Edge Is Nothing Then
            TmpListBase_Edge = TmpListBase_Edge.OrderByDescending(Function(f) f.CI_SideCode).tolist()
        End If

        If Not ListBase_Edge Is Nothing Then
            ListBase_Edge = ListBase_Edge.OrderByDescending(Function(f) f.CI_SideCode).tolist()
        End If

        If Not TmpListBase_Edge Is Nothing Then
            If TmpListBase_Edge.count <> ListBase_Edge.count Then F_Edge = True
        End If
	
        If Not ListBase_Edge Is Nothing And F_Edge = False Then

            If TmpListBase_Edge Is Nothing Then
                F_Edge = True
            Else
                For kk = 0 To TmpListBase_Edge.count - 1
                    If TmpListBase_Edge(kk).CI_SideCode <> ListBase_Edge(kk).CI_SideCode Then F_Edge = True
                    If TmpListBase_Edge(kk).CI_PathType <> ListBase_Edge(kk).CI_PathType Then F_Edge = True
                    If TmpListBase_Edge(kk).EdgeBarCurrent <> ListBase_Edge(kk).EdgeBarCurrent Then F_Edge = True
                    If TmpListBase_Edge(kk).EdgeLenCurrent <> ListBase_Edge(kk).EdgeLenCurrent Then F_Edge = True
                    If TmpListBase_Edge(kk).PathWidth <> ListBase_Edge(kk).PathWidth Then F_Edge = True
                    If TmpListBase_Edge(kk).AfterEditWidth <> ListBase_Edge(kk).AfterEditWidth Then F_Edge = True
                    If TmpListBase_Edge(kk).AfterEditBarLen <> ListBase_Edge(kk).AfterEditBarLen Then F_Edge = True
                    If TmpListBase_Edge(kk).AfterEditSideLen <> ListBase_Edge(kk).AfterEditSideLen Then F_Edge = True
                    If F_Edge = True Then Exit For
                Next
            End If
        End If
        
       'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 53)
    'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",5)
Msgbox("a",5)
        Dim DateT As String = BIZ.SA.ClsCommon.CurrentShamsiDateString
        Dim lisOtherEq1 As New List(Of DtoSC.Base_OtherEquipment)
        Dim lisOtherEqA As New List(Of DtoSC.Base_OtherEquipment)
        Dim lisOtherEq As New List(Of DtoSC.Base_OtherEquipment)
        Dim OtherEq As New DtoSC.Base_OtherEquipment

        lisOtherEq =  Info8.GetBarokaf.GetBase_OtherEquipment
        
		If lisOtherEq.any Then lisOtherEqA = lisOtherEq.where(Function(f) f.CI_OtherEquipmentType = 250).tolist

        If lisOtherEqA.any Then lisOtherEq1 = lisOtherEqA.where(Function(f) f.OtherEquipmentValue = DateT).tolist

        Dim Fdoc As Boolean = False
        If (TmpAreaAfterEdit <> AreaAfterEdit) Then Fdoc = True

        If (Not lisOtherEqA.any) And (Fdoc = True Or F_Edge = True) And F_changeb=False Then
        

            OtherEq.CI_OtherEquipmentType = 250
            OtherEq.OtherEquipmentValue = BIZ.SA.ClsCommon.CurrentShamsiDateString.tostring
            OtherEq.NidBase = Info8.GetRevisit_House.GetBase_HouseInfo.Nidbase 
            OtherEq.comments=getTime()
            Info8.GetBarokaf.GetBase_OtherEquipment.Add(OtherEq)

        ElseIf lisOtherEqA.count >0 And (Fdoc = True Or F_Edge = True) And F_changeb=False Then

			
            For kk = 0 To Info8.GetBarokaf.GetBase_OtherEquipment.count - 1
                If Info8.GetBarokaf.GetBase_OtherEquipment(kk).CI_OtherEquipmentType = 250 Then 
                	Info8.GetBarokaf.GetBase_OtherEquipment(kk).OtherEquipmentValue = BIZ.SA.ClsCommon.CurrentShamsiDateString
                	Info8.GetBarokaf.GetBase_OtherEquipment(kk).comments=getTime()
                End if	
            Next
			

        End If

    	
   ''''''''''''''''''''''''''''''''''''''''''''''''''175605
   Dim OtherEq_T As New List(Of DtoSC.Base_OtherEquipment)
   Dim kkk as Integer
   OtherEq_T=Info8.GetBarokaf.GetBase_OtherEquipment
   'Info8.AddError(BIZ.SA.EumErrorAction.Stop, " OtherEq_T.count A", OtherEq_T.count) 
   OtherEq_T=OtherEq_T.Where(Function(f) f.CI_OtherEquipmentType = 350).ToList
  
   Dim OtherEq_B As New DtoSC.Base_OtherEquipment
   msgbox("OtherEq_T:",OtherEq_T.count)
   msgbox("F_changeb:",F_changeb)
   
   'Inpath RE 4601401
    Dim InpathOld as Double=0
    Dim Inpathnew as Double=0
        Inpathnew = Info8.GetBarokaf.GetSh_BaroKaf.PathValue
        InpathOld = TmpInfo8.GetBarokaf.GetSh_BaroKaf.PathValue
        msgbox("xxx:","Inpathnew:" & Inpathnew & "InpathOld:" & InpathOld)
        if Inpathnew <> InpathOld Then F_changeb=False
   '
   
   
   
   
   if OtherEq_T.count=1 Then
   
   	  For kkk = 0 To Info8.GetBarokaf.GetBase_OtherEquipment.count - 1
        If Info8.GetBarokaf.GetBase_OtherEquipment(kkk).CI_OtherEquipmentType = 350 And F_changeb=False Then 
           	Info8.GetBarokaf.GetBase_OtherEquipment(kkk).OtherEquipmentValue = BIZ.SA.ClsCommon.CurrentShamsiDateString
           	Info8.GetBarokaf.GetBase_OtherEquipment(kkk).comments=getTime() 
        End if	
      Next

   ElseIf OtherEq_T.count=0  Then 
   if  F_changeb=False Then
   msgbox("abc:",F_changeb)
       'Info8.AddError(BIZ.SA.EumErrorAction.Stop, " OtherEq_T.count",  Info8.GetBarokaf.GetBase_OtherEquipment.count) 
   		OtherEq_B.CI_OtherEquipmentType = 350
        OtherEq_B.CI_OtherEquipmentGroup = 1
        OtherEq_B.OtherEquipmentValue = BIZ.SA.ClsCommon.CurrentShamsiDateString.tostring
        OtherEq_B.NidBase = Info8.GetRevisit_House.GetBase_HouseInfo.Nidbase
        OtherEq_B.comments=getTime()
        Info8.GetBarokaf.GetBase_OtherEquipment.add(OtherEq_B)
    End if    
      'Info8.AddError(BIZ.SA.EumErrorAction.Stop, " OtherEq_T.www",  Info8.GetBarokaf.GetBase_OtherEquipment.count) 
   Else     
            Info8.AddError(BIZ.SA.EumErrorAction.Stop, "توجه", "در سایر امکانات دیتا نامعتبر وارد نموده اید") : Exit Function 
   End if
  
    'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",6)
    
''''''''''''''''''''''''''''''''''''''''''''''''''''''''''175605
  	'Sampa 304313
Msgbox("a",6)  
  	 Dim p_RequestNo As String = ""
       
     Dim NOffice ,NMange ,Tel,NCard  as String
     Dim  District,Region  as Integer
     Dim strnosazicode As String = ""
     strnosazicode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
            
            
            Dim str() as String=strnosazicode.split("-")
           
            District= str(0)
            Region=str(1)
            
         
 Dim UserSms as Integer=1       
    if Not Sara10 Then 
          
            
   	Dim WorkflowTaskList As New List(Of BIZ.SC.SrvTaskService.Task)
   	Dim WorkflowTaskList_Convert As New List(Of BIZ.SC.SrvTaskService.Task)
   	WorkflowTaskList = Info8.GetFunctions().GetRequestLog(Guid.Parse(Info8.GetRequest.Info.NidProc.ToString))
   	WorkflowTaskList_Convert=WorkflowTaskList
   	WorkflowTaskList_convert = WorkflowTaskList.Where(Function(f) f.TaskTitel = "کانورت" ).ToList()
   if WorkflowTaskList_convert.count=0 Then  WorkflowTaskList_convert = WorkflowTaskList.Where(Function(f) f.TaskTitel = "بررسي اطلاعات درخواست" ).ToList()
	      
    if WorkflowTaskList_convert.count=0 Then WorkflowTaskList_convert = WorkflowTaskList.Where(Function(f) f.TaskTitel = "ثبت درخواست" ).ToList()
	       	               
  If WorkflowTaskList_Convert.Any Then
        
	        '-----convert
	         Dim Maxid As Double
            Maxid = IIf(IsDBNull(WorkflowTaskList_convert.max(Function(f) f.NidSort)), -1, WorkflowTaskList_convert.Max(Function(f) f.NidSort))
WorkflowTaskList_convert=WorkflowTaskList_convert.Where(Function(f) f.NidSort = Maxid ).ToList()
	         
	         if WorkflowTaskList_convert.any Then
	            '  Dim w  as Integer
	            '  For w=0 to WorkflowTaskList_convert.count-1
				         Dim StrUserGuid_convert,GroupGuid_convert As Guid
				         Dim GroupList_convert As New List(Of BIZ.SA.SrvSecurityWCF.UserGroup)
				           StrUserGuid_convert = WorkflowTaskList_convert(0).AssingTo
				           'logfileNew("StrUserGuid_convert*:",StrUserGuid_convert.tostring )
						   GroupList_convert = Info8.GetFunctions.GetUserGroups(StrUserGuid_convert)
						   'logfileNew("x:",GroupList_convert.count )
					     Dim q as Integer
					     For q = 0 To GroupList_convert.Count - 1
					        GroupGuid_convert=GroupList_convert(q).GUID
					        
					        if Ucase(GroupGuid_convert.tostring)=Ucase("4a58bd9f-df2a-4a15-b8a7-327d41542697") Or Ucase(GroupGuid_convert.tostring)=Ucase("dd1e6da0-4ba3-41dc-ac1d-85e7ba15d529") or _
					           Ucase(GroupGuid_convert.tostring)=Ucase("5aef8344-b54e-412e-8cd9-d03eda410071") Or Ucase(GroupGuid_convert.tostring)=Ucase("16bc1db1-a38d-4166-9ef4-ab5209dd562d") or _ 
					           Ucase(GroupGuid_convert.tostring)=Ucase("e4fd6027-2a09-4b82-b812-6644160636c5") Or Ucase(GroupGuid_convert.tostring)=Ucase("8bba5f92-c46f-4379-b59d-89ac3158164a") Then UserSms=0
					           'sp 334302
					   	 Next
			   '  Next
			 End if  
			 
			  
		
			 '-----  
End if 		   
       
End if ' sara10            
        	   
 Msgbox("a",7)   
             'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",7)
 if Info8.GetFunctions.GetNosaziCode_InNosaziCodeStr(strnosazicode, 0).EumNosaziCodeObjType  <4 And District<>80 And UserSms =1 Then  	
  	
        '-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.-.
        Dim OtherEquipmentMap As New List(Of DtoSC.Base_OtherEquipment)
        Dim OtherEquipmentMap2 As New List(Of DtoSC.Base_OtherEquipment)
        Dim insertOtherEquipmentMap As New DtoSC.Base_OtherEquipment
        Dim AreaAfterEditBarokaf As Double = Info8.GetBarokaf.GetBase_HouseInfo_Config.AreaAfterEdit
        Dim AreaAfterEditHouse As Double = Info8.GetRevisit_House.GetBase_HouseInfo.AreaAfterEdit
        Dim OtherEquipmentValueBarokaf As Double = 0.0
       ' msgBox("AreaAfterEditHouse" & AreaAfterEditHouse,"AreaAfterEditBarokaf" & AreaAfterEditBarokaf)
     
       ' مقادیر سایر امکانات ملک 
       '============================================================================================
        Dim OtherEquipmentRevisit As New List(Of DtoSC.Base_OtherEquipment)
        OtherEquipmentRevisit = Info8.GetRevisit_House.GetBase_OtherEquipment()
        

        Dim OtherEquipmentRevisit2  As New List(Of DtoSC.Base_OtherEquipment)
        OtherEquipmentRevisit2 = OtherEquipmentRevisit.where(Function(x) x.CI_OtherEquipmentType = 352).Tolist
       
        Dim ValueRevisit As String
        if OtherEquipmentRevisit2.count>0 Then 
        	ValueRevisit= OtherEquipmentRevisit2.First.OtherEquipmentValue 
        Else
        	ValueRevisit=0
        End if	
       

        '============================================================================================
        Dim ValueFinal As Double = 0
        OtherEquipmentMap =  Info8.GetBarokaf.GetBase_OtherEquipment()
         	

        OtherEquipmentMap2 = OtherEquipmentMap.where(Function(x) x.CI_OtherEquipmentType = 352).Tolist
      

        Dim OtherEquipmentValue As String ="1300/01/01"
        Dim NewOtherEquipmentValue As Double = 0 
        
      '  If  OtherEquipmentMap2.Any Then   	OtherEquipmentValue = OtherEquipmentMap2.First.OtherEquipmentValue 
       '     if  AreaAfterEditBarokaf <= 250 Then 
            	
         '   	OtherEquipmentValueBarokaf = 2000000
        '    ElseIf AreaAfterEditBarokaf <= 500 Then
       '     	
            	OtherEquipmentValueBarokaf = 4000000
      '      ElseIf AreaAfterEditBarokaf <= 1000 Then
            	
        '    	OtherEquipmentValueBarokaf = 6000000
        '    ElseIf AreaAfterEditBarokaf <= 5000 Then
            	
        '    	OtherEquipmentValueBarokaf = 10000000
        '    ElseIf AreaAfterEditBarokaf > 5000 Then
            	
         '   	OtherEquipmentValueBarokaf = 10000000 + (AreaAfterEditBarokaf - 5000) * 400
         '   End if
            IF AreaAfterEditBarokaf < 101
		        OtherEquipmentValueBarokaf = 2540000
		    ElseIf AreaAfterEditBarokaf > 100  And  AreaAfterEditBarokaf < 301 Then
		        OtherEquipmentValueBarokaf = 5080000
		    ElseIf AreaAfterEditBarokaf > 300 And  AreaAfterEditBarokaf < 501 Then
		        OtherEquipmentValueBarokaf = 7620000
		    ElseIf AreaAfterEditBarokaf > 500  And  AreaAfterEditBarokaf < 1001 Then
		        OtherEquipmentValueBarokaf = 10160000
		    ElseIf AreaAfterEditBarokaf > 1000
		
				Dim TmpInt AS Integer
				TmpInt=(AreaAfterEditBarokaf /1000)
				OtherEquipmentValueBarokaf = 10160000 + (TmpInt * (0.4*10160000))
			End if	
            ' NewOtherEquipmentValue = OtherEquipmentValueBarokaf
            ' ValueFinal = NewOtherEquipmentValue - ValueRevisit
            
            'if ValueFinal > 0 Then
            '	OtherEquipmentMap2.First.OtherEquipmentValue = ValueFinal
           ' Else
            '	OtherEquipmentMap2.First.OtherEquipmentValue = 0
           ' End if
             
       ' Else
         	 
        	insertOtherEquipmentMap.CI_OtherEquipmentType = 352
        	insertOtherEquipmentMap.CI_OtherEquipmentGroup=1
        '	if  AreaAfterEditBarokaf <= 250 Then 
          '  
          '  	OtherEquipmentValueBarokaf = 2000000
          '  ElseIf AreaAfterEditBarokaf <= 500 Then
        
          '  	OtherEquipmentValueBarokaf = 4000000
          '  ElseIf AreaAfterEditBarokaf <= 1000 Then
           
           ' 	OtherEquipmentValueBarokaf = 6000000	
          '  ElseIf AreaAfterEditBarokaf <= 5000 Then
            	
          '  	OtherEquipmentValueBarokaf = 10000000
          '  ElseIf AreaAfterEditBarokaf > 5000 Then
            
         '   	OtherEquipmentValueBarokaf = 10000000 + (AreaAfterEditBarokaf - 5000) * 400
         '  End if
           
         
             NewOtherEquipmentValue = OtherEquipmentValueBarokaf 
             ValueFinal = NewOtherEquipmentValue - 0 'ValueRevisit
          
             if ValueFinal > 0 Then
             	insertOtherEquipmentMap.OtherEquipmentValue = ValueFinal
             Else
             	insertOtherEquipmentMap.OtherEquipmentValue = 0
             End if
              
            insertOtherEquipmentMap.NidBase = Info8.GetBarokaf.GetBase_HouseInfo_Config.NidBase
           
           insertOtherEquipmentMap.comments = ""
             
            If Not OtherEquipmentMap2.Any Then  Info8.GetBarokaf.GetBase_OtherEquipment.Add(insertOtherEquipmentMap)
             
     Msgbox("a",8)      
       	
       ' End if
     '  sp 325041
'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",8)
    IF District = 1 Then
  
        NOffice = " دفترشهرسازان نام آشنا شرق خراسان-نقشه برداری "
        NMange = " علی صحافیان "
        Tel = " 9156452840 "
        NCard = " 6362-1411-0543-5961 "
    End if 

    IF District = 2  Then ' And  Region <> 1   And Region <> 2 Then
    
         NOffice = " دفتر توتال نقش آسیا- نقشه برداری "
         NMange = " محمد عرفانیان فعال یزد "
         Tel = " 9046578514 "
         NCard = " 5894-6311-2681-0530 "
    End if 
'sp 350340
   ' IF District = 2  And  (Region = 1  OR  Region = 2 ) Then
   '   NOffice = " دفتر تصویر نگاران فاتح شرق-نقشه برداری "
   '   NMange = " محمد هاشم انتظاری "
    '   Tel = " 9155093853 "
    '   NCard = " 6037-7011-5294-4382 "
   ' End  if

   IF District = 3 Then
    	if Region =1 or Region =2 or Region =3 or Region =5 or Region =6 or Region =7 or Region =8 or Region =27 or Region =25 or Region =20 or Region =21 or Region =26 Then
    		NOffice = " کیان طرح نقش جاوید "
			NMange =  "مجید زاهدی "
			Tel =  " 9306629380 "
			NCard = " 5041-7210-8621-8055 "
		Else	
		     NOffice = " دفترنگار شهر ارجمند "
		     NMange = " ابوذر محسنی ارجمند "
		     Tel = " 9330460266 "
		     NCard = " 5859-8311-1592-0307 "
		End if     
     End if

    

    IF District = 4 Then
     NOffice = " دفتر برداشت نگار پیمایش شرق-نقشه برداری "
     NMange = " آرمان واعظی "
     Tel = " 9150476613 "
     NCard = " 6362- 1411- 1151-3654 "
     End if

    IF District = 5  Then
     NOffice = " دفتر346 "
     NMange = " مهدی محمدپور "
     Tel = " 09044156571 "
     NCard = " 6037-6915-9637-2029 "
     End if

    IF District = 6 Then
     NOffice = " دفتر مسیر نقش آپادانا-نقشه برداری "
     NMange = " سید علیرضا سودمند "
     Tel = " 9151082320 "
     NCard = " 5859-8311-6680-7601 "
     End if

    IF District = 7
    
        NOffice = " شهرنگارسامان نقش پارسه-نقشه برداری "
        NMange = " علیرضا گلکار "
        Tel = " 9372752090 "
        NCard = " 5041-7210-9912-3961 "
     End if

    IF District = 8
    
        NOffice = " دفتر نقش آفرینان داوین-نقشه برداری "
        NMange = " افشین علی یاری شیروان "
        Tel = " 9335970175 "
        NCard = " 6037-6916-4957-2930 "
     End if

    IF District = 9
    
        NOffice = " دفتر آتی سازان آرادآروین -نقشه برداری "
        NMange = " رضا طوسی نیک "
        Tel = " 9306763574 "
        NCard = " 6362-1411-1159-3654 "
     End if

    IF District = 10
    	NOffice = " دفتر طرح مسیر پارت-نقشه برداری "
        NMange = " سید امیر غیاثی "
        Tel = " 9380198439 "
        NCard = " 5892-1010-9373-3794 "
     End if

    IF District = 11
    
        NOffice = " نقش آرا پیمایش شرق-نقشه برداری "
        NMange = " مهدی وظیفه دان "
        Tel = " 9933174531 "
        NCard = " 6362-1411-09745357 "
     End if

    IF District = 12
    
        NOffice = " دفتر باتاب آژند طوس-نقشه برداری "
        NMange = " سعید واحدی زاده "
    	Tel = " 9152378385 "
     	NCard = " 5029- 0810- 6302-0412 "
    End if
'Info8.AddError(BIZ.SA.EumErrorAction.warning, "1", 7)
    'Info8.AddError(BIZ.SA.EumErrorAction.warning, " a",9)
      Dim strValueFinal as String=""
     strValueFinal= ValueFinal.tostring
        if Not  OtherEquipmentMap2.Any Then 
     ' sp 325041  'if OtherEquipmentValue< Info8.getRequest.Info.RequestDate Then  
       ' sp 325041 if (OtherEquipmentValue <> NewOtherEquipmentValue) And (ValueFinal > 100) Then
         
               	Dim tmpSMS As New BIZ.SC.SrvSMSService.ClsParameters
				tmpSMS.BizCode = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
				tmpSMS.Number = Info8.GetRequest.Info.Cellphone
				tmpSMS.ScheduleSendDate =BIZ.SA.ClsCommon.CurrentShamsiDateString
				p_RequestNo = Info8.GetRequest.Info.NidWorkItem
				tmpSMS.SMSType = "نقشه برداری - بروکف"
				tmpSMS.Text =  "شهروند گرامی درخواست شماره  " & ReverseString(Info8.GetRequest.Info.NidWorkItem.tostring) & " به شرکت نقشه برداری " & _
				NOffice & " ارسال گردید و نهایتا 72 ساعت پس از واریز وجه اقدام خواهد شد . هزینه خدمات نقشه برداری به مبلغ" & strValueFinal & _
				" ریال به شماره کارت "  & ReverseString(NCard) & " بنام  " & NMange & " واریز و به شماره تلفن " & Tel & " اطلاع دهید. درصورت تقاضای مبلغ مازاداز جانب نقشه بردار، شکایت خود را به شماره 05132244123اعلام فرمایید . " & _ 
				"باتشکر، شهرداری مشهد "   
								
	            tmpSMS.UserID = Info8.User.UserGuid.ToString
				tmpSMS.UserName = Info8.User.UserName
				Dim smsErrorResult As New BIZ.SA.ClsErrorResult
			'problemsms	smsErrorResult = Info8.GetFunctions.SendSMS(tmpSMS)
            End if

  End if












Msgbox("a",9)      
' نحوه اقدام املاک واقع در بستر
'sp 410298
if Sara10=True Then 
  Dim Lst As New List(Of DtoSC.base_FrontAge)  
  Dim NidRivisit As String=""
      NidRivisit=Info8.FindParameter("NidRevisit")
      if String.IsNullOrEmpty(NidRivisit)=False Then
		 Lst=Info8.GetBarokaf.Base_FrontAge
		 if Lst.count>0 Then Lst = Lst.Where(Function(f) f.CI_Frontage = 15).ToList  
		 if Lst.count>0 Then Info8.GetBarokaf.GetBase_HouseInfo_Config.AreaAfterEdit=0
      End if
End if
'
'
    
   ' Info8.AddError(BIZ.SA.EumErrorAction.Stop, "فرمول بروکف h","S")
    
  Call CopyBaroKaf()  
    
    
    Catch ex As Exception
        Info8.AddError(BIZ.SA.EumErrorAction.Stop, "فرمول بروکف", ex.Message)
    End Try

End Function


Public Function Checkforbazdid( ByVal NidCheckList as String, ByVal Nidproc As String ) as Boolean 
 Dim result as Integer
 result= 0
  Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
  LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Nidproc.tostring)
  Dim i as Integer
  if LstObj.data.count>0 Then 
 
     For i = 0 To LstObj.data.count - 1
		Try
		
		  if ucase(LstObj.data(i).NidCheckList)= ucase(NidCheckList)    Then
			   		result= 1
			   		Dim OwnerConfirm as Integer = 0
			   		Try
				        OwnerConfirm= LstObj.data(i).OwnerConfirm
					Catch
						OwnerConfirm= 0
					End Try
					
			   		if OwnerConfirm =0 And LstObj.data(i).CI_CheckListValue=2 Then Return True
			   		if OwnerConfirm =1 Then Return True
			   		
			Else
				if result<>1 Then  result=2 
		    End if  
		          
	    Catch
		End Try        
	 Next
	
 End If
 

Return False	
End Function

Public Function CheckEditSara10(ByVal NidCheckList As String,Nidproc As String) As Boolean 
                 
   'sampa 358064
	Dim LstObj As New Global.Biz.SC.ClsNewWorkFlow.ClsGetCheckListByNidProcResponse
'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "*@@", "@@yyy") : Exit Function

'logfilefj("T1","TT")
    LstObj = Info8.GetFunctions.WF_GetCheckListByNidProc(Nidproc)
'    logfilefj("T2",LstObj.count.tostring)
  
        
	if LstObj.data.count>0 Then 
		      
		For i As Integer = 0 To LstObj.data.count - 1
				
					Try
					  

			               		If 	Ucase(LstObj.data(i).NidCheckList) = Ucase(NidCheckList) Then
			               	        
					                 If (LstObj.data(i).CI_CheckListValue) = 1  Then
	
											Return True
									 Else
									
											Return False	
					                 End If
				      
			                  End if  
			          
			          Catch
			
				      End Try        
		Next
	   
	End If

	     
End Function

Private Function CopyBaroKaf ()
		Dim str as String ="Base_OtherEquipment,Base_Edge,Base_AddressInfo,Base_CommonEstate,Base_HouseInfo,Sh_BaroKaf,Base_RegisterPlack,Base_Owner"
		Dim LstParent As New List(Of Global.Biz.SA.ClsNosaziCodeType)
       	Dim NidBaseFrom As Guid
       	Dim NidBaseTo As Guid
       	Dim NidNosaziCode As Guid
        
        LstParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
        LstParent = LstParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
		NidNosaziCode=LstParent(0).NidNosaziCode
		
		NidBaseFrom = Info8.GetFunctions.GetNidBase(NidNosaziCode,1,0,Info8.GetRequest.Info.NidRevisit)
		NidBaseTo =   Info8.GetFunctions.GetNidBase(NidNosaziCode,0,0,Guid.Parse("00000000-0000-0000-0000-000000000000"))
	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NidBaseFrom",NidBaseFrom.tostring)
	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,"NidBaseTo",NidBaseTo.tostring)
		Info8.GetFunctions.CopyBase(NidBaseFrom, NidBaseTo, NidNosaziCode, CByte(BIZ.SA.ClsEnum.EumBaseInfoGroup.Parvandeh), Info8.GetRequest.Info.NidRevisit, CByte(BIZ.SA.ClsEnum.EumRevisitGroup.Revisit),str)
End Function		
Private Function GetArchiveDWG ()  
	
	Dim district as String 

	Dim tmpDistrict() as String
	Dim tmpNosaziNickName As String = Info8.GetFunctions.GetNosaziNickName(Info8.GetRequest.Info.NidNosaziCode)
	if tmpNosaziNickName<>"" Then 
		tmpDistrict = tmpNosaziNickName.Split("-")
		district=tmpDistrict(0)
	End if	
	
	Dim str as biz.SA.ClsNosaziCodeType
	str=Info8.GetFunctions.GetNosaziCode_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0, 0, Guid.Parse("00000000-0000-0000-0000-000000000000"))
	Dim	M_ObjectType as Integer
	M_ObjectType =CInt(Str.EumNosaziCodeObjType)
	
	Dim ArcWrrap As List(Of Global.DtoSC.Archive_Wrapper)
	Dim Nidnosazimelak as guid
	If M_ObjectType = 4  or M_ObjectType = 6 or M_ObjectType = 2 or M_ObjectType = 5 Then
	   	Nidnosazimelak=Info8.GetRequest.Info.NidNosaziCode
	Else
	  	Dim ListParent As New List(Of BIZ.SA.ClsNosaziCodeType)
		ListParent = Info8.GetFunctions.GetParvandeh_Parents_InNidNosaziCode(Info8.GetRequest.Info.NidNosaziCode, 0)
		ListParent = ListParent.Where(Function(f) f.EumNosaziCodeObjType = 2).ToList
		Nidnosazimelak=ListParent(0).NidNosaziCode 
	End if 
	ArcWrrap = Info8.GetFunctions.GetArchiveBizCode(Nidnosazimelak)
	Dim TmpUrlFromBizCodeResult As New List(Of Global.Biz.SC.SrvArchive.SrvGetUrlFromBizCodeResult)
	Dim LstArchive As New List(Of Global.BIZ.SC.SrvArchive.ClsImageItem)
		
	if ArcWrrap.count >0 Then  
	  	ArcWrrap=ArcWrrap.where (Function (f) f.CI_ArchiveGroup =1 ).tolist ()
		if ArcWrrap.count >0 Then
		
			Dim BizCode as String  = ArcWrrap.min(Function (f) f.BizCode).tostring 
		
		' 	Dim Workspace_Name = "sara8"
		' 	Dim Workspace_Pass = "ark@2993"
		' 	Dim ArchiveCoreURL = "http://shahr1.mashhad.ir:8073/archive/"
			Dim Workspace_Name = "sara7"
		 	Dim Workspace_Pass = "ark@2993"
		 	Dim ArchiveCoreURL = "http://sarc10.mashhad.ir:8001/"
		 	 
		 	Dim ArchiveProxy As New Global.ArchiveCoreLib.ClsArchiveProxy(ArchiveCoreURL, Workspace_Name, Workspace_Pass)
		 	Dim result_login = ArchiveProxy.Login()
		    Dim ii as Integer =0 
		   
	 		If result_login Then
	 			Dim Response As New Global.ArchiveCoreLib.ClsMessage()
	 	
         		Response = ArchiveProxy.GetFiles(BizCode)
         	'	Info8.AddError(BIZ.SA.EumErrorAction.Stop,  "@@@@", Response.Files.Count.tostring) 
	 			'	TotalS= "S" &  Response.Files.Count.tostring
		' Return True
	 				
			  ' For ii=0 to  Response.Files.Count-1 
			    'TotalS=TotalS & "{}" & ii & " :E: " &  Response.Files(ii).EntityTitle.tostring
	              
	          '  Next
	  		End If 
	 
	 
			
		
		'A	Dim tmpSerchArchive As New Global.Biz.SC.SrvArchive.ClsSearchDataFile
		 'A   Dim tmpSerchArchiveOut As New  Global.Biz.SC.SrvArchive.ClsSearchDataFile
		'A    Dim TmpG As New List(Of Guid) 
		'A    TmpG.Add(Guid.Parse("540D119D-14F1-4F65-94D1-7F39FC2B3B48"))
	'A		tmpSerchArchive.BizCode =  ArcWrrap.min(Function (f) f.BizCode) 'ArcWrrap(0).BizCode.tostring
		'A	tmpSerchArchive.DomainName = district
	'A		tmpSerchArchive.AllGuids = TmpG
	'A		tmpSerchArchive.UserID ="540D119D-14F1-4F65-94D1-7F39FC2B3B48"
	'A		tmpSerchArchive.UserName ="tofighnezhad"
	'A	
	'A		tmpSerchArchiveOut = Info8.GetFunctions.GetArchiveDataFileByBizCode(tmpSerchArchive, district)		
			
	'A		TmpUrlFromBizCodeResult = tmpSerchArchiveOut.Result.GetFile
	'A		
	'A		  For 	ii=0 to TmpUrlFromBizCodeResult.count-1 
	'A		  	 TotalS=TotalS & "{}" & ii & " :E: " & TmpUrlFromBizCodeResult(ii).SerialNo.tostring
	       ' TmpUrlFromBizCodeResult = TmpUrlFromBizCodeResult.Where(Function(f) (f.NidEntity.tostring = "1" )And f.isdeleted=0 ).ToList
	 'A       Next	
	  
'	Return True
'	if Not TmpUrlFromBizCodeResult.any() Then Return True
		 End if 
	 End if 
'	 Return True
End Function

Public Function ControlTask(ByVal NIdNod as String,Optional ByVal Control as Integer=0) as Integer

		Dim TaskByNidProcResponse As New Global.Biz.SC.ClsNewWorkFlow.ClsGetTaskByNidProcResponse
		Dim strClose as String =""
	
		TaskByNidProcResponse=Info8.GetFunctions.WF_GetTaskByNidProc(Info8.GetRequest.Info.NidProc.tostring)
		Dim counter as Integer=0
		Dim Colescounter as Integer=0
		Dim Opencounter as Integer=0
		Dim ik as Integer=0
		For ik = 0 To TaskByNidProcResponse.data.Count - 1
		    If TaskByNidProcResponse.data(ik).NidNode = NIdNod Then Opencounter+=1	     
		Next
		
		For ik = 0 To TaskByNidProcResponse.data.Count - 1
		    If TaskByNidProcResponse.data(ik).NidNode = NIdNod Then
		    	if {1,3,5,6}.contains(TaskByNidProcResponse.data(ik).EumTaskStatus) Then Colescounter+=1    
		    End if	
		     
		Next
		
		if Colescounter=Opencounter And Opencounter =0 Then counter=0  'وجود ندارد
		if Colescounter<Opencounter Then  counter=1 ' وجود دارد
		if Colescounter=Opencounter And Opencounter <>0  Then  counter=2 ' بسته شده 
		if Control=1 And  Colescounter>0 Then   counter=3 ' بسته شده  حداقل برای یک بار    
		        ''''''''''''''''''''''''''''''''''''''
		 	
		 Return counter
	
End Function
		
Function ReverseString(input As String) As String
    Dim parts As String() = input.Split("-"c)
    Array.Reverse(parts)
    Return String.Join("-", parts)
End Function