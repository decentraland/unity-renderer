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
    public void ReturnTheSameEntityIdsCorrectly()
    {
        // Arrange
        string legacyEntityId = "E1";
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);

        // Act
        long result2 = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreEqual(result,result2);
        Assert.AreEqual(result,2 << 9);
    }
    
    [Test]
    public void ReserveThe512FirstEntitiesCorrectly()
    {
        // Arrange
        string legacyEntityId = "E0";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Assert
        Assert.AreNotEqual(result, 0);
        Assert.AreEqual(result,512);
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
    public void ConvertSeveralNotBase36EntityIdsConsecutevylyCorrectly()
    {
        // Arrange
        string legacyEntityId = "GEFDE";
        string legacyEntityId2 = "GEFDEFG";
        string legacyEntityId3 = "GEFDEEE";
        string legacyEntityId4 = "noBase36-d|";
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        
        // Act
        result = helper.EntityFromLegacyEntityString(legacyEntityId);
        long result2 = helper.EntityFromLegacyEntityString(legacyEntityId2);
        long result3 = helper.EntityFromLegacyEntityString(legacyEntityId3);
        
        // Assert
        Assert.AreEqual( 2147483648,result);
        Assert.AreEqual( 2147483649,result2);
        Assert.AreEqual( 2147483650,result3);
        Assert.AreNotEqual(result,result2);
    }

    [Test]
    public void ConvertTwoNotBased36EntitiesConsecutevily()
    {
        // Arrange
        string legacyEntityId = "1";
        string legacyEntityId2 = "2";
        
        // Act
        long result = helper.EntityFromLegacyEntityString(legacyEntityId);
        long result2 = helper.EntityFromLegacyEntityString(legacyEntityId2);
        
        // Assert
        Assert.AreEqual(result, 0x80000000);
        Assert.AreEqual(result2, 0x80000001);
    }
    
    [TestCase("1",ExpectedResult =  0x80000000)]
    [TestCase("2",ExpectedResult =  0x80000000)]
    [TestCase("E0",ExpectedResult =  1 << 9)]
    [TestCase("E1",ExpectedResult =  2 << 9)]
    [TestCase("Eed",ExpectedResult =  518 << 9)]
    [TestCase("Eb",ExpectedResult =  12 << 9)]
    [TestCase("E33",ExpectedResult =  112 << 9)]
    [TestCase("E444",ExpectedResult =  5333 << 9)]
    [TestCase("Euv",ExpectedResult =  1112 << 9)]
    [TestCase("E8kn",ExpectedResult =  11112 << 9)]
    [TestCase("E2dqf",ExpectedResult =  111112 << 9)]
    [TestCase("Ea",ExpectedResult =  11 << 9)]
    [TestCase("Eaa",ExpectedResult =  371 << 9)]
    [TestCase("Eaaa",ExpectedResult =  13331 << 9)]
    [TestCase("Eijealm",ExpectedResult =  0x85A1505600)]
    public long ConvertSeveralEntitysIdsCorrectly(string input)
    {
        // Act
        long result = helper.EntityFromLegacyEntityString(input);

        // Assert
        return result;
    }
    
    [TestCase("0")]
    [TestCase("AvatarEntityReference")]
    [TestCase("AvatarPositionEntityReference")]
    [TestCase("PlayerEntityReference")]
    [TestCase("FirstPersonCameraEntityReference")]
    [TestCase("1")]
    [TestCase("2")]
    [TestCase("E0")]
    [TestCase("E1")]
    [TestCase("Eed")]
    [TestCase("Eb")]
    [TestCase("E33")]
    [TestCase("E444")]
    [TestCase("Euv")]
    [TestCase("E8kn")]
    [TestCase("E2dqf")]
    [TestCase("Ea")]
    [TestCase("Eaa")]
    [TestCase("Eaaa")]
    [TestCase("Eijealm")]
    public void GetOriginalIdInAllCasesCorrectly(string input)
    {
        // Arrange
        long result = helper.EntityFromLegacyEntityString(input);
        
        // Act
        string entityId = helper.GetOriginalId(result);

        // Assert
        Assert.AreEqual(entityId,input);
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
}
