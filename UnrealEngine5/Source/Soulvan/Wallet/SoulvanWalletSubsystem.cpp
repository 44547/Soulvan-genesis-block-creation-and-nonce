// Copyright Soulvan 2025. All Rights Reserved.

#include "SoulvanWalletSubsystem.h"
#include "Engine/Engine.h"
#include "Kismet/GameplayStatics.h"

void USoulvanWalletSubsystem::Initialize(FSubsystemCollectionBase& Collection)
{
	Super::Initialize(Collection);

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Subsystem initialized"));

	// Load configuration from saved game or config file
	// Stub: would load RpcUrl, ChainId, contract addresses

	// Try to restore session
	// Stub: check for session passphrase in secure storage
}

void USoulvanWalletSubsystem::Deinitialize()
{
	// Lock wallet on shutdown
	if (bIsUnlocked)
	{
		LockWallet();
	}

	Super::Deinitialize();
}

void USoulvanWalletSubsystem::UnlockWallet(const FString& Passphrase)
{
	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Unlocking wallet..."));

	// Stub: Decrypt keystore with passphrase
	// Real implementation would use cryptography library
	
	// Simulate async unlock
	FTimerHandle TimerHandle;
	GetWorld()->GetTimerManager().SetTimer(TimerHandle, [this, Passphrase]()
	{
		// Generate deterministic address from passphrase for demo
		WalletAddress = FString::Printf(TEXT("0x%s...%s"), 
			*Passphrase.Left(4).ToUpper(), 
			*Passphrase.Right(4).ToUpper());
		bIsUnlocked = true;

		ProcessUnlockResponse(true, WalletAddress);
	}, 0.5f, false);
}

void USoulvanWalletSubsystem::LockWallet()
{
	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Locking wallet"));

	bIsUnlocked = false;
	WalletAddress.Empty();
	
	// Clear cached sensitive data
	CachedBalances = FBalanceState();
	CachedNfts.Empty();
	CachedProposals.Empty();

	OnWalletLocked.Broadcast();
}

void USoulvanWalletSubsystem::SendTokens(const FString& ToAddress, float Amount, float MaxFee)
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot send: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Sending %.2f SVN to %s"), Amount, *ToAddress);

	// Stub: Build and sign transaction
	// Real implementation would use Web3 library or RPC calls

	// Simulate async send
	FTimerHandle TimerHandle;
	GetWorld()->GetTimerManager().SetTimer(TimerHandle, [this, ToAddress, Amount]()
	{
		FString TxHash = FString::Printf(TEXT("0x%d"), FMath::Rand());
		OnTransactionComplete.Broadcast(TxHash, true);

		// Update cached balance
		CachedBalances.SoulvanCoin -= Amount;
	}, 1.0f, false);
}

void USoulvanWalletSubsystem::GetBalances()
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot get balances: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Fetching balances..."));

	// Stub: Query blockchain for balances
	// Real implementation would call RPC methods

	// Simulate async query
	FTimerHandle TimerHandle;
	GetWorld()->GetTimerManager().SetTimer(TimerHandle, [this]()
	{
		FBalanceState Balances;
		Balances.SoulvanCoin = 1000.0f;
		Balances.Eth = 0.5f;
		Balances.NftCount = 5;
		Balances.BadgeCount = 2;
		Balances.VotingPower = 100;

		ProcessBalancesResponse(Balances);
	}, 0.5f, false);
}

void USoulvanWalletSubsystem::MintNft(const FString& MetadataUri)
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot mint: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Minting NFT: %s"), *MetadataUri);

	// Cache pending reward
	PendingRewards.Add(MetadataUri);

	// Stub: Call mint function on NFT contract
	// Real implementation would build transaction and broadcast

	// Simulate async mint
	FTimerHandle TimerHandle;
	GetWorld()->GetTimerManager().SetTimer(TimerHandle, [this, MetadataUri]()
	{
		FString TokenId = FString::Printf(TEXT("%d"), CachedNfts.Num() + 1);
		FString TxHash = FString::Printf(TEXT("0x%d"), FMath::Rand());

		ProcessMintResponse(true, TokenId, TxHash);
		PendingRewards.Remove(MetadataUri);

		// Update cached NFT count
		CachedBalances.NftCount++;
	}, 2.0f, false);
}

void USoulvanWalletSubsystem::TransferNft(const FString& TokenId, const FString& ToAddress)
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot transfer: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Transferring NFT %s to %s"), *TokenId, *ToAddress);

	// Stub: Call transferFrom on NFT contract
}

void USoulvanWalletSubsystem::GetNfts()
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot get NFTs: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Fetching NFTs..."));

	// Stub: Query NFT contract for owned tokens
	// Real implementation would call tokenOfOwnerByIndex in loop

	// Simulate async query
	FTimerHandle TimerHandle;
	GetWorld()->GetTimerManager().SetTimer(TimerHandle, [this]()
	{
		TArray<FNftData> Nfts;
		
		// Example NFT
		FNftData ExampleNft;
		ExampleNft.TokenId = TEXT("1");
		ExampleNft.Name = TEXT("Bugatti Bolide Skin");
		ExampleNft.Description = TEXT("Storm Surge hypercar skin");
		ExampleNft.ImageUri = TEXT("https://soulvan.io/nft/cars/bugatti-bolide.png");
		ExampleNft.Rarity = TEXT("Legendary");
		ExampleNft.SeasonChapter = 1;
		ExampleNft.Type = ENftType::CarSkin;
		Nfts.Add(ExampleNft);

		ProcessNftsResponse(Nfts);
	}, 0.5f, false);
}

void USoulvanWalletSubsystem::VoteOnProposal(const FString& ProposalId, int32 Choice)
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot vote: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Voting on proposal %s: choice %d"), *ProposalId, Choice);

	// Stub: Call castVote on Governance contract
	// Real implementation would build and sign transaction

	// Simulate async vote
	FTimerHandle TimerHandle;
	GetWorld()->GetTimerManager().SetTimer(TimerHandle, [this, ProposalId, Choice]()
	{
		FString TxHash = FString::Printf(TEXT("0x%d"), FMath::Rand());
		ProcessVoteResponse(true, ProposalId, TxHash);
	}, 1.5f, false);
}

void USoulvanWalletSubsystem::SubmitProposal(const FString& Description, const TArray<uint8>& Calldata)
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot propose: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Submitting proposal: %s"), *Description);

	// Stub: Call propose on Governance contract
}

void USoulvanWalletSubsystem::GetProposals()
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot get proposals: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Fetching proposals..."));

	// Stub: Query Governance contract for active proposals

	// Simulate async query
	FTimerHandle TimerHandle;
	GetWorld()->GetTimerManager().SetTimer(TimerHandle, [this]()
	{
		TArray<FProposalData> Proposals;
		
		// Example proposal
		FProposalData ExampleProposal;
		ExampleProposal.Id = TEXT("1");
		ExampleProposal.Description = TEXT("Transition to Cosmic Season");
		ExampleProposal.ForVotes = 150;
		ExampleProposal.AgainstVotes = 30;
		ExampleProposal.AbstainVotes = 20;
		ExampleProposal.State = EProposalState::Active;
		ExampleProposal.Deadline = 1700000000;
		Proposals.Add(ExampleProposal);

		ProcessProposalsResponse(Proposals);
	}, 0.5f, false);
}

void USoulvanWalletSubsystem::GetChronicleEntries()
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot get chronicle: wallet locked"));
		return;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Fetching chronicle entries..."));

	// Stub: Query Chronicle contract for entries
}

bool USoulvanWalletSubsystem::ExportSeed(const FString& OutputPath)
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot export seed: wallet locked"));
		return false;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Exporting seed to %s"), *OutputPath);

	// Stub: Export encrypted seed phrase
	return true;
}

bool USoulvanWalletSubsystem::ChangePassphrase(const FString& OldPassphrase, const FString& NewPassphrase)
{
	if (!bIsUnlocked)
	{
		UE_LOG(LogTemp, Warning, TEXT("[SoulvanWallet] Cannot change passphrase: wallet locked"));
		return false;
	}

	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Changing passphrase..."));

	// Stub: Re-encrypt keystore with new passphrase
	return true;
}

// Response Handlers

void USoulvanWalletSubsystem::ProcessUnlockResponse(bool bSuccess, const FString& Address)
{
	if (bSuccess)
	{
		UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Unlocked: %s"), *Address);
		OnWalletUnlocked.Broadcast(Address);

		// Auto-fetch balances and NFTs
		GetBalances();
		GetNfts();
		GetProposals();
	}
	else
	{
		UE_LOG(LogTemp, Error, TEXT("[SoulvanWallet] Unlock failed"));
	}
}

void USoulvanWalletSubsystem::ProcessBalancesResponse(const FBalanceState& Balances)
{
	CachedBalances = Balances;
	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Balances updated: SVN=%.2f, NFTs=%d, VP=%d"),
		Balances.SoulvanCoin, Balances.NftCount, Balances.VotingPower);
}

void USoulvanWalletSubsystem::ProcessNftsResponse(const TArray<FNftData>& Nfts)
{
	CachedNfts = Nfts;
	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] NFTs updated: %d owned"), Nfts.Num());
}

void USoulvanWalletSubsystem::ProcessProposalsResponse(const TArray<FProposalData>& Proposals)
{
	CachedProposals = Proposals;
	UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Proposals updated: %d active"), Proposals.Num());
}

void USoulvanWalletSubsystem::ProcessMintResponse(bool bSuccess, const FString& TokenId, const FString& TxHash)
{
	if (bSuccess)
	{
		UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] NFT minted: TokenId=%s, Tx=%s"), *TokenId, *TxHash);
		OnNftMinted.Broadcast(TokenId, TxHash);
		
		// Refresh NFTs
		GetNfts();
	}
	else
	{
		UE_LOG(LogTemp, Error, TEXT("[SoulvanWallet] Mint failed"));
	}
}

void USoulvanWalletSubsystem::ProcessVoteResponse(bool bSuccess, const FString& ProposalId, const FString& TxHash)
{
	if (bSuccess)
	{
		UE_LOG(LogTemp, Log, TEXT("[SoulvanWallet] Vote cast: Proposal=%s, Tx=%s"), *ProposalId, *TxHash);
		OnVoteCast.Broadcast(ProposalId, 0); // Choice not stored in this stub
		
		// Refresh proposals
		GetProposals();
	}
	else
	{
		UE_LOG(LogTemp, Error, TEXT("[SoulvanWallet] Vote failed"));
	}
}
