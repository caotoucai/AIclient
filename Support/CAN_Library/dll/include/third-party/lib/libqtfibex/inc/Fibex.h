#pragma once

#include <QObject>

#include "platform.h"
#include "asam_fibex_export.h"

#include "FBSignal.h"
#include "FBPdu.h"
#include "FBFrame.h"
#include "FBChannel.h"
#include "FBEcu.h"
#include "FBCluster.h"
#include "FBSignalGroup.h"

#include "FXFibex.h"

#include <QObject>

class QDomElement;

namespace ASAM {
namespace FIBEX {

class ASAM_FIBEX_EXPORT Fibex : public FXFibex
{
public:
    Fibex();
    ~Fibex();

    /**
     * @brief Load Database
     * @param[in] filename File Name
     *
     * Loads database.
     */
    bool load(const QDomElement &element);

public:
    bool covertXml2Db();

    QHash<QString, FBCluster*> m_clusters;
    QHash<QString, FBEcu*> m_ecus;
    QHash<QString, FBChannel*> m_channels;
    QHash<QString, FBFrame*> m_frames;
    QHash<QString, FBPdu*> m_pdus;
    QHash<QString, FBSignal*> m_signals;
    QHash<QString, FBSignal*> m_signalNameMap;
    QHash<QString, FBSignalGroup*> m_signalGroups;

    const FBCluster *getFirstCluster();
    const FBChannel *getChannelByName(const QString &channelName = "Channel_A");
    const FBEcu *getNodeByName(const QString &nodeName, const QString &channelName = "Channel_A");
    bool createSigByUpdatebit();
private:
    QDomElement *m_domElement;

};

}
}
