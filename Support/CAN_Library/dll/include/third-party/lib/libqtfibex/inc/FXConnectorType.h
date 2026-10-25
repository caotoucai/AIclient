#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXEcuPortType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType CONNECTOR-TYPE
 *
 * Content model for the entity CONNECTOR with platform specific extendability.
 */
class ASAM_FIBEX_EXPORT FXConnectorType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXConnectorType(FXFibex *fibex);
    ~FXConnectorType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** attribute ID */
    QString m_id;

    /** element CHANNEL-REF */
    QString m_channelRef;
    /** element CONTROLLER-REF */
    QString m_controllerRef;
    /** @opt element INPUTS */
    QHash<QString, FXEcuPortType*> m_inputPorts;
    /** @opt element OUTPUTS */
    QHash<QString, FXEcuPortType*> m_outputPorts;
    /** @todo element MANUFACTURER-EXTENSION */

};

} // FIBEX
} // ASAM
