﻿using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IUserProfileBridge
{
    UniTask<UserProfile> SaveVerifiedName(string name, CancellationToken cancellationToken);
    UniTask<UserProfile> SaveUnverifiedName(string name, CancellationToken cancellationToken);
    UniTask<UserProfile> SaveDescription(string description, CancellationToken cancellationToken);
    UniTask<UserProfile> SaveAdditionalInfo(string country, string gender, string pronouns, string relationshipStatus,
        string sexualOrientation, string language, string profession, DateTime? birthDate,
        string realName, string hobbies, string employmentStatus, CancellationToken cancellationToken);
    void RequestFullUserProfile(string userId);
    UniTask<UserProfile> RequestFullUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    UserProfile GetOwn();
    UserProfile Get(string userId);
    UserProfile GetByName(string userName, bool caseSensitive = true);
    void SignUp();
    void SendSaveAvatar(AvatarModel avatar, Texture2D face256Snapshot, Texture2D bodySnapshot, bool isSignUpFlow = false);
    UniTask<UserProfile> SaveLinks(List<UserProfileModel.Link> links, CancellationToken cancellationToken);
    void LogOut();
}
