include(../../../appskeleton.pri)

TEMPLATE = lib
CONFIG += libqtdbc-buildlib

include(libqtfibex.pri)

TARGET = $$LIB_NAME

#CONFIG += debug_and_release build_all
CONFIG += skip_target_version_ext
