// Copyright Epic Games, Inc. All Rights Reserved.
// Soulvan Cinematic Motif Component Implementation

#include "SoulvanMotifComponent.h"
#include "NiagaraFunctionLibrary.h"
#include "Kismet/GameplayStatics.h"

USoulvanMotifComponent::USoulvanMotifComponent()
{
	PrimaryComponentTick.bCanEverTick = true;
	CurrentMotif = EMotif::Storm;
	CurrentIntensity = 0.5f;
}

void USoulvanMotifComponent::BeginPlay()
{
	Super::BeginPlay();
	
	// Initialize with default motif
	SetMotif(CurrentMotif, CurrentIntensity);
}

void USoulvanMotifComponent::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
}

void USoulvanMotifComponent::SetMotif(EMotif Motif, float Intensity01)
{
	CurrentMotif = Motif;
	CurrentIntensity = FMath::Clamp(Intensity01, 0.f, 1.f);

	UpdateVisualFX(Motif, CurrentIntensity);
	UpdateAudio(Motif, CurrentIntensity);

	UE_LOG(LogTemp, Log, TEXT("[MotifComponent] Set motif: %d, intensity: %f"), (int32)Motif, CurrentIntensity);
}

void USoulvanMotifComponent::UpdateVisualFX(EMotif Motif, float Intensity)
{
	// Activate/deactivate Niagara systems
	if (StormRain) StormRain->SetActive(Motif == EMotif::Storm);
	if (CalmFog) CalmFog->SetActive(Motif == EMotif::Calm);
	if (CosmicAurora) CosmicAurora->SetActive(Motif == EMotif::Cosmic);
	if (OracleRunes) OracleRunes->SetActive(Motif == EMotif::Oracle);

	// Scale emission rates based on intensity
	const float BaseRate = FMath::Lerp(10.f, 200.f, Intensity);
	
	SetNiagaraFloatParameter(StormRain, FName("EmissionRate"), BaseRate);
	SetNiagaraFloatParameter(CalmFog, FName("EmissionRate"), BaseRate * 0.5f);
	SetNiagaraFloatParameter(CosmicAurora, FName("EmissionRate"), BaseRate * 0.8f);
	SetNiagaraFloatParameter(OracleRunes, FName("EmissionRate"), BaseRate * 0.6f);
}

void USoulvanMotifComponent::UpdateAudio(EMotif Motif, float Intensity)
{
	if (!MusicBus) return;

	// Select music track based on motif
	USoundBase* TargetMusic = nullptr;
	switch (Motif)
	{
	case EMotif::Storm:
		TargetMusic = StormMusic;
		break;
	case EMotif::Calm:
		TargetMusic = CalmMusic;
		break;
	case EMotif::Cosmic:
		TargetMusic = CosmicMusic;
		break;
	case EMotif::Oracle:
		TargetMusic = OracleMusic;
		break;
	}

	// Crossfade to new track if different
	if (TargetMusic && MusicBus->Sound != TargetMusic)
	{
		MusicBus->SetSound(TargetMusic);
		MusicBus->Play();
	}

	// Adjust pitch and volume based on intensity
	MusicBus->SetPitchMultiplier(FMath::Lerp(0.95f, 1.08f, Intensity));
	MusicBus->SetVolumeMultiplier(FMath::Lerp(0.6f, 1.f, Intensity));
}

void USoulvanMotifComponent::SetNiagaraFloatParameter(UNiagaraComponent* Comp, FName ParamName, float Value)
{
	if (!Comp) return;
	Comp->SetFloatParameter(ParamName, Value);
}
