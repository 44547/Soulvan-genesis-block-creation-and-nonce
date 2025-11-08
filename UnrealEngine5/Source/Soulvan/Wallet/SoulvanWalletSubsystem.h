// Copyright Soulvan 2025. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Subsystems/GameInstanceSubsystem.h"
#include "SoulvanWalletSubsystem.generated.h"

UENUM(BlueprintType)
enum class ENftType : uint8
{
	CarSkin,
	Relic,
	ReplayToken,
	SeasonalBadge,
	BossTrophy
};

UENUM(BlueprintType)
enum class EProposalState : uint8
{
	Pending,
	Active,
	Canceled,
	Defeated,
	Succeeded,
	Queued,
	Expired,
	Executed
};

USTRUCT(BlueprintType)
struct FBalanceState
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite)
	float SoulvanCoin = 0.0f;

	UPROPERTY(BlueprintReadWrite)
	float Eth = 0.0f;

	UPROPERTY(BlueprintReadWrite)
	int32 NftCount = 0;

	UPROPERTY(BlueprintReadWrite)
	int32 BadgeCount = 0;

	UPROPERTY(BlueprintReadWrite)
	int32 VotingPower = 0;
};

USTRUCT(BlueprintType)
struct FNftData
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite)
	FString TokenId;

	UPROPERTY(BlueprintReadWrite)
	FString Name;

	UPROPERTY(BlueprintReadWrite)
	FString Description;

	UPROPERTY(BlueprintReadWrite)
	FString ImageUri;

	UPROPERTY(BlueprintReadWrite)
	FString Rarity;

	UPROPERTY(BlueprintReadWrite)
	int32 SeasonChapter = 0;

	UPROPERTY(BlueprintReadWrite)
	ENftType Type;
};

USTRUCT(BlueprintType)
struct FProposalData
{
	GENERATED_BODY()

	UPROPERTY(BlueprintReadWrite)
	FString Id;

	UPROPERTY(BlueprintReadWrite)
	FString Description;

	UPROPERTY(BlueprintReadWrite)
	int32 ForVotes = 0;

	UPROPERTY(BlueprintReadWrite)
	int32 AgainstVotes = 0;

	UPROPERTY(BlueprintReadWrite)
	int32 AbstainVotes = 0;

	UPROPERTY(BlueprintReadWrite)
	EProposalState State;

	UPROPERTY(BlueprintReadWrite)
	int32 Deadline = 0;
};

DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FOnWalletUnlocked, FString, Address);
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FOnWalletLocked);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnTransactionComplete, FString, TxHash, bool, Success);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnNftMinted, FString, TokenId, FString, MetadataUri);
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FOnVoteCast, FString, ProposalId, int32, Choice);

/**
 * Soulvan Wallet Subsystem for Unreal Engine 5.
 * Non-custodial blockchain operations with cinematic integration.
 */
UCLASS()
class SOULVAN_API USoulvanWalletSubsystem : public UGameInstanceSubsystem
{
	GENERATED_BODY()

public:
	// USubsystem interface
	virtual void Initialize(FSubsystemCollectionBase& Collection) override;
	virtual void Deinitialize() override;

	// Events
	UPROPERTY(BlueprintAssignable, Category = "Soulvan|Wallet")
	FOnWalletUnlocked OnWalletUnlocked;

	UPROPERTY(BlueprintAssignable, Category = "Soulvan|Wallet")
	FOnWalletLocked OnWalletLocked;

	UPROPERTY(BlueprintAssignable, Category = "Soulvan|Wallet")
	FOnTransactionComplete OnTransactionComplete;

	UPROPERTY(BlueprintAssignable, Category = "Soulvan|Wallet")
	FOnNftMinted OnNftMinted;

	UPROPERTY(BlueprintAssignable, Category = "Soulvan|Wallet")
	FOnVoteCast OnVoteCast;

	// Wallet Core
	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void UnlockWallet(const FString& Passphrase);

	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void LockWallet();

	UFUNCTION(BlueprintPure, Category = "Soulvan|Wallet")
	FString GetWalletAddress() const { return WalletAddress; }

	UFUNCTION(BlueprintPure, Category = "Soulvan|Wallet")
	bool IsWalletUnlocked() const { return bIsUnlocked; }

	// Token Operations
	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void SendTokens(const FString& ToAddress, float Amount, float MaxFee);

	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void GetBalances();

	// NFT Operations
	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void MintNft(const FString& MetadataUri);

	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void TransferNft(const FString& TokenId, const FString& ToAddress);

	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void GetNfts();

	// Governance
	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void VoteOnProposal(const FString& ProposalId, int32 Choice);

	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void SubmitProposal(const FString& Description, const TArray<uint8>& Calldata);

	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void GetProposals();

	// Chronicle
	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	void GetChronicleEntries();

	// Security
	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	bool ExportSeed(const FString& OutputPath);

	UFUNCTION(BlueprintCallable, Category = "Soulvan|Wallet")
	bool ChangePassphrase(const FString& OldPassphrase, const FString& NewPassphrase);

	// Cached Data
	UFUNCTION(BlueprintPure, Category = "Soulvan|Wallet")
	FBalanceState GetCachedBalances() const { return CachedBalances; }

	UFUNCTION(BlueprintPure, Category = "Soulvan|Wallet")
	TArray<FNftData> GetCachedNfts() const { return CachedNfts; }

	UFUNCTION(BlueprintPure, Category = "Soulvan|Wallet")
	TArray<FProposalData> GetCachedProposals() const { return CachedProposals; }

protected:
	// Configuration
	UPROPERTY()
	FString RpcUrl = TEXT("http://localhost:8545");

	UPROPERTY()
	int32 ChainId = 31337;

	UPROPERTY()
	FString WalletAddress;

	UPROPERTY()
	bool bIsUnlocked = false;

	// Cached State
	UPROPERTY()
	FBalanceState CachedBalances;

	UPROPERTY()
	TArray<FNftData> CachedNfts;

	UPROPERTY()
	TArray<FProposalData> CachedProposals;

	// Contract Addresses
	UPROPERTY()
	FString SoulvanCoinAddress;

	UPROPERTY()
	FString CarSkinAddress;

	UPROPERTY()
	FString ChronicleAddress;

	UPROPERTY()
	FString GovernanceAddress;

	// Off-chain cache
	TArray<FString> PendingRewards;

	// Async operation helpers
	void ProcessUnlockResponse(bool bSuccess, const FString& Address);
	void ProcessBalancesResponse(const FBalanceState& Balances);
	void ProcessNftsResponse(const TArray<FNftData>& Nfts);
	void ProcessProposalsResponse(const TArray<FProposalData>& Proposals);
	void ProcessMintResponse(bool bSuccess, const FString& TokenId, const FString& TxHash);
	void ProcessVoteResponse(bool bSuccess, const FString& ProposalId, const FString& TxHash);
};
