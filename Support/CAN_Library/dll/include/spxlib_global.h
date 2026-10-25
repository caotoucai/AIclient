#ifndef SPXLIB_GLOBAL_H
#define SPXLIB_GLOBAL_H
#include <iostream>

//#include <QtCore/qglobal.h>

#ifndef Q_DECL_EXPORT
#  define Q_DECL_EXPORT     __declspec(dllexport)
#  define Q_DECL_IMPORT     __declspec(dllimport)
#endif

#if defined(SPXLIB_LIBRARY)
#  define SPXLIBSHARED_EXPORT Q_DECL_EXPORT
#else
#  define SPXLIBSHARED_EXPORT //Q_DECL_IMPORT
#endif



#endif // SPXLIB_GLOBAL_H
