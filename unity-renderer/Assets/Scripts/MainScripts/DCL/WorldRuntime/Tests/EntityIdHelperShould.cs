using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;

public class EntityIdHelperShould
{
    private EntityIdHelper helper;
    
    [SetUp]
    public void Setup()
    {
        helper = new EntityIdHelper();
    }
    
    [Test]
    public void ConvertLegacyEntityIdsCorrectly()
    {
        // Arrange
        string legacyEntityId = "Eed";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result,264704);
        Assert.AreEqual(result,helper.DecodeBase36(legacyEntityId) << 9);
    }
    
    [Test]
    public void ReserveThe512FirstEntitiesCorrectly()
    {
        // Arrange
        string legacyEntityId = "Eed";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreNotEqual(result,-779);
        Assert.AreNotEqual(result,helper.DecodeBase36(legacyEntityId));
    }
    
    [Test]
    public void ConvertNotBase36EntityIdsCorrectly()
    {
        // Arrange
        string legacyEntityId = "EFDE";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result,-1);
    }
    
    [Test]
    public void ConvertNotBase36EntityIdsOnlyOneTime()
    {
        // Arrange
        string legacyEntityId = "EFDE";
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Act
        long result2 = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result,result2);
    }
    
    [Test]
    public void ConvertSeveralNotBase36EntityIdsCorrectly()
    {
        // Arrange
        string legacyEntityId = "EFDE";
        string legacyEntityId2 = "EFDEFG";
        string legacyEntityId3 = "EFDEEE";
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Act
        result = helper.EntityFromLegacyEntityString(legacyEntityId);
        long result2 = helper.EntityFromLegacyEntityString(legacyEntityId2);
        long result3 = helper.EntityFromLegacyEntityString(legacyEntityId3);
        
        // Assert
        Assert.AreEqual(-1,result);
        Assert.AreEqual(-2,result2);
        Assert.AreEqual(-3,result3);
    }
    
    [Test]
    public void EntityLegacyGiveRootCorrectly()
    {
        // Arrange
        string legacyEntityId = "0";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result, (long) SpecialEntityId.SCENE_ROOT_ENTITY);
    }
    
    [Test]
    public void EntityLegacyGiveThirdPersonCameraEntityCorrectly()
    {
        // Arrange
        string legacyEntityId = "PlayerEntityReference";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result, (long) SpecialEntityId.THIRD_PERSON_CAMERA_ENTITY_REFERENCE);
    }
        
    [Test]
    public void EntityLegacyGiveFirstPersonCameraEntityCorrectly()
    {
        // Arrange
        string legacyEntityId = "FirstPersonCameraEntityReference";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result, (long) SpecialEntityId.FIRST_PERSON_CAMERA_ENTITY_REFERENCE);
    }
    
    [Test]
    public void EntityLegacyGiveAvatarPositionEntityCorrectly()
    {
        // Arrange
        string legacyEntityId = "AvatarPositionEntityReference";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result, (long) SpecialEntityId.AVATAR_POSITION_REFERENCE);
    }
    
    [Test]
    public void EntityLegacyGiveAvatarEntityCorrectly()
    {
        // Arrange
        string legacyEntityId = "AvatarEntityReference";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result, (long) SpecialEntityId.AVATAR_ENTITY_REFERENCE);
    }
    
    [Test]
    public void TestUntilWhatId()
    {
        EntityIdHelper entityIdHelper = new EntityIdHelper();
        string lastEntityId = null;
        int numberOfEntities = 0;
        try
        {
            while(true)
            {
                numberOfEntities++;
                lastEntityId = entityIdHelper.ToBase36(numberOfEntities, 36);
                entityIdHelper.EntityFromLegacyEntityString(lastEntityId);
            }
        }
        catch(Exception e)
        {
            Debug.Log("Found! " + numberOfEntities + " lastENtityId "+ lastEntityId);
        }
    }
}
