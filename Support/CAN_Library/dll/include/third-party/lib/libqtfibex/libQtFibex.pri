# A. build library
# - Add directory to SUBDIRS
#
# B. use library
# - include <libName>.pri
#

QT += xml

LIB_SRCPATH     = $$PWD
PROJECT_LIBDIR = $$DESTDIR
PROJECT_BINDIR = $$DESTDIR

LIB_NAME = $$qtLibraryTarget(libQtFibex)

libqtdbc-buildlib{
    LIBS +=  -L$$PROJECT_LIBDIR -l$$LIB_NAME
}else{
    INCLUDEPATH += $$LIB_SRCPATH
    INCLUDEPATH += $$LIB_SRCPATH/inc
    DEPENDPATH  += $$LIB_SRCPATH

    win32 {
        QMAKE_CXXFLAGS += -march=i686
    }
    
    !contains(DEFINES, LIB_STATIC) {
        DEFINES += LIB_LIBRARY
        DEFINES += QLIBFIBEX_LIBRARY
    }

    LIB_VERSION_MAJOR   = 1
    LIB_VERSION_MINOR   = 0
    LIB_VERSION_RELEASE = 0
    LIB_VERSION_PATCH   = 0

    LIB_VERSION = '\\"$${LIB_VERSION_MAJOR}.$${LIB_VERSION_MINOR}.$${LIB_VERSION_RELEASE}\\"'
    DEFINES += LIB_VERSION_STR=\"$${LIB_VERSION}\"
    DEFINES += LIB_VERSION_MAJOR=$${LIB_VERSION_MAJOR}
    DEFINES += LIB_VERSION_MINOR=$${LIB_VERSION_MINOR}
    DEFINES += LIB_VERSION_PATCH=$${LIB_VERSION_PATCH}

    SOURCES += \
        $$LIB_SRCPATH/src/FBSignalGroup.cpp \
        $$LIB_SRCPATH/src/FXOrderedSignal.cpp \
        $$LIB_SRCPATH/src/FXRequirements.cpp \
        $$LIB_SRCPATH/src/FXSignalGroupType.cpp \
        $$LIB_SRCPATH/src/FibexFile.cpp\
        $$LIB_SRCPATH/src/Fibex.cpp \
        $$LIB_SRCPATH/src/FXFibex.cpp \
        $$LIB_SRCPATH/src/FXElements.cpp \
        $$LIB_SRCPATH/src/FXFrameType.cpp \
        $$LIB_SRCPATH/src/FibexTypes.cpp \
        $$LIB_SRCPATH/src/HONameDetails.cpp \
        $$LIB_SRCPATH/src/FXNamedElementType.cpp \
        $$LIB_SRCPATH/src/FXRevisedElementType.cpp \
        $$LIB_SRCPATH/src/FXPduInstances.cpp \
        $$LIB_SRCPATH/src/FXPduInstanceType.cpp \
        $$LIB_SRCPATH/src/FXGenericPduInstanceType.cpp \
        $$LIB_SRCPATH/src/FXPduType.cpp \
        $$LIB_SRCPATH/src/FXSignalInstances.cpp \
        $$LIB_SRCPATH/src/FXSignalInstanceType.cpp \
        $$LIB_SRCPATH/src/FXSignalRef.cpp \
        $$LIB_SRCPATH/src/FXCommonLayoutDetails.cpp \
        $$LIB_SRCPATH/src/FXSignalType.cpp \
        $$LIB_SRCPATH/src/FXSignalTypeType.cpp \
        $$LIB_SRCPATH/src/FXProcessingInformation.cpp \
        $$LIB_SRCPATH/src/FXCodings.cpp \
        $$LIB_SRCPATH/src/FXCodingType.cpp \
        $$LIB_SRCPATH/src/HOCodedType.cpp \
        $$LIB_SRCPATH/src/FXEcuType.cpp \
        $$LIB_SRCPATH/src/FXControllerType.cpp \
        $$LIB_SRCPATH/src/FRControllerType.cpp \
        $$LIB_SRCPATH/src/FRKeySlotUsage.cpp \
        $$LIB_SRCPATH/src/FXConnectorType.cpp \
        $$LIB_SRCPATH/src/FXEcuPortType.cpp \
        $$LIB_SRCPATH/src/FXIncludedPduType.cpp \
        $$LIB_SRCPATH/src/FXIncludedSignalType.cpp \
        $$LIB_SRCPATH/src/FRConnectorType.cpp \
        $$LIB_SRCPATH/src/HOCompuCategory.cpp \
        $$LIB_SRCPATH/src/HOCompuMethods.cpp \
        $$LIB_SRCPATH/src/HOScaleConstrType.cpp \
        $$LIB_SRCPATH/src/HOCompuRationalCoeffs.cpp \
        $$LIB_SRCPATH/src/HOCompuScale.cpp \
        $$LIB_SRCPATH/src/HOCompInternalToPhys.cpp \
        $$LIB_SRCPATH/src/HOCompuMethod.cpp \
        $$LIB_SRCPATH/src/HOUnit.cpp \
        $$LIB_SRCPATH/src/HOUnitSpec.cpp \
        $$LIB_SRCPATH/src/FXChannelType.cpp \
        $$LIB_SRCPATH/src/FXFrameTriggeringType.cpp \
        $$LIB_SRCPATH/src/FXPduTriggeringType.cpp \
        $$LIB_SRCPATH/src/FXFrameTimings.cpp \
        $$LIB_SRCPATH/src/FXAbsolutelyScheduledTimingType.cpp \
        $$LIB_SRCPATH/src/FXPduTimings.cpp \
        $$LIB_SRCPATH/src/FXCyclicTimingType.cpp \
        $$LIB_SRCPATH/src/FXEventControlledTimingType.cpp \
        $$LIB_SRCPATH/src/FXRequestControlledTimingType.cpp \
        $$LIB_SRCPATH/src/FXTimeRangeType.cpp \
        $$LIB_SRCPATH/src/FXActiveConditionType.cpp \
        $$LIB_SRCPATH/src/FXSystemTrigger.cpp \
        $$LIB_SRCPATH/src/FXExecuteConditionType.cpp \
        $$LIB_SRCPATH/src/FXSignalTrigger.cpp \
        $$LIB_SRCPATH/src/FXRelativelyScheduledTimingType.cpp \
        $$LIB_SRCPATH/src/FXIdentifier.cpp \
        $$LIB_SRCPATH/src/FXClusterType.cpp \
        $$LIB_SRCPATH/src/FRChannelType.cpp \
        $$LIB_SRCPATH/src/FRClusterType.cpp \
        $$LIB_SRCPATH/src/FBSignal.cpp \
        $$LIB_SRCPATH/src/HOScaleConstr.cpp \
        $$LIB_SRCPATH/src/HOCompuConst.cpp \
        $$LIB_SRCPATH/src/FBPdu.cpp \
        $$LIB_SRCPATH/src/FBFrame.cpp \
        $$LIB_SRCPATH/src/FBChannel.cpp \
        $$LIB_SRCPATH/src/FBCluster.cpp \
        $$LIB_SRCPATH/src/FBEcu.cpp \
        $$LIB_SRCPATH/src/FibexKey.cpp \
        $$LIB_SRCPATH/src/FRWakeUp.cpp

    HEADERS += \
        $$LIB_SRCPATH/inc/FBSignalGroup.h \
        $$LIB_SRCPATH/inc/FXOrderedSignal.h \
        $$LIB_SRCPATH/inc/FXRequirements.h \
        $$LIB_SRCPATH/inc/FXSignalGroupType.h \
        $$LIB_SRCPATH/inc/asam_fibex_export.h \
        $$LIB_SRCPATH/inc/File.h \
        $$LIB_SRCPATH/inc/platform.h \
        $$LIB_SRCPATH/inc/Fibex.h \
        $$LIB_SRCPATH/inc/FXFibex.h \
        $$LIB_SRCPATH/inc/FXElements.h \
        $$LIB_SRCPATH/inc/FXFrameType.h \
        $$LIB_SRCPATH/inc/FibexTypes.h \
        $$LIB_SRCPATH/inc/HONameDetails.h \
        $$LIB_SRCPATH/inc/FXNamedElementType.h \
        $$LIB_SRCPATH/inc/FXRevisedElementType.h \
        $$LIB_SRCPATH/inc/FXPduInstances.h \
        $$LIB_SRCPATH/inc/FXPduInstanceType.h \
        $$LIB_SRCPATH/inc/FXGenericPduInstanceType.h \
        $$LIB_SRCPATH/inc/FXPduType.h \
        $$LIB_SRCPATH/inc/FXSignalInstances.h \
        $$LIB_SRCPATH/inc/FXSignalInstanceType.h \
        $$LIB_SRCPATH/inc/FXSignalRef.h \
        $$LIB_SRCPATH/inc/FXCommonLayoutDetails.h \
        $$LIB_SRCPATH/inc/FXSignalType.h \
        $$LIB_SRCPATH/inc/FXSignalTypeType.h \
        $$LIB_SRCPATH/inc/FXProcessingInformation.h \
        $$LIB_SRCPATH/inc/FXCodings.h \
        $$LIB_SRCPATH/inc/FXCodingType.h \
        $$LIB_SRCPATH/inc/HOCodedType.h \
        $$LIB_SRCPATH/inc/FXEcuType.h \
        $$LIB_SRCPATH/inc/FXControllerType.h \
        $$LIB_SRCPATH/inc/FRControllerType.h \
        $$LIB_SRCPATH/inc/FRKeySlotUsage.h \
        $$LIB_SRCPATH/inc/FXConnectorType.h \
        $$LIB_SRCPATH/inc/FXEcuPortType.h \
        $$LIB_SRCPATH/inc/FXIncludedPduType.h \
        $$LIB_SRCPATH/inc/FXIncludedSignalType.h \
        $$LIB_SRCPATH/inc/FRConnectorType.h \
        $$LIB_SRCPATH/inc/HOCompuCategory.h \
        $$LIB_SRCPATH/inc/HOCompuMethods.h \
        $$LIB_SRCPATH/inc/HOScaleConstrType.h \
        $$LIB_SRCPATH/inc/HOCompuRationalCoeffs.h \
        $$LIB_SRCPATH/inc/HOCompuScale.h \
        $$LIB_SRCPATH/inc/HOCompInternalToPhys.h \
        $$LIB_SRCPATH/inc/HOCompuMethod.h \
        $$LIB_SRCPATH/inc/HOUnit.h \
        $$LIB_SRCPATH/inc/HOUnitSpec.h \
        $$LIB_SRCPATH/inc/FXChannelType.h \
        $$LIB_SRCPATH/inc/FXFrameTriggeringType.h \
        $$LIB_SRCPATH/inc/FXPduTriggeringType.h \
        $$LIB_SRCPATH/inc/FXFrameTimings.h \
        $$LIB_SRCPATH/inc/FXAbsolutelyScheduledTimingType.h \
        $$LIB_SRCPATH/inc/FXPduTimings.h \
        $$LIB_SRCPATH/inc/FXCyclicTimingType.h \
        $$LIB_SRCPATH/inc/FXEventControlledTimingType.h \
        $$LIB_SRCPATH/inc/FXRequestControlledTimingType.h \
        $$LIB_SRCPATH/inc/FXTimeRangeType.h \
        $$LIB_SRCPATH/inc/FXActiveConditionType.h \
        $$LIB_SRCPATH/inc/FXSystemTrigger.h \
        $$LIB_SRCPATH/inc/FXExecuteConditionType.h \
        $$LIB_SRCPATH/inc/FXSignalTrigger.h \
        $$LIB_SRCPATH/inc/FXRelativelyScheduledTimingType.h \
        $$LIB_SRCPATH/inc/FXIdentifier.h \
        $$LIB_SRCPATH/inc/FXClusterType.h \
        $$LIB_SRCPATH/inc/FRChannelType.h \
        $$LIB_SRCPATH/inc/FRClusterType.h \
        $$LIB_SRCPATH/inc/FBAttribute.h \
        $$LIB_SRCPATH/inc/FBSignal.h \
        $$LIB_SRCPATH/inc/FBAttributeValueType.h \
        $$LIB_SRCPATH/inc/HOScaleConstr.h \
        $$LIB_SRCPATH/inc/HOCompuConst.h \
        $$LIB_SRCPATH/inc/FBPdu.h \
        $$LIB_SRCPATH/inc/FBFrame.h \
        $$LIB_SRCPATH/inc/FBChannel.h \
        $$LIB_SRCPATH/inc/FBCluster.h \
        $$LIB_SRCPATH/inc/FBEcu.h \
        $$LIB_SRCPATH/inc/FibexKey.h \
        $$LIB_SRCPATH/inc/FRWakeUp.h
}

HEADERS += \
    $$PWD/fbframes.h

SOURCES += \
    $$PWD/fbframes.cpp
