#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXFrameType.h"
#include "FXPduType.h"
#include "FXSignalType.h"
#include "FXEcuType.h"
#include "FRChannelType.h"
#include "FRClusterType.h"

class QDomElement;

typedef int GatewayType;
typedef int FunctionType;
typedef int CompositeType;

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType ELEMENTS
 *
 * Root of all stand-alone elements
 */
class ASAM_FIBEX_EXPORT FXElements
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXElements(FXFibex *fibex);
    ~FXElements();

    /** load from XML DOM element */
    bool load(const QDomElement &element);

    /**
     * In multi-channel systems such as TTP or FlexRay known as 'bus'.
     * To avoid confusion, we decides not to use this term any more.
     */
    QHash<QString, FRClusterType*> m_clusters;

    /**
     * In single-channel systems such as CAN or LIN known as 'bus'.
     * To avoid confusion, we decides not to use this term any more.
     */
    QHash<QString, FRChannelType*> m_channels;

    /** Electronic Control Unit (sometimes called 'node') */
    QHash<QString, FXEcuType*> m_ecus;

    QHash<QString, FXPduTypeCt*> m_pdus;

    /**
     * Smallest piece of information exchanged over the communication system.
     * A frame normally consists of a header and a data (payload) section.
     * The data section contains signals.
     */
    QHash<QString, FXFrameTypeCt*> m_frames;

    /** Smallest logical piece of information exchanged by applications */
    QHash<QString, FXSignalType*> m_signals;

    /**
     * @todo A gateway is a specific ECU translating frames and signals from one channel into another.
     * Further description of gateway behaviour is intended for a later version of FIBEX
     */
    QHash<QString, GatewayType*> m_gateways;

    /** @todo Part of the application running in an ECU */
    QHash<QString, FunctionType*> m_functions;

    /** @todo A hierarchical structure on functions or ecus. */
    QHash<QString, CompositeType*> m_composites;
};

}
}
