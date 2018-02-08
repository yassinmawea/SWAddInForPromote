import java.io.StringReader;
import java.util.*;
import java.util.Properties;
import java.lang.*;
import java.io.ByteArrayInputStream;
import java.sql.Timestamp;
import java.text.DecimalFormat;

import matrix.db.*;
import matrix.util.*;
import matrix.util.StringList;

import com.matrixone.apps.domain.*;
import com.matrixone.apps.domain.util.*;
import com.matrixone.apps.common.*;
import com.matrixone.apps.domain.util.MapList;
import com.matrixone.apps.framework.ui.UIUtil;

 public class ${CLASSNAME}
 {
      /**
       * Constructor.
       *
       * @param context the eMatrix <code>Context</code> object.
       * @param args holds no arguments.
       * @throws Exception if the operation fails.
       * @since EC 9.5.JCI.0.
       */

	public final String ATTR_CADTYPE = PropertyUtil.getSchemaProperty("attribute_CADType");   
	public final String ATTR_ACCESSTYPE = PropertyUtil.getSchemaProperty("attribute_AccessType");
	public final String ATTR_DESIGNATEDUSER = PropertyUtil.getSchemaProperty("attribute_DesignatedUser");
	public final String ATTR_LANGUAGE = PropertyUtil.getSchemaProperty("attribute_Language");   

	public ${CLASSNAME} (Context context, String[] args)
	  throws Exception
	{

	}
	
	public void updateINVRevisionValue(Context context, String [] args) throws Exception
	{
		int var = 0;
		
		System.out.println("Coming inside updateINVRevisionValue");
		String sObjectId = args[0];
		String strName = args[1];
		String strRevision = args[2];
		String strType = args[3];
		String strOps = args[4];
		
		DomainObject doObject;
		Map mRelatedInstance;
		MapList mlRelatedInstances;
		
		StringList slObjectList = new StringList();
		slObjectList.add("id");
		slObjectList.add("type");
		slObjectList.add("revision");
		
		if (sObjectId.equals(""))
		{
			BusinessObject bo = new BusinessObject(strType, strName, strRevision, "eService Production");
			doObject = new DomainObject(bo);
		} else{
			doObject = new DomainObject(sObjectId);
		}
		

		if (strType.equals("SW Drawing"))
		{			
			System.out.println("strType --> " + strType);
			if (strOps.equals("Checkin")) {
				System.out.println("strOps --> " + strOps);
				Map mActiveVersion = doObject.getRelatedObject(context,"Active Version", true, slObjectList, null);
				String sRevisionActiveVersion = (String) mActiveVersion.get("revision");
				System.out.println("sRevisionActiveVersion --> " + sRevisionActiveVersion);
				doObject.setAttributeValue(context, "INV_Revision", sRevisionActiveVersion);
			} else if (strOps.equals("Release")){
				System.out.println("strOps --> " + strOps);
				String sRevisionInstanceOf = (String) doObject.getInfo(context, "revision");
				System.out.println("sRevisionInstanceOf --> " + sRevisionInstanceOf);
				doObject.setAttributeValue(context, "INV_Revision", strRevision);
			}	 
		}
		else
		{
			System.out.println("strType --> " + strType);
			mlRelatedInstances = doObject.getRelatedObjects(context, "Instance Of","*", slObjectList, null, false, true, (short)1, "", null);
			
			for (int i=0; i < mlRelatedInstances.size(); i++)
			{
				mRelatedInstance = (Map) mlRelatedInstances.get(i);
				String sObjectIdInstanceOf = (String) mRelatedInstance.get("id");
				System.out.println("sObjectIdInstanceOf --> " + sObjectIdInstanceOf);
				DomainObject doInstanceOf = new DomainObject(sObjectIdInstanceOf);
				if (strOps.equals("Checkin")) {					
					Map mActiveVersion = doInstanceOf.getRelatedObject(context,"Active Version", true, slObjectList, null);
					String sRevisionActiveVersion = (String) mActiveVersion.get("revision");
					System.out.println("sRevisionActiveVersion --> " + sRevisionActiveVersion);
					doInstanceOf.setAttributeValue(context, "INV_Revision", sRevisionActiveVersion);
				} else if (strOps.equals("Release")){
					String sRevisionInstanceOf = (String) mRelatedInstance.get("revision");
					System.out.println("sRevisionInstanceOf --> " + sRevisionInstanceOf);
					doInstanceOf.setAttributeValue(context, "INV_Revision", sRevisionInstanceOf);
				}
			}
		}
		//return var;
	}	
	
	public void createConnectDerivedOutput(Context context, String[] args) throws Exception
    {
		try
		{
			System.out.println("Coming inside createConnectDerivedOutput");
			String sPartNo = args[0];
			String sRev = args[1];
			
			BusinessObject bo = new BusinessObject("SW Drawing", sPartNo, sRev, "eService Production");
			
			//String sObjectId = args[0];
			//String strName = args[1];
			//String sRevision = "";
			String sRel_LatestVersion = PropertyUtil.getSchemaProperty("relationship_LatestVersion");

		
			DomainObject doObject = new DomainObject(bo);
			String sObjectId = doObject.getId(context);
			System.out.println("Coming inside sObjectId"+sObjectId);
			//DomainObject doObject = new DomainObject(sObjectId);
			String sRevision = doObject.getInfo(context, "from[Active Version].to.revision");
			String sFileName = doObject.getAttributeValue(context,"Title");
			String sProject = doObject.getInfo(context,"project");
			String sOrganization = doObject.getInfo(context,"organization");
			String strDerivedOutputId	= "";

			StringList slObjectList = new StringList();
			slObjectList.add("id");
			slObjectList.add("name");
			//derived output exists or not
			MapList mlRelatedComponentObjects = doObject.getRelatedObjects(context,
												"Derived Output",  // relationship pattern
												"Derived Output",       	// object pattern
												slObjectList,   	// object selects
												null,              		// relationship selects
												true,          		// to direction
												true,           		// from direction
												(short)1,   		// recursion level
												"name ~~'PDF*'",     				// object where clause
												null);
			System.out.println("mlRelatedComponentObjects"+mlRelatedComponentObjects);									
			if(mlRelatedComponentObjects.size()>0)
			{
				Map oMap = (Map)mlRelatedComponentObjects.get(0);
				strDerivedOutputId	= (String)oMap.get("id");
				System.out.println("strDerivedOutputId"+strDerivedOutputId);
			}
			System.out.println("Coming inside sFileName"+sFileName);
			sFileName = (sFileName.replace(".SLDDRW","")).trim();
			//System.out.println("Coming inside sFileName"+sFileName);
			String sVersionSWOid = "";
			String sDerivedOutputId = "";
			String sFilePath = findConfigObjectData(context, "DerivedOutputDirectory");
			//System.out.println("Coming inside sFilePath"+sFilePath);
			//System.out.println("Coming inside sFileName"+sFileName);
			//sFileName = "0409CompTest3";
			sFileName = (sFileName.replace(".SLDDRW","")).trim();
			//System.out.println("Coming inside sFileName"+sFileName);
			java.io.File dataFile = new java.io.File(sFilePath+sFileName+".PDF");
			if (dataFile.exists())
			{
				if(UIUtil.isNullOrEmpty(strDerivedOutputId))
				{
			//System.out.println("Coming inside create file exist PDF ==");
				sDerivedOutputId  = createDerivedOutput(context, sRevision, sFileName, sFilePath, sProject, sOrganization, "pdf", "PDF");
				
				sVersionSWOid = doObject.getInfo(context,"relationship["+sRel_LatestVersion+"].to.id");
				
			//System.out.println("Coming inside sVersionSWOid"+sVersionSWOid);
				connectDerivedOutput(context, sPartNo, sObjectId, sDerivedOutputId, sVersionSWOid);
				}
				else
				{
					sDerivedOutputId = strDerivedOutputId;
					DomainObject doDerivedOutput = new DomainObject(sDerivedOutputId);
					doDerivedOutput.checkinFile(context, true, true, "", "PDF", sFileName+"."+"PDF", sFilePath);
				}
				dataFile.delete();
				System.out.println("sDerivedOutput--> " + sDerivedOutputId);
				System.out.println("Coming inside sDerivedOutputId"+sDerivedOutputId);
			}
			
			dataFile = new java.io.File(sFilePath+sFileName+".DXF");
			if (dataFile.exists())
			{
			//System.out.println("Coming inside create file exist DXF ==");
				sDerivedOutputId  = createDerivedOutput(context, sRevision, sFileName, sFilePath, sProject, sOrganization, "dxf", "DXF");
				System.out.println("sDerivedOutput--> " + sDerivedOutputId);
				dataFile.delete();
				sVersionSWOid = doObject.getInfo(context,"relationship["+sRel_LatestVersion+"].to.id");
				connectDerivedOutput(context, sPartNo, sObjectId, sDerivedOutputId, sVersionSWOid);
			}
		}
		catch(Exception e)
		{
			System.out.println("Exeption==="+e);
		}
	}
	
	public String createDerivedOutput(Context context, String sRevision, String sFileName, String sFilePath, String sProject, String sOrganization, String CADType, String CADFormat)
	{
	System.out.println("Coming inside ==1");
		String sType_DerivedOutput = PropertyUtil.getSchemaProperty(context, "type_DerivedOutput");
		String sPolicy_DerivedOutputTEAMPolicy = PropertyUtil.getSchemaProperty(context, "policy_DerivedOutputTEAMPolicy");
		//System.out.println("Coming inside ==11");
		HashMap mCustomAttributes = new HashMap();
		mCustomAttributes.put(ATTR_CADTYPE, CADType);
		mCustomAttributes.put(ATTR_ACCESSTYPE, "Inherited");
		mCustomAttributes.put(ATTR_DESIGNATEDUSER, "Unassigned");
		mCustomAttributes.put(ATTR_LANGUAGE, "English");
		//System.out.println("Coming inside ==111");
		try {
		System.out.println("Coming inside ==2");
		Timestamp timestamp = new Timestamp(System.currentTimeMillis());
		//String sName = CADFormat+"-"+timestamp;
		String sName = CADFormat+"-"+Long.toHexString(timestamp.getTime()).toString().toUpperCase();
		// Create DerivedOutput
		DomainObject doDerivedOutput = new DomainObject();
		System.out.println("Derived Output Name == " + sName);
		doDerivedOutput.createObject(context, sType_DerivedOutput, sName, sRevision, sPolicy_DerivedOutputTEAMPolicy, "eService Production");
		//System.out.println("Coming inside ==4");
		// Update Collabspace details
		doDerivedOutput.setPrimaryOwnership(context, sProject, sOrganization);
		//System.out.println("Coming inside ==5");
		// Checkin File
		doDerivedOutput.checkinFile(context, true, true, "", CADFormat, sFileName+"."+CADFormat, sFilePath);
		//System.out.println("Coming inside ==6");
		// Update Attributes
		doDerivedOutput.setAttributeValues(context, mCustomAttributes);
		//System.out.println("Coming inside ==7");
		// Update Owner
		doDerivedOutput.setOwner(context, "Corporate");
		//System.out.println("Coming inside ==8"+doDerivedOutput.getInfo(context, "id").toString());
		
		return doDerivedOutput.getInfo(context, "id").toString();
		}
		catch(Exception e)
		{
			//System.out.println("Eception"+e);
			return e.toString();
		}
	}
	
	public void connectDerivedOutput(Context context, String strName, String sObjectId, String sDerivedOutputId, String sVersionSWOid)
	{
		String sRel_DerivedOutput = PropertyUtil.getSchemaProperty(context, "relationship_DerivedOutput");
        Map mRelationshipAttributes = new HashMap();
        mRelationshipAttributes.put("CAD Object Name", strName);
		
		try{
			
			DomainObject doObject = new DomainObject(sObjectId);
			DomainObject doDerivedOutput = new DomainObject(sDerivedOutputId);
			DomainObject doVersionObject = new DomainObject(sVersionSWOid);
			DomainRelationship doRelDerivedOutput = null;
			
			// Define Connection
			doRelDerivedOutput = DomainRelationship.connect(context, doObject, sRel_DerivedOutput, doDerivedOutput);
			// Update rel attributes
			doRelDerivedOutput.setAttributeValues(context, mRelationshipAttributes);
			doRelDerivedOutput = DomainRelationship.connect(context, doVersionObject, sRel_DerivedOutput, doDerivedOutput);
			// Update rel attributes
			doRelDerivedOutput.setAttributeValues(context, mRelationshipAttributes);
		}
		catch(Exception e)
		{
			System.out.println("Eception:::"+e);
		}
	}
	
	public static String findConfigObjectData(Context context, String objectName)throws Exception
	{
		StringList slObjectSelects = new StringList();
		slObjectSelects.add(DomainConstants.SELECT_ID);
		
		MapList mlObject = DomainObject.findObjects(context, "INV_Configuration", objectName, "-", "*", "*", null, false, slObjectSelects);
		String strObjectValue = "";
		if(mlObject.size() >= 1)
		{
			Map oMap = (Map)mlObject.get(0);
			String strId = (String)oMap.get(DomainConstants.SELECT_ID);
			DomainObject doObject = new DomainObject(strId);
			strObjectValue = doObject.getAttributeValue(context,"INV_ConfigurationData");
			////System.out.println("strObjectValue==="+strObjectValue);
		}
		return strObjectValue;
	}
	
}
  