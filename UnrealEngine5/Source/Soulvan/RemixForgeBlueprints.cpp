// Soulvan Remix Forge Blueprints
// Unreal Engine 5 C++ components for Remix Forge Graph visualization

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "RemixForgeBlueprints.generated.h"

/**
 * BP_RemixLineage
 * Blueprint component for rendering remix ancestry paths and contributor divergence FX
 * Syncs with SoulvanRemixForgeGraphAPI for real-time updates
 */
UCLASS(Blueprintable, BlueprintType)
class SOULVAN_API ARemixLineageActor : public AActor
{
    GENERATED_BODY()

public:
    ARemixLineageActor();

    // Configuration
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Lineage")
    FString ContributorId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Lineage")
    TArray<FString> AncestorIds;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Lineage")
    FLinearColor LineageColor;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Lineage")
    float PathWidth = 0.3f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Lineage")
    float GlowIntensity = 2.0f;

    // Blueprint Events
    UFUNCTION(BlueprintCallable, Category = "Remix Lineage")
    void RenderLineagePath(const TArray<FVector>& AncestorPositions);

    UFUNCTION(BlueprintCallable, Category = "Remix Lineage")
    void TriggerLineageGlow();

    UFUNCTION(BlueprintCallable, Category = "Remix Lineage")
    void AnimateAncestryTrail();

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;

private:
    UPROPERTY()
    class USplineComponent* LineageSpline;

    UPROPERTY()
    class UParticleSystemComponent* AncestryFX;

    void UpdateLineageVisuals();
    void PulseLineageGlow(float DeltaTime);
};

/**
 * BP_RemixEcho
 * Blueprint component for animating saga echo trails from remix events and DAO triggers
 */
UCLASS(Blueprintable, BlueprintType)
class SOULVAN_API ARemixEchoActor : public AActor
{
    GENERATED_BODY()

public:
    ARemixEchoActor();

    // Configuration
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Echo")
    FString SourceNodeId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Echo")
    TArray<FString> EchoNodeIds;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Echo")
    FString Intensity; // "low", "medium", "high"

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Echo")
    float RippleSpeed = 2.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Remix Echo")
    float MaxRippleRadius = 100.0f;

    // Blueprint Events
    UFUNCTION(BlueprintCallable, Category = "Remix Echo")
    void TriggerEchoRipple(FVector SourcePosition);

    UFUNCTION(BlueprintCallable, Category = "Remix Echo")
    void AnimateEchoWave(float DeltaTime);

    UFUNCTION(BlueprintCallable, Category = "Remix Echo")
    void PropagateToEchoNodes();

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;

private:
    UPROPERTY()
    class UNiagaraComponent* RippleFX;

    UPROPERTY()
    class USoundBase* EchoSound;

    float CurrentRippleRadius;
    FVector RippleCenter;

    void CalculateAffectedNodes();
    FLinearColor GetIntensityColor() const;
};

/**
 * BP_ScrollConstellation
 * Blueprint component for displaying lore scrolls orbiting remix nodes with export triggers
 */
UCLASS(Blueprintable, BlueprintType)
class SOULVAN_API AScrollConstellationActor : public AActor
{
    GENERATED_BODY()

public:
    AScrollConstellationActor();

    // Scroll Data
    USTRUCT(BlueprintType)
    struct FScrollData
    {
        GENERATED_BODY()

        UPROPERTY(EditAnywhere, BlueprintReadWrite)
        FString Title;

        UPROPERTY(EditAnywhere, BlueprintReadWrite)
        FString Format; // "scroll", "nft", "bundle"

        UPROPERTY(EditAnywhere, BlueprintReadWrite)
        FString URL;
    };

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Scroll Constellation")
    TArray<FScrollData> Scrolls;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Scroll Constellation")
    float OrbitRadius = 5.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Scroll Constellation")
    float OrbitSpeed = 30.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Scroll Constellation")
    FVector CenterPosition;

    // Blueprint Events
    UFUNCTION(BlueprintCallable, Category = "Scroll Constellation")
    void SpawnScrollOrbits();

    UFUNCTION(BlueprintCallable, Category = "Scroll Constellation")
    void AnimateScrollOrbit(float DeltaTime);

    UFUNCTION(BlueprintCallable, Category = "Scroll Constellation")
    void OnScrollClicked(int32 ScrollIndex);

    UFUNCTION(BlueprintCallable, Category = "Scroll Constellation")
    void HighlightScroll(int32 ScrollIndex, bool bHighlight);

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;

private:
    UPROPERTY()
    TArray<class UStaticMeshComponent*> ScrollMeshes;

    UPROPERTY()
    TArray<class UPointLightComponent*> ScrollLights;

    float CurrentOrbitAngle;

    void UpdateScrollPositions(float DeltaTime);
    FLinearColor GetFormatColor(const FString& Format) const;
};

/**
 * BP_RemixDivergence
 * Blueprint for cinematic remix divergence visualization
 */
UCLASS(Blueprintable, BlueprintType)
class SOULVAN_API ARemixDivergenceActor : public AActor
{
    GENERATED_BODY()

public:
    ARemixDivergenceActor();

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Divergence")
    FString ContributorId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Divergence")
    FString RemixId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Divergence")
    int32 BranchCount = 3;

    UFUNCTION(BlueprintCallable, Category = "Divergence")
    void TriggerDivergenceReveal();

    UFUNCTION(BlueprintCallable, Category = "Divergence")
    void AnimateSagaBranches();

protected:
    virtual void BeginPlay() override;

private:
    UPROPERTY()
    class UNiagaraComponent* DivergenceFX;

    void CreateBranchTrails();
};

/**
 * BP_DAORippleEvent
 * Blueprint for DAO-triggered ripple FX across contributor graph
 */
UCLASS(Blueprintable, BlueprintType)
class SOULVAN_API ADAORippleEventActor : public AActor
{
    GENERATED_BODY()

public:
    ADAORippleEventActor();

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "DAO Ripple")
    FString ProposalId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "DAO Ripple")
    FString SourceNodeId;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "DAO Ripple")
    int32 VotePower;

    UFUNCTION(BlueprintCallable, Category = "DAO Ripple")
    void TriggerDAORipple(FVector SourcePosition);

    UFUNCTION(BlueprintCallable, Category = "DAO Ripple")
    void PropagateRippleWave(float DeltaTime);

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;

private:
    UPROPERTY()
    class UNiagaraComponent* RippleWaveFX;

    float RippleProgress;
    TArray<FVector> AffectedNodePositions;

    void CalculateRippleImpact();
};

/**
 * BP_RemixForgeWidget
 * UI Blueprint for Remix Forge dashboard in Unreal
 */
UCLASS()
class SOULVAN_API URemixForgeWidget : public UUserWidget
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "Remix Forge UI")
    void DisplayContributorGraph();

    UFUNCTION(BlueprintCallable, Category = "Remix Forge UI")
    void ShowLineagePath(const FString& ContributorId);

    UFUNCTION(BlueprintCallable, Category = "Remix Forge UI")
    void ShowScrollOrbits(const FString& ContributorId);

    UFUNCTION(BlueprintCallable, Category = "Remix Forge UI")
    void TriggerRemixExport(const FString& RemixId, const FString& Format);

protected:
    virtual void NativeConstruct() override;

private:
    UPROPERTY(meta = (BindWidget))
    class UCanvasPanel* GraphCanvas;

    UPROPERTY(meta = (BindWidget))
    class UTextBlock* ContributorNameText;

    UPROPERTY(meta = (BindWidget))
    class UButton* ExportButton;

    void InitializeAPIConnection();
    void UpdateGraphVisualization();
};

/**
 * Remix Forge Blueprint Function Library
 * Utility functions for Remix Forge Graph operations
 */
UCLASS()
class SOULVAN_API URemixForgeBlueprintLibrary : public UBlueprintFunctionLibrary
{
    GENERATED_BODY()

public:
    UFUNCTION(BlueprintCallable, Category = "Remix Forge")
    static FLinearColor GetTierColor(const FString& Tier);

    UFUNCTION(BlueprintCallable, Category = "Remix Forge")
    static float CalculateOrbitPosition(float Angle, float Radius);

    UFUNCTION(BlueprintCallable, Category = "Remix Forge")
    static TArray<FVector> GenerateLineagePath(const TArray<FVector>& AncestorPositions);

    UFUNCTION(BlueprintCallable, Category = "Remix Forge")
    static void PlayRemixVoiceLine(const FString& VoiceLineType);
};

// ============================
// Implementation Notes
// ============================

/*
 * USAGE EXAMPLE (Blueprint Graph):
 * 
 * 1. Create BP_RemixLineage actor
 * 2. Set ContributorId and AncestorIds
 * 3. Call RenderLineagePath with ancestor positions
 * 4. Trigger lineage glow on remix event
 * 
 * 5. Create BP_RemixEcho actor for DAO ripples
 * 6. Set SourceNodeId and EchoNodeIds
 * 7. Call TriggerEchoRipple on DAO vote
 * 
 * 8. Create BP_ScrollConstellation for lore exports
 * 9. Populate Scrolls array with scroll data
 * 10. Call SpawnScrollOrbits to render
 * 
 * VOICE LINES:
 * - "Your legend diverges. The vault remembers." (Divergence)
 * - "Your remix echoes through the vault." (Echo)
 * - "Your scrolls orbit the forge." (Scroll)
 * - "The lineage reveals your saga." (Lineage)
 * - "The DAO ripples through the remix graph." (DAO)
 * 
 * VISUAL FX:
 * - Niagara systems for ripple waves, glow trails, particle bursts
 * - Spline components for lineage paths
 * - Dynamic materials for tier-based colors
 * - Point lights for node glow and scroll highlights
 */
