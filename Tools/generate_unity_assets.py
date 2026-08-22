#!/usr/bin/env python3
"""Generate .meta files, YAML scenes, and ScriptableObject assets with stable GUIDs."""
from __future__ import annotations

import hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "Assets"


def guid_for(rel: str) -> str:
    return hashlib.md5(f"bankrupt-vtuber:{rel}".encode()).hexdigest()


def write(path: Path, text: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text, encoding="utf-8")
    print("wrote", path.relative_to(ROOT))


def folder_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
folderAsset: yes
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def script_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
MonoImporter:
  externalObjects: {{}}
  serializedVersion: 2
  defaultReferences: []
  executionOrder: 0
  icon: {{instanceID: 0}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def default_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
DefaultImporter:
  externalObjects: {{}}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def native_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def font_meta(guid: str) -> str:
    return f"""fileFormatVersion: 2
guid: {guid}
TrueTypeFontImporter:
  externalObjects: {{}}
  serializedVersion: 4
  fontSize: 16
  forceTextureCase: -2
  characterSpacing: 0
  characterPadding: 1
  includeFontData: 1
  fontNames:
  - Noto Sans KR
  fallbackFontReferences: []
  customCharacters: 
  fontRenderingMode: 0
  ascentCalculationMode: 1
  useLegacyBoundsCalculation: 0
  shouldRoundAdvanceValue: 1
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


SCENE_HEAD = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_OcclusionBakeSettings:
    smallestOccluder: 5
    smallestHole: 0.25
    backfaceThreshold: 100
  m_SceneGUID: 00000000000000000000000000000000
  m_OcclusionCullingData: {fileID: 0}
--- !u!104 &2
RenderSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 10
  m_Fog: 0
  m_FogColor: {r: 0.5, g: 0.5, b: 0.5, a: 1}
  m_FogMode: 3
  m_FogDensity: 0.01
  m_LinearFogStart: 0
  m_LinearFogEnd: 300
  m_AmbientSkyColor: {r: 0.212, g: 0.227, b: 0.259, a: 1}
  m_AmbientEquatorColor: {r: 0.114, g: 0.125, b: 0.133, a: 1}
  m_AmbientGroundColor: {r: 0.047, g: 0.043, b: 0.035, a: 1}
  m_AmbientIntensity: 1
  m_AmbientMode: 3
  m_SubtractiveShadowColor: {r: 0.42, g: 0.478, b: 0.627, a: 1}
  m_SkyboxMaterial: {fileID: 0}
  m_HaloStrength: 0.5
  m_FlareStrength: 1
  m_FlareFadeSpeed: 3
  m_HaloTexture: {fileID: 0}
  m_SpotCookie: {fileID: 10001, guid: 0000000000000000e000000000000000, type: 0}
  m_DefaultReflectionMode: 0
  m_DefaultReflectionResolution: 128
  m_ReflectionBounces: 1
  m_ReflectionIntensity: 1
  m_CustomReflection: {fileID: 0}
  m_Sun: {fileID: 0}
  m_UseRadianceAmbientProbe: 0
--- !u!157 &3
LightmapSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 13
  m_BakeOnSceneLoad: 0
  m_GISettings:
    serializedVersion: 2
    m_BounceScale: 1
    m_IndirectOutputScale: 1
    m_AlbedoBoost: 1
    m_EnvironmentLightingMode: 0
    m_EnableBakedLightmaps: 0
    m_EnableRealtimeLightmaps: 0
  m_LightmapEditorSettings:
    serializedVersion: 12
    m_Resolution: 2
    m_BakeResolution: 40
    m_AtlasSize: 1024
    m_AO: 0
    m_AOMaxDistance: 1
    m_CompAOExponent: 1
    m_CompAOExponentDirect: 0
    m_ExtractAmbientOcclusion: 0
    m_Padding: 2
    m_LightmapParameters: {fileID: 0}
    m_LightmapsBakeMode: 1
    m_TextureCompression: 1
    m_ReflectionCompression: 2
    m_MixedBakeMode: 2
    m_BakeBackend: 2
    m_PVRSampling: 1
    m_PVRDirectSampleCount: 32
    m_PVRSampleCount: 512
    m_PVRBounces: 2
    m_PVREnvironmentSampleCount: 256
    m_PVREnvironmentReferencePointCount: 2048
    m_PVRFilteringMode: 1
    m_PVRDenoiserTypeDirect: 1
    m_PVRDenoiserTypeIndirect: 1
    m_PVRDenoiserTypeAO: 1
    m_PVRFilterTypeDirect: 0
    m_PVRFilterTypeIndirect: 0
    m_PVRFilterTypeAO: 0
    m_PVREnvironmentMIS: 1
    m_PVRCulling: 1
    m_PVRFilteringGaussRadiusDirect: 1
    m_PVRFilteringGaussRadiusIndirect: 1
    m_PVRFilteringGaussRadiusAO: 1
    m_PVRFilteringAtrousPositionSigmaDirect: 0.5
    m_PVRFilteringAtrousPositionSigmaIndirect: 2
    m_PVRFilteringAtrousPositionSigmaAO: 1
    m_ExportTrainingData: 0
    m_TrainingDataDestination: TrainingData
    m_LightProbeSampleCountMultiplier: 4
  m_LightingDataAsset: {fileID: 0}
  m_LightingSettings: {fileID: 0}
--- !u!196 &4
NavMeshSettings:
  serializedVersion: 2
  m_ObjectHideFlags: 0
  m_BuildSettings:
    serializedVersion: 3
    agentTypeID: 0
    agentRadius: 0.5
    agentHeight: 2
    agentSlope: 45
    agentClimb: 0.4
    ledgeDropHeight: 0
    maxJumpAcrossDistance: 0
    minRegionArea: 2
    manualCellSize: 0
    cellSize: 0.16666667
    manualTileSize: 0
    tileSize: 256
    buildHeightMesh: 0
    maxJobWorkers: 0
    preserveTilesOutsideBounds: 0
    debug:
      m_Flags: 0
  m_NavMeshData: {fileID: 0}
"""


def scene_body(name: str, script_guid: str) -> str:
    return SCENE_HEAD + f"""--- !u!1 &1000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 1003}}
  - component: {{fileID: 1002}}
  - component: {{fileID: 1001}}
  m_Layer: 0
  m_Name: Main Camera
  m_TagString: MainCamera
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!81 &1001
AudioListener:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1000}}
  m_Enabled: 1
--- !u!20 &1002
Camera:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1000}}
  m_Enabled: 1
  serializedVersion: 2
  m_ClearFlags: 2
  m_BackGroundColor: {{r: 0.16470589, g: 0.10588236, b: 0.2, a: 1}}
  m_projectionMatrixMode: 1
  m_GateFitMode: 2
  m_FOVAxisMode: 0
  m_Iso: 200
  m_ShutterSpeed: 0.005
  m_Aperture: 16
  m_FocusDistance: 10
  m_FocalLength: 50
  m_BladeCount: 5
  m_Curvature: {{x: 2, y: 11}}
  m_BarrelClipping: 0.25
  m_Anamorphism: 0
  m_SensorSize: {{x: 36, y: 24}}
  m_LensShift: {{x: 0, y: 0}}
  m_NormalizedViewPortRect:
    serializedVersion: 2
    x: 0
    y: 0
    width: 1
    height: 1
  near clip plane: 0.3
  far clip plane: 1000
  field of view: 60
  orthographic: 1
  orthographic size: 5
  m_Depth: -1
  m_CullingMask:
    serializedVersion: 2
    m_Bits: 4294967295
  m_RenderingPath: -1
  m_TargetTexture: {{fileID: 0}}
  m_TargetDisplay: 0
  m_TargetEye: 3
  m_HDR: 1
  m_AllowMSAA: 1
  m_AllowDynamicResolution: 0
  m_ForceIntoRT: 0
  m_OcclusionCulling: 1
  m_StereoConvergence: 10
  m_StereoSeparation: 0.022
--- !u!4 &1003
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 1000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: -10}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!1 &2000
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
  - component: {{fileID: 2002}}
  - component: {{fileID: 2001}}
  m_Layer: 0
  m_Name: {name}Director
  m_TagString: Untagged
  m_Icon: {{fileID: 0}}
  m_NavMeshLayer: 0
  m_StaticEditorFlags: 0
  m_IsActive: 1
--- !u!114 &2001
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 2000}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
--- !u!4 &2002
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 2000}}
  serializedVersion: 2
  m_LocalRotation: {{x: 0, y: 0, z: 0, w: 1}}
  m_LocalPosition: {{x: 0, y: 0, z: 0}}
  m_LocalScale: {{x: 1, y: 1, z: 1}}
  m_ConstrainProportionsScale: 0
  m_Children: []
  m_Father: {{fileID: 0}}
  m_LocalEulerAnglesHint: {{x: 0, y: 0, z: 0}}
--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
  - {{fileID: 1003}}
  - {{fileID: 2002}}
"""


def main() -> None:
    # Folder metas
    folders = [
        "Assets",
        "Assets/Scenes",
        "Assets/Scripts",
        "Assets/Scripts/Core",
        "Assets/Scripts/Data",
        "Assets/Scripts/Economy",
        "Assets/Scripts/Stream",
        "Assets/Scripts/Presentation",
        "Assets/Scripts/Input",
        "Assets/Resources",
        "Assets/Resources/Balance",
        "Assets/Resources/Fonts",
        "Assets/Editor",
        "Assets/Sprites",
    ]
    for folder in folders:
        path = ROOT / folder
        path.mkdir(parents=True, exist_ok=True)
        write(path.with_suffix(path.suffix + ".meta") if folder != "Assets" else ROOT / "Assets.meta",
              folder_meta(guid_for(folder + "/")))
        if folder != "Assets":
            write(Path(str(path) + ".meta"), folder_meta(guid_for(folder + "/")))

    # Scripts
    for cs in (ROOT / "Assets").rglob("*.cs"):
        rel = str(cs.relative_to(ROOT)).replace("\\", "/")
        write(Path(str(cs) + ".meta"), script_meta(guid_for(rel)))

    font = ROOT / "Assets/Resources/Fonts/NotoSansKR-Regular.ttf"
    if font.exists():
        write(Path(str(font) + ".meta"), font_meta(guid_for("Assets/Resources/Fonts/NotoSansKR-Regular.ttf")))

    week_guid = guid_for("Assets/Scripts/Data/Week1Balance.cs")
    chat_guid = guid_for("Assets/Scripts/Data/ChatCatalog.cs")
    title_script = guid_for("Assets/Scripts/Presentation/TitleDirector.cs")
    week_start_script = guid_for("Assets/Scripts/Presentation/WeekStartDirector.cs")
    live_script = guid_for("Assets/Scripts/Presentation/LiveStreamDirector.cs")
    settle_script = guid_for("Assets/Scripts/Presentation/SettlementDirector.cs")
    title_scene_guid = guid_for("Assets/Scenes/Title.unity")
    week_scene_guid = guid_for("Assets/Scenes/WeekStart.unity")
    live_scene_guid = guid_for("Assets/Scenes/LiveStream.unity")
    settle_scene_guid = guid_for("Assets/Scenes/Settlement.unity")

    write(ROOT / "Assets/Scenes/Title.unity", scene_body("Title", title_script))
    write(ROOT / "Assets/Scenes/WeekStart.unity", scene_body("WeekStart", week_start_script))
    write(ROOT / "Assets/Scenes/LiveStream.unity", scene_body("LiveStream", live_script))
    write(ROOT / "Assets/Scenes/Settlement.unity", scene_body("Settlement", settle_script))
    write(ROOT / "Assets/Scenes/Title.unity.meta", default_meta(title_scene_guid))
    write(ROOT / "Assets/Scenes/WeekStart.unity.meta", default_meta(week_scene_guid))
    write(ROOT / "Assets/Scenes/LiveStream.unity.meta", default_meta(live_scene_guid))
    write(ROOT / "Assets/Scenes/Settlement.unity.meta", default_meta(settle_scene_guid))

    write(
        ROOT / "Assets/Resources/Balance/Week1Balance.asset",
        f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {week_guid}, type: 3}}
  m_Name: Week1Balance
  m_EditorClassIdentifier: BankruptVtuber.Week1Balance
  startingCash: 45000
  startingDebt: 50000
  startingMental: 100
  maxMental: 100
  startingViewers: 12
  daysInWeek: 5
  winDebtMax: 30000
  winCashMin: 70000
  bankruptDebt: 180000
  billRent: 8000
  billElectricNet: 4000
  billAvatarLicense: 3000
  billFood: 5000
  billGear: 2000
  streamSeconds: 90
  incomePerViewerPerSec: 3
  minViewers: 1
  perfectViewerDelta: 0.5
  greatViewerDelta: 0.2
  goodViewerDelta: 0
  missViewerDelta: -1.2
  perfectWindow: 0.07
  greatWindow: 0.13
  goodWindow: 0.22
  approachSeconds: 1.35
  chatSpawnStart: 1.55
  chatSpawnEnd: 1.05
  superchatMinInterval: 9
  superchatMaxInterval: 11
  superchatMinCount: 8
  superchatMaxCount: 10
  superchatMinWon: 1000
  superchatMaxWon: 6000
  hypeSuperchatMinWon: 2000
  hypeSuperchatMaxWon: 12000
  comboIncomeThreshold: 5
  comboIncomeMultiplier: 1.5
  hypePerfectCombo: 9
  hypeSeconds: 12
  hypeIncomeMultiplier: 2.5
  hypeSuperchatMultiplier: 2
  hypeViewersPerSec: 1
  missStreakMental: 3
  missStreakMentalPenalty: 12
  missStreakViewerPenalty: 4
  totalMissMentalTrigger: 10
  totalMissMentalPenalty: 20
  forceEndIncomeNumerator: 1
  forceEndIncomeDenominator: 2
  mentalRestoreEachMorning: 15
  eventEarliestSeconds: 35
  eventLatestSeconds: 55
  eventWindowSeconds: 1.15
  eventAntiSuccessViewers: 3
  eventAntiFailViewers: 4
  eventAntiFailMental: 8
  eventLagShieldSeconds: 5
  eventLagFailFreezeSeconds: 3
  extraThreats:
  - id: gear_break
    displayName: 장비 고장
    minWon: 7000
    maxWon: 11000
    artPath: Art/bill_gear
    tintHex: FF6A6A
  - id: rival
    displayName: 라이벌 견제
    minWon: 5000
    maxWon: 9000
    artPath: Art/badge_troll
    tintHex: C47BFF
  - id: platform_fee
    displayName: 플랫폼 수수료
    minWon: 4000
    maxWon: 7000
    artPath: Art/badge_superchat
    tintHex: FFB020
  - id: scandal
    displayName: 스캔들 루머
    minWon: 8000
    maxWon: 12000
    artPath: Art/badge_troll
    tintHex: FF3355
  - id: net_drop
    displayName: 인터넷 끊김
    minWon: 4000
    maxWon: 6000
    artPath: Art/bill_electric
    tintHex: 4EC8FF
""",
    )
    write(
        ROOT / "Assets/Resources/Balance/Week1Balance.asset.meta",
        native_meta(guid_for("Assets/Resources/Balance/Week1Balance.asset")),
    )

    write(
        ROOT / "Assets/Resources/Balance/ChatCatalog.asset",
        f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {chat_guid}, type: 3}}
  m_Name: ChatCatalog
  m_EditorClassIdentifier: BankruptVtuber.ChatCatalog
  positive:
  - 오늘 컨디션 좋아 보여요!
  - 안녕하세요~ 들어왔어요
  - 고정닉 출석합니다
  - 배경 너무 귀여워요
  - 목소리 힐돼요
  - 이모트 폭탄 가즈아
  - 썸네일 보고 클릭함
  - 오늘도 화이팅이에요
  empathy:
  - 저녁 뭐 드셨어요?
  - 요즘 제일 힘든 거 뭐예요?
  - 다음 컨텐츠 뭐예요?
  - 노래 한 곡만 해주실 수 있어요?
  - 수면 시간은 괜찮아요?
  - 부채 괜찮아요…? 걱정됨
  - 아바타 라이선스 비싸요?
  - 오늘 목표 시청자 몇 명이에요?
  laugh:
  - 구독 취소함 ㅋ
  - 재미없는데요
  - 다른 방 가는 중
  - 목소리 작다
  - 돈 벌 생각은 있음?
  - 채팅 읽기는 하냐
  - 광고만 나와라
  - 저 방이 더 나음
  thanks:
  - 밥 챙겨 먹어요!!
  - 이번 달 월세 보태세요
  - 응원합니다 화이팅
  - 장비 업글 하세요
  - 멘탈 지키세요
  - 오늘 정산 꼭 남기세요
  - 슈퍼챗으로 전기세 냄
  - 파산만은 안 돼
""",
    )
    write(
        ROOT / "Assets/Resources/Balance/ChatCatalog.asset.meta",
        native_meta(guid_for("Assets/Resources/Balance/ChatCatalog.asset")),
    )

    write(
        ROOT / "ProjectSettings/EditorBuildSettings.asset",
        f"""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1045 &1
EditorBuildSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Scenes:
  - enabled: 1
    path: Assets/Scenes/Title.unity
    guid: {title_scene_guid}
  - enabled: 1
    path: Assets/Scenes/WeekStart.unity
    guid: {week_scene_guid}
  - enabled: 1
    path: Assets/Scenes/LiveStream.unity
    guid: {live_scene_guid}
  - enabled: 1
    path: Assets/Scenes/Settlement.unity
    guid: {settle_scene_guid}
  m_configObjects: {{}}
  m_UseUCBPForAssetBundles: 0
""",
    )

    print("week_guid", week_guid)
    print("week_start_script", week_start_script)
    print("week_scene", week_scene_guid)


if __name__ == "__main__":
    main()
