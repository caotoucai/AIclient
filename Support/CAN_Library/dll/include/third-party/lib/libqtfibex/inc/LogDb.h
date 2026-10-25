#ifndef LOGDB_H
#define LOGDB_H

//#include "QAppLogging.h"

#include <QDebug>

#define QLOG_TRACE()    qDebug()
#define QLOG_DEBUG()    qInfo()
#define QLOG_INFO()     qInfo()
#define QLOG_WARNING()  qWarning()

#endif // LOGDB_H
