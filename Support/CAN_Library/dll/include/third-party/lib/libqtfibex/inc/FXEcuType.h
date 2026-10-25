#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXRevisedElementType.h"
#include "FRControllerType.h"
#include "FRConnectorType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType fx:ECU-TYPE
 *
 * Content model for the entity ECU.
 *
 * Platform-specific extensions are not possible, to maintain the capability to
 * describe multi-platform networks.
 *
 */
class ASAM_FIBEX_EXPORT FXEcuType : public FXRevisedElementType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXEcuType(FXFibex *fibex = nullptr);
    ~FXEcuType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** @todo element DIAGNOSTIC-ADDRESSES */
    /** @todo element FUNCTION-REFS */
    /** @opt element CONTROLLERS */
    QHash<QString, FRControllerType*> m_controllers;
    /** @opt element CONNECTORS */
    QHash<QString, FRConnectorType*> m_connectors;

    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
