#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXConnectorType.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType CONNECTOR-TYPE
 */
class ASAM_FIBEX_EXPORT FRConnectorType : public FXConnectorType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FRConnectorType(FXFibex *fibex = nullptr);

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** element WAKE-UP-CHANNEL */
    bool m_wakeUpChannel;
};

} // FIBEX
} // ASAM
