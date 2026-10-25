#include "../inc/FibexFile.h"
#include "../inc/Fibex.h"

#include <QRegularExpression>
#include <QFile>
#include <QFileInfo>
#include <QXmlStreamReader>
#include <QDomDocument>
#include <QDomElement>
#include <QDebug>

//#define F_NO_DEBUG

namespace ASAM {
namespace FIBEX {

File::File()
{
    m_fibex = new Fibex();
}

File::~File()
{
    delete m_fibex;
}

bool File::load(const QString &fileName)
{
    bool ret = true;

    QFileInfo fi = QFileInfo(fileName);
    QString ext = fi.suffix();

    QFile *inFile = new QFile(fileName);

#ifndef F_NO_DEBUG
    qDebug() << "Fibex File: " << fileName;
#endif

    if (!inFile->open(QIODevice::ReadOnly | QIODevice::Text))
    {
        delete inFile;
        return false;
    }

    m_fileName = fileName;
#ifndef F_NO_DEBUG
    qDebug() << "Starting fibex load";
#endif

    QDomDocument doc;
    QString errorStr;
    int errorLine;
    int errorColumn;

    if (!doc.setContent(inFile, false, &errorStr, &errorLine, &errorColumn))
    {
        qDebug() << "Error: Parse error at line " << errorLine << ", "
                  << "column " << errorColumn << ": "
                  << qPrintable(errorStr);
		
        inFile->close();
        delete inFile;
        return false;
    }

    QDomElement root = doc.documentElement();

    qDebug() << root.tagName();

    m_fibex->load(root);

#ifndef F_NO_DEBUG
    qDebug() << "m_fibex load finish";
#endif
    inFile->close();
    delete inFile;
#ifndef F_NO_DEBUG
    qDebug() << "inFile closed";
#endif
    return ret;
}

}
}
