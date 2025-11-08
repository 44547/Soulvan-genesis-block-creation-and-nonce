// Copyright Epic Games, Inc. All Rights Reserved.
// Soulvan Cinematic Motif Component for Unreal Engine 5

#pragma once

#include "CoreMinimal.h"
#include "Components/ActorComponent.h"
#include "NiagaraComponent.h"
#include "Components/AudioComponent.h"
#include "SoulvanMotifComponent.generated.h"

UENUM(BlueprintType)
enum class EMotif : uint8
{
	Storm   UMETA(DisplayName = "Storm"),
	Calm    UMETA(DisplayName = "Calm"),
	Cosmic  UMETA(DisplayName = "Cosmic"),
	Oracle  UMETA(DisplayName = "Oracle")
};

/**
 * Cinematic motif system controlling visual/audio/haptic overlays in Unreal.
 * Integrates with Niagara VFX, Audio Components, and MetaSounds.
 */
UCLASS(ClassGroup=(Soulvan), meta=(BlueprintSpawnableComponent))
class SOULVAN_API USoulvanMotifComponent : public UActorComponent
{
	GENERATED_BODY()

public:	
	USoulvanMotifComponent();

protected:
	virtual void BeginPlay() override;

public:	
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	/**
	 * Set active motif and intensity.
	 * @param Motif - Target motif type
	 * @param Intensity01 - 0 = minimal, 1 = maximum intensity
	 */
	UFUNCTION(BlueprintCallable, Category = "Soulvan|Motif")
	void SetMotif(EMotif Motif, float Intensity01);

	/**
	 * Get current active motif.
	 */
	UFUNCTION(BlueprintPure, Category = "Soulvan|Motif")
	EMotif GetCurrentMotif() const { return CurrentMotif; }

	/**
	 * Get current intensity.
	 */
	UFUNCTION(BlueprintPure, Category = "Soulvan|Motif")
	float GetCurrentIntensity() const { return CurrentIntensity; }

protected:
	// Visual FX Components
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Motif|Visual")
	UNiagaraComponent* StormRain;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Motif|Visual")
	UNiagaraComponent* CalmFog;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Motif|Visual")
	UNiagaraComponent* CosmicAurora;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Motif|Visual")
	UNiagaraComponent* OracleRunes;

	// Audio
	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "Motif|Audio")
	UAudioComponent* MusicBus;

	UPROPERTY(EditAnywhere, Category = "Motif|Audio")
	USoundBase* StormMusic;

	UPROPERTY(EditAnywhere, Category = "Motif|Audio")
	USoundBase* CalmMusic;

	UPROPERTY(EditAnywhere, Category = "Motif|Audio")
	USoundBase* CosmicMusic;

	UPROPERTY(EditAnywhere, Category = "Motif|Audio")
	USoundBase* OracleMusic;

private:
	EMotif CurrentMotif;
	float CurrentIntensity;

	void UpdateVisualFX(EMotif Motif, float Intensity);
	void UpdateAudio(EMotif Motif, float Intensity);
	void SetNiagaraFloatParameter(UNiagaraComponent* Comp, FName ParamName, float Value);
};
