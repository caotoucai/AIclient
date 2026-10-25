#pragma once

#include "platform.h"
#include "asam_fibex_export.h"

#include "FXCommonLayoutDetails.h"
#include "FXSignalRef.h"

namespace ASAM {
namespace FIBEX {

class FXFibex;

/**
 * @brief complexType SIGNAL-INSTANCE-TYPE
 *
 * Content model for the entity SIGNAL-INSTANCE
 */
class ASAM_FIBEX_EXPORT FXSignalInstanceType
{
public:
    FXFibex *m_fibex = nullptr;

public:
    FXSignalInstanceType(FXFibex *fibex = nullptr);
    ~FXSignalInstanceType();

    /** load from XML DOM element */
    void load(const QDomElement &element);

    /** attribute ID */
    QString m_id;
    /** group COMMON-LAYOUT-DETAILS */
    FXCommonLayoutDetails m_commLayoutDetails;
    /** element SIGNAL-REF */
    FXSignalRef m_sigRef;
    /** @opt element SIGNAL-UPDATE-BIT-POSITION */
    quint32 *m_signalUpdateBitPosition = nullptr;

    /** @todo element MANUFACTURER-EXTENSION */
};

} // FIBEX
} // ASAM
